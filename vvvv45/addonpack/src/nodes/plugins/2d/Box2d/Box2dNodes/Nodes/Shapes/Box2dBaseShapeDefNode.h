#pragma once
#include "../../DataTypes/Shapes/ShapeDefDataType.h"

using namespace VVVV::DataTypes;

namespace VVVV 
{
	namespace Nodes 
	{
		ref class Box2dBaseShapeDefNode
		{
		public:
			Box2dBaseShapeDefNode(void);

			virtual void SetPluginHost(IPluginHost^ Host);
			virtual void Configurate(IPluginConfig^ Input);

			virtual void Evaluate(int SpreadMax) abstract;
			
			virtual property bool AutoEvaluate 
			{
				bool get() { return false; }
			}
	
		protected:
			IPluginHost^ FHost;
						//Details
			IValueIn^ vInDensity;
			IValueIn^ vInFriction;
			IValueIn^ vInRestitution;

			INodeOut^ vOutShapes;

			ShapeDefDataType^ m_shapes;

			virtual void OnPluginHostSet() abstract;

		private:
			

		};
	}
}
