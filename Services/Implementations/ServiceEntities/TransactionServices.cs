using Application.UnitOfWork.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entites;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Services.DTO_s.Response;
using Services.Helpers;
using Services.Interfaces.IServiceEntities;

namespace Services.Implementations.ServiceEntities
{
    public class TransactionServices : ITransactionServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public TransactionServices(IUnitOfWork unitOfWork, IConfiguration configuration, IMapper mapper, UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _mapper = mapper;
            _userManager = userManager;
        }

        private IWalletFundingService _walletFundingService;
        public IWalletFundingService WalletFundingService => _walletFundingService ??= new WalletFundingService(_unitOfWork, _configuration);

        private IVerifyPaymentService _verifyPaymentService;
        public IVerifyPaymentService VerifyPaymentService => _verifyPaymentService ??= new VerifyPaymentService(_unitOfWork, _configuration);

        public async Task<StandardResponse<string>> DeleteTransaction(string transactionRef, string userEmail)
        {
            var transactionToDelete = await _unitOfWork.TransactionQuery
                .GetTransactionsByReference(transactionRef, false)
                .SingleOrDefaultAsync();

            if (transactionToDelete == null)
            {
                return StandardResponse<string>.Failed("Transaction does not exist");
            }

            var isSenderWallet = _unitOfWork.UserWalletQuery.GetUserWalletsByUserEmail(userEmail, false)
                .Any(w => w.SentTransactions.Any(t => t.TransactionRef == transactionRef));

            var isRecipientWallet = _unitOfWork.UserWalletQuery.GetUserWalletsByUserEmail(userEmail, false)
                .Any(w => w.ReceivedTransactions.Any(t => t.TransactionRef == transactionRef));

            if (isSenderWallet && !transactionToDelete.IsSenderDeleted)
            {
                transactionToDelete.IsSenderDeleted = true;
            }
            else if (isRecipientWallet && !transactionToDelete.IsRecipientDeleted)
            {
                transactionToDelete.IsRecipientDeleted = true;
            }
            else
            {
                return StandardResponse<string>.Failed("Transaction failed to delete. Possible reasons: transaction already deleted by user, or user unauthorized.");
            }

            _unitOfWork.TransactionCommand.Update(transactionToDelete);
            await _unitOfWork.SaveChangesAsync();

            return StandardResponse<string>.Success("success", $"transaction with ref {transactionRef} deleted");
        }


        public async Task<StandardResponse<IEnumerable<TransactionResponseDto>>> GetAllTransactions(string userEmail)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user != null)
            {
                var userWallet = _unitOfWork.UserWalletQuery.GetUserWalletsByUserEmail(userEmail, false);
                var userRole = await _userManager.GetRolesAsync(user);
                var transactions = _unitOfWork.TransactionQuery.GetAll(false);
                if (!userRole.Contains("Admin"))
                {
                    transactions = transactions.Where(w => (w.RecipientUserWallet == userWallet && !w.IsRecipientDeleted)
                    || (w.SenderUserWallet == userWallet && !w.IsSenderDeleted));
                }


                var mapTransactions = await transactions.ProjectTo<TransactionResponseDto>(_mapper.ConfigurationProvider).ToListAsync();

                return StandardResponse<IEnumerable<TransactionResponseDto>>.Success("success", mapTransactions);
            }
            return StandardResponse<IEnumerable<TransactionResponseDto>>.Failed("user does not exist");
        }

        public async Task<StandardResponse<TransactionResponseDto>> GetTransactionByReferenceForUser(string transactionRef, string userEmail)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user != null)
            {
                var userWallet = _unitOfWork.UserWalletQuery.GetUserWalletsByUserEmail(userEmail, false);
                var userRole = await _userManager.GetRolesAsync(user);
                var transaction = _unitOfWork.TransactionQuery.GetTransactionsByReference(transactionRef, false);
                if (transaction != null)
                {
                    if (!userRole.Contains("Admin"))
                    {
                        transaction = transaction.Where(t => (t.SenderUserWallet == userWallet && !t.IsSenderDeleted)
                        || (t.RecipientUserWallet == userWallet && !t.IsRecipientDeleted));
                    }
                    var mappedTransaction = _mapper.Map<TransactionResponseDto>(await transaction.SingleOrDefaultAsync());              
                    
                    return StandardResponse<TransactionResponseDto>.Success("success", mappedTransaction);                  
                    
                }
                else
                {
                    return StandardResponse<TransactionResponseDto>.Failed($"No transaction with reference {transactionRef}");
                }
            }
            else
            {
                return StandardResponse<TransactionResponseDto>.Failed("User does not exist");
            }
        }

    }
}
