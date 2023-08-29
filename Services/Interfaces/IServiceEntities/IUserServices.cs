using Services.DTO_s.Request;

namespace Services.Interfaces.IServiceEntities
{
    public interface IUserServices
    {
        Task<string> CreateAdminUser(UserCreationRequestDto userRequest);
        Task<string> UserLogin(UserLoginRequestDto login);
    }
}
