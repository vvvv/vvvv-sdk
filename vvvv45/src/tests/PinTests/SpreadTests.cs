using System;
using System.Linq;
using NUnit.Framework;
using VVVV.PluginInterfaces.V2;

namespace PinTests
{
    [TestFixture]
    public class SpreadTests
    {
        static int[] sampleData = new int[] { 3, 4, 51, -343, 6453, 0 };
        
        [Test]
        public void TestSingleAdd()
        {
            var spread = new Spread<int>(0);
            spread.Add(10);
            Assert.AreEqual(1, spread.SliceCount);
            Assert.AreEqual(10, spread[0]);
        }
        
        [Test]
        public void TestMultipleAdd()
        {
            var spread = new Spread<int>(0);
            for (int i = 0; i < sampleData.Length; i++)
            {
                spread.Add(sampleData[i]);
            }
            
            Assert.AreEqual(sampleData.Length, spread.SliceCount);
            for (int i = 0; i < sampleData.Length; i++)
            {
                Assert.AreEqual(sampleData[i], spread[i]);
            }
        }
        
        [Test]
        public void TestAddRangeFromArray()
        {
            var spread = new Spread<int>(0);
            spread.AddRange(sampleData);
            
            Assert.AreEqual(sampleData.Length, spread.SliceCount);
            for (int i = 0; i < sampleData.Length; i++)
            {
                Assert.AreEqual(sampleData[i], spread[i]);
            }
        }
        
        [Test]
        public void TestAddRangeFromList()
        {
            var list = sampleData.ToList();
            var spread = new Spread<int>(0);
            spread.AddRange(list);
            
            Assert.AreEqual(list.Count, spread.SliceCount);
            for (int i = 0; i < list.Count; i++)
            {
                Assert.AreEqual(list[i], spread[i]);
            }
        }
        
        [Test]
        public void TestRemoveAt()
        {
            var spread = new Spread<int>(0);
            Assert.DoesNotThrow(() => spread.RemoveAt(19));
            Assert.AreEqual(0, spread.SliceCount);
            
            spread.AssignFrom(sampleData);
            
            // Remove last element
            Assert.DoesNotThrow(() => spread.RemoveAt(-1));
            Assert.AreEqual(sampleData.Length - 1, spread.SliceCount);
            Assert.AreEqual(sampleData[sampleData.Length - 2], spread[-1]);
        }
        
        [Test]
        public void TestRemove()
        {
            var spread = new Spread<int>(0);
            spread.AssignFrom(sampleData);
            
            Assert.IsFalse(spread.Remove(1999999));
            Assert.AreEqual(sampleData.Length, spread.SliceCount);
            
            Assert.IsTrue(spread.Remove(sampleData[3]));
            Assert.AreEqual(sampleData.Length - 1, spread.SliceCount);
        }
        
        [Test]
        public void TestRemoveAll()
        {
            var spread = new Spread<int>(0);
            spread.AssignFrom(sampleData);
            
            Assert.AreEqual(2, spread.RemoveAll((slice) => slice > 10));
            Assert.AreEqual(sampleData.Length - 2, spread.SliceCount);
            for (int i = 0; i < spread.SliceCount; i++)
                Assert.LessOrEqual(spread[i], 10);
        }
    }
}
