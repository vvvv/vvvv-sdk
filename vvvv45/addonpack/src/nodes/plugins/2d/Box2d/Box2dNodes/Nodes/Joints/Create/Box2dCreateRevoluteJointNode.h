#pragma once
#include "../../../DataTypes/BodyDataType.h"
#include "Box2dCreateJointNode.h"

using namespace VVVV::DataTypes;

namespace VVVV 
{
	namespace Nodes 
	{
		ref class Box2dCreateRevoluteJointNode : Box2dCreateJointNode, IPlugin, IPluginConnections
		{
		public:
			static property IPluginInfo^ PluginInfo 
				{
					IPluginInfo^ get() 
					{
						//IPluginInfo^ Info;
						IPluginInfo^ Info = gcnew VVVV::PluginInterfaces::V1::PluginInfo();
						Info->Name = "CreateRevoluteJoint";
						Info->Category = "Box2d";
						Info->Version = "";
						Info->Help = "Creates a revolute joint between 2 bodies";
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





			Box2dCreateRevoluteJointNode(void);
			virtual void Evaluate(int SpreadMax) override;
			virtual bool ForceBodyOneGround() override { return false; };
		protected:
			virtual void OnPluginHostSet() override;
		private:
			IValueIn^ vInPosition;
			IValueIn^ vInMinAngle;
			IValueIn^ vInMaxAngle;
			IValueIn^ vInEnableLimit;
			IValueIn^ vInMaxMotorTorque;
			IValueIn^ vInMotorSpeed;
			IValueIn^ vInEnableMotor;
		};
	}
}

