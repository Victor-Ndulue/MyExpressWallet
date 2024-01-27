using Common.Enums;
using Domain.Entites;
using Microsoft.AspNetCore.Identity;
using Services.DTO_s.Request;
using Services.DTO_s.Response;
using Services.Helpers;

namespace Services.Interfaces.IServiceEntities
{
    public interface IAuthenticationServices
    {
        Task<StandardResponse<UserLoginResponse>> CreateAdminUser(UserCreationRequestDto userRequest);
        Task<StandardResponse<RegularUserCreationResponse>> CreateRegularUser(UserCreationRequestDto userCreationRequest);
        Task<StandardResponse<UserLoginResponse>> UserLogin(UserLoginRequestDto login);
        Task<StandardResponse<string>> AddUserToRoleByUserName(string userName, UserRoles roles);
        Task<StandardResponse<string>> RemoveUserRole(string userName, UserRoles roles);
    }
}
