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

			this->FHost->CreateValueInput("Enable Limit",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInEnableLimit);
			this->vInEnableLimit->SetSubType(Double::MinValue,Double::MaxValue,1.0,0.0,false,true,false);

			this->FHost->CreateValueInput("Max Motor Torque",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInMaxMotorTorque);
			this->vInMaxMotorTorque->SetSubType(Double::MinValue,Double::MaxValue,0.01,0.0,false,false,false);

			this->FHost->CreateValueInput("Motor Speed",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInMotorSpeed);
			this->vInMotorSpeed->SetSubType(Double::MinValue,Double::MaxValue,0.01,0.0,false,false,false);

			this->FHost->CreateValueInput("Enable Motor",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInEnableMotor);
			this->vInEnableMotor->SetSubType(Double::MinValue,Double::MaxValue,1.0,0.0,false,true,false);
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
							double px,py,mina,maxa,cc;
							double enlimit,enmotor,motorq,motspeed;
							int realslice1,realslice2;
							String^ cust;

							b2Body* body1;

							this->vInBody1->GetUpsreamSlice(i % this->vInBody1->SliceCount,realslice1);
							body1 = this->m_body1->GetSlice(realslice1);
							
							this->vInBody2->GetUpsreamSlice(i % this->vInBody2->SliceCount,realslice2);
							b2Body* body2 = this->m_body2->GetSlice(realslice2);
						
							this->vInPosition->GetValue2D(i,px,py);
							this->vInMinAngle->GetValue(i,mina);
							this->vInMaxAngle->GetValue(i,maxa);
							this->vInCollideConnected->GetValue(i, cc);
							this->vInEnableLimit->GetValue(i, enlimit);
							this->vInEnableMotor->GetValue(i, enmotor);
							this->vInMaxMotorTorque->GetValue(i, motorq);
							this->vInMotorSpeed->GetValue(i, motspeed);
							this->vInCustom->GetString(i,cust);


							b2RevoluteJointDef jointDef;
							jointDef.Initialize(body1, body2, b2Vec2(px,py));
							jointDef.lowerAngle = mina * (Math::PI * 2.0);
							jointDef.upperAngle = maxa * (Math::PI * 2.0);
							jointDef.collideConnected= cc >= 0.5;
							jointDef.enableLimit = enlimit >= 0.5;
							jointDef.maxMotorTorque = Convert::ToSingle(motorq);
							jointDef.motorSpeed = Convert::ToSingle(motspeed);
							jointDef.enableMotor = enmotor >= 0.5;
							

							JointCustomData* jdata = new JointCustomData();
							jdata->Id = this->mWorld->GetNewJointId();
							jdata->Custom = (char*)(void*)Marshal::StringToHGlobalAnsi(cust);

							b2Joint* j = this->mWorld->GetWorld()->CreateJoint(&jointDef);
							j->SetUserData(jdata);
						}
					}
				}
			}

		}
	}
}

