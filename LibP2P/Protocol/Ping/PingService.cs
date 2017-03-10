using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LibP2P.Host;
using LibP2P.Net;
using LibP2P.Peer;
using LibP2P.Utilities.Extensions;

namespace LibP2P.Protocol.Ping
{
    public class PingService
    {
        public const int PingSize = 32;
        public static readonly ProtocolId Id = new ProtocolId("/ipfs/ping/1.0.0");
        private static readonly TimeSpan PingTimeout = TimeSpan.FromSeconds(60);

        public IHost Host { get; }

        public PingService(IHost host)
        {
            Host = host;
            Host.SetStreamHandler(Id, PingHandler);
        }

        public void PingHandler(INetworkStream stream)
        {
            var buffer = new byte[PingSize];

            using (var timer = new Timer(_ =>
            {
                stream.Dispose();
            }, null, TimeSpan.Zero, PingTimeout))
            {
                while (true)
                {
                    if (stream?.ReadFull(buffer) != buffer.Length)
                        break;

                    if (stream?.Write(buffer, 0, buffer.Length) != buffer.Length)
                        break;

                    timer.Change(TimeSpan.Zero, PingTimeout);
                }
            }
        }

        public async Task Ping(PeerId peer, Action<TimeSpan> onResponse, CancellationToken cancellationToken)
        {
            using (var s = await Host.NewStream(peer, new[] {Id}, cancellationToken))
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var t = await PingAsync(s, cancellationToken);
                    if (t == TimeSpan.Zero)
                        return;

                    Host.Peerstore.RecordLatency(peer, t);

                    if (!cancellationToken.IsCancellationRequested)
                        onResponse.Invoke(t);
                }
            }
        }

        private async Task<TimeSpan> PingAsync(INetworkStream stream, CancellationToken cancellationToken)
        {
            var buffer = new byte[PingSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetNonZeroBytes(buffer);
            }

            var sw = new Stopwatch();
            sw.Start();

            if (await stream.WriteAsync(buffer, 0, buffer.Length, cancellationToken) != buffer.Length)
                return TimeSpan.Zero;

            var rbuf = new byte[PingSize];
            if (await stream.ReadFullAsync(rbuf, cancellationToken: cancellationToken) != rbuf.Length)
                return TimeSpan.Zero;

            if (!rbuf.SequenceEqual(buffer))
                return TimeSpan.Zero;

            sw.Stop();
            return sw.Elapsed;
        }

        private TimeSpan Ping(INetworkStream stream)
        {
            var buffer = new byte[PingSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetNonZeroBytes(buffer);
            }

            var sw = new Stopwatch();
            sw.Start();

            if (stream.Write(buffer, 0, buffer.Length) != buffer.Length)
                return TimeSpan.Zero;

            var rbuf = new byte[PingSize];
            if (stream.ReadFull(rbuf) != rbuf.Length)
                return TimeSpan.Zero;

            if (!rbuf.SequenceEqual(buffer))
                return TimeSpan.Zero;

            sw.Stop();
            return sw.Elapsed;
        }
    }
}
