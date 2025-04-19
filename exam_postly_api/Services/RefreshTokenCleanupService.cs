using Microsoft.EntityFrameworkCore;

namespace exam_postly_api.Services
{
    public class RefreshTokenCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory factory;
        private readonly TimeSpan executionInterval = TimeSpan.FromMinutes(30);
        //private readonly TimeSpan executionInterval = TimeSpan.FromSeconds(10);//small value for a test

        public RefreshTokenCleanupService(IServiceScopeFactory factory)
        {
            this.factory = factory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var scope = factory.CreateScope();
            var _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

            while (!stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine("running a cleanup");
                var revokedTokens = await _dbContext.RefreshTokens.Where(refreshToken => refreshToken.IsRevoked).ToListAsync();
                var expiredTokens = await _dbContext.RefreshTokens.Where(refreshToken => refreshToken.ExpiresAt < DateTime.UtcNow).ToListAsync();

                _dbContext.RefreshTokens.RemoveRange(revokedTokens);
                _dbContext.RefreshTokens.RemoveRange(expiredTokens);

                await _dbContext.SaveChangesAsync();

                await Task.Delay(executionInterval);
            }
        }
    }
}
