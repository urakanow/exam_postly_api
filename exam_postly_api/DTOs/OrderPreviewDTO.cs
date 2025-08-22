using exam_postly_api.Enums;

namespace exam_postly_api.DTOs;

public class OrderPreviewDTO
{
    public string OfferTitle { get; set; }
    public Guid OrderId { get; set; }
    public OrderStatus Status { get; set; }
}