using exam_postly_api.Enums;

namespace exam_postly_api.Models;

public class Order
{
    public Guid Id { get; set; }
    
    required
        public int BuyerId { get; set; }
    
    required
        public int OfferId { get; set; }
    
    required
        public string DeliveryAddress { get; set; }
        
    public OrderStatus Status { get; set; } = OrderStatus.Unpaid;
    public DateTime? PayedAt { get; set; } = null;
    
    public virtual User Buyer { get; set; }
    public virtual Offer Offer { get; set; }
}