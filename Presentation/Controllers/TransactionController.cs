using Microsoft.AspNetCore.Mvc;
using Services.Interfaces.IServiceEntities;

namespace Presentation.Controllers
{
    public class TransactionController : BaseController
    {
        private readonly ITransactionServices _transactionServices;

        public TransactionController(ITransactionServices transactionServices)
        {
            _transactionServices = transactionServices;
        }

        [HttpPost]
        [Route("verify")]
        public IActionResult VerifyPayment(string reference, string walletId) 
        {
            var userMail = User.GetUsername();
            var response = _transactionServices.VerifyPayment(reference,userMail,walletId);
            return Ok(response);
        }
    }
}
