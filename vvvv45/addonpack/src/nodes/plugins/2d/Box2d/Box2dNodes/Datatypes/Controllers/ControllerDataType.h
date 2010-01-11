#pragma once

namespace VVVV 
{
	namespace DataTypes 
	{
		[GuidAttribute("2A312EA6-5767-4573-ADB7-1A98968161FE"),
		InterfaceType(ComInterfaceType::InterfaceIsIUnknown)]
		public interface class IControllerIO: INodeIOBase
		{
			b2Controller* GetController();
		};



		public ref class ControllerDataType : IControllerIO 
		{
			private:
				static Guid^ m_guid;
				b2Controller* m_controller;
			public:
				ControllerDataType();

				virtual b2Controller* GetController() { return m_controller; }
				void SetController(b2Controller* ctrl) { m_controller = ctrl; }


				static String^ FriendlyName = "Box2d Controller";
				static property Guid^ GUID 
				{
					Guid^ get() 
					{
						return gcnew System::Guid("2A312EA6-5767-4573-ADB7-1A98968161FE");
					}
				}
		};

	}
}