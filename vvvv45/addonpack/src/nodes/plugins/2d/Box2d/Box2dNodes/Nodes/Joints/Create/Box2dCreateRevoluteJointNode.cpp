#include "StdAfx.h"
#include "Box2dCreateRevoluteJointNode.h"

namespace VVVV 
{
	namespace Nodes 
	{
		Box2dCreateRevoluteJointNode::Box2dCreateRevoluteJointNode(void)
		{
		}

		void Box2dCreateRevoluteJointNode::OnPluginHostSet() 
		{
			this->FHost->CreateValueInput("Position 1",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInPosition);
			this->vInPosition->SetSubType2D(Double::MinValue,Double::MaxValue,0.01,0.0,0.0,false,false,false);

			this->FHost->CreateValueInput("Min Angle",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInMinAngle);
			this->vInMinAngle->SetSubType(Double::MinValue,Double::MaxValue,0.01,0.0,false,false,false);

			this->FHost->CreateValueInput("Max Angle",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInMaxAngle);
			this->vInMaxAngle->SetSubType(Double::MinValue,Double::MaxValue,0.01,0.0,false,false,false);
		}

		void Box2dCreateRevoluteJointNode::Evaluate(int SpreadMax)
		{
			this->vInBody1->PinIsChanged;
			this->vInBody2->PinIsChanged;

			bool bcnn = true;
			bcnn = (this->vInBody1->IsConnected) || (this->vInBody1->SliceCount > 0);

			if (this->vInBody2->IsConnected && bcnn && this->vInBody2->SliceCount > 0 && this->vInWorld->IsConnected) 
			{
				if (this->mWorld->GetIsValid()) 
				{
					for (int i = 0; i < SpreadMax; i++) 
					{
						double dblcreate;
						this->vInDoCreate->GetValue(i,dblcreate);
						if (dblcreate >= 0.5) 
						{
							double px,py,mina,maxa;
							int realslice1,realslice2;

							b2Body* body1;
							this->vInBody1->GetUpsreamSlice(i % this->vInBody1->SliceCount,realslice1);
							body1 = this->m_body1->GetSlice(realslice1);


							this->vInBody2->GetUpsreamSlice(i % this->vInBody2->SliceCount,realslice2);
							b2Body* body2 = this->m_body2->GetSlice(realslice2);
						
							this->vInPosition->GetValue2D(i,px,py);
							this->vInMinAngle->GetValue(i,mina);
							this->vInMaxAngle->GetValue(i,maxa);

							b2RevoluteJointDef jointDef;
							jointDef.Initialize(body1, body2, b2Vec2(px,py));
							jointDef.lowerAngle = mina /2.0f;
							jointDef.upperAngle = maxa /2.0f;
							jointDef.enableLimit = false;
							jointDef.maxMotorTorque = 10.0f;
							jointDef.motorSpeed = 0.0f;
							jointDef.enableMotor = true;

							this->mWorld->GetWorld()->CreateJoint(&jointDef);
						}
					}
				}
			}

		}
	}
}

