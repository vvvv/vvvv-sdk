#include "StdAfx.h"
#include "Box2dBuoyancyControllerDefNode.h"

namespace VVVV 
{
	namespace Nodes 
	{
		Box2dBuoyancyControllerDefNode::Box2dBuoyancyControllerDefNode(void)
		{
			this->m_controller = gcnew ControllerDataType();
			this->ctrldef = new b2BuoyancyControllerDef();
		}

		void Box2dBuoyancyControllerDefNode::OnEvaluate(int SpreadMax, bool reset)
		{
			if (this->vInNormal->PinIsChanged ||
				this->vInOffset->PinIsChanged ||
				this->vInDensity->PinIsChanged ||
				this->vInLinearDrag->PinIsChanged ||
				this->vInAngularDrag->PinIsChanged ||
				reset)
			{
				double nx, ny,offset,ld,ad,dens;
				this->vInNormal->GetValue2D(0, nx,ny);
				this->vInOffset->GetValue(0, offset);
				this->vInAngularDrag->GetValue(0, ad);
				this->vInDensity->GetValue(0, dens);
				this->vInLinearDrag->GetValue(0, ld);

				ctrldef->normal.x = nx;
				ctrldef->normal.y = ny;
				ctrldef->offset = offset;
				ctrldef->density = dens;
				ctrldef->linearDrag = ld;
				ctrldef->angularDrag = ad;

				if (reset)
				{
					this->ctrl = this->m_world->GetWorld()->CreateController(this->ctrldef);
				}
				else
				{
					b2BuoyancyController* bc = (b2BuoyancyController*) this->ctrl;
					bc->normal.x = nx;
					bc->normal.y = ny;
					bc->offset = offset;
					bc->density = dens;
					bc->linearDrag = ld;
					bc->angularDrag = ad;
				}
			}
		}
		
		void Box2dBuoyancyControllerDefNode::OnPluginHostSet()
		{
			this->FHost->CreateValueInput("Normal",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInNormal);
			this->vInNormal->SetSubType2D(Double::MinValue,Double::MaxValue,0.01,0.0,1.0,false,false,false);

			this->FHost->CreateValueInput("Offset",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInOffset);
			this->vInOffset->SetSubType(Double::MinValue,Double::MaxValue,0.01,0.0,false,false,false);

			this->FHost->CreateValueInput("Density",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInDensity);
			this->vInDensity->SetSubType(Double::MinValue,Double::MaxValue,0.01,2.0,false,false,false);

			this->FHost->CreateValueInput("Linear Drag",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInLinearDrag);
			this->vInLinearDrag->SetSubType(Double::MinValue,Double::MaxValue,0.01,2.0,false,false,false);

			this->FHost->CreateValueInput("Angular Drag",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInAngularDrag);
			this->vInAngularDrag->SetSubType(Double::MinValue,Double::MaxValue,0.01,1.0,false,false,false);
		}

	}
}