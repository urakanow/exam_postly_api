using System.Security.Claims;
using exam_postly_api.DTOs;
using exam_postly_api.Models;
using exam_postly_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace exam_postly_api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly ApplicationDBContext _dbContext;
    private readonly UserService _userService;

    public OrderController(ApplicationDBContext dbContext, UserService userService)
    {
        _dbContext = dbContext;
        _userService = userService;
    }

    [HttpPost]
    [Route("create")]
    public async Task<ActionResult> CreateOrder([FromBody] OrderCreateDTO dto)
    {
        var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var user = await _userService.GetUserByIdAsync(userId);
        
        var offer = await _dbContext.Offers.FindAsync(dto.OfferId);
        if (offer == null)
            return NotFound();
        
        var newOrder = new Order
        {
            BuyerId = userId,
            OfferId = dto.OfferId,
            Buyer = user,
            Offer = offer
        };
        
        await _dbContext.Orders.AddAsync(newOrder);
        await _dbContext.SaveChangesAsync();
        
        return Ok(newOrder);
    }
}