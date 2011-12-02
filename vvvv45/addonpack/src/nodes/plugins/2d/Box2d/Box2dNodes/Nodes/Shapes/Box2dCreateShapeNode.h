#pragma once

#include "../../DataTypes/BodyDataType.h"
#include "../../DataTypes/Shapes/ShapeDefDataType.h"
#include "../../Internals/Data/ShapeCustomData.h"

using namespace VVVV::DataTypes;

namespace VVVV 
{
	namespace Nodes 
	{
		public ref class Box2dCreateShapeNode : IPlugin,IPluginConnections
		{
		public:
			Box2dCreateShapeNode(void);

			static property IPluginInfo^ PluginInfo 
				{
					IPluginInfo^ get() 
					{
						//IPluginInfo^ Info;
						IPluginInfo^ Info = gcnew VVVV::PluginInterfaces::V1::PluginInfo();
						Info->Name = "CreateShape";
						Info->Category = "Box2d";
						Info->Version = "";
						Info->Help = "Creates box2d shape in an existing body";
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
			virtual void Evaluate(int SpreadMax);
			virtual void ConnectPin(IPluginIO^ Pin);
			virtual void DisconnectPin(IPluginIO^ Pin);

			virtual property bool AutoEvaluate 
			{
				bool get() { return true; }
			}
		private:
			IPluginHost^ FHost;

			INodeIn^ vInWorld;
			INodeIn^ vInBodies;
			INodeIn^ vInShapes;
			IValueIn^ vInShapeCount;
			IValueIn^ vInDoCreate;
			
			WorldDataType^ mWorld;
			ShapeDefDataType^ mShapes;
			BodyDataType^ mBodies;
		};

	}
}