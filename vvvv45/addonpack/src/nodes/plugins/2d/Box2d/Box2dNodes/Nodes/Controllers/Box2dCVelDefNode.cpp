#include "StdAfx.h"
#include "Box2dCVelDefNode.h"

namespace VVVV 
{
	namespace Nodes 
	{
		Box2dCVelDefNode::Box2dCVelDefNode(void)
		{
			this->m_controller = gcnew ControllerDataType();
		}

		void Box2dCVelDefNode::OnEvaluate(int SpreadMax, bool reset)
		{
			if (this->vInVelocity->PinIsChanged
				|| reset)
			{
				for (int i = 0; i < SpreadMax; i++)
				{
					double x,y;
					this->vInVelocity->GetValue2D(i, x,y);

					if (reset)
					{
						b2ConstantForceControllerDef ctrldef;

						ctrldef.F.x = x;
						ctrldef.F.y = y;

						this->ctrl->push_back(this->m_world->GetWorld()->CreateController(&ctrldef));
						this->m_controller->Add(this->ctrl->at(i));
					}
					else
					{
						b2ConstantForceController* ac = (b2ConstantForceController*) this->ctrl->at(i);
						ac->F.x = x;
						ac->F.y = y;
					}
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
