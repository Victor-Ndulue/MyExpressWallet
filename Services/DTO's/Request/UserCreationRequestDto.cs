namespace Services.DTO_s.Request
{
    public record UserCreationRequestDto(string UserName, string Password, string Email, string PhoneNumber)
    {
    }
}
