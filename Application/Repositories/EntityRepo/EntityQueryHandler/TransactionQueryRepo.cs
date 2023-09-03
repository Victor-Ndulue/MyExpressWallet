using Application.Interfaces.IEntityRepo.IEntityQueryHandler;
using Application.Repositories.CommonRepo;
using Application.Repositories.CommonRepo.GenericRepository;
using Domain.Entites;
using Microsoft.EntityFrameworkCore;

namespace Application.Repositories.EntityRepo.EntityQueryHandler
{
    public class TransactionQueryRepo : QueryRepository<Transaction>, ITransactionQueryRepo
    {
        public TransactionQueryRepo(AppDbContext context) : base(context)
        {
        }

        public IQueryable<Transaction> GetTransactionsByTransactionId(string Id, bool trackChanges) 
        {
            return GetByCondition(t => t.Id == Id, trackChanges);
        }
        public IQueryable<Transaction> GetTransactionsByDate(DateTime date, bool trackChanges) 
        {
            return GetByCondition(t => t.CreatedOn.Date == date.Date, trackChanges);
        }
        public IQueryable<Transaction> GetTransactionsByReference(string reference, bool trackChanges)
        {
            return GetByCondition(t => t.TransactionRef == reference, trackChanges);
        }
        public IQueryable<Transaction> GetTransactionsBySenderId(string senderId, bool trackChanges) 
        {
            return GetByCondition(t => t.SenderUserWallet.Id == senderId, trackChanges);
        }
        public IQueryable<Transaction> GetTransactionsBySenderPhoneNumber(string senderPhoneNumber, bool trackChanges) 
        {
            return GetByCondition(t => t.SenderUserWallet.AppUser.PhoneNumber == senderPhoneNumber, trackChanges);
        }
        public IQueryable<Transaction> GetTransactionsByRecipientId(string recipientId, bool trackChanges) 
        {
            return GetByCondition( t => t.RecipientUserWallet.Id == recipientId, trackChanges);
        }
        public IQueryable<Transaction> GetTransactionsByRecipientsPhoneNumber(string recipientPhoneNumber, bool trackChanges) 
        {
            return GetByCondition(t => t.RecipientUserWallet.AppUser.PhoneNumber ==  recipientPhoneNumber, trackChanges);
        }
    }
}