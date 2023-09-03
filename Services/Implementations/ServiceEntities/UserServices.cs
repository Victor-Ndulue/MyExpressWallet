using AutoMapper;
using Domain.Entites;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Services.DTO_s.Request;
using Services.DTO_s.Response;
using Services.Helpers;
using Services.Interfaces.IServiceCommon;
using Services.Interfaces.IServiceEntities;
using Services.LoggerService.Interface;

namespace Services.Implementations.ServiceEntities
{
    public class UserServices : IUserServices
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        private readonly ITokenService _token;

        public UserServices(UserManager<AppUser> userManager, RoleManager<AppRole> role, IMapper mapper, ITokenService token, ILoggerManager loggerManager)
        {
            _userManager = userManager;
            _roleManager = role;
            _mapper = mapper;
            _token = token;
            _logger = loggerManager;
        }

        public async Task<StandardResponse<UserResponse>> CreateAdminUser(UserCreationRequestDto userRequest)
        {
            _logger.LogInfo($"Attempting to create user account for {userRequest.UserName}");
            var result = await CreateUser(userRequest);
            if (result.Item1.Succeeded)
            {
                var user = result.Item2;
                _logger.LogInfo("Attempting to map user to user response dto");
                var mapUser = _mapper.Map<UserResponse>(user);                
                string[] roleNames = { "Admin", "User" };
                _logger.LogInfo($"Attempting to add {roleNames} roles to user");
                var addRoleResponse = await AddUserRoleByUserId(user.Id, roleNames);
                if (addRoleResponse.Succeeded)
                {
                    _logger.LogInfo("Assigning token to user");
                    mapUser.token = await GenerateUserToken(user);
                    _logger.LogInfo("Returning success message.");
                    return StandardResponse<UserResponse>.Success("Account Successfully created", mapUser);
                }
                var errorMsg = string.Join(" , ", addRoleResponse.Message);
                _logger.LogError($"Failed to add {roleNames} for {userRequest.UserName}. Details: {errorMsg}. \n Returning error message");
                return StandardResponse<UserResponse>.UnExpectedError($"Failed to add user to roles. Error: {errorMsg}",mapUser, StatusCodes.Status501NotImplemented);
            }
            var errorMessage = string.Join(", ", result.Item1.Errors.Select(e => e.Description));
            _logger.LogError($"User account creation for {userRequest.UserName} unsuccessful. Details: {errorMessage}. \n Returning error message");
            return StandardResponse<UserResponse>.Failed("Failed to create user: " + errorMessage);
        }

        public async Task<StandardResponse<UserResponse>> CreateRegularUser(UserCreationRequestDto userCreationRequest)
        {
            var result = await CreateUser(userCreationRequest);
            if (result.Item1.Succeeded)
            {
                var user = result.Item2;
                _logger.LogInfo("Attempting to map user to user response dto");
                var mapUser = _mapper.Map<UserResponse>(user);
                _logger.LogInfo("Successfully created user.\n Attempting to add roles to user");
                string[] roleNames = { "User" };
                var addRoleResponse = await AddUserRoleByUserId(user.Id, roleNames);
                if (addRoleResponse.Succeeded)
                {
                    _logger.LogInfo("Assigning token to user");
                    mapUser.token = await GenerateUserToken(user);
                    _logger.LogInfo("Returning success message.");
                    return StandardResponse<UserResponse>.Success("Account Successfully created", mapUser, 201);
                }
                return StandardResponse<UserResponse>.UnExpectedError("Failed to add user to roles", mapUser, StatusCodes.Status501NotImplemented);
            }
            var errorMessage = string.Join(", ", result.Item1.Errors.Select(e => e.Description));
            _logger.LogError(errorMessage + "\n Returning error message");
            return StandardResponse<UserResponse>.Failed("Failed to create user: " + errorMessage);
        }

        public async Task<StandardResponse<UserResponse>> UserLogin(UserLoginRequestDto login) 
        {
            _logger.LogInfo($"Checking if username {login.UserName} exists");
            var user = await _userManager.FindByNameAsync(login.UserName);
            _logger.LogInfo($"Verifying {login.UserName} password");
            var result = await _userManager.CheckPasswordAsync(user, login.Password);
            if (user != null && result)
            {
                var mapUser = _mapper.Map<UserResponse>(user);
                _logger.LogInfo("Creating token");
                mapUser.token = await GenerateUserToken(user);
                _logger.LogInfo($"{login.UserName} successfully logged in. Returning success message.");
                return StandardResponse<UserResponse>.Success("Account login Successfully", mapUser);                
            }
            _logger.LogWarn($"Unsuccessfully attempt to log in {login.UserName} due to invalid username or wrong password");
            return StandardResponse<UserResponse>.Failed("Invalid username or password");
        }

        public async Task<StandardResponse<UserResponse>> UpdateUser(string userEmail,UserUpdateRequestDto updateRequest) 
        {
            var user = await GetUserWithEmail(userEmail);
            if (user != null)
            {
                var mapUser = _mapper.Map(updateRequest, user);
                var result = await _userManager.UpdateAsync(mapUser);
                if(result.Succeeded) 
                {
                    var mapUserResponse = _mapper.Map<UserResponse>(user);
                    return StandardResponse<UserResponse>.Success("Success", mapUserResponse);
                }
                _logger.LogError($"An error occured tyring to update user {user}");
                return StandardResponse<UserResponse>.UnExpectedError("An unexpected error happened updating user", null);
            }
            _logger.LogWarn($"trying to update a user that does not exist {userEmail}");
            return StandardResponse<UserResponse>.Failed("User does not exist");
        }

        public async Task<StandardResponse<IEnumerable<UserResponse>>> GetAllUsers()
        {
            _logger.LogInfo("Attempting to get list of users from database.");
            var users = await _userManager.Users.ToListAsync();
            var mapUsers = _mapper.Map<IEnumerable<UserResponse>>(users);
            _logger.LogInfo("Returning list of users.");
            return StandardResponse<IEnumerable<UserResponse>>.Success("successful", mapUsers);
        }

        public async Task<StandardResponse<UserResponse>> GetUserById(string id)
        {
            _logger.LogInfo($"Retrieving user with id {id} from database");
            var user = await GetUserWithId(id);
            _logger.LogWarn($"User with id {id} not found in database.");
            if (user == null) return StandardResponse<UserResponse>.Failed($"User with id: {id} does not exist.", StatusCodes.Status404NotFound);
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

        public async Task<StandardResponse<string>> AddUserRoleByUserId(string id, string[] roles)
        {
            _logger.LogInfo($"Getting user with id {id}");
            var userToAssignRole = await GetUserWithId(id);
            if (userToAssignRole == null) {
                _logger.LogWarn($"User with id {id} attempted to be assigned roles {roles} does not exist");
                return StandardResponse<string>.Failed($"User with id {id} does not exist."); }
            _logger.LogInfo($"Checking if user roles {roles} exists.");
            await CheckAndCreateUserRoles(roles);
            _logger.LogInfo($"Assigning roles {roles} to user {userToAssignRole.UserName}");
            await _userManager.AddToRolesAsync(userToAssignRole, roles);
            return StandardResponse<string>.Success("successful", $"{userToAssignRole} successfully added to role {roles}", StatusCodes.Status201Created);
        }

        public async Task<StandardResponse<string>> AddUserRoleByUserName(string userName, string[] roles)
        {
            _logger.LogInfo($"Getting user with username {userName}");
            var userToAssignRole = await GetUserWithUserName(userName);
            if (userToAssignRole == null)
            {
                _logger.LogWarn($"User with username {userName} attempted to be assigned roles {roles} does not exist");
                return StandardResponse<string>.Failed($"User with username {userName} does not exist.");
            }
            _logger.LogInfo($"Checking and Creating user roles {roles}.");
            await CheckAndCreateUserRoles(roles);
            _logger.LogInfo($"Assigning roles {roles} to user {userToAssignRole.UserName}");
            await _userManager.AddToRolesAsync(userToAssignRole, roles);
            return StandardResponse<string>.Success
                ("successful", $"{userToAssignRole} successfully added to role {roles}", StatusCodes.Status201Created);
        }

        public async Task<StandardResponse<string>> RemoveUserRole(string userName, string[] roles) 
        {
            _logger.LogInfo($"Remove user role for {userName}");
            var userToRemoveRole = await GetUserWithUserName(userName);
            if (userToRemoveRole == null) {
                _logger.LogWarn($"User with username {userName} attempted to be removed from roles {roles} does not exist");
                return StandardResponse<string>.Failed($"User with id {userName} does not exist.");
            }
            var result = await _userManager.RemoveFromRolesAsync(userToRemoveRole, roles);
            if(result.Succeeded) {
                _logger.LogInfo($"User with name {userName} success removed from {roles} roles.");
                return StandardResponse<string>.Success("Successful", $"{userToRemoveRole.UserName} successfully removed from role."); }
            var errorMsg = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError($"Failed to remove user with username {userName} from reoles {roles}. Detail: {errorMsg}");
            return StandardResponse<string>.Failed(errorMsg);
        }

        public async Task<StandardResponse<string>> DeleteUser(string userNameOfUserToDelete, string userEmailOfDeletor)
        {
            var userToDelete = await GetUserWithUserName(userNameOfUserToDelete);
            var userDeletor = await GetUserWithEmail(userEmailOfDeletor);
            var deleteResult = await _userManager.DeleteAsync(userToDelete);
            
            if (deleteResult.Succeeded) {
                _logger.LogInfo($"{userToDelete.UserName} deleted from database by {userDeletor.UserName}");
                return StandardResponse<string>.Success("success", $" {userToDelete.UserName} successfully deleted from database"); }
            return StandardResponse<string>.Failed($"Request unsucessful, user does not exist or {deleteResult.Errors.Select(e => e.Description)}");
        }

        private async Task<(IdentityResult, AppUser)> CreateUser(UserCreationRequestDto userCreationRequest)
        {             
            var user = _mapper.Map<AppUser>(userCreationRequest);
            user.Email = userCreationRequest.Email.ToLower();
            var result = await _userManager.CreateAsync(user, userCreationRequest.Password);
            return (result, user);
        }

        private async Task<string> GenerateUserToken(AppUser user)
        {
            return await _token.CreateToken(user);
        }

        private async Task CheckAndCreateUserRoles(string[] roleNames)
        {
            foreach (var roleName in roleNames)
            {
                var roleExists = await _roleManager.RoleExistsAsync(roleName);
                var appRole = new AppRole();
                appRole.Name = roleName;
                if (!roleExists) await _roleManager.CreateAsync(appRole);           
            }
        }

        private async Task<AppUser> GetUserWithUserName(string userName)
        {
            return await _userManager.FindByNameAsync(userName);
        }

        private async Task<AppUser> GetUserWithEmail(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        private async Task<AppUser> GetUserWithId(string id)
        {
            return await _userManager.Users.SingleOrDefaultAsync(x => x.Id == id);
        }        
    }
}