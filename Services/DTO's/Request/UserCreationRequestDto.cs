namespace Services.DTO_s.Request
{
    public class UserCreationRequestDto
    {
        public string UserName { get; set; }    
        public string Password { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}
