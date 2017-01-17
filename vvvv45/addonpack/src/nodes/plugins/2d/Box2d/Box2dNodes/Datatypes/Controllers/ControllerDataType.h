#pragma once

namespace VVVV 
{
	namespace DataTypes 
	{
		[GuidAttribute("2A312EA6-5767-4573-ADB7-1A98968161FE"),
		InterfaceType(ComInterfaceType::InterfaceIsIUnknown)]
		public interface class IControllerIO: INodeIOBase
		{
			//b2Controller* GetController();
			b2Controller* GetSlice(int index);
		};



		public ref class ControllerDataType : IControllerIO 
		{
			private:
				static Guid^ m_guid;
				vector<b2Controller*>* m_controllers;
			public:
				ControllerDataType();


				virtual b2Controller* GetSlice(int index);
				void Reset();
				void Add(b2Controller* ctrl);
				int Size();


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