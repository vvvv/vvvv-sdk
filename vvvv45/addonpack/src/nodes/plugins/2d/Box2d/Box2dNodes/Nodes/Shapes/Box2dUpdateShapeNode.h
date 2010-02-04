#pragma once
#include "../../DataTypes/Shapes/ShapeDataType.h"

using namespace VVVV::DataTypes;

namespace VVVV 
{
namespace Nodes 
{
	public ref class Box2dUpdateShapeNode : IPlugin,IPluginConnections
	{
	public:
		Box2dUpdateShapeNode(void);

		static property IPluginInfo^ PluginInfo 
			{
				IPluginInfo^ get() 
				{
					//IPluginInfo^ Info;
					IPluginInfo^ Info = gcnew VVVV::PluginInterfaces::V1::PluginInfo();
					Info->Name = "UpdateShape";
					Info->Category = "Box2d";
					Info->Version = "";
					Info->Help = "Update box2d shape";
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

		INodeIn^ vInShapes;
		ShapeDataType^ m_shapes;

		IValueIn^ vInDensity;
		IValueIn^ vInSetDensity;

		IValueIn^ vInFriction;
		IValueIn^ vInSetFriction;

		IValueIn^ vInRestitution;
		IValueIn^ vInSetRestitution;

		IValueIn^ vInGroup;
		IValueIn^ vInSetGroup;

		IStringIn^ vInCustom;
		IValueIn^ vInSetCustom;

	};
}
}
