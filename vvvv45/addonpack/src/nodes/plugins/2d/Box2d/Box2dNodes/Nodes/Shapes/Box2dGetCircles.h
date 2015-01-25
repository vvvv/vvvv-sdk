#pragma once
#include "../../DataTypes/Shapes/ShapeDataType.h"

using namespace VVVV::DataTypes;

namespace VVVV 
{
	namespace Nodes 
	{
		public ref class Box2dGetCircles: IPlugin,IPluginConnections
		{
		public:
			Box2dGetCircles(void);

			static property IPluginInfo^ PluginInfo 
				{
					IPluginInfo^ get() 
					{
						//IPluginInfo^ Info;
						IPluginInfo^ Info = gcnew VVVV::PluginInterfaces::V1::PluginInfo();
						Info->Name = "GetCircles";
						Info->Category = "Box2d";
						Info->Version = "";
						Info->Help = "Box2d Circle Shape";
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
				bool get() { return false; }
			}
		protected:

		private:
			IPluginHost^ FHost;

			INodeIn^ vInShapes;
			IValueIn^ vInLocal;
			ShapeDataType^ m_circles;

			IValueOut^ vOutPosition;
			IValueOut^ vOutRadius;
			IValueOut^ vOutIsSensor;
			IValueOut^ vOutId;
			IStringOut^ vOutCustom;

			bool m_local;
		};
	}
}
