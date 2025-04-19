using exam_postly_api.DTOs;
using exam_postly_api.Models;
using exam_postly_api.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace exam_postly_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDBContext _dbContext;
        private readonly IConfiguration _config;

        public UserController(ApplicationDBContext dbContext, IConfiguration config)
        {
            _dbContext = dbContext;
            _config = config;
        }

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
                if (_dbContext.Users.Any(user => user.Email == dto.Email))
                {
                    return Conflict(new { message = "User with this email already exists" });
                }
                var saltPasswordPair = PasswordEncryptor.EncryptPassword(dto.Password);
                string hashedPassword = saltPasswordPair.hashedPassword;
                string salt = saltPasswordPair.salt;

                //var user = new User(dto.Name, dto.Email, hashedPassword, salt);
                var user = new User
                {
                    Username = dto.Username,
                    Email = dto.Email,
                    PasswordHash = hashedPassword,
                    Salt = salt
                };

                await _dbContext.Users.AddAsync(user);
                await _dbContext.SaveChangesAsync();

                //return await AuthenticateUser(new LoginDTO { Email = dto.Email, Password = dto.Password });
                return Ok("user created succesfully");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Unexpected error: " + ex.Message });
            }
        }
    }
}
