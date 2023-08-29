using Application.Interfaces.IEntityRepo.IEntityQueryHandler;
using Application.Repositories.CommonRepo;
using Application.Repositories.CommonRepo.GenericRepository;
using Domain.Entites;

namespace Application.Repositories.EntityRepo.EntityQueryHandler
{
    public class UserWalletQueryRepo :  QueryRepository<UserWallet>, IUserWalletQueryRepo
    {
        public UserWalletQueryRepo(AppDbContext context) : base(context)
        {
        }

        public IQueryable<UserWallet> GetWalletByWalletId(string Id, bool trackChanges)
        {
            return GetByCondition(w => w.Id == Id, trackChanges);
        }

        public IQueryable<UserWallet> GetUserWalletsByDateCreated(DateTime dateCreated, bool trackChanges) 
        {
            return GetByCondition(t => t.CreatedOn.Date == dateCreated.Date, trackChanges);
        }
        public IQueryable<UserWallet> GetUserWalletsByUserPhoneNumber(string phoneNumber, bool trackChanges) 
        {
            return GetByCondition(w => w.AppUser.PhoneNumber == phoneNumber, trackChanges);
        }

        public IQueryable<UserWallet> GetUserWalletsByUserEmail(string email, bool trackChanges)
        {
            return GetByCondition(w => w.AppUser.Email == email, trackChanges);
        }

        public IQueryable<UserWallet> GetUserWalletsByUserId(string Id, bool trackChanges)
        {
            return GetByCondition(w => w.AppUser.Id == Id, trackChanges);
        }

        public IQueryable<UserWallet> GetUserWalletsByCurrency(string Currency, bool trackChanges)
        {
            return GetByCondition(w => w.Currency == Currency, trackChanges);
        }
    }
}
