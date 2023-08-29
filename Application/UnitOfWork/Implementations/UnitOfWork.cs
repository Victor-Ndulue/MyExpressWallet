using Application.Interfaces.IEntityRepo.IEntityCommandHandler;
using Application.Interfaces.IEntityRepo.IEntityQueryHandler;
using Application.Repositories.CommonRepo;
using Application.Repositories.EntityRepo.EntityCommandHandler;
using Application.Repositories.EntityRepo.EntityQueryHandler;
using Application.UnitOfWork.Interfaces;

namespace Application.UnitOfWork.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _appDbContext;
        private  ITransactionQueryRepo _transactionQuery;
        private ITransactionCommandRepo _transactionCommand;
        private  IUserWalletQueryRepo _userWalletQuery;
        private IUserWalletCommandRepo _userWalletCommand;
        public UnitOfWork(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public ITransactionCommandRepo TransactionCommand => _transactionCommand ??= new TransactionCommandRepo(_appDbContext);

        public ITransactionQueryRepo TransactionQuery => _transactionQuery ??= new TransactionQueryRepo(_appDbContext);

        public IUserWalletCommandRepo UserWalletCommand => _userWalletCommand ??= new UserWalletCommandRepo(_appDbContext);

        public IUserWalletQueryRepo UserWalletQuery => _userWalletQuery ??= new UserWalletQueryRepo(_appDbContext);

        public async Task SaveChangesAsync()
        {
            await _appDbContext.SaveChangesAsync();
        }
    }
}
