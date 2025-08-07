using AuthClassLibray.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthClassLibray.Services
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}
