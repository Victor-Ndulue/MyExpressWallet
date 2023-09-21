using System.ComponentModel.DataAnnotations;

namespace Services.DTO_s.Request
{
    public class UserCreationRequestDto
    {
        [Required(ErrorMessage = "username cannot be left vacant")]
        [MaxLength(13, ErrorMessage = "userName cannot exceed 13 charaacters")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Password cannot be left vacant")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required(ErrorMessage = "Email address cannot be left vacant")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required(ErrorMessage = "Phone number must be numbers and cannot be left vacant")]
        [MaxLength(13, ErrorMessage = "Phone number cannot exceed 13 charaacters")]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }
    }
}
