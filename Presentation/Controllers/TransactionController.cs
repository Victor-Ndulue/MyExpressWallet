using Microsoft.AspNetCore.Mvc;
using Services.DTO_s.Request;
using Services.Helpers.MailServices;
using Services.Interfaces.IServiceCommon;
using Services.Interfaces.IServiceEntities;
using Shared.PaginationDefiners;

namespace Presentation.Controllers
{
    public class TransactionController : BaseController
    {
        private readonly ITransactionServices _transactionServices;
        private readonly IEmailService _emailService;

        public TransactionController(IServiceManager services, IEmailService emailService)
        {
            _transactionServices = services.TransactionServices;
            _emailService = emailService;
        }


        /// <summary>
        /// Get all transactions
        /// </summary>
        /// <returns> a list of users account</returns>
        [HttpGet("all-transactions")]
        public async Task<IActionResult> GetAllTransactions([FromQuery] PaginationParams pagination)
        {
            var result = await _transactionServices.GetAllTransactions(pagination);
            return Ok(result);
        }

        /// <summary>
        /// Gets list of logged in user transactions
        /// </summary>
        /// <returns>list of transaction belonging to a particular user</returns>
        [HttpGet("all-user-transactions")]
        public async Task<IActionResult> GetAllUserTransactions([FromQuery] PaginationParams pagination)
        {
            var usermail = User.GetUserEmail();
            var response = await _transactionServices.GetAllUserTransactions(usermail, pagination);
            return Ok(response);
        }

        /// <summary>
        /// method to get transaction based on transaction reference
        /// </summary>
        /// <param name="reference">transaction refernce</param>
        /// <param name="walletId">id of wallet</param>
        /// <returns></returns>
        [HttpGet("get-by-reference")]
        public async Task<IActionResult> GetTransactionByRefernceForUser([FromQuery] string reference, string walletId)
        {
            var userMail = User.GetUserEmail();
            var result = await _transactionServices.GetTransactionByReferenceForUser(reference, userMail, walletId);
            return Ok(result);
        }

        /// <summary>
        /// method to get transaction by date
        /// </summary>
        /// <param name="dateOfTransaction">date transaction took place</param>
        /// <returns></returns>
        [HttpGet("get-by-date")]
        public async Task<IActionResult> GetTransactionsByDate([FromQuery] DateOnly dateOfTransaction)
        {
            var userMail = User.GetUserEmail();
            var result = await _transactionServices.GetTransactionByDate(dateOfTransaction, userMail);
            return Ok(result);
        }

        /// <summary>
        /// removes transaction based on reference
        /// </summary>
        /// <param name="transactionRef">transaction reference</param>
        /// <returns>a transaction detail</returns>
        [HttpDelete("delete/{transactionRef}")]
        public async Task<IActionResult> DeleteTransaction ([FromForm]string transactionRef)
        {
            var userMail = User.GetUserEmail();
            var result = await _transactionServices.DeleteTransaction(transactionRef, userMail);
            return Ok(result);
        }
    }
}
