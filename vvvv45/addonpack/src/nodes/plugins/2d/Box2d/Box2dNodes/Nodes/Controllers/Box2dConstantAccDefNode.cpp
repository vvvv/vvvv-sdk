#include "StdAfx.h"
#include "Box2dConstantAccDefNode.h"

namespace VVVV 
{
	namespace Nodes 
	{
		Box2dConstantAccDefNode::Box2dConstantAccDefNode(void)
		{
			this->m_controller = gcnew ControllerDataType();

		}

		void Box2dConstantAccDefNode::OnEvaluate(int SpreadMax, bool reset)
		{
			if (this->vInAcceleration->PinIsChanged
			|| reset)
			{
				for (int i = 0; i <SpreadMax; i++)
				{
					double x,y;
					this->vInAcceleration->GetValue2D(i, x,y);


					if (reset)
					{
						b2ConstantAccelControllerDef ctrldef;
						ctrldef.A.x = x;
						ctrldef.A.y = y;

						this->ctrl->push_back(this->m_world->GetWorld()->CreateController(&ctrldef));
						this->m_controller->Add(this->ctrl->at(i));
					}
					else
					{
						b2ConstantAccelController* ac = (b2ConstantAccelController*) this->ctrl->at(i);
						ac->A.x = x;
						ac->A.y = y;
					}
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