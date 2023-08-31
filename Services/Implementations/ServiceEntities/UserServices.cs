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
            var result = await CreateUser(userRequest);
            if (result.Item1.Succeeded)
            {
                var user = result.Item2;
                _logger.LogInfo("Attempting to map user to user response dto");
                var mapUser = _mapper.Map<UserResponse>(user);
                _logger.LogInfo("Successfully created user.\n Attempting to add roles to user");
                string[] roleNames = { "Admin", "User" };
                var addRoleResponse = await AddUserRoleByUserId(user.Id, roleNames);
                if (addRoleResponse.Succeeded)
                {
                    _logger.LogInfo("Assigning token to user");
                    mapUser.token = await GenerateUserToken(user);
                    _logger.LogInfo("Returning success message.");
                    return StandardResponse<UserResponse>.Success("Account Successfully created", mapUser);
                }
                return StandardResponse<UserResponse>.UnExpectedError("Failed to add user to roles",mapUser, StatusCodes.Status501NotImplemented);
            }
            var errorMessage = string.Join(", ", result.Item1.Errors.Select(e => e.Description));
            _logger.LogError(errorMessage + "\n Returning error message");
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
            _logger.LogInfo("Checking if user exists");
            var user = await _userManager.FindByNameAsync(login.UserName);
            _logger.LogInfo("Verifying user password");
            var result = await _userManager.CheckPasswordAsync(user, login.Password);
            if (result) 
            {
                _logger.LogInfo("Creating token");
                var mapUser = _mapper.Map<UserResponse>(user);
                mapUser.token = await GenerateUserToken(user);
                _logger.LogInfo("Returning success message.");
                return StandardResponse<UserResponse>.Success("Account login Successfully", mapUser);
            }
            return StandardResponse<UserResponse>.Failed("Invalid username or password");
        }

        public async Task<StandardResponse<IEnumerable<UserResponse>>> GetAllUsers()
        {
            var users = _userManager.Users.ToList();
            var mapUsers = _mapper.Map<IEnumerable<UserResponse>>(users);
            return StandardResponse<IEnumerable<UserResponse>>.Success("successful", mapUsers);
        }

        public async Task<StandardResponse<UserResponse>> GetUserById(string id)
        {
            var user = await GetUserWithId(id);
            if (user == null) return StandardResponse<UserResponse>.Failed($"User with id: {id} does not exist.", StatusCodes.Status404NotFound);
            var mapUser = _mapper.Map<UserResponse>(user);
            return StandardResponse<UserResponse>.Success("successful", mapUser);
        }

        public async Task<StandardResponse<IEnumerable<UserResponse>>> GetUsersByRoles(string[] roles)
        {
            List<AppUser> appUsers = new List<AppUser>();
            foreach (var role in roles)
            {
                var users = await _userManager.GetUsersInRoleAsync(role);
                appUsers.AddRange(users);
            }
            var mapUsers = _mapper.Map<IEnumerable<UserResponse>>(appUsers);
            return StandardResponse<IEnumerable<UserResponse>>.Success("successful", mapUsers);
        }

        public async Task<StandardResponse<string>> AddUserRoleByUserId(string id, string[] roles)
        {
            var userToAssignRole = await GetUserWithId(id);
            await CheckAndCreateUserRoles(roles);
            await _userManager.AddToRolesAsync(userToAssignRole, roles);
            return StandardResponse<string>.Success("successful", $"{userToAssignRole} successfully added to role {roles}", StatusCodes.Status201Created);
        }

        public async Task<StandardResponse<string>> RemoveUserRole(string userName, string[] roles) 
        {
            var userToRemoveRole = await GetUserWithUserName(userName);
            var result = await _userManager.RemoveFromRolesAsync(userToRemoveRole, roles);
            if(result.Succeeded) { return StandardResponse<string>.Success("Successful", $"{userToRemoveRole.UserName} successfully removed from {roles} role/s."); }
            return StandardResponse<string>.Failed( string.Join(", ", result.Errors.Select(e=>e.Description)));
        }

        private async Task<(IdentityResult, AppUser)> CreateUser(UserCreationRequestDto userCreationRequest)
        {
            userCreationRequest.Email = userCreationRequest.Email.ToLower();
            var user = _mapper.Map<AppUser>(userCreationRequest);
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

        private async Task<AppUser> GetUserWithId(string id)
        {
            return await _userManager.Users.SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<StandardResponse<string>> AddUserRoleByUserName(string userName, string[] roles)
        {
            var userToAssignRole = await GetUserWithUserName(userName);
            await CheckAndCreateUserRoles(roles);
            await _userManager.AddToRolesAsync(userToAssignRole, roles);
            return StandardResponse<string>.Success
                ("successful", $"{userToAssignRole} successfully added to role {roles}", StatusCodes.Status201Created);
        }
    }
}