using Application.Interfaces.IEntityRepo.IEntityCommandHandler;
using Application.Repositories.CommonRepo;
using Application.Repositories.CommonRepo.GenericRepository;
using Domain.Entites;

namespace Application.Repositories.EntityRepo.EntityCommandHandler
{
    public class PaymentRecordCommandRepo : CommandRepository<PaymentRecord>, IPaymentRecordCommandRepo
    {
        public PaymentRecordCommandRepo(AppDbContext context) : base(context)
        {
        }
    }
}
