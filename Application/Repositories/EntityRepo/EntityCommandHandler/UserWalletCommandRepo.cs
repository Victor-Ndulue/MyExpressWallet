using Application.Interfaces.IEntityRepo.IEntityCommandHandler;
using Application.Repositories.CommonRepo;
using Application.Repositories.CommonRepo.GenericRepository;
using Domain.Entites;

namespace Application.Repositories.EntityRepo.EntityCommandHandler
{
    public class UserWalletCommandRepo : CommandRepository<UserWallet>, IUserWalletCommandRepo
    {
        public UserWalletCommandRepo(AppDbContext context) : base(context)
        {
        }
    }
}
