using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public static class AuthorizationProtocol
    {
        public const string GetTokenCommand = "T";
        public const string SuccessReply = "S";
        public const string ErrorReply = "E";
        public const int Port = 5557;
    }
}
