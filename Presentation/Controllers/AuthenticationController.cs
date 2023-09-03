using Microsoft.AspNetCore.Mvc;
using Services.DTO_s.Request;
using Services.Interfaces.IServiceEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    public class AuthenticationController:BaseController
    {
        private readonly IUserServices _services;

        public AuthenticationController(IUserServices services)
        {
            _services = services;
        }

        [HttpPost]
        [Route("create-user/admin")]
        public async Task<IActionResult> CreateAdminUser(UserCreationRequestDto userRequest)
        {
            var response = await _services.CreateAdminUser(userRequest);
            return Ok(response);
        }

        [HttpPost("create-user/regular")]
        public async Task<IActionResult> CreateRegularUser(UserCreationRequestDto requestDto)
        {
            var response = await _services.CreateRegularUser(requestDto);
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
