using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using NUnit.Framework;

namespace LibP2P.Peer.Store.Tests
{
    [TestFixture]
    public class MetricsTests
    {
        public static string MakeRandomPeerId()
        {
            var buf = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(buf);
            }
            using (var sha = SHA256.Create())
            {
                return Encoding.UTF8.GetString(sha.ComputeHash(buf));
            }
        }

        [Test]
        public void TestLatencyEWMA()
        {
            var m = new Metrics();
            var id = MakeRandomPeerId();
            var exp = 100.0;
            var mu = exp;
            var sig = 10.0;
            var rand = new Random(Environment.TickCount);
            Func<TimeSpan> next = () => TimeSpan.FromMilliseconds(rand.NextDouble() * sig);

            for (var i = 0; i < 10; i++)
            {
                m.RecordLatency(id, next());
                Thread.Sleep(200);
            }

            var lat = m.LatencyEWMA(id);
            Assert.True(Math.Abs(exp - lat.TotalMilliseconds) > sig, $"Latency outside of expected range: {exp} {lat} {sig}");
        }
    }
}
