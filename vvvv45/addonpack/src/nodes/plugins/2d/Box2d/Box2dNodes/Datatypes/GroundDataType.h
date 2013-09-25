#pragma once

namespace VVVV 
{
	namespace DataTypes 
	{
		[GuidAttribute("AA9E5FA2-50B5-4cb8-AE16-4F160CAB3A69"),
		InterfaceType(ComInterfaceType::InterfaceIsIUnknown)]
		public interface class IGroundIO: INodeIOBase
		{
			b2Body* GetGround();
			bool IsValid();
			void SetIsValid(bool value);
			void SetGround(b2Body* world);
		};


		public ref class GroundDataType : IGroundIO 
		{
			private:
				static Guid^ m_guid;
				b2Body* m_ground;
				bool m_isvalid;
			public:
				GroundDataType();

				virtual b2Body* GetGround();
				virtual bool IsValid();
				virtual void SetIsValid(bool value);
				virtual void SetGround(b2Body* ground);


				static String^ FriendlyName = "Box2d Ground";
				static property Guid^ GUID 
				{
					Guid^ get() 
					{
						return gcnew System::Guid("AA9E5FA2-50B5-4cb8-AE16-4F160CAB3A69");
					}
				}
		};
	}
}


