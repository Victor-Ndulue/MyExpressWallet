using Application.Interfaces.Common.IGenericRepository;
using Domain.Entites;

namespace Application.Interfaces.IEntityRepo.IEntityQueryHandler
{
    public interface ITransactionQueryRepo : IQueryRepository<Transaction>
    {
        IQueryable<Transaction> GetTransactionsByTransactionId(string Id, bool trackChanges);
        IQueryable<Transaction> GetTransactionsByDate(DateTime date, bool trackChanges);
        IQueryable<Transaction> GetTransactionsByReference(string reference, bool trackChanges);
        IQueryable<Transaction> GetTransactionsBySenderId(string senderId, bool trackChanges);
        IQueryable<Transaction> GetTransactionsBySenderPhoneNumber(string senderPhoneNumber, bool trackChanges);
        IQueryable<Transaction> GetTransactionsByRecipientId(string recipientId, bool trackChanges);
        IQueryable<Transaction> GetTransactionsByRecipientsPhoneNumber(string recipientPhoneNumber, bool trackChanges);
    }
}
