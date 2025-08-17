using System.Security.Cryptography;
using System.Text;
using exam_postly_api.Models;
using exam_postly_api.Services;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace exam_postly_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GoogleAuthController : ControllerBase
{
    private readonly ApplicationDBContext _dbContext;
    private readonly IConfiguration _config;
    private readonly UserService _userService;
    
    public GoogleAuthController(ApplicationDBContext context, IConfiguration config, UserService userService)
    {
        _dbContext = context;
        _config = config;
        _userService = userService;
    }
    
    [Route("authorize")]
    [HttpPost(Name = "GoogleAuth")]
    public async Task<IActionResult> GoogleAuth([FromBody] string credential)
    {
        var clientId = "131530890468-fh6f28mtkb04gs02hva387frkbvieqs1.apps.googleusercontent.com";
        try
        {
            // Step 1: Verify the JWT token with Google's public keys
            var payload = await GoogleJsonWebSignature.ValidateAsync(
                credential, 
                new GoogleJsonWebSignature.ValidationSettings()
                {
                    // Audience = new[] { _config["GoogleAuth:ClientId"] }
                    Audience = new[] { clientId }
                });
    
            // Step 2: Extract user information from verified token
            var email = payload.Email;
            var googleId = payload.Subject; // This is the Google user ID
            var name = payload.Name;
            var picture = payload.Picture;
    
            // Step 3: Check if user exists in your database
            // var existingUser = await _dbContext.Users.FirstOrDefaultAsync(user => user.Email == email);
            var existingUser = await _userService.GetUserByEmailAsync(email);
            
            if (existingUser != null)
            {
                // User exists - update Google ID if not present
                if (string.IsNullOrEmpty(existingUser.GoogleId))
                {
                    existingUser.GoogleId = googleId;
                    await _userService.UpdateUserAsync(existingUser);
                }
                
                // Return success with existing user info
                // return Ok(new { 
                //     success = true, 
                //     user = existingUser,
                //     message = "Login successful" 
                // });
                
                var accessToken = UserController.GenerateAccessToken(existingUser.Email, existingUser.Id, _config, existingUser.Role);

                var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
                var hashedRefreshToken = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
                var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);
                //var refreshTokenExpiry = DateTime.UtcNow.AddMinutes(1);//small value for a test

                await _dbContext.AddAsync(new RefreshToken
                {
                    TokenHash = hashedRefreshToken,
                    UserId = existingUser.Id,
                    ExpiresAt = refreshTokenExpiry,
                    User = existingUser
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
            
            // Step 4: Create new user account
            var newUser = new User
            {
                Email = email,
                Username = name,
                GoogleId = googleId,
                // ProfilePicture = picture,
                CreatedAt = DateTime.UtcNow,
                IsVerified = true // Google accounts are pre-verified
            };
            
            await _userService.CreateUserAsync(newUser);
            
            // return Ok(new { 
            //     success = true, 
            //     user = newUser,
            //     message = "Account created successfully" 
            // });
            var newAccessToken = UserController.GenerateAccessToken(newUser.Email, newUser.Id, _config, newUser.Role);

            var newRefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            var newHashedRefreshToken = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(newRefreshToken)));
            var newRefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            //var refreshTokenExpiry = DateTime.UtcNow.AddMinutes(1);//small value for a test

            await _dbContext.AddAsync(new RefreshToken
            {
                TokenHash = newHashedRefreshToken,
                UserId = newUser.Id,
                ExpiresAt = newRefreshTokenExpiry,
                User = newUser
            });
            await _dbContext.SaveChangesAsync();

            Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = newRefreshTokenExpiry
            });

            return Ok(new { AccessToken = newAccessToken, message = "Account created succesfully" });
        }
        catch (InvalidJwtException)
        {
            return BadRequest(new { success = false, message = "Invalid Google token" });
        }
        catch (Exception ex)
        {
            // Log the exception
            return StatusCode(500, new { success = false, message = "Authentication failed" });
        }
    }
}