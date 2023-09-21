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
using Services.Interfaces.IServiceCommon;
using Services.Interfaces.IServiceEntities;
using Services.LoggerService.Exceptions.EntityExceptions.EntityNotFoundException;
using Services.LoggerService.Interface;
using Shared.PaginationDefiners;

namespace Services.Implementations.ServiceEntities
{
    public class TransactionServices : ITransactionServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITransactionCommandRepo _transactionCommand;
        private readonly ITransactionQueryRepo _transactionQuery;
        private readonly IServiceManager _services;

        private readonly IMapper _mapper;
        private readonly ILoggerManager _logger;

        public TransactionServices(IUnitOfWork unitOfWork, IMapper mapper, ILoggerManager logger, IServiceManager serviceManager)
        {
            _unitOfWork = unitOfWork;
            _transactionCommand = _unitOfWork.TransactionCommand;
            _transactionQuery = _unitOfWork.TransactionQuery;
            _services = serviceManager;

            _mapper = mapper;
            _logger = logger;
        }


        public async Task<StandardResponse<TransactionResponseDto>> CreateTransaction(TransactionCreationRequestDto transactionCreationDto, string senderUserWalletId, string recipientUserWalletId)
        {
            var transaction = _mapper.Map<Transaction>(transactionCreationDto);
            transaction.SenderUserWalletId = senderUserWalletId;
            transaction.RecipientUserWalletId = recipientUserWalletId;
            await _transactionCommand.CreateAsync(transaction);
            await _unitOfWork.SaveChangesAsync();
            var transactionResponse = _mapper.Map<TransactionResponseDto>(transaction);
            return StandardResponse<TransactionResponseDto>.Success("successful", transactionResponse, 201);
        }

        public async Task<StandardResponse<TransactionResponseDto>> UpdateTransaction(string transactionRef, decimal amount, decimal updatedBalance)
        {
            var transaction = await GetTransactionByReferenceAsync(transactionRef);
            var transactionUpdateDto = new TransactionUpdateRequestDto { Amount = amount, RemainingBalance = updatedBalance };
            var mapTransaction = _mapper.Map(transactionUpdateDto, transaction);
            mapTransaction.Status = "successful";
            _unitOfWork.TransactionCommand.Update(mapTransaction);
            await _unitOfWork.SaveChangesAsync();
            var transactionResponse = _mapper.Map<TransactionResponseDto>(transaction);
            return StandardResponse<TransactionResponseDto>.Success("success", transactionResponse);

        }


        public async Task<StandardResponse<PagedList<TransactionResponseDto>>> GetAllTransactions(PaginationParams pagination)
        {
            _logger.LogInfo("started protocol to retrieve all transactions from database");
            var transactions =  _transactionQuery.GetAll(false)
                .Include(x => x.RecipientUserWallet).Include(x => x.SenderUserWallet);

            var mapTransactions = transactions.ProjectTo<TransactionResponseDto>(_mapper.ConfigurationProvider);
            var pagedTransactions = await PagedList<TransactionResponseDto>.CreateAsync(mapTransactions, pagination.PageNumber, pagination.PageSize);

            _logger.LogInfo("successfully returning transactions to user");
            return StandardResponse<PagedList<TransactionResponseDto>>.Success("success", pagedTransactions);
        }

        public async Task<StandardResponse<PagedList<TransactionResponseDto>>> GetAllUserTransactions(string userEmail, PaginationParams pagination)
        {
            _logger.LogInfo("Started protocol to get all undeleted transactions for user");

            var user = await _services.UserServices.GetUserWithEmail(userEmail);
            if (user.Data == null)
            {
                _logger.LogError($"Failed to find user with email {userEmail}");
                return StandardResponse<PagedList<TransactionResponseDto>>.Failed("user does not exist");
            }

            var userWalletsResponse = await _services.WalletServices.GetAllUserWalletsForWork(userEmail);
            var userWallets = userWalletsResponse.Data;


            _logger.LogInfo("Getting user transactions and filtering for undeleted transactions");

            var userTransactions = new List<Transaction>();

            foreach (var userWallet in userWallets)
            {
                _logger.LogInfo($"getting all userwallet undeleted recieved transactions for {userWallet.Id}");

                var recipientTransactions = await _unitOfWork.TransactionQuery.GetByCondition(x =>
                    x.RecipientUserWalletId == userWallet.Id && !x.IsRecipientDeleted,
                    false).ToListAsync();

                _logger.LogInfo($"getting all userwallet undeleted sent transactions for {userWallet.Id}");

                var senderTransactions = await _unitOfWork.TransactionQuery.GetByCondition(x =>
                    x.SenderUserWalletId == userWallet.Id && !x.IsSenderDeleted,
                    false).ToListAsync();

                userTransactions.AddRange(recipientTransactions);
                userTransactions.AddRange(senderTransactions);
            }

            var mapTransactions = _mapper.Map<IEnumerable<TransactionResponseDto>>(userTransactions).AsQueryable();
            
            _logger.LogInfo("Successfully returning transactions to user");
            
            var pagedTransaction = await PagedList<TransactionResponseDto>.CreateAsync(mapTransactions, pagination.PageNumber, pagination.PageSize);
            return StandardResponse<PagedList<TransactionResponseDto>>.Success("success", pagedTransaction);
        }

        public async Task<StandardResponse<TransactionResponseDto>> GetTransactionByReferenceForUser(string transactionRef, string userEmail, string walletId)
        {
           
            if (userEmail!=null)
            {
                var userWallet = await _services.WalletServices.GetWalletById(walletId);
                var userWalletId = userWallet.Data.Id;
                var transaction = _unitOfWork.TransactionQuery.GetTransactionsByReference(transactionRef, false);
                if (transaction != null)
                {

                      transaction = transaction.Where(t => (t.SenderUserWalletId == userWalletId && !t.IsSenderDeleted)
                      || (t.RecipientUserWalletId == userWalletId && !t.IsRecipientDeleted));
                    

                    transaction = transaction.Include(t => t.SenderUserWallet).ThenInclude(sw => sw.AppUser).Include(t => t.RecipientUserWallet).ThenInclude(rw => rw.AppUser);    

                    var mappedTransaction = await transaction.ProjectTo<TransactionResponseDto>(_mapper.ConfigurationProvider).SingleOrDefaultAsync();

                    return StandardResponse<TransactionResponseDto>.Success("success", mappedTransaction);
                }
                else
                {
                    throw new TransactionNotFoundException("transaction reference", transactionRef);
                }
            }
            else
            {
                throw new AppUserNotFoundException("email", userEmail);
            }
        }

        public async Task<StandardResponse<IEnumerable<TransactionResponseDto>>> GetTransactionByDate(DateOnly date, string userEmail)
        {
            _logger.LogInfo("started protocol to Get transaction for user by transaction reference");
            var user = await _services.UserServices.GetUserWithEmail(userEmail);
            if (user.Succeeded)
            {
                var userWallet = _unitOfWork.UserWalletQuery.GetUserWalletsByUserEmail(userEmail, false);
                var userRoleResponse = await _services.UserServices.GetUserRolesByEmail(userEmail);
                var userRole = userRoleResponse.Data;
                var dateTime = date.ToDateTime(TimeOnly.MinValue);
                var transactions = _unitOfWork.TransactionQuery.GetTransactionsByDate(dateTime, false);
                if (transactions != null)
                {
                    if (!userRole.Contains("Admin"))
                    {
                        transactions = transactions.Where(t => (t.SenderUserWallet == userWallet && !t.IsSenderDeleted)
                        || (t.RecipientUserWallet == userWallet && !t.IsRecipientDeleted));
                    }
                    transactions.Include(s => s.SenderUserWalletId).Include(r => r.RecipientUserWallet);
                    var mappedTransaction = await transactions.ProjectTo<TransactionResponseDto>(_mapper.ConfigurationProvider).ToListAsync();

                    return StandardResponse<IEnumerable<TransactionResponseDto>>.Success("success", mappedTransaction);

                }
                throw new TransactionNotFoundException("date", date.ToString());
            }
            throw new AppUserNotFoundException("email", userEmail);
        }

        public async Task<StandardResponse<string>> DeleteTransaction(string transactionRef, string userEmail)
        {
            _logger.LogInfo($"Started protocol to remove  transaction for user with mail {userEmail} ");
            if (userEmail == null) { return StandardResponse<string>.Failed("Failed. UserMail is null"); }

            var userRoleResponse = await _services.UserServices.GetUserRolesByEmail(userEmail);
            var userRole = userRoleResponse.Data;

            var transactionToDelete = _unitOfWork.TransactionQuery
                .GetTransactionsByReference(transactionRef, false);
            if (!transactionToDelete.Any(w => w.RecipientUserWallet.AppUser.Email == userEmail || w.SenderUserWallet.AppUser.Email == userEmail))
            {
                var isSenderWalletAndDeleted = await transactionToDelete.AnyAsync(w => w.SenderUserWallet.AppUser.Email == userEmail && w.IsSenderDeleted);
                var isRecipientWalletAndDeleted = await transactionToDelete.AnyAsync(w => w.RecipientUserWallet.AppUser.Email == userEmail && w.IsRecipientDeleted);

                var transaction = await transactionToDelete.SingleOrDefaultAsync();

                if (!isRecipientWalletAndDeleted || !isSenderWalletAndDeleted)
                {
                    if (!isSenderWalletAndDeleted)
                    {
                        transaction.IsSenderDeleted = true;
                    }
                    else if (isRecipientWalletAndDeleted)
                    {
                        transaction.IsRecipientDeleted = true;
                    }

                    _unitOfWork.TransactionCommand.Update(transaction);
                    await _unitOfWork.SaveChangesAsync();

                    _logger.LogInfo("successfully removed transaction for user");
                    return StandardResponse<string>.Success("success", $"transaction with ref {transactionRef} deleted");
                }
                throw new TransactionNotFoundException("transaction reference", transactionRef);
            }
            else
            {
                throw new TransactionNotFoundException("transaction reference", transactionRef);
            }
        }

        private async Task<Transaction> GetTransactionByReferenceAsync(string reference)
        {
            _logger.LogInfo($"reading database to find wallet transaction with ref {reference}");

            return await _unitOfWork.TransactionQuery.GetTransactionsByReference(reference, false).Include(t => t.SenderUserWallet)
                .Include(x => x.RecipientUserWallet).SingleOrDefaultAsync();
        }
    }
}