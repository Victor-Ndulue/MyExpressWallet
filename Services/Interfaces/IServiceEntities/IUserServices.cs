﻿using Services.DTO_s.Request;
using Services.DTO_s.Response;
using Services.Helpers;

namespace Services.Interfaces.IServiceEntities
{
    public interface IUserServices
    {
        Task<StandardResponse<UserResponse>> CreateAdminUser(UserCreationRequestDto userRequest);
        Task<StandardResponse<UserResponse>> CreateRegularUser(UserCreationRequestDto userCreationRequest);
        Task<StandardResponse<UserResponse>> UserLogin(UserLoginRequestDto login);
        Task<StandardResponse<UserResponse>> UpdateUser(string userEmail, UserUpdateRequestDto updateRequest);
        Task<StandardResponse<string>> AddUserRoleByUserId(string id, string[] roles);
        Task<StandardResponse<string>> AddUserRoleByUserName(string userName, string[] roles);
        Task<StandardResponse<string>> RemoveUserRole(string userName, string[] roles);
        Task<StandardResponse<IEnumerable<UserResponse>>> GetUsersByRoles(string[] roles);
        Task<StandardResponse<UserResponse>> GetUserById(string id);
        Task<StandardResponse<IEnumerable<UserResponse>>> GetAllUsers();
        Task<StandardResponse<string>> DeleteUser(string userNameOfUserToDelete, string userEmailOfDeletor);
    }
}