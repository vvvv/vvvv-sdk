#pragma once
#include "../../DataTypes/BodyDataType.h"
#include "../../DataTypes/Shapes/ShapeDataType.h"

using namespace VVVV::DataTypes;

namespace VVVV 
{
	namespace Nodes 
	{
		public ref class GetBodyDetailsNode : IPlugin,IPluginConnections
		{
		public:
			GetBodyDetailsNode(void);

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
						Info->Name = "GetBodyDetails";
						Info->Category = "Box2d";
						Info->Version = "";
						Info->Help = "Get details about a created box2d body";
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

			INodeIn^ vInBodies;
			BodyDataType^ m_bodies;

			IValueOut^ vOutPosition;
			IValueOut^ vOutRotation;
			IValueOut^ vOutVelocity;
			IValueOut^ vOutIsDynamic;
			IValueOut^ vOutIsSleeping;
			IValueOut^ vOutMass;
			IValueOut^ vOutInertia;
			IStringOut^ vOutCustom;
			INodeOut^ vOutShapes;
			IStringOut^ vOutShapeType;
			IValueOut^ vOutShapeCount;

			IValueOut^ vOutId;
			ShapeDataType^ m_shapes;

		};
	}
}
