using exam_postly_api.Models;

namespace exam_postly_api.DTOs;

public class PersonalDataDTO
{
    required
        public string Username { get; set; }

    required
        public string Email { get; set; }

    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string City { get; set; } = "";
    public string PostCode { get; set; } = "";
    public string Address { get; set; } = "";
    public string ApartmentNumber { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    
    public PersonalDataDTO(){}
    
    public PersonalDataDTO(User user)
    {
        FirstName = user.FirstName;
        LastName = user.LastName;
        City = user.City;
        PostCode = user.PostCode;
        Address = user.Address;
        ApartmentNumber = user.ApartmentNumber;
        PhoneNumber = user.PhoneNumber;
    }
}