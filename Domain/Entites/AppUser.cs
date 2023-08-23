using Microsoft.AspNetCore.Identity;

namespace Domain.Entites
{
    public class AppUser : IdentityUser
    {
        public ICollection<UserWallet> Wallets { get; set; }
    }
}
