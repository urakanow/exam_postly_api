using Microsoft.EntityFrameworkCore;

namespace exam_postly_api.CleanupServices
{
    public class RefreshTokenCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _factory;
        private readonly TimeSpan _executionInterval = TimeSpan.FromMinutes(30);
        //private readonly TimeSpan _executionInterval = TimeSpan.FromSeconds(10);//small value for a test

        public RefreshTokenCleanupService(IServiceScopeFactory factory)
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
                var revokedTokens = await dbContext.RefreshTokens.Where(refreshToken => refreshToken.IsRevoked).ToListAsync();
                var expiredTokens = await dbContext.RefreshTokens.Where(refreshToken => refreshToken.ExpiresAt < DateTime.UtcNow).ToListAsync();

                dbContext.RefreshTokens.RemoveRange(revokedTokens);
                dbContext.RefreshTokens.RemoveRange(expiredTokens);

                await dbContext.SaveChangesAsync();

                await Task.Delay(_executionInterval);
            }
        }
    }
}
