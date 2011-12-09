#pragma once
#include "Box2dBaseShapeDefNode.h"

namespace VVVV 
{
	namespace Nodes 
	{
		ref class Box2dCircleShapeDefNode : Box2dBaseShapeDefNode,IPlugin
		{
		public:
			Box2dCircleShapeDefNode(void);

			static property IPluginInfo^ PluginInfo 
				{
					IPluginInfo^ get() 
					{
						//IPluginInfo^ Info;
						IPluginInfo^ Info = gcnew VVVV::PluginInterfaces::V1::PluginInfo();
						Info->Name = "Circle";
						Info->Category = "Box2d";
						Info->Version = "";
						Info->Help = "Box2d Circle Shape definition";
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





			virtual void Evaluate(int SpreadMax) override;
		
		protected:
			virtual void OnPluginHostSet() override;

		private:
			IValueIn^ vInLocalPosition;
			IValueIn^ vInRadius;
		};
	}
}