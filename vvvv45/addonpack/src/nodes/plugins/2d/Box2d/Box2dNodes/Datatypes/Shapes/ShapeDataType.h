#pragma once

namespace VVVV 
{
	namespace DataTypes 
	{
		[GuidAttribute("D0FA06A8-E796-46b2-AB21-497009F871BD"),
		InterfaceType(ComInterfaceType::InterfaceIsIUnknown)]
		public interface class IShapeIO: INodeIOBase
		{
			b2Fixture* GetSlice(int index);
		};


		public ref class ShapeDataType : IShapeIO 
		{
			private:
				static Guid^ m_guid;
				vector<b2Fixture*>* m_shapes;
			public:
				ShapeDataType();

				virtual b2Fixture* GetSlice(int index);
				void Reset();
				void Add(b2Fixture* shape);
				int Count();


				static String^ FriendlyName = "Box2d Shape";
				static property Guid^ GUID 
				{
					Guid^ get() 
					{
						return gcnew System::Guid("D0FA06A8-E796-46b2-AB21-497009F871BD");
					}
				}
		};
	}
}
