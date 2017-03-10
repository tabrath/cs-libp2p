using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LibP2P.Utilities.Extensions;
using Multiformats.Hash;
using Multiformats.Hash.Algorithms;
using NUnit.Framework;

namespace LibP2P.Peer.Store.Tests
{
    [TestFixture]
    public class PeerQueueTests
    {
        [Test]
        public void TestQueue()
        {
            var p1 = (PeerId) "11140beec7b5ea3f0fdbc95d0dd47f3c5bc275da8a31";
            var p2 = (PeerId) "11140beec7b5ea3f0fdbc95d0dd47f3c5bc275da8a32";
            var p3 = (PeerId) "11140beec7b5ea3f0fdbc95d0dd47f3c5bc275da8a33";
            var p4 = (PeerId) "11140beec7b5ea3f0fdbc95d0dd47f3c5bc275da8a34";
            var p5 = (PeerId) "11140beec7b5ea3f0fdbc95d0dd47f3c5bc275da8a31";

            var pq = PeerQueue.CreateDistanceQueue("11140beec7b5ea3f0fdbc95d0dd47f3c5bc275da8a31");
            pq.Enqueue(p3);
            pq.Enqueue(p1);
            pq.Enqueue(p2);
            pq.Enqueue(p4);
            pq.Enqueue(p5);
            pq.Enqueue(p1);

            var d = pq.Dequeue();
            Assert.That(d, Is.EqualTo(p1).Or.EqualTo(p5));

            d = pq.Dequeue();
            Assert.That(d, Is.EqualTo(p1).Or.EqualTo(p5));

            d = pq.Dequeue();
            Assert.That(d, Is.EqualTo(p1).Or.EqualTo(p5));

            Assert.That(pq.Dequeue(), Is.EqualTo(p4));
            Assert.That(pq.Dequeue(), Is.EqualTo(p3));
            Assert.That(pq.Dequeue(), Is.EqualTo(p2));
        }

        private static PeerId MakePeer(DateTime dt)
            => Multihash.Sum<SHA2_256>(Encoding.UTF8.GetBytes($"hmmm time: {dt}")).ToString();

        private class SyncQueueState
        {
            public SemaphoreSlim Sync { get; set; }
            public int Iter { get; set; }
        }

        //[Test]
        public void TestSyncQueue()
        {
            const int max = 5000;
            const int consumerN = 10;
            var countsIn = new int[consumerN*2];
            var countsOut = new int[consumerN];
            var dq = PeerQueue.CreateDistanceQueue("11140beec7b5ea3f0fdbc95d0dd47f3c5bc275da8a31");
            var pq = PeerQueue.CreateAsyncQueue(dq);
            var sync = new SemaphoreSlim(1, 1);

            var produce = new Action<object>(async state =>
            {
                var sqs = (SyncQueueState) state;
                if (sqs.Iter >= await sqs.Sync.LockAsync(() => countsIn.Length, CancellationToken.None))
                    return;

                for (var i = 0; i < max; i++)
                {
                    await Task.Delay(TimeSpan.FromTicks(50));
                    await sqs.Sync.LockAsync(() => countsIn[sqs.Iter]++, CancellationToken.None);
                    await pq.EnqueueAsync(MakePeer(DateTime.Now), CancellationToken.None);
                }
            });

            var consume = new Action<object>(async state =>
            {
                var sqs = (SyncQueueState)state;
                if (sqs.Iter >= await sqs.Sync.LockAsync(() => countsOut.Length, CancellationToken.None))
                    return;

                while ((await pq.DequeueAsync(CancellationToken.None)) != null)
                {
                    if (await sqs.Sync.LockAsync(() =>
                    {
                        countsOut[sqs.Iter]++;
                        return countsOut[sqs.Iter] < max*2;
                    }, CancellationToken.None) == false)
                        break;
                }
            });

            var tasks = new List<Task>();
            for (var i = 0; i < consumerN; i++)
            {
                var x = i;
                tasks.Add(Task.Factory.StartNew(produce, new SyncQueueState { Iter = x, Sync = sync}));
                tasks.Add(Task.Factory.StartNew(produce, new SyncQueueState { Iter = consumerN + x, Sync = sync }));
                tasks.Add(Task.Factory.StartNew(consume, new SyncQueueState { Iter = x, Sync = sync }));
            }
            Task.WaitAll(tasks.ToArray());

            sync.Dispose();

            Assert.That(countsOut.Sum(), Is.EqualTo(countsIn.Sum()));
        }
    }


}
