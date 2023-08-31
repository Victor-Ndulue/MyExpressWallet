using Application.UnitOfWork.Interfaces;
using Microsoft.Extensions.Configuration;
using Services.Interfaces.IServiceEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementations.ServiceEntities
{
    public class TransactionServices : ITransactionServices
    {
        private IWalletFundingService _walletFundingService;
        private IVerifyPaymentService _verifyPaymentService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public TransactionServices(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public IWalletFundingService WalletFundingService => _walletFundingService ??= new WalletFundingService(_unitOfWork, _configuration);

        public IVerifyPaymentService VerifyPaymentService => _verifyPaymentService ??= new VerifyPaymentService(_unitOfWork, _configuration);
    }
}
