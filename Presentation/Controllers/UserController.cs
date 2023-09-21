using Microsoft.AspNetCore.Mvc;
using Services.DTO_s.Request;
using Services.Interfaces.IServiceCommon;
using Services.Interfaces.IServiceEntities;
using Shared.PaginationDefiners;

namespace Presentation.Controllers
{
    public class UserController : BaseController
    {
        private readonly IUserServices _services;

        public UserController(IServiceManager services)
        {
            _services = services.UserServices;
        }

        /// <summary>
        /// Updates user details
        /// </summary>
        /// <param name="userUpdateRequestDto"> user details to update with values</param>
        /// <returns> </returns>
        [HttpPut("update")]
        public async Task<IActionResult> UpdateUser([FromForm] UserUpdateRequestDto userUpdateRequestDto)
        {
            var userEmail = User.GetUserEmail();
            var response = await _services.UpdateUser(userEmail, userUpdateRequestDto);
            return Ok(response);
        }

        /// <summary>
        /// Gets list of app users
        /// </summary>
        /// <returns>A list of app users</returns>
        [HttpGet]
        [Route("allusers")]
        public async Task<IActionResult> GetAllUsers([FromQuery] PaginationParams pagination)
        {
            var response = await _services.GetAllUsers(pagination);
            return Ok(response);
        }

        /// <summary>
        /// gets a user by id
        /// </summary>
        /// <param name="id">id of user</param>
        /// <returns>User with id details</returns>
        [HttpGet]
        [Route("by-id")]
        public async Task<IActionResult> GetUserById([FromQuery] string id)
        {
            var response = await _services.GetUserById(id);
            return Ok(response);
        }

        /// <summary>
        /// returns users based on roles
        /// </summary>
        /// <param name="roles">roles to use for sorting</param>
        /// <returns>users falling under details</returns>
        [HttpGet]
        [Route("by-roles")]
        public async Task<IActionResult> GetUsersByRoles([FromQuery] string[] roles)
        {
            var response = await _services.GetUsersByRoles(roles);
            return Ok(response);
        }

        /// <summary>
        /// returns user with username
        /// </summary>
        /// <param name="userName">username of user</param>
        /// <returns>user with username details</returns>
        [HttpGet("by-username")]
        public async Task<IActionResult> GetUserWithUserName([FromQuery]string userName)
        {
            var result = await _services.GetUserWithUserName(userName);
            return Ok(result);
        }

        /// <summary>
        /// returns user role by entered username
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        [HttpGet("roles/by-username")]
        public async Task<IActionResult> GetUserRolesByUserName ([FromQuery] string userName)
        {
            var result = await _services.GetUserRolesByUserName(userName);
            return Ok(result);
        }

        /// <summary>
        /// deletes user account
        /// </summary>
        /// <param name="userNameToDelete">unsername of user to delete</param>
        /// <returns>string notification of delete</returns>
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteUser([FromForm]string userNameToDelete) 
        {
            var userEmailOfDeletor = User.GetUserEmail();
            var response = await _services.DeleteUser(userNameToDelete, userEmailOfDeletor);
            return Ok(response);
        }        
    }
}
