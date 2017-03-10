using LibP2P.Abstractions.Connection;

namespace LibP2P.Abstractions.Transport
{
    public interface ITransportConnection : IConnection
    {
        ITransport Transport { get; }
    }
}