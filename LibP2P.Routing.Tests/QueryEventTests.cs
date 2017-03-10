using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
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
    public class QueryEventTests
    {
        [Test]
        public void CanMarshalJson()
        {
            var query = new QueryEvent(
                new PeerId("random_peer_id"),
                QueryEventType.Value,
                new []
                {
                    new PeerInfo(new PeerId("another_random_peer_id"), new []
                    {
                        new Multiaddress().Add<IP4>(IPAddress.Loopback).Add<TCP>((ushort)8080)
                    })
                },
                "hello_world");

            var json = query.MarshalJson();
            var decoded = QueryEvent.UnmarshalJson(json);

            Assert.That(decoded.Id, Is.EqualTo(query.Id));
            Assert.That(decoded.Type, Is.EqualTo(query.Type));
            Assert.That(decoded.Responses.Length, Is.EqualTo(query.Responses.Length));
            Assert.That(decoded.Responses[0].Id, Is.EqualTo(query.Responses[0].Id));
            Assert.That(decoded.Extra, Is.EqualTo(query.Extra));
        }
    }
}
