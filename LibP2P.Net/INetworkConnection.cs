using LibP2P.Abstractions.Connection;

namespace LibP2P.Net
{
    public interface INetworkConnection : IPeerConnection
    {
        INetworkStream NewStream();
        INetworkStream[] GetStreams();
    }

    public delegate void ConnectionHandler(INetworkConnection connection);
}