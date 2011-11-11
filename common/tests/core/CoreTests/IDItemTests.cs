using System;
using NUnit.Framework;
using VVVV.Core;
using VVVV.Core.Collections;
using VVVV.Core.Model;

namespace CoreTests
{
    [TestFixture]
    public class IDItemTests
    {
        [Test]
        public void TestContainerRooting()
        {
            var rootContainer = new IDContainer("RootContainer", true);
            var subContainer = new IDContainer("SubContainer");
            var idItem = new IDItem("Item1");
            
            subContainer.Add(idItem);
            
            Assert.AreEqual(false, idItem.IsRooted);
            
            rootContainer.Add(subContainer);
            Assert.AreEqual(true, idItem.IsRooted);
        }
        
        [Test]
        public void TestCollectionRooting()
        {
            var rootContainer = new IDContainer("RootContainer", true);
            var subContainer1 = new EditableIDList<IIDItem>("SubContainer1");
            var subContainer2 = new EditableIDList<IIDItem>("SubContainer2");
            var idItem = new IDItem("Item1");
            
            subContainer1.Add(subContainer2);
            subContainer2.Add(idItem);
            
            Assert.AreEqual(false, idItem.IsRooted);
            
            rootContainer.Add(subContainer1);
            Assert.AreEqual(true, idItem.IsRooted);
            
            subContainer1.Remove(subContainer2);
            Assert.AreEqual(false, idItem.IsRooted);
            
            // Now check that events are fired not too often
            int eventFiredCount = 0;
            idItem.RootingChanged += delegate(object sender, RootingChangedEventArgs args) 
            { 
                eventFiredCount++;
                switch (args.Rooting)
                {
                    case RootingAction.Rooted:
                        Assert.IsNotNull(idItem.Owner);
                        break;
                    case RootingAction.ToBeUnrooted:
                        Assert.IsNotNull(idItem.Owner);
                        break;
                }
            };
            
            subContainer1.Add(subContainer2); // This should trigger a Rooted event
            Assert.AreEqual(1, eventFiredCount, "Rooted event didn't occur.");
            subContainer2.Remove(idItem); // This should trigger a ToBeUnrooted event
            Assert.AreEqual(2, eventFiredCount, "ToBeUnrooted event didn't occur.");
            subContainer1.Remove(subContainer2); // This shouldn't trigger anything
            Assert.AreEqual(2, eventFiredCount, "IdItem did not unsubcribe from event.");
            subContainer2.Add(idItem); // This should also not trigger anything
            Assert.AreEqual(2, eventFiredCount, "RootingChanged event triggered even if not in object graph.");
            subContainer1.Add(subContainer2); // Now this should finally trigger the RootingChanged event.
            Assert.AreEqual(3, eventFiredCount);
        }
    }
}
