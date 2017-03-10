using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using LibP2P.Utilities.Extensions;
using NUnit.Framework;

namespace LibP2P.Utilities.Tests
{
    [TestFixture]
    public class ArrayTests
    {
        [Test]
        public void Slice_GivenValidRange_ReturnsCorrectSlice()
        {
            var array = new int[] {0,1,2,3,4,5,6,7,8,9};
            var sliced = array.Slice(3, 3);

            Assert.That(sliced, Is.EqualTo(new int[] { 3, 4, 5 }));
        }

        [Test]
        public void Slice_GivenOnlyOffset_ReturnsCorrectSlice()
        {
            var array = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var sliced = array.Slice(6);

            Assert.That(sliced, Is.EqualTo(new int[] { 6, 7, 8, 9 }));
        }

        [Test]
        public void Slice_GivenInvalidOffset_ThrowsArgumentOutOfRangeException()
        {
            var array = new int[] {0, 1, 2};

            Assert.Throws<ArgumentOutOfRangeException>(() => array.Slice(4));
        }

        [Test]
        public void Slice_GivenZeroOrNegativeCount_ThrowsArgumentOutOfRangeException()
        {
            var array = new int[] {0, 1, 2};

            Assert.Throws<ArgumentOutOfRangeException>(() => array.Slice(0, -1));
        }

        [Test]
        public void Slice_GivenCountLargerThanArray_ReturnsValidSlice()
        {
            var array = new int[] {0, 1, 2};
            var sliced = array.Slice(1, 3);

            Assert.That(sliced, Is.EqualTo(new int[] {1, 2}));
        }

        [Test]
        public void Append_GivenOneItem_ReturnsTwoItems()
        {
            var one = new int[] {0};
            var two = one.Append(1);

            Assert.That(two, Is.EqualTo(new int[] {0,1}));
        }

        [Test]
        public void Append_GivenFiveItems_ReturnsSixItems()
        {
            var one = new int[] { 0 };
            var six = one.Append(1,2,3,4,5);

            Assert.That(six, Is.EqualTo(new int[] { 0, 1, 2, 3, 4, 5 }));
        }

        [Test]
        public void Copy_GivenValidRange_ReturnsActualItemsCopied()
        {
            var array = new int[] {0, 1, 2, 3};
            var copy = new int[2];
            var copied = array.Copy(copy);

            Assert.That(copied, Is.EqualTo(2));
            Assert.That(copy, Is.EqualTo(new int[] {0,1}));
        }
    }
}
