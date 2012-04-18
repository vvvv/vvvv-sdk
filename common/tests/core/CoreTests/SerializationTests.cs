// Not sure why, but these tests seem to fails since we switched to .NET 4.
// Getting a BadImageFormatException when resolving ISerializer.
// After some google researcb i couldn't come up with anything useful.
// The exception is inside of Unity/ObjectBuilder calling _CompileMethod.

//using System;
//using System.IO;
//using System.Xml.Linq;
//using NUnit.Framework;
//using Rhino.Mocks;
//using Rhino.Mocks.Constraints;
//using VVVV.Core;
//using VVVV.Core.Collections;
//using VVVV.Core.Commands;
//using VVVV.Core.Model;
//using VVVV.Core.Serialization;
//
//namespace CoreTests
//{
//    [TestFixture]
//    public class SerializationTests
//    {
//        private readonly Serializer FSerializer = new Serializer();
//        private MockRepository FMocks;
//        
//        [SetUp]
//        public void Init()
//        {
//            FMocks = new MockRepository();
//        }
//        
//        [TearDown]
//        public void Cleanup()
//        {
//        }
//        
//        [Test]
//        public void TestAddCommandSerializationNotRooted()
//        {
//            var root = new IDContainer("Root", true);
//            var list = FMocks.DynamicMock<IEditableIDList<IIDItem>>();
//            var item = new TestIDItem("TestItem");
//            
//            Shell.Instance.Root = root;
//            
//            using(FMocks.Record())
//            {
//                SetupResult.For(list.Name).Return("Items");
//                SetupResult.For(list.Owner).PropertyBehavior();
//                SetupResult.For(list.Mapper).Return(new ModelMapper(list, new MappingRegistry()));
//                Expect.Call(() => ((IEditableCollection) list).Add(null)).Constraints(Property.Value("Name", item.Name));
//            }
//            
//            using(FMocks.Playback())
//            {
//                root.Add(list);
//                
//                var command = Command.Add(list, item);
//                
//                var xElement = FSerializer.Serialize(command);
//                
//                Assert.IsNull(xElement.Attribute("Item"), "Item is not rooted and should therefor not being referenced by its ID in AddCommand.");
//                
//                var command2 = FSerializer.Deserialize<Command>(xElement);
//                command2.Execute();
//            }
//        }
//        
//        [Test]
//        public void TestAddCommandSerializationRooted()
//        {
//            var root = new IDContainer("Root", true);
//            var list = new EditableIDList<IIDItem>("Items");
//            var item = new TestIDItem("TestItem");
//            
//            Shell.Instance.Root = root;
//            
//            root.Add(list);
//            list.Add(item);
//            
//            var command = Command.Add(list, item);
//            
//            var xElement = FSerializer.Serialize(command);
//            
//            Assert.IsNotNull(xElement.Attribute("Item"), "Item is rooted and should therefor being referenced by its ID in AddCommand.");
//            
//            var command2 = FSerializer.Deserialize<Command>(xElement);
//            Assert.AreEqual(item, list[item.Name]);
//        }
//    }
//}
