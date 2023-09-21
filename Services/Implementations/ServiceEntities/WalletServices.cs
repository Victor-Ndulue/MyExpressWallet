using Application.Interfaces.IEntityRepo.IEntityCommandHandler;
using Application.Interfaces.IEntityRepo.IEntityQueryHandler;
using Application.UnitOfWork.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entites;
using Microsoft.EntityFrameworkCore;
using Services.DTO_s.Request;
using Services.DTO_s.Response;
using Services.Helpers;
using Services.Helpers.MailServices;
using Services.Interfaces.IServiceCommon;
using Services.Interfaces.IServiceEntities;
using Services.LoggerService.Exceptions.EntityExceptions.EntityNotFoundException;
using Services.LoggerService.Interface;
using Shared.PaginationDefiners;

namespace Services.Implementations.ServiceEntities
{
    public class WalletServices : IWalletServices
    {
        private readonly IUnitOfWork _unitOfWork;
        public readonly IMapper _mapper;
        private readonly ILoggerManager _logger;

        private readonly IUserWalletCommandRepo _userWalletCommand;
        private readonly IUserWalletQueryRepo _userWalletQuery;
        private readonly IServiceManager _services;
        private readonly IEmailService _emailService;
        public WalletServices(IUnitOfWork unitOfWork, IMapper mapper, ILoggerManager logger, IServiceManager serviceManager, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;

            _userWalletCommand = _unitOfWork.UserWalletCommand;
            _userWalletQuery = _unitOfWork.UserWalletQuery;
            _services = serviceManager;
            _emailService = emailService;
        }


        public async Task<StandardResponse<WalletResponseDto>> CreateWalletAccount(string userId)
        {
            if (userId == null) return StandardResponse<WalletResponseDto>.Failed("userId cannot be null");
            var walletDto = new WalletCreationDto();
            var wallet = _mapper.Map<UserWallet>(walletDto);
            wallet.AppUserId = userId;
            await _userWalletCommand.CreateAsync(wallet);
            await _unitOfWork.SaveChangesAsync();
            var walletResponse = _mapper.Map<WalletResponseDto>(wallet);
            return StandardResponse<WalletResponseDto>.Success("success", walletResponse, 201);

        }

        public async Task UpdateWallet(string walletId, decimal updatedBalance, string? payStackAuth)
        {
            var wallet = await GetUserWalletWithId(walletId);
            wallet.Balance = updatedBalance;
            wallet.PayStackAuth = payStackAuth;
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<StandardResponse<TransactionResponseDto>> SendMoneyAsync(SendMoneyRequestDto requestDetails, string senderEmail)
        {
            // Get the sender user based on the provided email.
            var sender = await _services.UserServices.GetUserWithEmail(senderEmail);

            if (sender.Data == null)
            {
                throw new AppUserNotFoundException("Email", senderEmail);
            }

            // Verify the sender's login credentials.
            var verifyUser = new UserLoginRequestDto(sender.Data.UserName, requestDetails.loginPassword);
            var confirmPassword = await _services.AuthenticationServices.UserLogin(verifyUser);

            if (!confirmPassword.Succeeded)
            {
                throw new UnauthorizedAccessException("Sender credential verification failed. Check the password and try again.");
            }

            // Get sender's wallet.
            var senderWallet = await _userWalletQuery.GetUserWalletsByUserEmail(senderEmail, false)
                .Include(x => x.AppUser)
                .SingleOrDefaultAsync(x => x.Id == requestDetails.senderWalletId);

            // Get recipient's wallet.
            var recipientWallet = await _userWalletQuery.GetByCondition(x => x.Id == requestDetails.recipientWalletId, false)
                .Include(x => x.AppUser)
                .SingleOrDefaultAsync();

            if (senderWallet == null)
            {
                throw new UserWalletNotFoundException("Wallet id", requestDetails.senderWalletId);
            }

            if (recipientWallet == null)
            {
                throw new UserWalletNotFoundException("Wallet id", requestDetails.recipientWalletId);
            }

            if (senderWallet.Id == requestDetails.recipientWalletId)
            {
                throw new ArgumentException("Sender wallet cannot be the recipient wallet.");
            }

            // Calculate the amount to withdraw and include charges.
            var charge = requestDetails.amountToSend * (decimal)0.04;
            var amountToWithdrawAndCharges = requestDetails.amountToSend +charge;

            if (senderWallet.Balance <= amountToWithdrawAndCharges)
            {
                return StandardResponse<TransactionResponseDto>.Failed("Insufficient funds.");
            }


            // Deduct the amount from the sender's wallet.
            var senderWalletBalance = senderWallet.Balance - amountToWithdrawAndCharges;
            senderWallet.Balance = senderWalletBalance;

            await UpdateWallet(senderWallet.Id, senderWalletBalance, null);


            // Add the amount to the recipient's wallet.
            var recipientWalletBalance = recipientWallet.Balance + requestDetails.amountToSend;
            recipientWallet.Balance = recipientWalletBalance;

            await UpdateWallet(recipientWallet.Id, recipientWalletBalance, null);

            // Define transaction types and status.
            var senderTransactionType = "SentTransfer";
            var recipientTransactionType = "ReceivedTransfer";
            var status = "Successful";
            var senderWalletId = senderWallet.Id;
            var recipientWalletId = recipientWallet.Id;

            // Create sender's transaction.
            var senderTransactionCreationDto = new TransactionCreationRequestDto
            {
                TransactionType = senderTransactionType,
                TransactionRef = string.Empty,
                Status = status,
                RemainingBalance = senderWalletBalance,
                Amount = amountToWithdrawAndCharges
            };

            var senderTransactionResponse = await _services.TransactionServices.CreateTransaction(senderTransactionCreationDto, senderWalletId, recipientWalletId);

            // Create recipient's transaction.
            var recipientTransactionCreationDto = new TransactionCreationRequestDto
            {
                TransactionType = recipientTransactionType,
                TransactionRef = string.Empty,
                Status = status,
                RemainingBalance = recipientWalletBalance,
                Amount = requestDetails.amountToSend
            };

            var sendMsgEmail = senderEmail;
            var receiveEmail = recipientWallet.AppUser.Email;

            var sendSubject = "MyExpressWallet Debit Alert";
            var receiveSubject = "MyExpressWallet Debit Alert";

            var sendBody = $"You sent the sum of N{requestDetails.amountToSend} with added charge of N{charge} to {recipientWalletId}." +
                $" \n Reply to this mail if you did not initiate transfer.";
            var receiveBody = $"You received the sum of {requestDetails.amountToSend} in your MyExpressWallet. Log in to see transaction details";

            await _emailService.SendEmailAsync(sendMsgEmail, sendSubject, sendBody);

            await _services.TransactionServices.CreateTransaction(recipientTransactionCreationDto, senderWalletId, recipientWalletId);

            return StandardResponse<TransactionResponseDto>.Success("Successful", senderTransactionResponse.Data);
        }


        public async Task<StandardResponse<WalletResponseDto>> GetUserWalletById(string walletId)
        {
            var wallet = await _unitOfWork.UserWalletQuery.GetByCondition(x => x.Id == walletId, false).SingleOrDefaultAsync();
            if (wallet == null) { throw new UserWalletNotFoundException("wallet id", walletId); }

            var mapWalletResponse = _mapper.Map<WalletResponseDto>(wallet);
            return StandardResponse<WalletResponseDto>.Success("success", mapWalletResponse);
        }

        public async Task<StandardResponse<WalletResponseDto>> GetWalletById(string walletId)
        {
            var wallet = _unitOfWork.UserWalletQuery.GetByCondition(w => w.Id == walletId, false);

            if (wallet == null) throw new UserWalletNotFoundException("wallet Id", walletId);

            var walletResponse = await wallet.ProjectTo<WalletResponseDto>(_mapper.ConfigurationProvider).SingleOrDefaultAsync();
            return StandardResponse<WalletResponseDto>.Success("success", walletResponse);
        }

        public async Task<StandardResponse<PagedList<WalletResponseDto>>> GetAllWallets(PaginationParams pagination)
        {
            var wallets = _unitOfWork.UserWalletQuery.GetAll(false);
            var mapWallets = wallets.ProjectTo<WalletResponseDto>(_mapper.ConfigurationProvider);
            var pagedList = await PagedList<WalletResponseDto>.CreateAsync(mapWallets, pagination.PageNumber, pagination.PageSize);
            
            return StandardResponse<PagedList<WalletResponseDto>>.Success("sucess", pagedList);
        }

        public async Task<StandardResponse<PagedList<WalletResponseDto>>> GetAllUserWallets(string email, PaginationParams pagination)
        {
            var wallets = _unitOfWork.UserWalletQuery.GetUserWalletsByUserEmail(email, false);
            if (wallets != null)
            {
                var mapWallets = wallets.ProjectTo<WalletResponseDto>(_mapper.ConfigurationProvider);                

                var pageWallet = await PagedList<WalletResponseDto>.CreateAsync(mapWallets, pagination.PageNumber, pagination.PageSize);
                return StandardResponse<PagedList<WalletResponseDto>>.Success("success", pageWallet);
            }
            return StandardResponse<PagedList<WalletResponseDto>>.Failed("User has no wallet");
        }

        public async Task<StandardResponse<IEnumerable<WalletResponseDto>>> GetAllUserWalletsForWork(string email) 
        {
            var wallets = _unitOfWork.UserWalletQuery.GetUserWalletsByUserEmail(email, false);
            if (wallets != null)
            {
                var mapWallets = await wallets.ProjectTo<WalletResponseDto>(_mapper.ConfigurationProvider).ToListAsync();

                return StandardResponse<IEnumerable<WalletResponseDto>>.Success("success", mapWallets);
            }
            return StandardResponse<IEnumerable<WalletResponseDto>>.Failed("User has no wallet");
        }

        public async Task<StandardResponse<string>> DeleteUserWallet(string walletId)
        {
            var wallet = await _unitOfWork.UserWalletQuery.GetByCondition(x => x.Id == walletId, false).SingleOrDefaultAsync();
            if (wallet != null)
            {
                _unitOfWork.UserWalletCommand.Delete(wallet);
                await _unitOfWork.SaveChangesAsync();
                return StandardResponse<string>.Success("success", $"wallet with id {walletId} successfully deleted");
            }
            throw new UserWalletNotFoundException("wallet Id", walletId);
        }

        private async Task<UserWallet> GetUserWalletWithId(string walletId)
        {
            return await _unitOfWork.UserWalletQuery.GetByCondition(w => w.Id == walletId, false).SingleOrDefaultAsync();
        }
    }
}
