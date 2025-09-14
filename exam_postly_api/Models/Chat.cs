namespace exam_postly_api.Models;

public class Chat
{
    public Guid Id { get; set; }
    public int BuyerId { get; set; }
    public int OfferId { get; set; }
    public virtual Offer Offer { get; set; }
    public virtual User Buyer { get; set; } = null!;
    public virtual ICollection<Message> Messages { get; set; } =  new List<Message>();
}