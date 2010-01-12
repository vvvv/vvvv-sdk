#pragma once

#include "../../DataTypes/BodyDataType.h"
#include "../../DataTypes/JointDataType.h"

using namespace VVVV::DataTypes;

namespace VVVV 
{
	namespace Nodes 
	{
		public ref class GetJointDetailsNode : IPlugin,IPluginConnections
		{
		public:
			GetJointDetailsNode(void);

			virtual void SetPluginHost(IPluginHost^ Host);
			virtual void Configurate(IPluginConfig^ Input);
			virtual void Evaluate(int SpreadMax);
			virtual void ConnectPin(IPluginIO^ Pin);
			virtual void DisconnectPin(IPluginIO^ Pin);
			
			virtual property bool AutoEvaluate 
			{
				bool get() { return false; }
			}

			static property IPluginInfo^ PluginInfo 
			{
				IPluginInfo^ get() 
					{
						//IPluginInfo^ Info;
						IPluginInfo^ Info = gcnew VVVV::PluginInterfaces::V1::PluginInfo();
						Info->Name = "GetJointDetails";
						Info->Category = "Box2d";
						Info->Version = "";
						Info->Help = "Get details about a created box2d joint";
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



		private:
			IPluginHost^ FHost;

			INodeIn^ vInJoints;
			JointDataType^ m_joints;


			INodeOut^ vOutBody1;
			BodyDataType^ m_bodies1;
			IValueOut^ vOutPosition1;

			INodeOut^ vOutBody2;
			BodyDataType^ m_bodies2;
			IValueOut^ vOutPosition2;

			IStringOut^ vOutType;

			IStringOut^ vOutCustom;
			IValueOut^ vOutId;

		};
	}
}

