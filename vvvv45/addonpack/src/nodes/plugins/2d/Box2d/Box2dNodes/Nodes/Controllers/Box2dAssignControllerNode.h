#pragma once

#include "../../DataTypes/BodyDataType.h"
#include "../../DataTypes/Controllers/ControllerDataType.h"

using namespace VVVV::DataTypes;

namespace VVVV 
{
	namespace Nodes 
	{
		public ref class Box2dAssignControllerNode : IPlugin,IPluginConnections
		{
		public:
			Box2dAssignControllerNode(void);
			static property IPluginInfo^ PluginInfo 
				{
					IPluginInfo^ get() 
					{
						//IPluginInfo^ Info;
						IPluginInfo^ Info = gcnew VVVV::PluginInterfaces::V1::PluginInfo();
						Info->Name = "AssignController";
						Info->Category = "Box2d";
						Info->Version = "";
						Info->Help = "Assign a controller to a box2d body";
						Info->Bugs = "";
						Info->Credits = "Box2d";
						Info->Warnings = "";
						Info->Author = "vux";
						Info->Tags="Physics,2d,Collision";

						//leave below as is
						System::Diagnostics::StackTrace^ st = gcnew System::Diagnostics::StackTrace(true);
						System::Diagnostics::StackFrame^ sf = st->GetFrame(0);
						System::Reflection::MethodBase^ method = sf->GetMethod();
						Info->Namespace = method->DeclaringType->Namespace;
						Info->Class = method->DeclaringType->Name;
						return Info;
					}
				}





			virtual void SetPluginHost(IPluginHost^ Host);
			virtual void Configurate(IPluginConfig^ Input);
			virtual void ConnectPin(IPluginIO^ Pin);
			virtual void DisconnectPin(IPluginIO^ Pin);

			virtual void Evaluate(int SpreadMax);
			
			virtual property bool AutoEvaluate 
			{
				bool get() { return true; }
			}
		private:
			IPluginHost^ FHost;
			INodeIn^ vInController;
			ControllerDataType^ m_controller;

			INodeIn^ vInBody;
			BodyDataType^ m_body;

			IValueIn^ vInDoCreate;
		};
	}
}
