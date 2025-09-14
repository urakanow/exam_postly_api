namespace exam_postly_api.DTOs;

public class OrderCreateDTO
{
    // required
    //     public int BuyerId { get; set; }
    required
        public int OfferId { get; set; }
    
    required
        public string DeliveryAddress { get; set; }
    
}