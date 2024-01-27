using System.ComponentModel.DataAnnotations;

namespace Services.DTO_s.Request
{
    public record UserCreationRequestDto
    {
        [Required(ErrorMessage = "username cannot be left vacant")]
        [MaxLength(13, ErrorMessage = "userName cannot exceed 13 charaacters")]
        public string UserName { get; init; }
        [Required(ErrorMessage = "Password cannot be left vacant")]
        [DataType(DataType.Password)]
        public string Password { get; init; }
        [Required(ErrorMessage = "Email address cannot be left vacant")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; init; }
        [Required(ErrorMessage = "Phone number cannot be left vacant")]
        [MaxLength(13, ErrorMessage = "Phone number cannot exceed 13 charaacters")]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; init; }
    }
}
