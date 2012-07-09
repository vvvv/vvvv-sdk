#pragma once

namespace VVVV 
{
	namespace DataTypes
	{
		[GuidAttribute("D2E73B5C-22DA-449f-A6FB-4117A5761307"),
		InterfaceType(ComInterfaceType::InterfaceIsIUnknown)]
		public interface class IJointIO: INodeIOBase
		{
			b2Joint* GetSlice(int index);
		};


		public ref class JointDataType : IJointIO 
		{
			private:
				static Guid^ m_guid;
				vector<b2Joint*>* m_joints;
			public:
				JointDataType();

				virtual b2Joint* GetSlice(int index);
				void Reset();
				void Add(b2Joint* body);
				int Size();

				static String^ FriendlyName = "Box2d Joint";
				static property Guid^ GUID 
				{
					Guid^ get() 
					{
						return gcnew System::Guid("D2E73B5C-22DA-449f-A6FB-4117A5761307");
					}
				}
		};
	}
}