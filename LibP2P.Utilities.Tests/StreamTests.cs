using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LibP2P.Utilities.Extensions;
using NUnit.Framework;

namespace LibP2P.Utilities.Tests
{
    [TestFixture]
    public class StreamTests
    {
        [Test]
        public void AsReader_GivenMemoryStream_CanReadAll()
        {
            var bytes = new byte[4096];
            TestContext.CurrentContext.Random.NextBytes(bytes);
            using (var memory = new MemoryStream(bytes))
            {
                var reader = memory.AsReader();
                var buffer = new byte[bytes.Length];
                var bytesRead = reader.ReadFull(buffer);

                Assert.That(bytesRead, Is.EqualTo(bytes.Length));
                Assert.That(buffer, Is.EqualTo(bytes));
            }
        }

        [Test]
        public async Task AsReader_GivenMemoryStream_CanReadAllAsync()
        {
            var bytes = new byte[4096];
            TestContext.CurrentContext.Random.NextBytes(bytes);
            using (var memory = new MemoryStream(bytes))
            {
                var reader = memory.AsReader();
                var buffer = new byte[bytes.Length];
                var bytesRead = await reader.ReadFullAsync(buffer);

                Assert.That(bytesRead, Is.EqualTo(bytes.Length));
                Assert.That(buffer, Is.EqualTo(bytes));
            }
        }

        [Test]
        public void AsWriter_GivenMemoryStream_CanWriteAll()
        {
            using (var memory = new MemoryStream())
            {
                var writer = memory.AsWriter();
                var bytes = new byte[4096];
                TestContext.CurrentContext.Random.NextBytes(bytes);
                writer.Write(bytes, 0, bytes.Length);

                Assert.That(memory.ToArray(), Is.EqualTo(bytes));
            }
        }

        [Test]
        public async Task AsWriter_GivenMemoryStream_CanWriteAllAsync()
        {
            using (var memory = new MemoryStream())
            {
                var writer = memory.AsWriter();
                var bytes = new byte[4096];
                TestContext.CurrentContext.Random.NextBytes(bytes);
                await writer.WriteAsync(bytes, 0, bytes.Length, CancellationToken.None);

                Assert.That(memory.ToArray(), Is.EqualTo(bytes));
            }
        }
    }
}
