using Services.LoggerService.Exceptions.CommonExceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.LoggerService.Exceptions.EntityExceptions.EntityNotFoundException
{
    public sealed class TransactionNotFoundException : NotFoundException
    {
        public TransactionNotFoundException(string uniqueProperty, string enteredPropertyDetail) : base($"The Transaction with {uniqueProperty} : {enteredPropertyDetail} does not exist")
        {
        }
    }
}
