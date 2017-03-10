using Multiformats.Address;
using NUnit.Framework;

namespace LibP2P.Peer.Store.Tests
{
    [TestFixture]
    public class PeerInfoTests
    {
        private static Multiaddress MustAddr(string s) => Multiaddress.Decode(s);

        [Test]
        public void TestPeerInfoMarshal()
        {
            var a = MustAddr("/ip4/1.2.3.4/tcp/4536");
            var b = MustAddr("/ip4/1.2.3.8/udp/7777");
            var id = PeerId.Decode("QmaCpDMGvV2BGHeYERUEnRQAwe3N8SzbUtfsmvsqQLuvuJ");
            var pi = new PeerInfo(id, new[] {a,b});
            var data = pi.MarshalJson();
            var pi2 = PeerInfo.UnmarshalJson(data);

            Assert.That(pi2.Id, Is.EqualTo(pi.Id));
            Assert.That(pi2.Addresses[0], Is.EqualTo(pi.Addresses[0]));
            Assert.That(pi2.Addresses[1], Is.EqualTo(pi.Addresses[1]));
            Assert.That(pi2.Id.ToString(), Is.EqualTo(id.ToString()));
        }
    }
}
