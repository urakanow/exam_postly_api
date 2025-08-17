using exam_postly_api.DTOs;
using exam_postly_api.Models;
using exam_postly_api.Services;
using exam_postly_api.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace exam_postly_api.Controllers;

[Authorize(Policy = IdentityData.AdminPolicyName)]
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly UserService _userService;
    private readonly ApplicationDBContext _dbContext;

    public AdminController(UserService userService, ApplicationDBContext dbContext)
    {
        _userService = userService;
        _dbContext = dbContext;
    }

    [Route("users")]
    [HttpGet]
    public async Task<ActionResult<List<UserPreviewDTO>>> GetUsers()
    {
        var users = _userService.getUsers();
        var userDTOs = users.Select(user => new UserPreviewDTO()
        {
            Id = user.Id,
            Username = user.Username
        }).ToList();
        return Ok(userDTOs);
    }
    
    [Route("offers")]
    [HttpGet]
    public async Task<ActionResult<List<OfferPreviewDTO>>> GetOffers()
    {
        var offers = _dbContext.Offers.Include(o => o.Images).ToList();
        var offerDTOs = offers.Select(offer => new OfferPreviewDTO(offer)).ToList();
        
        return Ok(offerDTOs);
    }

    [Route("delete-offer/{id}")]
    [HttpDelete]
    public async Task<ActionResult> DeleteOffer(int id)
    {
        var offer = await _dbContext.Offers.FindAsync(id);
        if(offer == null)
            return NotFound();
        
        _dbContext.Offers.Remove(offer);
        await _dbContext.SaveChangesAsync();

        return Ok("offer deleted");
    }
    
    [Route("delete-user/{id}")]
    [HttpDelete]
    public async Task<ActionResult> DeleteUser(int id)
    {
        var user = await _dbContext.Users.FindAsync(id);
        if(user == null)
            return NotFound();
        
        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();

        return Ok("user deleted");
    }
}