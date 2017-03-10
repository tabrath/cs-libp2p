using System;

namespace LibP2P.Peer.Store
{
    public interface IMetrics
    {
        void RecordLatency(string peerId, TimeSpan duration);
        TimeSpan LatencyEWMA(string peerId);
    }
}
