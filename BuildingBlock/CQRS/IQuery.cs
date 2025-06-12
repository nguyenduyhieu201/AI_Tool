using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlock.CQRS
{
    public interface IQuery<out TResponse> : IRequest<TResponse> where TResponse : notnull
    {
        // The interface does not need to define any additional members,
        // as it inherits from IRequest which already defines the necessary methods.
    }
    
}
