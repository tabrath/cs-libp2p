using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibP2P.Utilities.Extensions;
using Multiformats.Hash;
using Multiformats.Hash.Algorithms;
using NUnit.Framework;

namespace LibP2P.Utilities.Tests
{
    [TestFixture]
    public class MultihashTests
    {
        [Test]
        public void Compare_GivenEqualHashes_ReturnsZero()
        {
            var mh1 = Multihash.Sum<SHA1>(Encoding.UTF8.GetBytes("hello world"));
            var mh2 = Multihash.Sum<SHA1>(Encoding.UTF8.GetBytes("hello world"));

            Assert.That(mh1.Compare(mh2), Is.EqualTo(0));
        }

        [Test]
        public void Compare_GivenNonEqualHashes_ReturnsOneOrMinusOne()
        {
            var mh1 = Multihash.Sum<SHA1>(Encoding.UTF8.GetBytes("hello world"));
            var mh2 = Multihash.Sum<SHA1>(Encoding.UTF8.GetBytes("hello_world!"));

            Assert.That(mh1.Compare(mh2), Is.Not.EqualTo(0));
        }
    }
}
