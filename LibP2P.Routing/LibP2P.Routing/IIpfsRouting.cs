using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LibP2P.Routing
{
    public interface IIpfsRouting : IContentRouting, IPeerRouting, IValueStore, IDisposable
    {
        Task Bootstrap(CancellationToken cancellationToken);
    }
}
