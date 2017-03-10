using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LibP2P.Utilities.Extensions;
using Multiformats.Address;
using Multiformats.Address.Protocols;
using NUnit.Framework;

namespace LibP2P.Utilities.Tests
{
    [TestFixture]
    public class MultiaddressTests
    {
        [Test]
        public void IsIPLoopback_GivenIP4LoopbackAddress_ReturnsTrue()
        {
            var maddr = new Multiaddress().Add<IP4>(IPAddress.Loopback);

            Assert.That(maddr.IsIPLoopback(), Is.True);
        }

        [Test]
        public void IsIPLoopback_GivenIP6LoopbackAddress_ReturnsTrue()
        {
            var maddr = new Multiaddress().Add<IP6>(IPAddress.IPv6Loopback);

            Assert.That(maddr.IsIPLoopback(), Is.True);
        }

        [Test]
        public void IsIPLoopback_GivenIP4NoLoopbackAddress_ReturnsFalse()
        {
            var maddr = new Multiaddress().Add<IP4>(IPAddress.Parse("192.168.0.1"));

            Assert.That(maddr.IsIPLoopback(), Is.False);
        }

        [Test]
        public void IsIPLoopback_GivenIP6NoLoopbackAddress_ReturnsFalse()
        {
            var maddr = new Multiaddress().Add<IP6>(IPAddress.Parse("fe80::5066:4785:4a2:a9b7"));

            Assert.That(maddr.IsIPLoopback(), Is.False);
        }

        [Test]
        public void IsIPLookback_GivenNoIPAddress_ReturnsFalse()
        {
            var maddr = new Multiaddress();

            Assert.That(maddr.IsIPLoopback(), Is.False);
        }

        [Test]
        public void IsFDCostlyTransport_GivenTcp_ReturnsTrue()
        {
            var maddr = new Multiaddress().Add<TCP>((ushort)1024);

            Assert.That(maddr.IsFDCostlyTransport(), Is.True);
        }

        [Test]
        public void IsFDCostlyTransport_GivenHTTP_ReturnsFalse()
        {
            var maddr = new Multiaddress().Add<HTTP>();

            Assert.That(maddr.IsFDCostlyTransport(), Is.False);
        }

        [Test]
        public void IsFDCostlyTransport_GivenEmptyAddress_ReturnsFalse()
        {
            var maddr = new Multiaddress();

            Assert.That(maddr.IsFDCostlyTransport(), Is.False);
        }
    }
}
