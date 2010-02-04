#pragma once
#include "../../../DataTypes/Shapes/ShapeDefDataType.h"
#include "../../../Internals/Data/ShapeCustomData.h"

using namespace VVVV::DataTypes;

namespace VVVV 
{
	namespace Nodes 
	{
		public ref class Box2dBaseShapeDefNode
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
			IValueIn^ vInIsSensor;
			IValueIn^ vInGroupIndex;
			IStringIn^ vInCustom;

			INodeOut^ vOutShapes;

			ShapeDefDataType^ m_shapes;

			virtual void OnPluginHostSet() abstract;

		private:
			

		};
	}
}
