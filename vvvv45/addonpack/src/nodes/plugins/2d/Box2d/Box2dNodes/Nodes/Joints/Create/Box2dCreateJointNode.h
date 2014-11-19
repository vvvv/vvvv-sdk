#pragma once
#include "../../../DataTypes/BodyDataType.h"
#include "../../../DataTypes/WorldDataType.h"
#include "../../../Internals/Data/JointCustomData.h"

using namespace VVVV::DataTypes;

namespace VVVV 
{
	namespace Nodes 
	{
		ref class Box2dCreateJointNode
		{
		public:
			Box2dCreateJointNode(void);

			virtual void SetPluginHost(IPluginHost^ Host);
			virtual void Configurate(IPluginConfig^ Input);
			virtual void ConnectPin(IPluginIO^ Pin);
			virtual void DisconnectPin(IPluginIO^ Pin);

			virtual void Evaluate(int SpreadMax) abstract;
			
			virtual property bool AutoEvaluate 
			{
				bool get() { return true; }
			}
	
		protected:
			IPluginHost^ FHost;

			INodeIn^ vInWorld;
			WorldDataType^ mWorld;

			INodeIn^ vInBody1;
			BodyDataType^ m_body1;
			//GroundDataType^ m_ground1;
			//bool isbody;

			INodeIn^ vInBody2;
			BodyDataType^ m_body2;

			IValueIn^ vInCollideConnected;

			IStringIn^ vInCustom;
			IValueIn^ vInDoCreate;

			virtual void OnPluginHostSet() abstract;
			virtual bool ForceBodyOneGround() abstract;

		private:
			

		};
	}
}
