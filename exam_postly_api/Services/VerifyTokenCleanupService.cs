using Microsoft.EntityFrameworkCore;

namespace exam_postly_api.Services;

public class VerifyTokenCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _factory;
    private readonly TimeSpan _executionInterval = TimeSpan.FromHours(1);
    // private readonly TimeSpan _executionInterval = TimeSpan.FromMinutes(5);//small value for a test

    public VerifyTokenCleanupService(IServiceScopeFactory factory)
    {
        _factory = factory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var scope = _factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        while (!stoppingToken.IsCancellationRequested)
        {
            Console.WriteLine("running a verify tokens cleanup");
            var expiredTokens = await dbContext.VerifyTokens
                .Include(verifiedToken => verifiedToken.User)
                .Where(verifyToken => verifyToken.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();

            foreach (var token in expiredTokens)
            {
                var user = token.User;

                if (!user.IsVerified)
                {
                    dbContext.Users.Remove(user);
                }
            }
            
            dbContext.VerifyTokens.RemoveRange(expiredTokens);

            await dbContext.SaveChangesAsync();

            await Task.Delay(_executionInterval);
        }
    }
}