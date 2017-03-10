using System;
using System.Linq;
using System.Threading;
using Multiformats.Address;
using NUnit.Framework;

namespace LibP2P.Peer.Store.Tests
{
    [TestFixture]
    public class AddressManagerTests
    {
        private static PeerId Id(string s) => PeerId.Decode(s);
        private static Multiaddress Ma(string s) => Multiaddress.Decode(s);

        private static void TestHas(Multiaddress[] exp, Multiaddress[] act)
        {
            Assert.That(act.Length, Is.EqualTo(exp.Length));

            foreach (var a in exp)
            {
                Assert.That(act.Any(m => m.Equals(a)), Is.True);
            }
        }

        [Test]
        public void TestAddresses()
        {
            var id1 = Id("QmcNstKuwBBoVTpSCSDrwzjgrRcaYXK833Psuz2EMHwyQN");
            var id2 = Id("QmRmPL3FDZKE3Qiwv1RosLdwdvbvg17b2hB39QPScgWKKZ");
            var id3 = Id("QmPhi7vBsChP7sjRoZGgg7bcKqF6MmCcQwvRbDte8aJ6Kn");
            var id4 = Id("QmPhi7vBsChP7sjRoZGgg7bcKqF6MmCcQwvRbDte8aJ5Kn");
            var id5 = Id("QmPhi7vBsChP7sjRoZGgg7bcKqF6MmCcQwvRbDte8aJ5Km");

            var ma11 = Ma("/ip4/1.2.3.1/tcp/1111");
            var ma21 = Ma("/ip4/2.2.3.2/tcp/1111");
            var ma22 = Ma("/ip4/2.2.3.2/tcp/2222");
            var ma31 = Ma("/ip4/3.2.3.3/tcp/1111");
            var ma32 = Ma("/ip4/3.2.3.3/tcp/2222");
            var ma33 = Ma("/ip4/3.2.3.3/tcp/3333");
            var ma41 = Ma("/ip4/4.2.3.3/tcp/1111");
            var ma42 = Ma("/ip4/4.2.3.3/tcp/2222");
            var ma43 = Ma("/ip4/4.2.3.3/tcp/3333");
            var ma44 = Ma("/ip4/4.2.3.3/tcp/4444");
            var ma51 = Ma("/ip4/5.2.3.3/tcp/1111");
            var ma52 = Ma("/ip4/5.2.3.3/tcp/2222");
            var ma53 = Ma("/ip4/5.2.3.3/tcp/3333");
            var ma54 = Ma("/ip4/5.2.3.3/tcp/4444");
            var ma55 = Ma("/ip4/5.2.3.3/tcp/5555");

            var ttl = TimeSpan.FromHours(1);
            var m = new AddressManager();
            m.AddAddress(id1, ma11, ttl);

            m.AddAddresses(id2, new [] { ma21, ma22 }, ttl);
            m.AddAddresses(id2, new [] { ma21, ma22 }, ttl);

            m.AddAddress(id3, ma31, ttl);
            m.AddAddress(id3, ma32, ttl);
            m.AddAddress(id3, ma33, ttl);
            m.AddAddress(id3, ma33, ttl);
            m.AddAddress(id3, ma33, ttl);

            m.AddAddresses(id4, new [] {ma41, ma42, ma43, ma44}, ttl);

            m.AddAddresses(id5, new [] {ma21, ma22}, ttl);
            m.AddAddresses(id5, new [] {ma41, ma42, ma43, ma44}, ttl);
            m.ClearAddresses(id5);
            m.AddAddresses(id5, new [] {ma51, ma52, ma53, ma54, ma55}, ttl);

            Assert.That(m.Peers.Length, Is.EqualTo(5));

            TestHas(new [] { ma11 }, m.Addresses(id1));
            TestHas(new [] { ma21, ma22 }, m.Addresses(id2));
            TestHas(new [] { ma31, ma32, ma33 }, m.Addresses(id3));
            TestHas(new [] { ma41, ma42, ma43, ma44 }, m.Addresses(id4));
            TestHas(new [] { ma51, ma52, ma53, ma54, ma55 }, m.Addresses(id5));
        }

        [Test]
        public void TestAddressesExpire()
        {
            var id1 = Id("QmcNstKuwBBoVTpSCSDrwzjgrRcaYXK833Psuz2EMHwyQN");
            var id2 = Id("QmcNstKuwBBoVTpSCSDrwzjgrRcaYXK833Psuz2EMHwyQM");
            var ma11 = Ma("/ip4/1.2.3.1/tcp/1111");
            var ma12 = Ma("/ip4/2.2.3.2/tcp/2222");
            var ma13 = Ma("/ip4/3.2.3.3/tcp/3333");
            var ma24 = Ma("/ip4/4.2.3.3/tcp/4444");
            var ma25 = Ma("/ip4/5.2.3.3/tcp/5555");

            var m = new AddressManager();
            m.AddAddress(id1, ma11, TimeSpan.FromHours(1));
            m.AddAddress(id1, ma12, TimeSpan.FromHours(1));
            m.AddAddress(id1, ma13, TimeSpan.FromHours(1));
            m.AddAddress(id2, ma24, TimeSpan.FromHours(1));
            m.AddAddress(id2, ma25, TimeSpan.FromHours(1));

            Assert.That(m.Peers.Length, Is.EqualTo(2));

            TestHas(new[] {ma11, ma12, ma13}, m.Addresses(id1));
            TestHas(new[] {ma24, ma25}, m.Addresses(id2));

            m.SetAddress(id1, ma11, TimeSpan.FromHours(2));
            m.SetAddress(id1, ma12, TimeSpan.FromHours(2));
            m.SetAddress(id1, ma13, TimeSpan.FromHours(2));
            m.SetAddress(id2, ma24, TimeSpan.FromHours(2));
            m.SetAddress(id2, ma25, TimeSpan.FromHours(2));

            TestHas(new[] { ma11, ma12, ma13 }, m.Addresses(id1));
            TestHas(new[] { ma24, ma25 }, m.Addresses(id2));

            m.SetAddress(id1, ma11, TimeSpan.FromMilliseconds(1));
            Thread.Sleep(1);
            TestHas(new[] {ma12, ma13}, m.Addresses(id1));
            TestHas(new[] {ma24, ma25}, m.Addresses(id2));

            m.SetAddress(id1, ma13, TimeSpan.FromMilliseconds(1));
            Thread.Sleep(1);
            TestHas(new[] {ma12}, m.Addresses(id1));
            TestHas(new[] {ma24, ma25}, m.Addresses(id2));

            m.SetAddress(id2, ma24, TimeSpan.FromMilliseconds(1));
            Thread.Sleep(1);
            TestHas(new [] {ma12}, m.Addresses(id1));
            TestHas(new [] {ma25}, m.Addresses(id2));

            m.SetAddress(id2, ma25, TimeSpan.FromMilliseconds(1));
            Thread.Sleep(1);
            TestHas(new[] {ma12}, m.Addresses(id1));
            TestHas(Array.Empty<Multiaddress>(), m.Addresses(id2));

            m.SetAddress(id1, ma12, TimeSpan.FromMilliseconds(1));
            Thread.Sleep(1);
            TestHas(Array.Empty<Multiaddress>(), m.Addresses(id1));
            TestHas(Array.Empty<Multiaddress>(), m.Addresses(id2));
        }

        [Test]
        public void TesetClearWorks()
        {
            var id1 = Id("QmcNstKuwBBoVTpSCSDrwzjgrRcaYXK833Psuz2EMHwyQN");
            var id2 = Id("QmcNstKuwBBoVTpSCSDrwzjgrRcaYXK833Psuz2EMHwyQM");
            var ma11 = Ma("/ip4/1.2.3.1/tcp/1111");
            var ma12 = Ma("/ip4/2.2.3.2/tcp/2222");
            var ma13 = Ma("/ip4/3.2.3.3/tcp/3333");
            var ma24 = Ma("/ip4/4.2.3.3/tcp/4444");
            var ma25 = Ma("/ip4/5.2.3.3/tcp/5555");

            var m = new AddressManager();
            m.AddAddress(id1, ma11, TimeSpan.FromHours(1));
            m.AddAddress(id1, ma12, TimeSpan.FromHours(1));
            m.AddAddress(id1, ma13, TimeSpan.FromHours(1));
            m.AddAddress(id2, ma24, TimeSpan.FromHours(1));
            m.AddAddress(id2, ma25, TimeSpan.FromHours(1));

            TestHas(new[] {ma11, ma12, ma13}, m.Addresses(id1));
            TestHas(new[] {ma24, ma25}, m.Addresses(id2));

            m.ClearAddresses(id1);
            m.ClearAddresses(id2);

            TestHas(Array.Empty<Multiaddress>(), m.Addresses(id1));
            TestHas(Array.Empty<Multiaddress>(), m.Addresses(id2));
        }

        [Test]
        public void TestSetNegativeTTLClears()
        {
            var id1 = Id("QmcNstKuwBBoVTpSCSDrwzjgrRcaYXK833Psuz2EMHwyQN");
            var ma11 = Ma("/ip4/1.2.3.1/tcp/1111");

            var m = new AddressManager();
            m.SetAddress(id1, ma11, TimeSpan.FromHours(1));

            TestHas(new [] {ma11}, m.Addresses(id1));

            m.SetAddress(id1, ma11, TimeSpan.Zero);

            TestHas(Array.Empty<Multiaddress>(), m.Addresses(id1));
        }

        [Test]
        public void TestNilAddrsDontBreak()
        {
            var id1 = Id("QmcNstKuwBBoVTpSCSDrwzjgrRcaYXK833Psuz2EMHwyQN");
            
            var m = new AddressManager();
            Assert.DoesNotThrow(() =>
            {
                m.SetAddress(id1, null, TimeSpan.FromHours(1));
                m.AddAddress(id1, null, TimeSpan.FromHours(1));
            });
        }
    }
}
