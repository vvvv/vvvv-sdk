#include "StdAfx.h"
#include "Box2dBuoyancyControllerDefNode.h"

namespace VVVV 
{
	namespace Nodes 
	{
		Box2dBuoyancyControllerDefNode::Box2dBuoyancyControllerDefNode(void)
		{
			this->m_controller = gcnew ControllerDataType();
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
				for (int i = 0; i < SpreadMax; i++)
				{

					double nx, ny,offset,ld,ad,dens;
					this->vInNormal->GetValue2D(i, nx,ny);
					this->vInOffset->GetValue(i, offset);
					this->vInAngularDrag->GetValue(i, ad);
					this->vInDensity->GetValue(i, dens);
					this->vInLinearDrag->GetValue(i, ld);

					if (reset)
					{
						b2BuoyancyControllerDef ctrldef;

						ctrldef.normal.x = nx;
						ctrldef.normal.y = ny;
						ctrldef.offset = offset;
						ctrldef.density = dens;
						ctrldef.linearDrag = ld;
						ctrldef.angularDrag = ad;


						this->ctrl->push_back(this->m_world->GetWorld()->CreateController(&ctrldef));
						this->m_controller->Add(this->ctrl->at(i));
					}
					else
					{
						b2BuoyancyController* bc = (b2BuoyancyController*) this->ctrl->at(i);
						bc->normal.x = nx;
						bc->normal.y = ny;
						bc->offset = offset;
						bc->density = dens;
						bc->linearDrag = ld;
						bc->angularDrag = ad;
					}
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