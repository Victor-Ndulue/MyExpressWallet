using Domain.Entites;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Application.Repositories.CommonRepo
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, string>
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<UserWallet> UserWallet { get; set; }
        public DbSet<Transaction> Transaction { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Transaction>()
                .HasKey(k => new {k.Id,k.SenderUserWalletId, k.RecipientUserWalletId});

            builder.Entity<Transaction>()
                .HasOne(r => r.RecipientUserWallet)
                .WithMany(l => l.ReceivedTransactions)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Transaction>()
                .HasOne(s => s.SenderUserWallet)
                .WithMany(t => t.SentTransactions)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
