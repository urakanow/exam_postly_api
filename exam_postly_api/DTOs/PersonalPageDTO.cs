using exam_postly_api.Models;

namespace exam_postly_api.DTOs;

public class PersonalPageDTO
{
    public int Id { get; set; }

    public PersonalDataDTO PersonalData { get; set; }
    
    public ICollection<OfferPreviewDTO> Offers { get; set; } = new List<OfferPreviewDTO>();
    
    public PersonalPageDTO(){}

    public PersonalPageDTO(User user)
    {
        Id = user.Id;
        PersonalData = new PersonalDataDTO(user)
        {
            Username = user.Username,
            Email = user.Email
        };

        foreach (var offer in user.Offers)
        {
            Offers.Add(new OfferPreviewDTO(offer));
        }
    }
}