using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LibP2P.Abstractions.Connection
{
    public static class Pipe
    {
        internal static bool Create(out Socket a, out Socket b)
        {
            using (var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                listener.Bind(new IPEndPoint(IPAddress.Loopback, 0));
                listener.Listen(1);
                var accept = Task.Factory.FromAsync(
                    (callback, state) => ((Socket)state).BeginAccept(callback, state),
                    asyncResult => ((Socket) asyncResult.AsyncState).EndAccept(asyncResult),
                    state: listener);

                b = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                b.NoDelay = false;
                b.LingerState = new LingerOption(false, 0);
                b.Connect(listener.LocalEndPoint);

                if (!accept.Wait(TimeSpan.FromSeconds(3)))
                {
                    a = null;
                    b.Dispose();
                    return false;
                }

                a = accept.Result;
                return true;
            }
        }

        public static bool Create(out IConnection a, out IConnection b)
        {
            Socket sa, sb;
            if (!Create(out sa, out sb))
            {
                a = null;
                b = null;
                return false;
            }

            a = new SocketConnection(sa);
            b = new SocketConnection(sb);

            return true;
        }
    }
}
