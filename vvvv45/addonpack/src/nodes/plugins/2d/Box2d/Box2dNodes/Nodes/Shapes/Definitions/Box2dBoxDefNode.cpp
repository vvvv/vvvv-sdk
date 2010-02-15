#include "StdAfx.h"
#include "Box2dBoxDefNode.h"

namespace VVVV 
{
	namespace Nodes 
	{
		Box2dBoxDefNode::Box2dBoxDefNode(void) 
		{

		}

		Box2dBoxDefNode::~Box2dBoxDefNode(void)
		{
			delete this->m_shapes;
		}

		void Box2dBoxDefNode::Evaluate(int SpreadMax)
		{
			if (this->vInLocalPosition->PinIsChanged
				|| this->vInSize->PinIsChanged
				|| this->vInAngle->PinIsChanged
				|| this->vInFriction->PinIsChanged
				|| this->vInDensity->PinIsChanged
				|| this->vInRestitution->PinIsChanged
				|| this->vInIsSensor->PinIsChanged
				|| this->vInCustom->PinIsChanged
				|| this->vInGroupIndex->PinIsChanged) 
			{

				double x,y,r,sx,sy,a,friction,restitution,density,issensor,group;
				String^ custom;
				this->vOutShapes->SliceCount = SpreadMax;

				this->m_shapes->Reset();
				
				for (int i = 0; i < SpreadMax;i++) 
				{		
					this->vInLocalPosition->GetValue2D(i,x,y);
					this->vInSize->GetValue2D(i,sx,sy);
					this->vInAngle->GetValue(i,a);
					this->vInFriction->GetValue(i,friction);
					this->vInDensity->GetValue(i,density);
					this->vInRestitution->GetValue(i,restitution);
					this->vInIsSensor->GetValue(i,issensor);
					this->vInCustom->GetString(i, custom);
					this->vInGroupIndex->GetValue(i, group);

					b2FixtureDef* shapeDef = this->m_shapes->AddPolygon();


					b2Vec2 center(x,y);
					b2PolygonShape* poly = new b2PolygonShape();
					poly->SetAsBox(sx / 2.0f, sy / 2.0,center,a * (Math::PI * 2.0));

					shapeDef->density = density;
					shapeDef->friction = friction;
					shapeDef->restitution = restitution;
					shapeDef->isSensor = issensor >= 0.5;
					shapeDef->filter.groupIndex = Convert::ToInt32(group);
					shapeDef->shape = poly;

					this->m_shapes->AddCustom(custom);
				}

				this->vOutShapes->MarkPinAsChanged();
			}

		}
		
		void Box2dBoxDefNode::OnPluginHostSet() 
		{
			this->FHost->CreateValueInput("Position",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInLocalPosition);
			this->vInLocalPosition->SetSubType2D(0,Double::MaxValue,0.01,0.0,0.0,false,false,false);

			this->FHost->CreateValueInput("Size",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInSize);
			this->vInSize->SetSubType2D(0,Double::MaxValue,0.01,1.0,1.0,false,false,false);

			this->FHost->CreateValueInput("Angle",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInAngle);
			this->vInAngle->SetSubType(0,Double::MaxValue,0.01,0.0,false,false,false);
		}
	}
}
