using Microsoft.AspNetCore.Mvc;
using Services.DTO_s.Request;
using Services.Interfaces.IServiceEntities;

namespace Presentation.Controllers
{
    public class UserController : BaseController
    {
        private readonly IUserServices _services;

        public UserController(IUserServices services)
        {
            _services = services;
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateAdminUser(UserCreationRequestDto userRequest)
        {
            var response = await _services.CreateAdminUser(userRequest);
            return Ok(response);
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> LoginUser(UserLoginRequestDto loginDetails)
        {
            var response = await _services.UserLogin(loginDetails);
            return Ok(response);
        }

        [HttpPost]
        [Route("add-user-roles/id")]
        public async Task<IActionResult> AddUserRoleByUserId(string id, string[] roles)
        {
            var response = await _services.AddUserRoleByUserId(id, roles);
            return Ok(response);
        }

        [HttpPost]
        [Route("remove-user-roles")]
        public async Task<IActionResult> RemoveUserRoleByUserName(string userName, string[] roles) 
        {
            var response = await _services.RemoveUserRole(userName, roles);
            return Ok(response);
        }

        [HttpPost]
        [Route("add-user-roles/username")]
        public async Task<IActionResult> AddUserRoleByUserName(string userName, string[] roles)
        {
            var response = await _services.AddUserRoleByUserName(userName, roles);
            return Ok(response);
        }

        [HttpGet]
        [Route("by-roles")]
        public async Task<IActionResult> GetUsersByRoles([FromQuery] string[] roles)
        { 
            var response = await _services.GetUsersByRoles(roles);
            return Ok(response);
        }

        [HttpGet]
        [Route("by-id")]
        public async Task<IActionResult> GetUserById([FromQuery]string id)
        {
            var response = await _services.GetUserById(id);
            return Ok(response);
        }

        [HttpGet]
        [Route("allusers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var response = await _services.GetAllUsers();
            return Ok(response);
        }
    }
}
