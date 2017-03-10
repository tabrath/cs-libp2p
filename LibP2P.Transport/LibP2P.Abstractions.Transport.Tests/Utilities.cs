using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LibP2P.Abstractions.Transport;
using Multiformats.Address;
using NUnit.Framework;

namespace LibP2P.Transport.Tests
{
    public static class Utilities
    {
        public static void SubtestTransport(ITransport ta, ITransport tb, string addr)
        {
            var maddr = Multiaddress.Decode(addr);
            Assert.That(maddr, Is.Not.Null);

            var listener = ta.Listen(maddr);
            Assert.That(listener, Is.Not.Null);

            var dialer = tb.Dialer(maddr);
            Assert.That(dialer, Is.Not.Null);

            var accepted = listener.AcceptAsync(CancellationToken.None);
            var dialed = dialer.DialAsync(listener.Multiaddress, CancellationToken.None);

            Task.WaitAll(accepted, dialed);

            var a = accepted.Result;
            var b = dialed.Result;

            try
            {
                Assert.DoesNotThrow(() => CheckDataTransfer(a, b));
            }
            finally
            {
                a.Dispose();
                b.Dispose();
            }
        }

        private static void CheckDataTransfer(ITransportConnection a, ITransportConnection b)
        {
            var data = Encoding.UTF8.GetBytes("this is some test data");

            var taska = Task.Factory.StartNew(() =>
            {
                var n = a.Write(data, 0, data.Length);

                Assert.That(n, Is.EqualTo(data.Length));

                var buf = new byte[data.Length];
                n = a.Read(buf, 0, buf.Length);
                Assert.That(n, Is.EqualTo(buf.Length));
            });

            var taskb = Task.Factory.StartNew(() =>
            {
                var buf = new byte[data.Length];
                var n = b.Read(buf, 0, buf.Length);
                Assert.That(n, Is.EqualTo(buf.Length));

                n = b.Write(data, 0, data.Length);
                Assert.That(n, Is.EqualTo(data.Length));
            });

            Task.WaitAll(taska, taskb);
        }
    }
}
