using AutoMapper;
using Domain.Entites;
using Microsoft.AspNetCore.Identity;
using Services.DTO_s.Request;
using Services.Interfaces.IServiceCommon;
using Services.Interfaces.IServiceEntities;

namespace Services.Implementations.ServiceEntities
{
    public class UserServices : IUserServices
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly ITokenService _token;

        public UserServices(UserManager<AppUser> userManager,RoleManager<AppRole> role, IMapper mapper, ITokenService token)
        {
            _userManager = userManager;
            _roleManager = role;
            _mapper = mapper;
            _token = token;
        }

        public async Task<string> CreateAdminUser(UserCreationRequestDto userRequest)
        {
            var user = _mapper.Map<AppUser>(userRequest);
            user.Email = userRequest.Email.ToLower();
            var result = await _userManager.CreateAsync(user,userRequest.Password);
            string[] roleNames = { "Admin", "User" };
            await CheckAndCreateUserRoles(roleNames);
            result = await _userManager.AddToRolesAsync(user, roleNames);
            if (result.Succeeded) { var token = await _token.CreateToken(user); return token; }
            return string.Join(", ", result.Errors.Select(e => e.Description));
        }

        public async Task<string> UserLogin(UserLoginRequestDto login) 
        {
            var user = await _userManager.FindByNameAsync(login.UserName);
            var result = await _userManager.CheckPasswordAsync(user, login.Password);
            if (result) { return await _token.CreateToken(user);}
            return "Invalid login details";
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
    }
}
