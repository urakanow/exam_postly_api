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
        public async Task<ActionResult> CreateOffer([FromForm] CreateOfferDTO dto)
        {
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (userId == null)
                return Unauthorized();

            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
                return NotFound("user not found");
            
            var newOffer = new Offer()
            {
                Title = dto.Title,
                State = dto.State,
                Description = dto.Description,
                Category = dto.Category,
                Price = Convert.ToDouble(dto.Price),
                UserId = user.Id,
                Contacter = dto.Contacter,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                User = user,
                Images = new List<Image>() //CHANGE
            };
            await _dbContext.Offers.AddAsync(newOffer);
            await _dbContext.SaveChangesAsync();
            var newOfferId = newOffer.Id;

            Cloudinary cloudinary = new Cloudinary(cloudinaryUrl);
            cloudinary.Api.Secure = true;

            var images = dto.Images.ToList();

            for (int i = 0; i < images.Count; i++)
            {
                var image = images[i];
                string name = Guid.NewGuid().ToString();
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(name, image.OpenReadStream()),
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

                var newImage = new Image()
                {
                    Url = imageUrl,
                    OfferId = newOfferId,
                    Offer = newOffer
                };
                _dbContext.Images.Add(newImage);
                await _dbContext.SaveChangesAsync();

                newOffer.Images.Add(newImage);
                await _dbContext.SaveChangesAsync();
            }

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

            var user = await _dbContext.Users
                .Include(u => u.Offers)
                .ThenInclude(o => o.Images)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return NotFound("user not found");

            var offerPreviews = user.Offers.Select(r => new OfferPreviewDTO(r)).ToList();
            
            // return Ok(user.Offers);
            return Ok(offerPreviews);
        }

        [Authorize]
        [Route("delete-offer")]
        [HttpDelete(Name = "DeleteOffer")]
        public async Task<ActionResult> DeleteOffer([FromBody] int id)
        {
            var offer = await _dbContext.Offers.FindAsync(id);

            if (offer == null)
            {
                return NotFound("offer not found");
            }
            
            _dbContext.Offers.Remove(offer);
            await _dbContext.SaveChangesAsync();
            
            return Ok(new { message = "offer deleted"});
        }

        [Route("offer/{id}")]
        [HttpGet(Name = "GetOffer")]
        public async Task<ActionResult> GetOffer( int id)
        {
            // var offer = await _dbContext.Offers.FindAsync(id);
            var offer = await _dbContext.Offers
                .Include(o => o.Images) // Eager load the User
                .FirstOrDefaultAsync(o => o.Id == id);
            
            if (offer == null)
            {
                return NotFound("offer not found");
            }

            return Ok(offer);
        }

        [Authorize]
        [Route("my-offer/{id}")]
        [HttpGet(Name = "GetMyOffer")]
        public async Task<ActionResult> GetMyOffer(int id)
        {
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            
            var offer = await _dbContext.Offers
                .Include(o => o.Images)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (offer == null) 
            {
                return NotFound("offer not found");
            }

            if (offer.UserId != userId)
            {
                return Forbid();
            }
            
            return Ok(offer);
        }

        [Authorize]
        [Route("edit-offer")]
        [HttpPut(Name = "EditOffer")]
        public async Task<ActionResult> EditOffer([FromForm] EditOfferDTO dto)
        {
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (userId == 0)
                return Unauthorized();

            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found");

            var offer = await _dbContext.Offers.FindAsync(dto.Id);
            if (offer == null)
                return NotFound("Offer not found");

            if (offer.UserId != userId)
                return Forbid();

            // Alternatively, update properties manually:
            offer.Title = dto.Title;
            offer.Price = Convert.ToDouble(dto.Price);
            offer.Description = dto.Description;
            offer.Category = dto.Category;
            offer.Email = dto.Email;
            offer.PhoneNumber = dto.PhoneNumber;
            offer.Address = dto.Address;
            offer.Contacter = dto.Contacter;
            // offer.Images = dto.Images;

            try
            {
                await _dbContext.SaveChangesAsync();
                return Ok(new { message = "offer edited" });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, "An error occurred while updating the offer: " + ex.Message);
            }
        }

        [Route("filtered-offers")]
        [HttpGet(Name = "GetFilteredOffers")]
        public async Task<ActionResult<IEnumerable<Offer>>> GetFilteredOffers([FromQuery] OfferFilterDTO filters)
        {
            var query = _dbContext.Offers.Include(o => o.Images).AsQueryable();
    
            if (filters.CategoryId.HasValue)
            {
                query = query.Where(o => o.Category == filters.CategoryId.Value);
            }
            if (filters.State.HasValue)
            {
                query = query.Where(o => o.State == filters.State.Value);
            }
            if (filters.minPrice.HasValue)
            {
                query = query.Where(o => o.Price >= filters.minPrice.Value);
            }
            if (filters.maxPrice.HasValue)
            {
                query = query.Where(o => o.Price <= filters.maxPrice.Value);
            }

            if (!string.IsNullOrEmpty(filters.title))
            {
                query = query.Where(o => o.Title.ToLower().Contains(filters.title.ToLower()));
            }
    
            var results = await query
                .Skip((filters.Page - 1) * filters.PageSize)
                .Take(filters.PageSize)
                .ToListAsync();

            var offerPreviews = results.Select(r => new OfferPreviewDTO(r)).ToList();
            
            return Ok(offerPreviews);
        }
    }
}