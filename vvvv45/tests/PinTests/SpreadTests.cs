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
            var spread = new Spread<int>(1);
            Assert.DoesNotThrow(() => spread.RemoveAt(19));
            Assert.AreEqual(0, spread.SliceCount);
            
            spread.AssignFrom(sampleData);
            
            // Remove last element
            Assert.DoesNotThrow(() => spread.RemoveAt(-1));
            Assert.AreEqual(sampleData.Length - 1, spread.SliceCount);
            Assert.AreEqual(sampleData[sampleData.Length - 2], spread[-1]);
        }
        
        [Test]
        public void TestRemoveRange()
        {
            var spread = new Spread<int>(0);
            spread.AssignFrom(sampleData);
            
            var list = spread.ToList();
            
            int oldSliceCount = spread.SliceCount;
            spread.RemoveRange(0, 3);
            list.RemoveRange(0, 3);
            Assert.AreEqual(oldSliceCount - 3, spread.SliceCount, "SliceCount after RemoveRange at index 0 doesn't match.");
            
            for (int i = 0; i < spread.SliceCount; i++)
            {
                Assert.AreEqual(list[i], spread[i], "RemoveRange at index 0 failed.");
            }
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
        
        [Test]
        public void TestGetRange()
        {
            var spread = new Spread<int>(0);
            spread.AssignFrom(sampleData);
            
            int index = 1;
            int count = 3;
            var subSpread = spread.GetRange(index, count);
            
            Assert.AreEqual(subSpread.SliceCount, count);
            for (int i = 0; i < count; i++)
            {
                Assert.AreEqual(subSpread[i], spread[i + index]);
            }
            
            
        }
        
        [Test]
        public void TestGetRangeOutOfBounds()
        {
            var spread = new Spread<int>(0);
            spread.AssignFrom(sampleData);
            
            // Go out of bounds
            int index = 15;
            int count = 21;
            var subSpread = spread.GetRange(index, count);
            
            Assert.AreEqual(subSpread.SliceCount, count);
            for (int i = 0; i < count; i++)
            {
                Assert.AreEqual(subSpread[i], spread[i + index]);
            }
        }
        
        [Test]
        public void TestIndexAt()
        {
            var spread = new Spread<int>(0);
            spread.AssignFrom(sampleData);
            
            Assert.AreEqual(3, spread.IndexOf(sampleData[3]));
            Assert.AreEqual(5, spread.IndexOf(sampleData[5]));
        }
        
        [Test]
        public void TestInsert()
        {
            var spread = new Spread<int>(0);
            spread.AssignFrom(sampleData);
            
            var list = spread.ToList();
            
            int oldSliceCount = spread.SliceCount;
            spread.Insert(0, 12);
            list.Insert(0, 12);
            Assert.AreEqual(oldSliceCount + 1, spread.SliceCount, "SliceCount after insert at index 0 doesn't match.");
            
            for (int i = 0; i < spread.SliceCount; i++)
            {
                Assert.AreEqual(list[i], spread[i], "Insert at index 0 failed.");
            }
            
            oldSliceCount = spread.SliceCount;
            spread.Insert(oldSliceCount, 13);
            list.Insert(oldSliceCount, 13);
            Assert.AreEqual(oldSliceCount + 1, spread.SliceCount, "SliceCount after insert at end of spread doesn't match.");
            
            for (int i = 0; i < spread.SliceCount; i++)
            {
                Assert.AreEqual(list[i], spread[i], "Insert at end of spread failed.");
            }
        }
        
        [Test]
        public void TestInsertRange()
        {
            var spread = new Spread<int>(0);
            spread.AssignFrom(sampleData);
            
            var list = spread.ToList();
            var listToInsert = list.GetRange(2, 4);
            
            int oldSliceCount = spread.SliceCount;
            spread.InsertRange(0, listToInsert);
            list.InsertRange(0, listToInsert);
            Assert.AreEqual(oldSliceCount + listToInsert.Count, spread.SliceCount, "SliceCount after InsertRange at index 0 doesn't match.");
            
            for (int i = 0; i < spread.SliceCount; i++)
            {
                Assert.AreEqual(list[i], spread[i], "InsertRange at index 0 failed.");
            }
            
            oldSliceCount = spread.SliceCount;
            spread.InsertRange(oldSliceCount, listToInsert);
            list.InsertRange(oldSliceCount, listToInsert);
            Assert.AreEqual(oldSliceCount + listToInsert.Count, spread.SliceCount, "SliceCount after InsertRange at end of spread doesn't match.");
            
            for (int i = 0; i < spread.SliceCount; i++)
            {
                Assert.AreEqual(list[i], spread[i], "InsertRange at end of spread failed.");
            }
        }
    }
}
