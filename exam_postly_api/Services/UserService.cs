using exam_postly_api.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace exam_postly_api.Services;

public class UserService
{
    private readonly ApplicationDBContext _dbContext;

    public UserService(ApplicationDBContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<User> GetUserByEmailAsync(string email)
    {
        var user =  await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        return user;
    }

    public async Task UpdateUserAsync(User updatedUser)
    {
        _dbContext.Users.Update(updatedUser);
        await _dbContext.SaveChangesAsync();
    }

    public async Task CreateUserAsync(User user)
    {
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        var  user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        return user;
    }

    public List<User> getUsers()
    {
        return _dbContext.Users.ToList();
    }
}