#include "StdAfx.h"
#include "Box2dCVelDefNode.h"

namespace VVVV 
{
	namespace Nodes 
	{
		Box2dCVelDefNode::Box2dCVelDefNode(void)
		{
			this->m_controller = gcnew ControllerDataType();
			ctrldef = new b2ConstantForceControllerDef();
		}

		void Box2dCVelDefNode::OnEvaluate(int SpreadMax, bool reset)
		{
			if (this->vInVelocity->PinIsChanged
				|| reset)
			{
				double x,y;
				this->vInVelocity->GetValue2D(0, x,y);
				ctrldef->F.x = x;
				ctrldef->F.y = y;

				if (reset)
				{
					this->ctrl = this->m_world->GetWorld()->CreateController(this->ctrldef);
				}
				else
				{
					b2ConstantForceController* ac = (b2ConstantForceController*) this->ctrl;
					ac->F.x = x;
					ac->F.y = y;
				}
			}
		}
		
		void Box2dCVelDefNode::OnPluginHostSet()
		{
			this->FHost->CreateValueInput("Force",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInVelocity);
			this->vInVelocity->SetSubType2D(Double::MinValue,Double::MaxValue,0.01,0.0,0.0,false,false,false);
		}

	}
}
