using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using VVVV.Core;
using VVVV.Core.Collections;
using VVVV.Core.Collections.Sync;
using VVVV.Core.Model;

namespace CoreTests
{
    [TestFixture]
    public class SynchronizerTests
    {
        static int[] GenerateSampleData(int length)
        {
            var result = new int[length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = i;
            }
            return result;
        }
        
        [Test]
        public void TestEditableCollectionAndViewableCollection()
        {
            var sampleData = GenerateSampleData(10);
            var sourceCollection = new ViewableCollection<int>();
            IEditableCollection<int> targetCollection = new EditableCollection<int>();
            
            var syncer = targetCollection.SyncWith(sourceCollection, (itemB) => itemB);
            sourceCollection.AddRange(sampleData);
            
            CollectionAssert.AreEqual(sourceCollection, targetCollection);
            
            sourceCollection.Remove(sampleData[0]);
            
            CollectionAssert.AreEqual(sourceCollection, targetCollection);
            
            // Make sure a Clear triggers following event chaing: UpdateBegun / Removed ... / Cleared / Updated
            int updateBegunCount = 0;
            sourceCollection.UpdateBegun += delegate(IViewableCollection collection) { updateBegunCount++; };
            int removedCount = 0;
            int removeCount = sourceCollection.Count;
            sourceCollection.Removed += delegate(IViewableCollection<int> collection, int item) { removedCount++; };
            int clearedCount = 0;
            sourceCollection.Cleared += delegate(IViewableCollection<int> collection) { clearedCount++; };
            int updatedCount = 0;
            sourceCollection.Updated += delegate(IViewableCollection collection) { updatedCount++; };
            
            sourceCollection.Clear();
            
            Assert.AreEqual(1, updateBegunCount);
            Assert.AreEqual(1, updatedCount);
            Assert.AreEqual(removeCount, removedCount);
            Assert.AreEqual(1, clearedCount);
            
            CollectionAssert.AreEqual(sourceCollection, targetCollection);
        }
        
        [Test]
        public void TestEditableCollectionAndEditableCollection()
        {
            var sampleData = GenerateSampleData(10);
            var sourceCollection = new EditableCollection<int>();
            IEditableCollection<int> targetCollection = new EditableCollection<int>();
            
            var syncer = targetCollection.SyncWith(sourceCollection, (itemB) => itemB);
            sourceCollection.AddRange(sampleData);
            
            CollectionAssert.AreEqual(sourceCollection, targetCollection);
            
            sourceCollection.Remove(sampleData[0]);
            
            CollectionAssert.AreEqual(sourceCollection, targetCollection);
            
            sourceCollection.Clear();
            
            CollectionAssert.AreEqual(sourceCollection, targetCollection);
        }
        
        [Test]
        public void TestEditableCollectionAndViewableList()
        {
            var sampleData = GenerateSampleData(10);
            var sourceCollection = new ViewableList<int>();
            IEditableCollection<int> targetCollection = new EditableCollection<int>();
            
            var syncer = targetCollection.SyncWith(sourceCollection, (itemB) => itemB);
            sourceCollection.AddRange(sampleData);
            
            CollectionAssert.AreEqual(sourceCollection, targetCollection);
            
            sourceCollection.Remove(sampleData[0]);
            
            CollectionAssert.AreEqual(sourceCollection, targetCollection);
            
            sourceCollection.Clear();
            
            CollectionAssert.AreEqual(sourceCollection, targetCollection);
        }
        
        [Test]
        public void TestNonGenericListAndViewableCollection()
        {
            var sampleData = GenerateSampleData(10);
            var sourceCollection = new ViewableCollection<int>();
            var targetList = new ArrayList();
            
            var syncer = targetList.SyncWith(sourceCollection, (itemB) => itemB);
            sourceCollection.AddRange(sampleData);
            
            CollectionAssert.AreEqual(sourceCollection, targetList);
            
            sourceCollection.Remove(sampleData[0]);
            
            CollectionAssert.AreEqual(sourceCollection, targetList);
            
            sourceCollection.Clear();
            
            CollectionAssert.AreEqual(sourceCollection, targetList);
        }
        
        [Test]
        public void TestGenericListAndViewableCollection()
        {
            var sampleData = GenerateSampleData(10);
            var sourceCollection = new ViewableCollection<int>();
            var targetCollection = new List<int>();
            
            var syncer = targetCollection.SyncWith(sourceCollection, (itemB) => itemB);
            sourceCollection.AddRange(sampleData);
            
            CollectionAssert.AreEqual(sourceCollection, targetCollection);
            
            sourceCollection.Remove(sampleData[0]);
            
            CollectionAssert.AreEqual(sourceCollection, targetCollection);
            
            sourceCollection.Clear();
            
            CollectionAssert.AreEqual(sourceCollection, targetCollection);
        }
        
        [Test]
        public void TestEditableCollectionAndEditableIDList()
        {
            var sampleData = new IIDItem[] { new IDItem("item1"), new IDItem("item2"), new IDItem("item3"), new IDItem("item4"), new IDItem("item5"), new IDItem("item6") };
            var sourceCollection = new EditableIDList<IIDItem>("source");
            var targetCollection = new EditableCollection<IIDItem>();
            
            var syncer = targetCollection.SyncWith(sourceCollection, (itemB) => itemB);
            sourceCollection.AddRange(sampleData);
            
            CollectionAssert.AreEqual(sourceCollection, targetCollection);
            
            sourceCollection.Remove(sampleData[0]);
            
            CollectionAssert.AreEqual(sourceCollection, targetCollection);
            
            sourceCollection.Clear();
            
            CollectionAssert.AreEqual(sourceCollection, targetCollection, "Clear() on EditableIDList failed.");
        }
        
        [Test]
        public void TestBatchUpdate()
        {
            var sampleData = GenerateSampleData(10);
            IEditableCollection<int> sourceCollection = new EditableCollection<int>();
            IEditableCollection<int> targetCollection = new EditableCollection<int>();
            
            var syncer = targetCollection.SyncWith(sourceCollection, (itemB) => itemB);
            
            sourceCollection.BeginUpdate();
            sourceCollection.Add(sampleData[0]);
            sourceCollection.Add(sampleData[1]);
            sourceCollection.Add(sampleData[2]);
            
            CollectionAssert.AreNotEqual(sourceCollection, targetCollection);
            
            sourceCollection.Remove(sampleData[0]);
            sourceCollection.Remove(sampleData[2]);
            
            CollectionAssert.AreNotEqual(sourceCollection, targetCollection);
            
            sourceCollection.EndUpdate();
            
            CollectionAssert.AreEqual(sourceCollection, targetCollection);
        }
        
        [Test]
        public void TestEventsFired()
        {
            bool eventFired = false;
            
            var sampleData = GenerateSampleData(10);
            IEditableCollection<int> sourceCollection = new EditableCollection<int>();
            IEditableCollection<int> targetCollection = new EditableCollection<int>();
            
            var syncer = targetCollection.SyncWith(sourceCollection, (itemB) => itemB);
            syncer.Synced += delegate(object sender, SyncEventArgs<int, int> args) { eventFired = true; };
            
            sourceCollection.Add(sampleData[0]);
            Assert.AreEqual(true, eventFired);
            
            eventFired = false;
            sourceCollection.Remove(sampleData[0]);
            Assert.AreEqual(true, eventFired);
            
            eventFired = false;
            sourceCollection.BeginUpdate();
            sourceCollection.Add(sampleData[0]);
            sourceCollection.Add(sampleData[1]);
            sourceCollection.EndUpdate();
            Assert.AreEqual(true, eventFired);
        }
        
        [Test]
        public void TestSyncerChain()
        {
            var sampleData = GenerateSampleData(10);
            IEditableCollection<int> sourceCollection = new EditableCollection<int>();
            IEditableCollection<int> interCollection = new EditableCollection<int>();
            IEditableCollection<int> targetCollection = new EditableCollection<int>();
            
            var syncer1 = targetCollection.SyncWith(interCollection, (itemB) => itemB);
            var syncer2 = interCollection.SyncWith(sourceCollection, (itemB) => itemB);
            
            sourceCollection.BeginUpdate();
            sourceCollection.Add(sampleData[0]);
            sourceCollection.Add(sampleData[1]);
            sourceCollection.Add(sampleData[2]);
            
            CollectionAssert.AreNotEqual(sourceCollection, targetCollection);
            
            sourceCollection.Remove(sampleData[0]);
            
            CollectionAssert.AreNotEqual(sourceCollection, targetCollection);
            
            bool eventFired = false;
            syncer1.Synced += delegate(object sender, SyncEventArgs<int, int> args) { eventFired = true; };
            
            sourceCollection.EndUpdate();
            
            Assert.AreEqual(true, eventFired);
            CollectionAssert.AreEqual(sourceCollection, targetCollection);
            
            sourceCollection.AddRange(sampleData);
            CollectionAssert.AreEqual(sourceCollection, targetCollection);
            
            sourceCollection.Clear();
            CollectionAssert.AreEqual(sourceCollection, targetCollection);
        }
        
        [Test]
        public void TestCollections()
        {
            var numbers = new EditableCollection<int>();
            var results = new EditableCollection<int>();
            
            using (results.SyncWith(numbers, x => x))
            {
                var viewableResults = results.AsViewableCollection();
                
                // when doing more than one change:
                // use beginupdate/endupdate to reduce syncing events
                numbers.BeginUpdate();
                try
                {
                    numbers.Add(4);
                    numbers.Add(7);
                }
                finally
                {
                    numbers.EndUpdate();
                }

                // Magically results (x) are already added to the Results list
                CollectionAssert.AreEqual(numbers, viewableResults);

                // You can't add a result to the public Results list
                // Results.Add(17);

                // again: change source collection:
                numbers.Add(8);

                // synced results collection is already updated.
                CollectionAssert.AreEqual(numbers, viewableResults);
            }
        }
        
        class MockObject
        {
            public int Order;
            
            public MockObject(int order)
            {
                Order = order;
            }
        }
        
        [Test]
        public void TestObjectLifetime()
        {
            int createCount = 0;
            int destroyCount = 0;
            
            var source = new SortedEditableList<MockObject, int>(mo => mo.Order);
            var target = new EditableCollection<MockObject>();
            
            Func<MockObject, MockObject> createMo = mo =>
            {
                createCount++;
                return new MockObject(mo.Order);
            };
            
            Action<MockObject> destroyMo = mo =>
            {
                destroyCount++;
            };
            
            target.SyncWith(source, createMo, destroyMo);
            
            source.Add(new MockObject(0));
            
            Assert.AreEqual(1, createCount, "Create count");
            Assert.AreEqual(0, destroyCount, "Destroy count");
            
            var mock = new MockObject(-1);
            source.Add(mock);
            source.Add(new MockObject(-2));
            source.Add(new MockObject(10));
            
            Assert.AreEqual(4, createCount, "Create count");
            Assert.AreEqual(0, destroyCount, "Destroy count");
            
            source.Remove(mock);
            
            Assert.AreEqual(4, createCount, "Create count");
            Assert.AreEqual(1, destroyCount, "Destroy count");
            
            Assert.AreEqual(source.Count, target.Count);
            
            source.Clear();
            
            Assert.AreEqual(source.Count, target.Count);
            Assert.AreEqual(createCount, destroyCount, "Create count doesn't match destroy count after source.Clear()");
        }
    }
}
