using System.Security.Claims;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using exam_postly_api.DTOs;
using exam_postly_api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace exam_postly_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OfferController : ControllerBase
    {
        private readonly string cloudinaryUrl = "cloudinary://946252865213996:c9iWNeLr9vVfY2zwYlWFc-mqfyg@dxvwnanu4";
        //public Offer[] offers = { new Offer { Title = "product 1", Price = 12.34, ImageUrl = "apple_z0rh3i"}, new Offer { Title = "product 2", Price = 56.78, ImageUrl = "apple_z0rh3i"} };
        private readonly ApplicationDBContext _dbContext;
        private readonly IConfiguration _config;

        public OfferController(ApplicationDBContext dbContext, IConfiguration config)
        {
            _dbContext = dbContext;
            _config = config;
        }

        [Route("offers")]
        [HttpGet(Name = "GetOffers")]
        public async Task<ActionResult> GetOffers()
        {
            var offers = _dbContext.Offers;
            return Ok(offers);
        }

        [Authorize]
        [Route("create-offer")]
        [HttpPost(Name = "CreateOffer")]
        public async Task<ActionResult> CreateOffer([FromForm] OfferDTO dto)
        {
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (userId == null)
                return Unauthorized();

            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
                return NotFound("user not found");

            

            Cloudinary cloudinary = new Cloudinary(cloudinaryUrl);
            cloudinary.Api.Secure = true;
            string name = Guid.NewGuid().ToString();
            
            var uploadParams = new ImageUploadParams()
            {
                // File = new FileDescription(@"https://cloudinary-devs.github.io/cld-docs-assets/assets/images/cld-sample.jpg"),
                // File = new FileDescription(@"./Controllers/mango.jpeg"),
                File = new FileDescription(name, dto.Image.OpenReadStream()),
                UseFilename = true,
                UniqueFilename = false,
                Overwrite = true
            };
            var uploadResult = cloudinary.Upload(uploadParams);
            if (uploadResult == null)
            {
                return BadRequest();
            }
            
            string imageUrl = uploadResult.JsonObj["original_filename"].ToString();
            
            await _dbContext.Offers.AddAsync(new Offer
            {
                Title = dto.Title,
                Price = Convert.ToDouble(dto.Price),
                ImageUrl = imageUrl,
                UserId = user.Id,
                User = user
            });
            await _dbContext.SaveChangesAsync();
            
            return Ok();
        }

        [Authorize]
        [Route("my-offers")]
        [HttpGet(Name = "GetMyOffers")]
        public async Task<ActionResult> GetMyOffers()
        {
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (userId == null)
                return Unauthorized();

            var user = await _dbContext.Users.Include(u => u.Offers).FirstOrDefaultAsync(u => u.Id == userId);;
            if (user == null)
                return NotFound("user not found");
            
            return Ok(user.Offers);
        }
    }
}
