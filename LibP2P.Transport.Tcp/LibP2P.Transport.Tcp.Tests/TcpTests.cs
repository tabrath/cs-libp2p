using System;
using Multiformats.Address;
using NUnit.Framework;

namespace LibP2P.Transport.Tcp.Tests
{
    [TestFixture]
    public class TcpTests
    {
        [Test]
        public void TestTcpTransport()
        {
            var ta = new TcpTransport();
            var tb = new TcpTransport();

            var zero = "/ip4/127.0.0.1/tcp/0";

            Transport.Tests.Utilities.SubtestTransport(ta, tb, zero);
        }

        [Test]
        public void TestTcpTransportCantListenUtp()
        {
            var utpa = Multiaddress.Decode("/ip4/127.0.0.1/udp/0/utp");
            Assert.That(utpa, Is.Not.Null);

            var tpt = new TcpTransport();
            Assert.Throws<NotSupportedException>(() => tpt.Listen(utpa));
        }
    }
}
