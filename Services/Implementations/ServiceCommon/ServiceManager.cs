using Application.UnitOfWork.Interfaces;
using AutoMapper;
using Domain.Entites;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Services.Helpers.MailServices;
using Services.Implementations.ServiceEntities;
using Services.Interfaces.IServiceCommon;
using Services.Interfaces.IServiceEntities;
using Services.LoggerService.Interface;

namespace Services.Implementations.ServiceCommon
{
    public class ServiceManager : IServiceManager
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly ILoggerManager _loggerManager;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public ServiceManager(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, IUnitOfWork unitOfWork, ITokenService tokenService, ILoggerManager loggerManager, IMapper mapper, IConfiguration configuration, IEmailService emailService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _loggerManager = loggerManager;
            _mapper = mapper;
            _configuration = configuration;
            _emailService = emailService;
        }

        private IAuthenticationServices _authenticationServices;
        public IAuthenticationServices AuthenticationServices => _authenticationServices ??= new AuthenticationServices(_userManager, _roleManager, _loggerManager, _mapper, _tokenService, this, _emailService);

        private ITransactionServices _transactionServices;
        public ITransactionServices TransactionServices => _transactionServices ??= new TransactionServices(_unitOfWork, _mapper, _loggerManager,this);

        private IUserServices _userServices;
        public IUserServices UserServices => _userServices ??= new UserServices( _userManager, _mapper, _loggerManager);

        private IVerifyWalletFundingService _verifyWalletFundingService;
        public IVerifyWalletFundingService VerifyWalletFundingService => _verifyWalletFundingService ??= new VerifyWalletFundingService(_unitOfWork, _configuration, _loggerManager,this, _emailService);

        private IWalletFundingService _walletFundingService;
        public IWalletFundingService WalletFundingService => _walletFundingService ??= new WalletFundingService(_configuration, _loggerManager, this, _emailService);

        private IWalletServices _walletServices;
        public IWalletServices WalletServices => _walletServices ??= new WalletServices(_unitOfWork, _mapper, _loggerManager,this, _emailService);

        private IWithdrawFromWalletService _withdrawFromWalletService;
        public IWithdrawFromWalletService WithdrawFromWalletService => _withdrawFromWalletService ??= new WithdrawFromWalletService(_unitOfWork, this, _configuration, _mapper, _loggerManager, _emailService);
    }
}
