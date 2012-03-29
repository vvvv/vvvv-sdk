#include "StdAfx.h"
#include "Box2dCircleShapeDefNode.h"

namespace VVVV 
{
	namespace Nodes 
	{
		Box2dCircleShapeDefNode::Box2dCircleShapeDefNode(void)
		{

		}

		void Box2dCircleShapeDefNode::Evaluate(int SpreadMax)
		{
			if (this->vInLocalPosition->PinIsChanged
				|| this->vInRadius->PinIsChanged
				|| this->vInFriction->PinIsChanged
				|| this->vInDensity->PinIsChanged
				|| this->vInRestitution->PinIsChanged
				|| this->vInIsSensor->PinIsChanged
				|| this->vInCustom->PinIsChanged
				|| this->vInGroupIndex->PinIsChanged) 
			{

				double x,y,r,friction,restitution,density,issensor,group;
				String^ custom;

				this->vOutShapes->SliceCount = SpreadMax;

				this->m_shapes->Reset();
				
				for (int i = 0; i < SpreadMax;i++) 
				{		
					this->vInLocalPosition->GetValue2D(i,x,y);
					this->vInRadius->GetValue(i,r);
					this->vInFriction->GetValue(i,friction);
					this->vInDensity->GetValue(i,density);
					this->vInRestitution->GetValue(i,restitution);
					this->vInIsSensor->GetValue(i,issensor);
					this->vInCustom->GetString(i, custom);
					this->vInGroupIndex->GetValue(i, group);

					b2CircleShape* cir = new b2CircleShape();
					b2FixtureDef* shapeDef = this->m_shapes->AddCircle();

					b2Vec2 center(x,y);
					cir->m_radius = r;
					cir->m_p = center;

					shapeDef->density = density;
					shapeDef->friction = friction;
					shapeDef->restitution = restitution;
					shapeDef->isSensor = issensor >= 0.5;
					shapeDef->filter.groupIndex = Convert::ToInt32(group);
					shapeDef->shape = cir;

					this->m_shapes->AddCustom(custom);
				}

				this->vOutShapes->MarkPinAsChanged();
			}

		}
		
		void Box2dCircleShapeDefNode::OnPluginHostSet() 
		{
			this->FHost->CreateValueInput("Position",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInLocalPosition);
			this->vInLocalPosition->SetSubType2D(0,Double::MaxValue,0.01,0.0,0.0,false,false,false);

			this->FHost->CreateValueInput("Radius",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInRadius);
			this->vInRadius->SetSubType(0,Double::MaxValue,0.01,1.0,false,false,false);
		}
	}
}
