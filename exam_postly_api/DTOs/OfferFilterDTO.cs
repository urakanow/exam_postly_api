namespace exam_postly_api.DTOs;

public class OfferFilterDTO
{
    public int? CategoryId  { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}