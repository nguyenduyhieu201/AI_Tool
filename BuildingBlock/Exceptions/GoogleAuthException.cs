using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlock.Exceptions
{
    public class GoogleAuthException : Exception
    {
        public GoogleAuthException(string message) : base(message)
        {
        }
        public GoogleAuthException(string message, string detail) : base(message)
        {
            Detail = detail;
        }
        public string? Detail { get; }
    }
}
