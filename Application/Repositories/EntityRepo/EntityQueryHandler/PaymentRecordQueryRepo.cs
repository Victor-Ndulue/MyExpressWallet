using Application.Interfaces.IEntityRepo.IEntityQueryHandler;
using Application.Repositories.CommonRepo;
using Application.Repositories.CommonRepo.GenericRepository;
using Domain.Entites;

namespace Application.Repositories.EntityRepo.EntityQueryHandler
{
    public class PaymentRecordQueryRepo : QueryRepository<PaymentRecord>, IPaymentRecordQueryRepo
    {
        public PaymentRecordQueryRepo(AppDbContext context) : base(context)
        {
        }

        public IQueryable<PaymentRecord> GetByTransferCode(string transaferCode) 
        {
            return GetByCondition(x => x.transfer_code == transaferCode, false);
        }

        public IQueryable<PaymentRecord> GetAllUnfinalizedPayments()
        {
            return GetByCondition(p => p.IsFinalized==false, false);
        }

        public IQueryable<PaymentRecord> GetAllFianlizedPayments()
        {
            return GetByCondition(p=>p.IsFinalized==true, false);
        }

        public IQueryable<PaymentRecord> GetAllPaymentRecords()
        {
            return GetAll(false);
        }
    }
}
