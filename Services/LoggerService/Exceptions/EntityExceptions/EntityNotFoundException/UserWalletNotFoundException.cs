using Services.LoggerService.Exceptions.CommonExceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.LoggerService.Exceptions.EntityExceptions.EntityNotFoundException
{
    public sealed class UserWalletNotFoundException : NotFoundException
    {
        public UserWalletNotFoundException(string uniqueProperty, string enteredPropertyDetail) : base($"The wallet with {uniqueProperty} : {enteredPropertyDetail} does not exist")
        {
        }
    }
}
