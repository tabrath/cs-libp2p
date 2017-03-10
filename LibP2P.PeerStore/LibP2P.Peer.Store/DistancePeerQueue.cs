using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using KeySpace;
using LibP2P.Utilities.Extensions;

namespace LibP2P.Peer.Store
{
    internal class DistancePeerQueue : PeerQueue
    {
        private class PeerMetric : IComparable<PeerMetric>
        {
            public PeerId Peer { get; }
            public BigInteger Metric { get; }

            public PeerMetric(PeerId peer, BigInteger metric)
            {
                Peer = peer;
                Metric = metric;
            }

            public int CompareTo(PeerMetric other) => Metric.CompareTo(other.Metric);
        }

        private readonly List<PeerMetric> _queue;
        private readonly SemaphoreSlim _sync;
        private readonly Key _from;

        public override int Count => _sync.Lock(() => _queue.Count);

        public DistancePeerQueue(Key from)
        {
            _from = from;
            _queue = new List<PeerMetric>();
            _sync = new SemaphoreSlim(1, 1);
        }

        public override void Enqueue(PeerId p) => _sync.Lock(() => _queue.Add(new PeerMetric(p, XORKeySpace.Instance.Key(p).Distance(_from))));

        public override PeerId Dequeue()
        {
            return _sync.Lock(() =>
            {
                if (!_queue.Any())
                    throw new Exception("Called Dequeue on an empty queue");

                var item = _queue.OrderBy(i => i.Metric).First();
                _queue.Remove(item);
                return item.Peer;
            });
        }
    }
}