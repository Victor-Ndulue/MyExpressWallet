using Common.Enums;
using Microsoft.AspNetCore.Mvc;
using Services.DTO_s.Request;
using Services.Interfaces.IServiceCommon;
using Services.Interfaces.IServiceEntities;

namespace Presentation.Controllers
{
    public class AuthenticationController:BaseController
    {
        private readonly IAuthenticationServices _services;

        public AuthenticationController(IServiceManager services)
        {
            _services = services.AuthenticationServices;
        }

        /// <summary>
        /// Creates a user account and wallet
        /// </summary>
        /// <param name="userRequest">details to create user and user wallet account</param>
        /// <returns></returns>
        [HttpPost]
        [Route("create-user/admin")]
        public async Task<IActionResult> CreateAdminUser(UserCreationRequestDto userRequest)
        {
            var response = await _services.CreateAdminUser(userRequest);
            return Ok(response);
        }

        /// <summary>
        /// Creates a user account and wallet
        /// </summary>
        /// <param name="requestDto">details to create user and user wallet account</param>
        /// <returns></returns>
        [HttpPost("create-user/regular")]
        public async Task<IActionResult> CreateRegularUser([FromForm] UserCreationRequestDto requestDto)
        {
            var response = await _services.CreateRegularUser(requestDto);
            return StatusCode(response.StatusCode, response.Data);
        }

        /// <summary>
        /// Authenticates and logs in a user
        /// </summary>
        /// <param name="loginDetails">validation and authentication details needed</param>
        /// <returns>token and login response for user</returns>
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> LoginUser([FromForm] UserLoginRequestDto loginDetails)
        {
            var response = await _services.UserLogin(loginDetails);
            return Ok(response);
        }

        /// <summary>
        /// adds a user to a particular role
        /// </summary>
        /// <param name="userName">username of user to save</param>
        /// <param name="role">role to assign user</param>
        /// <returns>success or failed message</returns>
        [HttpPost]
        [Route("add-user-roles/username")]
        public async Task<IActionResult> AddUserRoleByUserName(string userName, UserRoles role)
        {
            var response = await _services.AddUserToRoleByUserName(userName, role);
            return Ok(response);
        }

        /// <summary>
        /// method to remove user from a role
        /// </summary>
        /// <param name="userName">user name of user to remove from role</param>
        /// <param name="role">role to remove user from</param>
        /// <returns>success or failure msg</returns>
        [HttpPost]
        [Route("remove-user-roles")]
        public async Task<IActionResult> RemoveUserRoleByUserName(string userName, UserRoles role)
        {
            var response = await _services.RemoveUserRole(userName, role);
            return Ok(response);
        }

    }
}
