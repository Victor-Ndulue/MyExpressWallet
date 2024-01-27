using AutoMapper;
using Common.Enums;
using Domain.Entites;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Services.DTO_s.Request;
using Services.DTO_s.Response;
using Services.Helpers;
using Services.Helpers.MailServices;
using Services.Interfaces.IServiceCommon;
using Services.Interfaces.IServiceEntities;
using Services.LoggerService.Exceptions.EntityExceptions.EntityNotFoundException;
using Services.LoggerService.Interface;

namespace Services.Implementations.ServiceEntities;

public class AuthenticationServices : IAuthenticationServices
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly IServiceManager _services;
    private readonly IEmailService _emailService;

    private readonly ILoggerManager _logger;
    private readonly IMapper _mapper;
    private readonly ITokenService _token;

    public AuthenticationServices(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, ILoggerManager logger, IMapper mapper, ITokenService token, IServiceManager serviceManager, IEmailService emailService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
        _mapper = mapper;
        _token = token;
        _services = serviceManager;
        _emailService = emailService;
    }

    public async Task<StandardResponse<UserLoginResponse>> CreateAdminUser(UserCreationRequestDto userRequest)
    {
        _logger.LogInfo($"Attempting to create user account for {userRequest.UserName}");
        var userCreationResult = await CreateUser(userRequest);

        if (userCreationResult.Item1.Succeeded)
        {
            var user = userCreationResult.Item2;
            string roleName = UserRoles.Admin.ToString();
            var addRoleResponse = await AddUserToRole(user, roleName);
            _logger.LogInfo("Assigning token to user");
            var token = await GenerateUserToken(user);
            var userLoginResponse = new UserLoginResponse(user.UserName, token);
            if (addRoleResponse.Data.Succeeded)
            {
                _logger.LogInfo("Returning success message.");
                return StandardResponse<UserLoginResponse>.Success("success", userLoginResponse, 201);
            }

            var errorMsg = addRoleResponse.Data;
            _logger.LogError($" Created account but failed to add role {roleName} for {userRequest.UserName}. Details: {errorMsg}. \n Returning error message");
            return StandardResponse<UserLoginResponse>.UnExpectedError($"Acoount created, but failed to add user to role. Error: {errorMsg}", userLoginResponse, 500);
        }

        var errorMessage = string.Join(", ", userCreationResult.Item1.Errors.Select(e => e.Description));
        _logger.LogError($"User account creation for {userRequest.UserName} unsuccessful. " +
            $"Details: {errorMessage}. \n Returning error message");
        return StandardResponse<UserLoginResponse>.Failed("Failed to create user: " + errorMessage);
    }

    public async Task<StandardResponse<RegularUserCreationResponse>> CreateRegularUser(UserCreationRequestDto userCreationRequest)
    {
        _logger.LogInfo("Attempting to create a regular user");
        var userCreationResult = await CreateUser(userCreationRequest);

        if (userCreationResult.Item1.Succeeded)
        {
            var user = userCreationResult.Item2;

            _logger.LogInfo("Assigning token to user");
            string roleName = "User";
            var addRoleResponse = await AddUserToRole(user, roleName);
            var token = await GenerateUserToken(user);
            var userCreationResponse = new RegularUserCreationResponse(user.UserName, token, null);
            if (addRoleResponse.Data.Succeeded)
            {
                _logger.LogInfo($"Attemting to create wallet account for user, {user.UserName}");
                var walletCreationResponse = await _services.WalletServices.CreateWalletAccount(user.Id);
                if (walletCreationResponse.Succeeded)
                {
                    var walletData = walletCreationResponse.Data;
                    userCreationResponse.WalletResponse = walletData;
                    _logger.LogInfo("Returning success message.");

                    var subject = "Account Creation With MyExpressWallet";
                    var message = $"Thank you {userCreationRequest.UserName} for choosing MyExpressWallet.\n Your wallet id is {walletData.Id}.\n Welcome to a world of limitless transactions";
                    await _emailService.SendEmailAsync(user.Email, subject, message);
                    return StandardResponse<RegularUserCreationResponse>.Success("Account Successfully created", userCreationResponse, 201);
                }
                return StandardResponse<RegularUserCreationResponse>.UnExpectedError("Failed to create wallet for user", userCreationResponse);
            }
            var addToRoleErrorMesg = addRoleResponse.Data;
            return StandardResponse<RegularUserCreationResponse>.UnExpectedError($"User Account Created but failed to add user to role: " + addToRoleErrorMesg,
                userCreationResponse, 500);
        }
        var userCreationErrorMsg = string.Join(", ", userCreationResult.Item1.Errors.Select(e => e.Description));
        _logger.LogError("failed to create user. Details ; " + userCreationErrorMsg + "\n Returning error message");
        return StandardResponse<RegularUserCreationResponse>.Failed("Failed to create user: " + userCreationErrorMsg);
    }

    public async Task<StandardResponse<UserLoginResponse>> UserLogin(UserLoginRequestDto login)
    {
        _logger.LogInfo($"Checking if username {login.UserName} exists");
        var user = await GetUserWithUserName(login.UserName);
        _logger.LogInfo($"Verifying {login.UserName} password");
        var result = await _userManager.CheckPasswordAsync(user, login.Password);
        if (user is not null && result)
        {
            _logger.LogInfo("Creating token");
            var token = await GenerateUserToken(user);
            var userLoginResponse = new UserLoginResponse(user.UserName, token);

            _logger.LogInfo($"{login.UserName} successfully logged in. Returning success message.");
            return StandardResponse<UserLoginResponse>.Success("Account login Successful", userLoginResponse);
        }
        _logger.LogWarn($"Unsuccessful attempt to log in {login.UserName} due to invalid username or wrong password");
        return StandardResponse<UserLoginResponse>.Failed("Invalid username or password");
    }

    public async Task<StandardResponse<string>> AddUserToRoleByUserName(string userName, UserRoles roles)
    {
        _logger.LogInfo($"Getting user with username {userName}");
        var userToAssignRole = await GetUserWithUserName(userName);
        var role = roles.ToString();
        if (userToAssignRole is null)
        {
            _logger.LogWarn($"User with username {userName} attempted to be assigned roles {role} does not exist");
            return StandardResponse<string>.Failed("User to assign to role not found");
        }

        var result = await AddUserToRole(userToAssignRole, role);
        if (result.Data.Succeeded)
        {
            _logger.LogInfo($"successfully added {userToAssignRole.UserName} to role {role}. Returning response to user");
            return StandardResponse<string>.Success
                ("successful", $"{userToAssignRole.UserName} successfully added to role {role}", 200);
        }

        var errorMsg = result.Data;
        _logger.LogError($"an error occured adding user to role: {errorMsg}");
        return StandardResponse<string>.Failed("an error occured adding user to role");
    }

    public async Task<StandardResponse<string>> RemoveUserRole(string userName, UserRoles roles)
    {
        _logger.LogInfo($"Remove user role for {userName}");
        var userToRemoveRole = await GetUserWithUserName(userName);
        var role = roles.ToString();
        if (userToRemoveRole is null)
        {
            _logger.LogWarn($"User with username {userName} attempted to be removed from roles {role} does not exist");
            return StandardResponse<string>.Failed("User to remove from role not found");
        }

        var result = await _userManager.RemoveFromRoleAsync(userToRemoveRole, role);
        if (result.Succeeded)
        {
            _logger.LogInfo($"User with name {userName} success removed from {role} role.");
            return StandardResponse<string>.Success("Successful", $"{userToRemoveRole.UserName} successfully removed from role.");
        }
        var errorMsg = string.Join(", ", result.Errors.Select(e => e.Description));
        _logger.LogError($"Failed to remove user with username {userName} from role {role}. Detail: {errorMsg}");
        return StandardResponse<string>.Failed($"Failed to remove user with username {userName} from role {role}." + errorMsg);
    }


    private async Task<StandardResponse<IdentityResult>> AddUserToRole(AppUser userToAssignRole, string role)
    {
        if (userToAssignRole is null)
        {
            _logger.LogWarn($"User {userToAssignRole.UserName} attempted to be assigned role {role} does not exist");
            throw new AppUserNotFoundException("username", userToAssignRole.UserName);
        }

        _logger.LogInfo($"Checking if user roles {role} exists, and creating if it doesn't");
        await CheckAndCreateUserRoles(role);

        _logger.LogInfo($"Assigning role {role} to user {userToAssignRole.UserName}");
        var result = await _userManager.AddToRoleAsync(userToAssignRole, role);

        if (result.Succeeded)
        {
            return StandardResponse<IdentityResult>.Success("successful", result, 201);
        }

        var errorMsg = string.Join(", ", result.Errors.Select(x => x.Description));
        return StandardResponse<IdentityResult>
            .Failed($"failed to assign role to user {errorMsg}", 500);
    }

    private async Task<AppUser> GetUserWithUserName(string userName)
    {
        return await _userManager.FindByNameAsync(userName);
    }

    private async Task CheckAndCreateUserRoles(string roleName)
    {
        var roleExists = await _roleManager.RoleExistsAsync(roleName);
        var appRole = new AppRole();
        appRole.Name = roleName;
        if (!roleExists) await _roleManager.CreateAsync(appRole);
    }

    private async Task<string> GenerateUserToken(AppUser user)
    {
        return await _token.CreateToken(user);
    }

    private async Task<(IdentityResult, AppUser)> CreateUser(UserCreationRequestDto userCreationRequest)
    {
        var user = _mapper.Map<AppUser>(userCreationRequest);
        user.Email = userCreationRequest.Email.ToLower();
        var result = await _userManager.CreateAsync(user, userCreationRequest.Password);
        return (result, user);
    }
}
