#include "StdAfx.h"
#include "Box2dConstantAccDefNode.h"

namespace VVVV 
{
	namespace Nodes 
	{
		Box2dConstantAccDefNode::Box2dConstantAccDefNode(void)
		{
			this->m_controller = gcnew ControllerDataType();
			this->ctrldef = new b2ConstantAccelControllerDef();
		}

		void Box2dConstantAccDefNode::OnEvaluate(int SpreadMax, bool reset)
		{
			if (this->vInAcceleration->PinIsChanged
			|| reset)
			{
				double x,y;
				this->vInAcceleration->GetValue2D(0, x,y);
				ctrldef->A.x = x;
				ctrldef->A.y = y;

				if (reset)
				{
					this->ctrl = this->m_world->GetWorld()->CreateController(this->ctrldef);
				}
				else
				{
					b2ConstantAccelController* ac = (b2ConstantAccelController*) this->ctrl;
					ac->A.x = x;
					ac->A.y = y;
				}
			}
		}
		
		void Box2dConstantAccDefNode::OnPluginHostSet()
		{
			this->FHost->CreateValueInput("Acceleration",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInAcceleration);
			this->vInAcceleration->SetSubType2D(Double::MinValue,Double::MaxValue,0.01,0.0,0.0,false,false,false);
		}

	}
}