using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LibP2P.Peer;
using LibP2P.Peer.Store;
using LibP2P.Routing.Notifications;
using Multiformats.Address;
using Multiformats.Address.Protocols;
using NUnit.Framework;

namespace LibP2P.Routing.Tests
{
    [TestFixture]
    public class QueryContextTests
    {
        [Test]
        public void TestContext()
        {
            var query = new QueryEvent(new PeerId("random_peer_id"), QueryEventType.AddingPeer, new[] { new PeerInfo(new PeerId("another_random_peer_id"), new[] { new Multiaddress().Add<IP4>(IPAddress.Loopback) }) }, "some_extra_data");
            var waiter = new ManualResetEventSlim();

            var ev = new Action<QueryContext, QueryEvent>((ctx, q) =>
            {
                Assert.That(q.Id, Is.EqualTo(query.Id));
                waiter.Set();
            });

            using (var context = QueryContext.RegisterForQueryEvents(ev))
            {
                context.PublishQueryEvent(query);

                Assert.True(waiter.Wait(100));
            }
        }
    }
}
