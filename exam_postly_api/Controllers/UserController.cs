using exam_postly_api.DTOs;
using exam_postly_api.Models;
using exam_postly_api.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using exam_postly_api.Interfaces;
using exam_postly_api.Services;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.WebUtilities;

namespace exam_postly_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDBContext _dbContext;
        private readonly UserService _userService;
        private readonly IConfiguration _config;
        private readonly IEmailSender _emailSender;

        public UserController(ApplicationDBContext dbContext, UserService userService, IConfiguration config, IEmailSender emailSender)
        {
            _dbContext = dbContext;
            _userService = userService;
            _config = config;
            _emailSender = emailSender;
        }

        [Authorize]
        [Route("user")]
        [HttpGet(Name = "GetUser")]
        public async Task<ActionResult> GetUser()
        {
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            
            var user = await _userService.GetUserByIdAsync(userId);
            if(user == null)
                return NotFound();
            
            return Ok(user);
        }

        [Route("users")]
        [HttpGet(Name = "GetUsers")]
        public async Task<ActionResult> GetUsers()
        {
            var users = _dbContext.Users;
            return Ok(users);
        }

        [Route("signup")]
        [HttpPost(Name = "CreateUser")]
        public async Task<ActionResult> CreateUser([FromBody] UserCreateDTO dto)
        {
            try
            {
                if (_dbContext.Users.Any(user => user.PhoneNumber == dto.PhoneNumber))
                {
                    return Conflict(new { message = "User with this phone number already exists" });
                }
                var saltPasswordPair = PasswordEncryptor.EncryptPassword(dto.Password);
                string hashedPassword = saltPasswordPair.hashedPassword;
                string salt = saltPasswordPair.salt;

                //var user = new User(dto.Name, dto.Email, hashedPassword, salt);
                var user = new User
                {
                    Username = dto.Username,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    PasswordHash = hashedPassword,
                    Salt = salt
                };
                
                await _userService.CreateUserAsync(user);
                
                var tokenBytes = RandomNumberGenerator.GetBytes(32);
                var encodedTokenString = WebEncoders.Base64UrlEncode(tokenBytes); // Direct to URL-safe
                var token = new VerifyToken()
                {
                    Token = encodedTokenString,
                    UserId = user.Id,
                    User = user
                };
                
                await _dbContext.VerifyTokens.AddAsync(token);
                await _dbContext.SaveChangesAsync();
                
                var frontendUrl = _config["Routing:FrontendUrl"];
                var verificationLink = frontendUrl + "/verify-email?token=" + encodedTokenString;
                
                await _emailSender.SendEmailAsync(dto.Email, "email verification", "Для верифікації акаунту перейдіть за цим посиланням: " + verificationLink);
                // return await AuthenticateUser(new LoginDTO { Username = dto.Username, Password = dto.Password });
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Unexpected error: " + ex.Message });
            }
        }

        [Route("verify-email")]
        [HttpPost(Name = "VerifyEmail")]
        public async Task<ActionResult> VerifyEmail([FromBody] string token)
        {
            var storedToken = await _dbContext.VerifyTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == token);
            if (storedToken == null) return Unauthorized(new  { message = "Token not found" });
            
            var user = storedToken.User;
            user.IsVerified = true;
            await _dbContext.SaveChangesAsync();
            
            // _dbContext.VerifyTokens.Remove(storedToken);
            // await _dbContext.SaveChangesAsync();
            
            return Ok();
        }

        [Authorize]
        [Route("delete-user")]
        [HttpDelete(Name = "DeleteUser")]
        public async Task<ActionResult> DeleteUser()
        {
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (userId == null)
                return Unauthorized();

            var user = await _dbContext.Users
                .Include(u => u.Offers)
                .ThenInclude(o => o.Images)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return NotFound("user not found");
            
            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();
            
            return Ok();
        }

        [Route("signin")]
        [HttpPost(Name = "AuthenticateUser")]
        public async Task<ActionResult> AuthenticateUser([FromBody] LoginDTO dto)
        {
            string username = dto.Username;
            string password = dto.Password;

            try
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(user => user.Username == username);
                if (user == null)
                {
                    return Unauthorized(new { message = "Wrong email or password" });
                }

                if (!user.IsVerified)
                {
                    return Unauthorized(new { message = "You must verify the account first" });
                }

                if (!PasswordEncryptor.VerifyPassword(password, user.PasswordHash, user.Salt))
                {
                    return Unauthorized(new { message = "Wrong email or password" });
                }

                var accessToken = GenerateAccessToken(user.Email, user.Id, _config, user.Role);

                var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
                var hashedRefreshToken = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
                var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);
                //var refreshTokenExpiry = DateTime.UtcNow.AddMinutes(1);//small value for a test

                await _dbContext.AddAsync(new RefreshToken
                {
                    TokenHash = hashedRefreshToken,
                    UserId = user.Id,
                    ExpiresAt = refreshTokenExpiry,
                    User = user
                });
                await _dbContext.SaveChangesAsync();

                Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = refreshTokenExpiry
                });

                return Ok(new { AccessToken = accessToken, message = "Login successful" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Unexpected error: " + ex.Message });
            }
        }

        public static string GenerateAccessToken(string email, int id, IConfiguration config, string role)
        {
            
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim( JwtRegisteredClaimNames.Sub, id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(IdentityData.AdminUserClaimName, role)
            };

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                // expires: DateTime.UtcNow.AddSeconds(30), // small value for a test

                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [Route("refresh")]
        [HttpPost(Name = "Refresh")]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken)) return Unauthorized();

            var hashedToken = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
            var storedToken = await _dbContext.RefreshTokens
                .Include(refreshToken => refreshToken.User)
                .FirstOrDefaultAsync(token => token.TokenHash == hashedToken && !token.IsRevoked);

            if(storedToken == null ) return Unauthorized();

            if (storedToken.ExpiresAt < DateTime.UtcNow) return Unauthorized();

            var user = storedToken.User;
            var newAccessToken = GenerateAccessToken(user.Email, user.Id, _config, user.Role);

            storedToken.IsRevoked = true;
            await _dbContext.SaveChangesAsync();

            var newRefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            var hashedNewRefreshToken = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(newRefreshToken)));
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            //var refreshTokenExpiry = DateTime.UtcNow.AddMinutes(1);//small value for a test

            await _dbContext.AddAsync(new RefreshToken
            {
                TokenHash = hashedNewRefreshToken,
                UserId = user.Id,
                ExpiresAt = refreshTokenExpiry,
                User = user
            });
            await _dbContext.SaveChangesAsync();

            Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = refreshTokenExpiry
            });

            return Ok(new { AccessToken = newAccessToken });
        }

        [Authorize]
        [Route("personal-page")]
        [HttpGet(Name = "GetCurrentUser")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (userId == null)
                return Unauthorized();

            var user = await _dbContext.Users
                .Include(u => u.Offers)
                .ThenInclude(o => o.Images)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return NotFound("user not found");

            var personalPageData = new PersonalPageDTO(user);

            return Ok(personalPageData);
        }

        [Authorize]
        [Route("edit-personal-data")]
        [HttpPut(Name = "EditPersonalData")]
        public async Task<IActionResult> EditPersonalData([FromBody] PersonalDataDTO dto)
        {
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (userId == null)
                return Unauthorized();

            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
                return NotFound("user not found");
            
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.City = dto.City;
            user.PostCode = dto.PostCode;
            user.Address = dto.Address;
            user.ApartmentNumber = dto.ApartmentNumber;
            user.Email = dto.Email;
            user.PhoneNumber = dto.PhoneNumber;
            
            try
            {
                await _dbContext.SaveChangesAsync();
                return Ok(new { message = "user data edited" });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, "An error occurred while updating the user: " + ex.Message);
            }
        }
        
        [Authorize]
        [Route("get-personal-data")]
        [HttpGet(Name = "GetPersonalData")]
        public async Task<IActionResult> GetPersonalData()
        {
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (userId == null)
                return Unauthorized();

            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
                return NotFound("user not found");
            
            return Ok(new PersonalDataDTO(user)
            {
                Username = user.Username,
                Email = user.Email
            });
        }

        [Authorize]
        [Route("logout")]
        [HttpPost(Name = "Logout")]
        public async Task<IActionResult> Logout()
        {
            int userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (userId == null)
                return Unauthorized();

            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
                return NotFound("user not found");

            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken)) return Unauthorized();

            var hashedToken = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
            var storedToken = await _dbContext.RefreshTokens
                .Include(refreshToken => refreshToken.User)
                .FirstOrDefaultAsync(token => token.TokenHash == hashedToken && !token.IsRevoked);
            
            if(storedToken == null) return Unauthorized();
            if (storedToken?.ExpiresAt < DateTime.UtcNow) return Unauthorized();

            storedToken.IsRevoked = true;
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = user.Username + " logged out" });
        }

        private readonly string token = "secret-token";

        [Route("forgot-password")]
        [HttpPost(Name = "ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] string email)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if(user == null) return Unauthorized(new { message = "no user with corresponding email found" });
            
            // var tokenString = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            // var encodedTokenString = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(tokenString));
            var tokenBytes = RandomNumberGenerator.GetBytes(32);
            var encodedTokenString = WebEncoders.Base64UrlEncode(tokenBytes); // Direct to URL-safe

            var token = new RestoreToken
            {
                Token = encodedTokenString,
                UserId = user.Id,
                User = user
            };
            
            await _dbContext.RestoreTokens.AddAsync(token);
            await _dbContext.SaveChangesAsync();
            
            var frontendUrl = _config["Routing:FrontendUrl"];
            var message = frontendUrl + "/restore-password?token=" + token.Token;
            
            await _emailSender.SendEmailAsync(email, "password restoration", message);
            return Ok();
        }

        [Route("validate-restore-token")]
        [HttpPost(Name = "ValidateRestoreToken")]
        public async Task<IActionResult> ValidateRestoreToken([FromBody] string token)
        {
            // var encodedTokenString = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            // Console.WriteLine(encodedTokenString);
            
            var storedToken = await _dbContext.RestoreTokens.FirstOrDefaultAsync(restoreToken => restoreToken.Token == token);
            if(storedToken == null) return Unauthorized(new { message = "restore token not found" });
            if (storedToken.ExpiresAt < DateTime.UtcNow) return Unauthorized(new { message = "restore token is expired" });
            //
            // var storedTokenString = storedToken.Token;
            //
            // if(storedTokenString != token) return Unauthorized(new { message = "tokens don't match" });
            return Ok(new { message = "restore token validated" });
        }

        [Route("restore-password")]
        [HttpPost(Name = "RestorePassword")]
        public async Task<IActionResult> RestorePassword([FromBody] RestorePasswordDTO dto)
        {
            var validationResult = await ValidateRestoreToken(dto.token);

            try
            {
                if (validationResult.GetType() == typeof(OkObjectResult))
                {
                    var storedToken = _dbContext.RestoreTokens
                        .Include(token => token.User)
                        .FirstOrDefault(t => t.Token == dto.token);
                    var user = storedToken.User;
                    
                    var saltPasswordPair = PasswordEncryptor.EncryptPassword(dto.newPassword);
                    string hashedPassword = saltPasswordPair.hashedPassword;
                    string salt = saltPasswordPair.salt;
                    
                    user.Salt = salt;
                    user.PasswordHash = hashedPassword;
                    
                    _dbContext.RestoreTokens.Remove(storedToken);
                    
                    await _dbContext.SaveChangesAsync();
                    
                    return Ok(new { message = "password restored. new password: " + dto.newPassword });
                }
                
                if (validationResult.GetType() == typeof(UnauthorizedResult))
                {
                    return Unauthorized(new { message = "invalid restore token" });
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, new { message = e.Message });
            }
            
            return validationResult;
        }
        
        
    }
}
