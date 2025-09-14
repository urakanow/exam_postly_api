using System.Security.Claims;
using exam_postly_api.DTOs;
using exam_postly_api.Models;
using exam_postly_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

    [Route("create")]
    [HttpPost]
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
            Offer = offer,
            DeliveryAddress = dto.DeliveryAddress
        };
        
        await _dbContext.Orders.AddAsync(newOrder);
        await _dbContext.SaveChangesAsync();
        
        return Ok(newOrder);
    }

    [Route("status/{id}")]
    [HttpGet]
    public async Task<ActionResult> GetStatus(Guid id)
    {
        var order = await _dbContext.Orders.FindAsync(id);
        if (order == null)
            return NotFound();
        
        var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        if(order.BuyerId != userId)
            return Unauthorized();
        
        return Ok(order.Status);
    }

    [Route("my-orders")]
    [HttpGet]
    public async Task<ActionResult> GetMyOrders()
    {
        var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var user = await _dbContext.Users
            .Include(u => u.Orders)
            .ThenInclude(o => o.Offer)
            .FirstOrDefaultAsync(u => u.Id == userId);

        var orders = user.Orders;
        // var orderDTOs = orders.Select(order => new OrderInfoDTO()
        // {
        //     OfferTitle = order.Offer.Title,
        //     OfferPrice = order.Offer.Price,
        //     DeliveryAddress = order.DeliveryAddress,
        // }).ToList();
        var orderDTOs = orders.Select(order => new OrderPreviewDTO()
        {
            OfferTitle = order.Offer.Title,
            OrderId = order.Id,
            Status = order.Status
        }).ToList();
        return Ok(orderDTOs);
    }

    [Route("order/{id}")]
    [HttpGet]
    public async Task<ActionResult> GetOrder(Guid id)
    {
        var order = await _dbContext.Orders
            .Include(o => o.Offer)
            .FirstOrDefaultAsync(o => o.Id == id);

        var orderDTO = new OrderInfoDTO()
        {
            OfferTitle = order.Offer.Title,
            OfferPrice = order.Offer.Price,
            DeliveryAddress = order.DeliveryAddress,
            PayedAt = order.PayedAt,
        };
        return Ok(orderDTO);
    }
}