#pragma once
#include "../DataTypes/WorldDataType.h"
#include "../DataTypes/Shapes/ShapeDataType.h"
#include "../DataTypes/BodyDataType.h"

using namespace VVVV::DataTypes;

namespace VVVV 
{
	namespace Nodes 
	{
		ref class Box2dAABBQueryNode : IPlugin,IPluginConnections
		{
		public:
			Box2dAABBQueryNode(void);
			
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
						Info->Name = "AABBQuery";
						Info->Category = "Box2d";
						Info->Version = "";
						Info->Help = "Get all shapes from an aabb query";
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

			INodeIn^ vInWorld;
			WorldDataType^ m_world;

			IValueIn^ vInLowerBound;
			IValueIn^ vInUpperBound;
			IValueIn^ vInDoQuery;

			IValueOut^ vOutQueryIndex;

			INodeOut^ vOutShapes;
			ShapeDataType^ mShapes;
			IValueOut^ vOutShapeId;
			
			INodeOut^ vOutBodies;
			BodyDataType^ mBodies;
			IValueOut^ vOutBodyId;
			
		};
	}
}
