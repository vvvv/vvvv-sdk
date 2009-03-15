#pragma once


namespace VVVV 
{
	namespace DataTypes 
	{
		[GuidAttribute("2104667E-DEF9-402c-BC1A-CF94616B62FC"),
		InterfaceType(ComInterfaceType::InterfaceIsIUnknown)]
		public interface class IShapeDefIO: INodeIOBase
		{
			b2ShapeDef* GetSlice(int index);
		};


		public ref class ShapeDefDataType : IShapeDefIO 
		{
			private:
				vector<b2ShapeDef*>* m_shapes;
			public:

				ShapeDefDataType();
				virtual b2ShapeDef* GetSlice(int index);
				void Reset();
				b2CircleDef* AddCircle();
				b2PolygonDef* AddPolygon();

				static String^ FriendlyName = "Box2d Shape Definition";
				static property Guid^ GUID 
				{
					Guid^ get() 
					{
						return gcnew System::Guid("2104667E-DEF9-402c-BC1A-CF94616B62FC");
					}
				}
		};
	}
}
