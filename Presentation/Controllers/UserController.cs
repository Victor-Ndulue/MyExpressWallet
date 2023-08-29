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
    }
}
