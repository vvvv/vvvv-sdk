#include "StdAfx.h"
#include "Box2dGravityControllerNode.h"


namespace VVVV 
{
	namespace Nodes 
	{
		Box2dGravityControllerDefNode::Box2dGravityControllerDefNode(void)
		{
			this->m_controller = gcnew ControllerDataType();
		}

		void Box2dGravityControllerDefNode::OnEvaluate(int SpreadMax, bool reset)
		{	
			if (this->vInForce->PinIsChanged ||
				this->vInInvSquare->PinIsChanged ||
				reset)
			{
				for (int i = 0; i < SpreadMax; i++)
				{
					double f,inv;

					this->vInForce->GetValue(i, f);
					this->vInInvSquare->GetValue(i,inv);

					if (reset)
					{
						b2GravityControllerDef ctrldef;
						ctrldef.G = f;
						ctrldef.invSqr = inv >= 0.5;

						this->ctrl->push_back(this->m_world->GetWorld()->CreateController(&ctrldef));
						this->m_controller->Add(this->ctrl->at(i));
					}
					else
					{
						b2GravityController* gc = (b2GravityController*) this->ctrl->at(i);
						gc->G = f;
						gc->invSqr = inv >= 0.5;
					}
				}
			}
		}
		
		void Box2dGravityControllerDefNode::OnPluginHostSet()
		{
			this->FHost->CreateValueInput("Force",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInForce);
			this->vInForce->SetSubType(Double::MinValue,Double::MaxValue,0.01,1.0,false,false,false);

			this->FHost->CreateValueInput("Inv Square",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInInvSquare);
			this->vInInvSquare->SetSubType(Double::MinValue,Double::MaxValue,0.01,0.0,false,true,false);
		}

	}
}

