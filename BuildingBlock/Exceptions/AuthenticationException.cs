using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlock.Exceptions
{
    public class AuthenticationException : Exception
    {
        public AuthenticationException(string message): base(message)
        {
        }
        public AuthenticationException(string message, string detail): base(message)
        {
            Detail = detail;
        }
        public string? Detail { get; }
    }
}
