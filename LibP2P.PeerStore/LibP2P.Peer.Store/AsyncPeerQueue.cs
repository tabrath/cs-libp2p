using System.Threading;
using System.Threading.Tasks;

namespace LibP2P.Peer.Store
{
    public class AsyncPeerQueue : PeerQueue
    {
        private readonly PeerQueue _queue;

        public override int Count => _queue.Count;

        internal AsyncPeerQueue(PeerQueue queue)
        {
            _queue = queue;
        }

        public Task<PeerId> DequeueAsync(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<PeerId>();

            Task.Factory.StartNew(() =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    tcs.TrySetCanceled();
                    return;
                }

                if (_queue.Count > 0)
                {
                    var p = _queue.Dequeue();
                    tcs.TrySetResult(p);
                }
                else
                {
                    tcs.TrySetResult(null);
                }
            }, cancellationToken);

            return tcs.Task;
        }

        public Task EnqueueAsync(PeerId p, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled(cancellationToken);

            _queue.Enqueue(p);
            return Task.FromResult(true);
        }

        public override void Enqueue(PeerId p) => _queue.Enqueue(p);
        public override PeerId Dequeue() => _queue.Dequeue();
    }
}