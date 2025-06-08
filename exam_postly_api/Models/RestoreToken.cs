using System.Diagnostics.CodeAnalysis;

namespace exam_postly_api.Models;

public class RestoreToken
{
    public int Id { get; set; }
    
    required 
        public string Token { get; set; }
    
    required
        public int UserId { get; set; }

    required
        public User User { get; set; }
    
    public DateTime ExpiresAt { get; set; } =  DateTime.UtcNow +  TimeSpan.FromHours(1);
}