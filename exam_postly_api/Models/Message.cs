namespace exam_postly_api.Models;

public class Message
{
    public Guid Id { get; set; }
    public Guid ChatId { get; set; }
    public int SenderId { get; set; }
    required
        public string Text { get; set; }
    public DateTime SendingTimeUTC { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;
    
    public virtual Chat Chat { get; set; } =  null!;
    public virtual User Sender { get; set; } = null!;
}