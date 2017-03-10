using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibP2P.Peer;
using LibP2P.Peer.Store;
using NUnit.Framework;

namespace LibP2P.KBucket.Tests
{
    [TestFixture]
    public class TableTests
    {
        [Test]
        public void TestBucket()
        {
            var b = new Bucket();

            var peers = new PeerId[100];
            for (var i = 0; i < 100; i++)
            {
                peers[i] = new PeerId($"Random PeerId {i}");
                b.PushFront(peers[i]);
            }

            var local = new PeerId($"Local PeerId");
            var localId = DhtId.ConvertPeerId(local);
            var x = new Random(Environment.TickCount).Next(peers.Length);
            Assert.That(b.Has(peers[x]), Is.True);

            var spl = b.Split(0, DhtId.ConvertPeerId(local));
            var llist = b.Peers;
            foreach (var e in llist)
            {
                var p = DhtId.ConvertPeerId(e);
                var cpl = DhtId.CommonPrefixLength(p, localId);
                Assert.That(cpl, Is.Not.GreaterThan(0));
            }
            var rlist = spl.Peers;
            foreach (var e in rlist)
            {
                var p = DhtId.ConvertPeerId(e);
                var cpl = DhtId.CommonPrefixLength(p, localId);
                Assert.That(cpl, Is.Not.EqualTo(0));
            }
        }

        [Test]
        public void TestTableUpdate()
        {
            var local = new PeerId("Random Local Peer");
            var m = new Metrics();
            var rt = new RoutingTable(10, DhtId.ConvertPeerId(local), TimeSpan.FromHours(1), m);

            var peers = Enumerable.Range(0, 100)
                .Select(i => new PeerId($"Random Peer {i}"))
                .ToArray();

            var rand = new Random(Environment.TickCount);
            for (var i = 0; i < 10000; i++)
            {
                rt.Update(peers[rand.Next(peers.Length)]);
            }

            for (var i = 0; i < 100; i++)
            {
                var id = DhtId.ConvertPeerId(new PeerId($"Random Peer {rand.Next(1024)}"));
                var ret = rt.NearestPeers(id, 5);
                
                Assert.That(ret.Length, Is.GreaterThan(0));
            }
        }

        [Test]
        public void TestTableFind()
        {
            var local = new PeerId("Random Local Peer");
            var m = new Metrics();
            var rt = new RoutingTable(10, DhtId.ConvertPeerId(local), TimeSpan.FromHours(1), m);

            var peers = new PeerId[100];
            for (var i = 0; i < 5; i++)
            {
                peers[i] = new PeerId($"Random Peer {i}");
                rt.Update(peers[i]);
            }

            var found = rt.NearestPeer(DhtId.ConvertPeerId(peers[2]));
            Assert.That(found, Is.EqualTo(peers[2]));
        }

        [Test]
        public void TestTableFindMultiple()
        {
            var local = new PeerId("Random Local Peer");
            var m = new Metrics();
            var rt = new RoutingTable(20, DhtId.ConvertPeerId(local), TimeSpan.FromHours(1), m);

            var peers = new PeerId[100];
            for (var i = 0; i < 18; i++)
            {
                peers[i] = new PeerId($"Random Peer {i}");
                rt.Update(peers[i]);
            }

            var found = rt.NearestPeers(DhtId.ConvertPeerId(peers[2]), 15);
            Assert.That(found.Length, Is.EqualTo(15));
        }

        [Test]
        public void TestTableMultithreaded()
        {
            var local = new PeerId("Random Local Peer");
            var m = new Metrics();
            var rt = new RoutingTable(20, DhtId.ConvertPeerId(local), TimeSpan.FromHours(1), m);

            var peers = Enumerable.Range(0, 500)
                .Select(i => new PeerId($"Random Peer {i}"))
                .ToArray();

            var t1 = Task.Factory.StartNew(() =>
            {
                var rand = new Random(Environment.TickCount);
                for (var i = 0; i < 1000; i++)
                {
                    var n = rand.Next(peers.Length);
                    rt.Update(peers[n]);
                }
            });

            var t2 = Task.Factory.StartNew(() =>
            {
                var rand = new Random(Environment.TickCount);
                for (var i = 0; i < 1000; i++)
                {
                    var n = rand.Next(peers.Length);
                    rt.Update(peers[n]);
                }
            });

            var t3 = Task.Factory.StartNew(() =>
            {
                var rand = new Random(Environment.TickCount);
                for (var i = 0; i < 1000; i++)
                {
                    var n = rand.Next(peers.Length);
                    rt.Find(peers[n]);
                }
            });

            Task.WaitAll(t1, t2, t3);
        }
    }
}
