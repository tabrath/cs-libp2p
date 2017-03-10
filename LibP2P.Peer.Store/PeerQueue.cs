using KeySpace;

namespace LibP2P.Peer.Store
{
    public abstract class PeerQueue
    {
        public abstract int Count { get; }
        public abstract void Enqueue(PeerId p);
        public abstract PeerId Dequeue();

        public static PeerQueue CreateDistanceQueue(string from) => new DistancePeerQueue(XORKeySpace.Instance.Key((PeerId)from));
        public static AsyncPeerQueue CreateAsyncQueue(PeerQueue queue) => new AsyncPeerQueue(queue);
    }
}
