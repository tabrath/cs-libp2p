using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibP2P.Utilities.Extensions;
using NUnit.Framework;

namespace LibP2P.Utilities.Tests
{
    [TestFixture]
    public class TupleTests
    {
        [Test]
        public void Swap_GivenTwoValues_ReturnsSwappedTuple()
        {
            var tuple = Tuple.Create(1, 2);
            
            Assert.That(tuple.Swap(), Is.EqualTo(Tuple.Create(2, 1)));
        }
    }
}
