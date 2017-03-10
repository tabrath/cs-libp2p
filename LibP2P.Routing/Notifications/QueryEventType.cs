using System.Threading.Tasks;

namespace LibP2P.Routing.Notifications
{
    public enum QueryEventType
    {
        SendingQuery,
        PeerResponse,
        FinalPeer,
        QueryError,
        Provider,
        Value,
        AddingPeer,
        DialingPeer
    }
}
