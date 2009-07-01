#include "StdAfx.h"
#include "Box2dBoxDefNode.h"

namespace VVVV 
{
	namespace Nodes 
	{
		Box2dBoxDefNode::Box2dBoxDefNode(void) 
		{

		}

		void Box2dBoxDefNode::Evaluate(int SpreadMax)
		{
			if (this->vInLocalPosition->PinIsChanged
				|| this->vInSize->PinIsChanged
				|| this->vInAngle->PinIsChanged
				|| this->vInFriction->PinIsChanged
				|| this->vInDensity->PinIsChanged
				|| this->vInRestitution->PinIsChanged
				|| this->vInIsSensor->PinIsChanged) 
			{

				double x,y,r,sx,sy,a,friction,restitution,density,issensor;

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

					b2PolygonDef* shapeDef = this->m_shapes->AddPolygon();
					b2Vec2 center(x,y);
					shapeDef->SetAsBox(sx / 2.0f, sy / 2.0,center,a);
					shapeDef->density = density;
					shapeDef->friction = friction;
					shapeDef->restitution = restitution;
					shapeDef->isSensor = issensor >= 0.5;

					
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
