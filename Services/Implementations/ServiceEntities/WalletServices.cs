using Application.UnitOfWork.Interfaces;
using AutoMapper;
using Domain.Entites;
using Microsoft.AspNetCore.Identity;
using Services.Interfaces.IServiceEntities;

namespace Services.Implementations.ServiceEntities
{
    public class WalletServices : IWalletServices
    {
        private readonly IUnitOfWork _unitOfWork;
        public readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager; 
        public WalletServices(IUnitOfWork unitOfWork, IMapper mapper, UserManager<AppUser> user) 
            => (_unitOfWork, _mapper, _userManager) = (unitOfWork, mapper, user);

        public async Task CreateWalletAccount(string Email)
        {
            var user = await _userManager.FindByEmailAsync(Email);
            var wallet = new UserWallet();
            wallet.AppUser = user;
            await _unitOfWork.UserWalletCommand.CreateAsync(wallet);
            await _unitOfWork.SaveChangesAsync();
            user.Wallets.Add(wallet);
            await _userManager.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
