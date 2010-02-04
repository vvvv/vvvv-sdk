#pragma once

#include "../../DataTypes/Controllers/ControllerDefDataType.h"
#include "Box2dBaseControllerDefNode.h"

using namespace VVVV::DataTypes;

namespace VVVV 
{
	namespace Nodes 
	{
		public ref class Box2dBuoyancyControllerDefNode : Box2dBaseControllerDefNode,IPlugin,IPluginConnections,public IDisposable
		{
		public:
			Box2dBuoyancyControllerDefNode(void);

			static property IPluginInfo^ PluginInfo 
				{
					IPluginInfo^ get() 
					{
						//IPluginInfo^ Info;
						IPluginInfo^ Info = gcnew VVVV::PluginInterfaces::V1::PluginInfo();
						Info->Name = "Buoyancy";
						Info->Category = "Box2d";
						Info->Version = "Controller";
						Info->Help = "Box2d Constant Force controller";
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

				
		protected:
			virtual void OnPluginHostSet() override;
			virtual void OnEvaluate(int SpreadMax, bool reset) override;

		private:
			IValueIn^ vInNormal;
			IValueIn^ vInOffset;
			IValueIn^ vInDensity;
			IValueIn^ vInLinearDrag;
			IValueIn^ vInAngularDrag;
		};
	}

}