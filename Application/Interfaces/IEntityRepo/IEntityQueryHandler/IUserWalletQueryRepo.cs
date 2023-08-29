using Application.Interfaces.Common.IGenericRepository;
using Domain.Entites;

namespace Application.Interfaces.IEntityRepo.IEntityQueryHandler
{
    public interface IUserWalletQueryRepo : IQueryRepository<UserWallet>
    {
        IQueryable<UserWallet> GetUserWalletsByUserPhoneNumber(string phoneNumber, bool trackChanges);
        IQueryable<UserWallet> GetUserWalletsByUserEmail(string email, bool trackChanges);
        IQueryable<UserWallet> GetUserWalletsByUserId(string Id, bool trackChanges);
    }
}
