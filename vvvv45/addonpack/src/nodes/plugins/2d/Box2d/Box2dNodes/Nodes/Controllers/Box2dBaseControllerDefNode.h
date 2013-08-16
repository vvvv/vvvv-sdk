#pragma once

#include "../../DataTypes/WorldDataType.h"
#include "../../DataTypes/Controllers/ControllerDataType.h"

using namespace VVVV::DataTypes;

namespace VVVV 
{
	namespace Nodes 
	{
		public ref class Box2dBaseControllerDefNode
		{
		public:
			Box2dBaseControllerDefNode(void);
			~Box2dBaseControllerDefNode();

			virtual void SetPluginHost(IPluginHost^ Host);
			virtual void Configurate(IPluginConfig^ Input) {}
			virtual void ConnectPin(IPluginIO^ Pin);
			virtual void DisconnectPin(IPluginIO^ Pin);
			virtual void Evaluate(int SpreadMax);

			virtual property bool AutoEvaluate 
			{
				bool get() { return true; }
			}
		protected:
			virtual void OnPluginHostSet() abstract;
			virtual void OnEvaluate(int SpreadMax, bool reset) abstract;
		

			IPluginHost^ FHost;
			INodeIn^ vInWorld;
			WorldDataType^ m_world;
			IValueIn^ vInClear;

			ControllerDataType^ m_controller;
			INodeOut^ vOutController;
			std::vector<b2Controller*>* ctrl;
			//IValueOut^ vOutBodyCount;
		};


	}
}
