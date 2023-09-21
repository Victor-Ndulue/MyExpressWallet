using Services.LoggerService.Exceptions.CommonExceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.LoggerService.Exceptions.EntityExceptions.EntityNotFoundException
{
    public sealed class AppRoleNotFoundException : NotFoundException
    {
        public AppRoleNotFoundException(string Role) : base($"The role{Role} does not exist")
        {
        }
    }
}
