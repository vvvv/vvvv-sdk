#include "StdAfx.h"
#include "Box2dGravityControllerNode.h"


namespace VVVV 
{
	namespace Nodes 
	{
		Box2dGravityControllerDefNode::Box2dGravityControllerDefNode(void)
		{
			this->m_controller = gcnew ControllerDataType();
			ctrldef = new b2GravityControllerDef();
		}

		void Box2dGravityControllerDefNode::OnEvaluate(int SpreadMax, bool reset)
		{	
			if (this->vInForce->PinIsChanged ||
				this->vInInvSquare->PinIsChanged ||
				reset)
			{
				double f,inv;

				this->vInForce->GetValue(0, f);
				this->vInInvSquare->GetValue(0,inv);

				ctrldef->G = f;
				ctrldef->invSqr = inv >= 0.5;

				if (reset)
				{
					this->ctrl = this->m_world->GetWorld()->CreateController(this->ctrldef);
				}
				else
				{
					b2GravityController* gc = (b2GravityController*) this->ctrl;
					gc->G = f;
					gc->invSqr = inv >= 0.5;
				}
			}
		}
		
		void Box2dGravityControllerDefNode::OnPluginHostSet()
		{
			this->FHost->CreateValueInput("Force",1,ArrayUtils::Array1D(),TSliceMode::Single,TPinVisibility::True,this->vInForce);
			this->vInForce->SetSubType(Double::MinValue,Double::MaxValue,0.01,1.0,false,false,false);

			this->FHost->CreateValueInput("Inv Square",1,ArrayUtils::Array1D(),TSliceMode::Single,TPinVisibility::True,this->vInInvSquare);
			this->vInInvSquare->SetSubType(Double::MinValue,Double::MaxValue,0.01,0.0,false,true,false);
		}

	}
}

