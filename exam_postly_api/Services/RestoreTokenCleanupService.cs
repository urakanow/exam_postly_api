using Microsoft.EntityFrameworkCore;

namespace exam_postly_api.Services;

public class RestoreTokenCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _factory;
    private readonly TimeSpan _executionInterval = TimeSpan.FromHours(1);
    // private readonly TimeSpan _executionInterval = TimeSpan.FromSeconds(10);//small value for a test

    public RestoreTokenCleanupService(IServiceScopeFactory factory)
    {
        _factory = factory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var scope = _factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        while (!stoppingToken.IsCancellationRequested)
        {
            Console.WriteLine("running a cleanup");
            var expiredTokens = await dbContext.RestoreTokens.Where(restoreToken => restoreToken.ExpiresAt < DateTime.UtcNow).ToListAsync();

            dbContext.RestoreTokens.RemoveRange(expiredTokens);

            await dbContext.SaveChangesAsync();

            await Task.Delay(_executionInterval);
        }
    }

    
}