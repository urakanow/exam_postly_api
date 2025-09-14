namespace exam_postly_api.DTOs;

public class OrderInfoDTO
{
    public string OfferTitle { get; set; }
    public double OfferPrice { get; set; }
    public string DeliveryAddress { get; set; }
    public DateTime PayedAt { get; set; }
}