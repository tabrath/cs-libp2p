using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibP2P.Host.Basic
{
    internal class NatManager : IDisposable
    {
        private readonly BasicHost _host;

        public NatManager(BasicHost host)
        {
            _host = host;
        }

        public void Dispose()
        {
        }
    }
}
