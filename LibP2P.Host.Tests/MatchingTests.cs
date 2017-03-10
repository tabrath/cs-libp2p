using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace LibP2P.Host.Tests
{
    [TestFixture]
    public class MatchingTests
    {
        [TestCase("/testing/4.3.0",   true)]
		[TestCase("/testing/4.3.7",   true)]
        [TestCase("/testing/4.3.5",   true)]
        [TestCase("/testing/4.2.7",   true)]
        [TestCase("/testing/4.0.0",   true)]
        [TestCase("/testing/5.0.0",   false)]
        [TestCase("/cars/dogs/4.3.5", false)]
        [TestCase("/foo/1.0.0",       false)]
        [TestCase("",                 false)]
        [TestCase("dogs",             false)]
        [TestCase("/foo",             false)]
        [TestCase("/foo/1.1.1.1",     false)]
        public void TestSemverMatching(string p, bool ok)
        {
            var m = MultistreamMatchers.SemverMatcher("/testing/4.3.5");

            Assert.That(m(p), Is.EqualTo(ok));
        }
    }
}
