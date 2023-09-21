using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entites;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Services.DTO_s.Request;
using Services.DTO_s.Response;
using Services.Helpers;
using Services.Interfaces.IServiceEntities;
using Services.LoggerService.Exceptions.EntityExceptions.EntityNotFoundException;
using Services.LoggerService.Interface;
using Shared.PaginationDefiners;

namespace Services.Implementations.ServiceEntities
{
    public class UserServices : IUserServices
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;

        public UserServices(UserManager<AppUser> userManager, IMapper mapper, ILoggerManager loggerManager)
        {
            _userManager = userManager;
            _mapper = mapper;
            _logger = loggerManager;
        }

       
        public async Task<StandardResponse<UserResponse>> UpdateUser(string userEmail, UserUpdateRequestDto updateRequest)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user != null)
            {
                var mapUser = _mapper.Map(updateRequest, user);
                var result = await _userManager.UpdateAsync(mapUser);
                if (result.Succeeded)
                {
                    var mapUserResponse = _mapper.Map<UserResponse>(user);
                    return StandardResponse<UserResponse>.Success("Success", mapUserResponse);
                }
                _logger.LogError($"An error occured tyring to update user {user.UserName}");
                return StandardResponse<UserResponse>.UnExpectedError("An unexpected error happened updating user", null);
            }
            _logger.LogWarn($"trying to update a user that does not exist {userEmail}");
            throw new AppUserNotFoundException("email", userEmail);
        }

        public async Task<StandardResponse<PagedList<UserResponse>>> GetAllUsers(PaginationParams pagination)
        {
            _logger.LogInfo("Attempting to get list of users from database.");
            var users =  _userManager.Users;
            var mapUsers = users.ProjectTo<UserResponse>(_mapper.ConfigurationProvider);
            _logger.LogInfo("Returning list of users.");

            var pagedList = await PagedList<UserResponse>.CreateAsync(mapUsers, pagination.PageNumber, pagination.PageSize);           

            return StandardResponse<PagedList<UserResponse>>.Success("successful", pagedList);
        }

        public async Task<StandardResponse<UserResponse>> GetUserById(string id)
        {
            _logger.LogInfo($"Retrieving user with id {id} from database");
            var user = await _userManager.FindByIdAsync(id);
            
            if (user == null) {
                _logger.LogWarn($"User with id {id} not found in database.");
                throw new AppUserNotFoundException("id", id); }

            var mapUser = _mapper.Map<UserResponse>(user);
            _logger.LogInfo($"returning user data for user with id {id}");
            return StandardResponse<UserResponse>.Success("successful", mapUser);
        }

        public async Task<StandardResponse<IEnumerable<UserResponse>>> GetUsersByRoles(string[] roles)
        {
            _logger.LogInfo("Creating a list of AppUser");
            List<AppUser> appUsers = new List<AppUser>();
            foreach (var role in roles)
            {
                _logger.LogInfo($"Getting users with {role} role.");
                var users = await _userManager.GetUsersInRoleAsync(role);
                appUsers.AddRange(users);
            }
            var mapUsers = _mapper.Map<IEnumerable<UserResponse>>(appUsers);
            _logger.LogInfo("Returning users list");
            return StandardResponse<IEnumerable<UserResponse>>.Success("successful", mapUsers);
        }

        public async Task<StandardResponse<string>> DeleteUser(string userNameOfUserToDelete, string userEmailOfDeletor)
        {
            var userToDelete = await _userManager.FindByNameAsync(userNameOfUserToDelete);
            var userDeletor = await GetUserWithEmail(userEmailOfDeletor);
            var deleteResult = await _userManager.DeleteAsync(userToDelete);

            if (deleteResult.Succeeded)
            {
                _logger.LogInfo($"{userToDelete.UserName} deleted from database by {userDeletor.Data.UserName}");
                return StandardResponse<string>.Success("success", $" {userToDelete.UserName} successfully deleted from database");
            }
            return StandardResponse<string>.Failed($"Request unsucessful, user does not exist or {deleteResult.Errors.Select(e => e.Description)}");
        }               
        
        public async Task<StandardResponse<UserResponse>> GetUserWithUserName(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user != null)
            {
                var mapUser = _mapper.Map<UserResponse>(user);
                return StandardResponse<UserResponse>.Success("success", mapUser);
            }
            throw new AppUserNotFoundException("username", userName);
        }

        public async Task<StandardResponse<UserResponse>> GetUserWithEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var mapUser = _mapper.Map<UserResponse>(user);
                return StandardResponse<UserResponse>.Success("success", mapUser);
            }
            throw new AppUserNotFoundException("email", email);
        }        

        public async Task<StandardResponse<List<string>>> GetUserRolesByUserName(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user != null)
            {
                var roles = new List<string>();
                var userRoles = await _userManager.GetRolesAsync(user);
                foreach ( var role in userRoles) { roles.Add(role); }
                return StandardResponse<List<string>>.Success("success", roles);
            }
            throw new AppUserNotFoundException("username", userName);
        }

        public async Task<StandardResponse<List<string>>> GetUserRolesByEmail(string userEmail)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user != null)
            {
                var roles = new List<string>();
                var userRoles = await _userManager.GetRolesAsync(user);
                foreach (var role in userRoles) { roles.Add(role); }
                return StandardResponse<List<string>>.Success("success", roles);
            }
            throw new AppUserNotFoundException("email", userEmail);
        }
    }
}