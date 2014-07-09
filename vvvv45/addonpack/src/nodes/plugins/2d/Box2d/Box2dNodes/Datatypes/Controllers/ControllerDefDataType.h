#pragma once

namespace VVVV 
{
	namespace DataTypes 
	{
		[GuidAttribute("6A531781-24FC-4948-AA97-5DDDFE5A3125"),
		InterfaceType(ComInterfaceType::InterfaceIsIUnknown)]
		public interface class IControllerDefIO: INodeIOBase
		{
			b2ControllerDef* GetController();
		};

		public ref class ControllerDefDataType : IControllerDefIO
		{
			private:
				static Guid^ FGuid;
				b2ControllerDef* m_controller;

			public:
				ControllerDefDataType(void);
				
				static String^ FriendlyName = "Box2d Controller Definition";
				static property Guid^ GUID 
				{
					Guid^ get() 
					{
						if (FGuid == Guid::Empty) 
						{
							FGuid = gcnew System::Guid("6A531781-24FC-4948-AA97-5DDDFE5A3125");
						}
						return FGuid;
					}
				}

				virtual b2ControllerDef* GetController()
				{
					return this->m_controller;
				}

				virtual void SetController(b2ControllerDef* ctrl)
				{
					this->m_controller = ctrl;
				}
		};
	
	}
}



