using System;
using System.Collections.Concurrent;

namespace LibP2P.Peer.Store
{
    public class Metrics : IMetrics
    {
        public static readonly float LatencyEWMASmoothing = 0.1f;

        private readonly ConcurrentDictionary<string, TimeSpan> _latencyMap;

        public Metrics()
        {
            _latencyMap = new ConcurrentDictionary<string, TimeSpan>();
        }

        public void RecordLatency(string peerId, TimeSpan duration)
        {
            var s = LatencyEWMASmoothing;
            if (s > 1 || s < 0)
                s = 0.1f;

            TimeSpan ewma;
            if (_latencyMap.TryGetValue(peerId, out ewma))
            {
                duration = TimeSpan.FromMilliseconds(((1.0f - s)*ewma.TotalMilliseconds) + (s*duration.TotalMilliseconds));
                _latencyMap.TryUpdate(peerId, duration, duration);
            }
            else
            {
                _latencyMap.TryAdd(peerId, duration);
            }
        }

        public TimeSpan LatencyEWMA(string peerId)
        {
            TimeSpan latency;
            return _latencyMap.TryGetValue(peerId, out latency) ? latency : TimeSpan.Zero;
        }
    }
}