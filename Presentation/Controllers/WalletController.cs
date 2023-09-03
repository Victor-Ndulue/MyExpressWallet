using Microsoft.AspNetCore.Mvc;
using Services.Interfaces.IServiceEntities;

namespace Presentation.Controllers
{
    public class WalletController : BaseController
    {
        private readonly IWalletServices _walletServices;

        public WalletController(IWalletServices walletServices)
        {
            _walletServices = walletServices;
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateWalletAccount()
        {
            var userMail = User.GetUserEmail();
             await _walletServices.CreateWalletAccount(userMail);
            return Ok();
        }        
    }
}
