using Domain.Entites;
using Services.DTO_s.Request;
using Services.DTO_s.Response;
using Services.Helpers;
using Shared.PaginationDefiners;

namespace Services.Interfaces.IServiceEntities
{
    public interface IUserServices
    {
        Task<StandardResponse<UserResponse>> UpdateUser(string userEmail, UserUpdateRequestDto updateRequest);
        Task<StandardResponse<PagedList<UserResponse>>> GetAllUsers(PaginationParams pagination);
        Task<StandardResponse<UserResponse>> GetUserById(string id);        
        Task<StandardResponse<string>> DeleteUser(string userNameOfUserToDelete, string userEmailOfDeletor);
        Task<StandardResponse<IEnumerable<UserResponse>>> GetUsersByRoles(string[] roles);
        Task<StandardResponse<UserResponse>> GetUserWithUserName(string userName);
        Task<StandardResponse<UserResponse>> GetUserWithEmail(string email);
        Task<StandardResponse<List<string>>> GetUserRolesByUserName(string userName);
        Task<StandardResponse<List<string>>> GetUserRolesByEmail(string userEmail);
    }
}
