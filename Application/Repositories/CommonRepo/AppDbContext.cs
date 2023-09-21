using Domain.Entites;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Application.Repositories.CommonRepo
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, string>
    {
        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<UserWallet> UserWallet { get; set; }
        public DbSet<Transaction> Transaction { get; set; }
        public DbSet<PaymentRecord> PaymentRecord { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserWallet>()
                .HasMany(t => t.SentTransactions)
                .WithOne(s => s.SenderUserWallet)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UserWallet>()
                .HasMany(t=>t.ReceivedTransactions)
                .WithOne(r => r.RecipientUserWallet)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
