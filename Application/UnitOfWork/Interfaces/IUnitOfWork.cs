using Application.Interfaces.IEntityRepo.IEntityCommandHandler;
using Application.Interfaces.IEntityRepo.IEntityQueryHandler;

namespace Application.UnitOfWork.Interfaces
{
    public interface IUnitOfWork
    {
        ITransactionCommandRepo TransactionCommand { get; }
        ITransactionQueryRepo TransactionQuery { get; }
        IUserWalletCommandRepo UserWalletCommand { get; }
        IUserWalletQueryRepo UserWalletQuery { get; }
        Task SaveChangesAsync();
    }
}
