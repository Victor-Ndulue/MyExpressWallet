using Application.Interfaces.Common.IGenericRepository;
using Domain.Entites;

namespace Application.Interfaces.IEntityRepo.IEntityCommandHandler
{
    public interface IUserWalletCommandRepo : ICommandRepository<UserWallet>
    {
    }
}
