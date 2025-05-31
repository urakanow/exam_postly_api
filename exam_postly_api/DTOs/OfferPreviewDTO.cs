using exam_postly_api.Models;

namespace exam_postly_api.DTOs;

public class OfferPreviewDTO
{
    public int Id { get; set; }
    public string PreviewImageUrl { get; set; }
    public string Title { get; set; }
    public double Price { get; set; }
    
    public OfferPreviewDTO() {}

    public OfferPreviewDTO(Offer offer)
    {
        Id = offer.Id;
        PreviewImageUrl = offer.Images.First().Url;
        Title = offer.Title;
        Price = offer.Price;
    }
}