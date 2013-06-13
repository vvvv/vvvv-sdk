#pragma once
#include "../../../DataTypes/BodyDataType.h"
#include "Box2dCreateJointNode.h"

using namespace VVVV::DataTypes;

namespace VVVV 
{
	namespace Nodes 
	{
		ref class Box2dCreatePulleyJointNode : Box2dCreateJointNode, IPlugin, IPluginConnections
		{
		public:
			static property IPluginInfo^ PluginInfo 
				{
					IPluginInfo^ get() 
					{
						//IPluginInfo^ Info;
						IPluginInfo^ Info = gcnew VVVV::PluginInterfaces::V1::PluginInfo();
						Info->Name = "CreatePulleyJoint";
						Info->Category = "Box2d";
						Info->Version = "";
						Info->Help = "Creates a distance joint between 2 bodies";
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





			Box2dCreatePulleyJointNode(void);
			virtual void Evaluate(int SpreadMax) override;
			virtual bool ForceBodyOneGround() override { return false; };
		protected:
			virtual void OnPluginHostSet() override;
		private:
			IValueIn^ vInPosition1;
			IValueIn^ vInAnchor1;
			IValueIn^ vInMaxLength1;
			IValueIn^ vInPosition2;
			IValueIn^ vInAnchor2;
			IValueIn^ vInMaxLength2;
			IValueIn^ vInRatio;
			
		};
	}
}

