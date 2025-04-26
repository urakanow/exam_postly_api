using exam_postly_api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace exam_postly_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OfferController : ControllerBase
    {
        public Offer[] offers = { new Offer { Title = "product 1", Price = 12.34 }, new Offer { Title = "product 2", Price = 56.78 } };

        [Route("offers")]
        [HttpGet(Name = "GetOffers")]
        public async Task<ActionResult> GetOffers()
        {
            //var users = _dbContext.Users;
            return Ok(offers);
        }
    }
}
