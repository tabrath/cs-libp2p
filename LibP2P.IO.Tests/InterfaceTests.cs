using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace LibP2P.IO.Tests
{
    [TestFixture]
    public class InterfaceTests
    {
        [Test]
        public void Closer_Test()
        {
            var closed = false;
            var closer = new Mock<ICloser>();
            closer.Setup(c => c.Close()).Callback(() => closed = true);
            closer.Object.Close();

            Assert.That(closed, Is.True);
        }

        [Test]
        public void Reader_Test()
        {
            var data = new byte[4096];
            var dataOffset = 0;
            TestContext.CurrentContext.Random.NextBytes(data);
            var reader = new Mock<IReader>();
            reader.Setup(r => r.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns<byte[], int, int>((buffer, offset, count) =>
                {
                    count = Math.Min(count, data.Length - dataOffset);
                    Buffer.BlockCopy(data, dataOffset, buffer, offset, count);
                    dataOffset += count;
                    return count;
                });

            var buf = new byte[data.Length];
            var total = 0;
            while (total < data.Length)
            {
                total += reader.Object.Read(buf, total, 512);
            }

            Assert.That(total, Is.EqualTo(data.Length));
            Assert.That(dataOffset, Is.EqualTo(data.Length));
            Assert.That(buf, Is.EqualTo(data));
        }

        [Test]
        public void Writer_Test()
        {
            var data = new byte[4096];
            var dataOffset = 0;
            var writer = new Mock<IWriter>();
            writer.Setup(w => w.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns<byte[], int, int>((buffer, offset, count) =>
                {
                    count = Math.Min(count, data.Length - dataOffset);
                    Buffer.BlockCopy(buffer, offset, data, dataOffset, count);
                    dataOffset += count;
                    return count;
                });

            var buf = new byte[data.Length];
            TestContext.CurrentContext.Random.NextBytes(buf);
            var total = 0;
            while (total < data.Length)
            {
                total += writer.Object.Write(buf, total, 512);
            }

            Assert.That(total, Is.EqualTo(data.Length));
            Assert.That(dataOffset, Is.EqualTo(data.Length));
            Assert.That(buf, Is.EqualTo(data));
        }
    }
}
