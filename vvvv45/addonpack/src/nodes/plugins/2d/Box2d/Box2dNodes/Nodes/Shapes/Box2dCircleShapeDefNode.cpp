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
				|| this->vInRestitution->PinIsChanged) 
			{

				double x,y,r,friction,restitution,density;

				this->vOutShapes->SliceCount = SpreadMax;

				this->m_shapes->Reset();
				
				for (int i = 0; i < SpreadMax;i++) 
				{		
					this->vInLocalPosition->GetValue2D(i,x,y);
					this->vInRadius->GetValue(i,r);
					this->vInFriction->GetValue(i,friction);
					this->vInDensity->GetValue(i,density);
					this->vInRestitution->GetValue(i,restitution);

					b2CircleDef* shapeDef = this->m_shapes->AddCircle();
					shapeDef->radius = r;
					shapeDef->density = density;
					shapeDef->friction = friction;
					shapeDef->restitution = restitution;
					shapeDef->localPosition = b2Vec2(x,y);
				}
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
