#include "StdAfx.h"
#include "Box2dTensorDampingControllerDefNode.h"

namespace VVVV 
{
	namespace Nodes 
	{
		Box2dTensorDampingControllerDefNode::Box2dTensorDampingControllerDefNode(void)
		{
			this->m_controller = gcnew ControllerDataType();
			this->ctrldef = new b2TensorDampingControllerDef();
		}

		void Box2dTensorDampingControllerDefNode::OnEvaluate(int SpreadMax, bool reset)
		{
			if (this->vInModel->PinIsChanged
				|| reset)
			{
				double x,y,z,w;
				this->vInModel->GetValue4D(0, x,z,y,w);
				b2Mat22* mat = new b2Mat22(x,y,z,w);
				ctrldef->T = *mat;
				

				if (reset)
				{
					this->ctrl = this->m_world->GetWorld()->CreateController(this->ctrldef);
				}
				else
				{
					b2TensorDampingController* gc = (b2TensorDampingController*) this->ctrl;
					gc->T = *mat;
				}
			}
		}
		
		void Box2dTensorDampingControllerDefNode::OnPluginHostSet()
		{
			this->FHost->CreateValueInput("Model",4,ArrayUtils::Array4D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInModel);
			this->vInModel->SetSubType4D(Double::MinValue,Double::MaxValue,0.01,-1.0,0.0,0.0,-1.0,false,false,false);
		}

	}
}
