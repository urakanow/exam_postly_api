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
    public class FavoriteController : ControllerBase
    {
        private readonly ApplicationDBContext _dbContext;
        
        public FavoriteController(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        [Authorize]
        [Route("add-favorite")]
        [HttpPost(Name = "AddFavorite")]
        public async Task<ActionResult> AddFavorite([FromBody] int offerId)
        {
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (userId == null)
                return Unauthorized();

            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
                return NotFound("user not found");
            
            var offer = await _dbContext.Offers.FindAsync(offerId);
            if (offer == null)
                return NotFound("offer not found");

            await _dbContext.Favorites.AddAsync(new Favorite()
            {
                OfferId = offerId,
                UserId = userId,
                Offer = offer,
                User = user
            });
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [Authorize]
        [Route("delete-favorite")]
        [HttpDelete(Name = "DeleteFavorite")]
        public async Task<ActionResult> DeleteFavorite([FromBody] int offerId)
        {
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (userId == null)
                return Unauthorized();
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
                return NotFound("user not found");
            
            var favorite = await _dbContext.Favorites.FirstOrDefaultAsync(f => f.UserId == userId);
            if (favorite == null)
                return NotFound("favorite not found");
            
            _dbContext.Favorites.Remove(favorite);
            await _dbContext.SaveChangesAsync();
            
            return Ok();
        }

        [Authorize]
        [Route("get-user-favorites")]
        [HttpGet(Name = "GetUserFavorites")]
        public async Task<ActionResult> GetUserFavorites()
        {
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (userId == null)
                return Unauthorized();
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
                return NotFound("user not found");
            
            var favoriteOfferPreviews = await _dbContext.Favorites
                .Include(f => f.Offer)
                .ThenInclude(o => o.Images)
                .Where(f => f.UserId == userId)
                .Select(f => new OfferPreviewDTO(f.Offer))
                .ToListAsync();
            
            return Ok(favoriteOfferPreviews);
        }
        
        [Authorize]
        [Route("is-favorite")]
        [HttpPost(Name = "IsFavorite")]
        public async Task<ActionResult> IsFavorite([FromBody] int offerId)
        {
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (userId == null)
                return Unauthorized();
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
                return NotFound("user not found");
           
            // var offer = await _dbContext.Offers.FindAsync(offerId);
            // if (offer == null)
            //     return NotFound("offer not found");
            
            var isFavorite = await _dbContext.Favorites
                .FirstOrDefaultAsync(f => f.OfferId == offerId  && f.UserId == userId);
             
            return Ok(isFavorite != null);
        }
    }
}