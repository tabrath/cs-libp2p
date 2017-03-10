using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibP2P.Utilities.Extensions;
using NUnit.Framework;
using ProtoBuf;

namespace LibP2P.Utilities.Tests
{
    [TestFixture]
    public class ProtoBufTests
    {
        [Test]
        public void Serialize_GivenContract_SerializesAndDeserializes()
        {
            var contract = new TestProtoBufContract
            {
                TestString = "Hello World!",
                TestBytes = Encoding.UTF8.GetBytes("Hello World!").ComputeHash(),
                TestBool = true
            };

            var bytes = contract.SerializeToBytes();
            var deserialized = bytes.Deserialize<TestProtoBufContract>();

            Assert.That(deserialized.TestString, Is.EqualTo(contract.TestString));
            Assert.That(deserialized.TestBytes, Is.EqualTo(contract.TestBytes));
            Assert.That(deserialized.TestBool, Is.EqualTo(contract.TestBool));
        }

        [ProtoContract]
        internal class TestProtoBufContract
        {
            [ProtoMember(1)]
            public string TestString { get; set; }
            [ProtoMember(2)]
            public byte[] TestBytes { get; set; }
            [ProtoMember(3)]
            public bool TestBool { get; set; }
        }

        [Test]
        public void Serialize_GivenNoContract_ThrowsInvalidOperationException()
        {
            var obj = new Version(1, 2, 3, 4);

            Assert.Throws<InvalidOperationException>(() => obj.SerializeToBytes());
        }
    }
}
