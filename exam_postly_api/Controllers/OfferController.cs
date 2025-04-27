using System.Security.Claims;
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
        public async Task<ActionResult> CreateOffer([FromBody] OfferDTO dto)
        {
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (userId == null)
                return Unauthorized();

            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
                return NotFound("user not found");

            await _dbContext.Offers.AddAsync(new Offer
            {
                Title = dto.Title,
                Price = Convert.ToDouble(dto.Price),
                ImageUrl = dto.ImageUrl,
                UserId = user.Id,
                User = user
            });
            await _dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}
