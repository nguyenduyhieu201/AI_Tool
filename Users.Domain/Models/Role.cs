using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Domain.Abstractions;

namespace Users.Domain.Models
{
    public class Role
    {
        public Guid Id { set;get; }
        public string Name { set; get; } = string.Empty;
    }
}
