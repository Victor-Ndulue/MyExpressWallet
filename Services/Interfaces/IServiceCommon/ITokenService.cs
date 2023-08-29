using Domain.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces.IServiceCommon
{
    public interface ITokenService
    {
        Task<string> CreateToken(AppUser user);
    }
}
