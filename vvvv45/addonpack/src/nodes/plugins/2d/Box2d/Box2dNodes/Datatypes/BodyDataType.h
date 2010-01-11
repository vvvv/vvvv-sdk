#pragma once

#include "WorldDataType.h"

namespace VVVV 
{
	namespace DataTypes 
	{
		[GuidAttribute("69EEFDBE-50BD-42b9-A77F-111D6D8F375B"),
		InterfaceType(ComInterfaceType::InterfaceIsIUnknown)]
		public interface class IBodyIO: INodeIOBase
		{
			b2Body* GetSlice(int index);
		};


		public ref class BodyDataType : IBodyIO 
		{
			private:
				static Guid^ m_guid;
				vector<b2Body*>* m_bodies;
			public:
				BodyDataType();

				virtual b2Body* GetSlice(int index);
				void Reset();
				void Add(b2Body* body);
				int Size();

				static String^ FriendlyName = "Box2d Body";
				static property Guid^ GUID 
				{
					Guid^ get() 
					{
						return gcnew System::Guid("69EEFDBE-50BD-42b9-A77F-111D6D8F375B");
					}
				}
		};
	}
}

