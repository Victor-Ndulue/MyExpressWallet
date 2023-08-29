using Application.Interfaces.IEntityRepo.IEntityCommandHandler;
using Application.Repositories.CommonRepo;
using Application.Repositories.CommonRepo.GenericRepository;
using Domain.Entites;

namespace Application.Repositories.EntityRepo.EntityCommandHandler
{
    public class TransactionCommandRepo : CommandRepository<Transaction>, ITransactionCommandRepo
    {
        public TransactionCommandRepo(AppDbContext context) : base(context)
        {
        }
    }
}
