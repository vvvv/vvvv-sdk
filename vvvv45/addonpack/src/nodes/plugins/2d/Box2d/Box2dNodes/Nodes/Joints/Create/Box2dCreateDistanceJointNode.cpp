#include "StdAfx.h"
#include "Box2dCreateDistanceJointNode.h"

namespace VVVV 
{
	namespace Nodes 
	{
		Box2dCreateDistanceJointNode::Box2dCreateDistanceJointNode(void)
		{
		}

		void Box2dCreateDistanceJointNode::OnPluginHostSet() 
		{
			this->FHost->CreateValueInput("Position 1",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInPosition1);
			this->vInPosition1->SetSubType2D(Double::MinValue,Double::MaxValue,0.01,0.0,0.0,false,false,false);

			this->FHost->CreateValueInput("Position 2",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInPosition2);
			this->vInPosition2->SetSubType2D(Double::MinValue,Double::MaxValue,0.01,0.0,0.0,false,false,false);

			this->FHost->CreateValueInput("Frequency",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInFrequency);
			this->vInFrequency->SetSubType(Double::MinValue,Double::MaxValue,0.01,0.0,false,false,false);

			this->FHost->CreateValueInput("Damping Ratio",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInDampingRatio);
			this->vInDampingRatio->SetSubType(Double::MinValue,Double::MaxValue,0.01,0.0,false,false,false);
		}

		void Box2dCreateDistanceJointNode::Evaluate(int SpreadMax)
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
							double b1x,b1y,b2x,b2y,freq,dr,cc;
							int realslice1,realslice2;
							String^ cust;

							this->vInFrequency->GetValue(i, freq);
							this->vInDampingRatio->GetValue(i,dr);
							this->vInCollideConnected->GetValue(i, cc);
							this->vInCustom->GetString(i,cust);

							b2Body* body1;

							this->vInBody1->GetUpsreamSlice(i % this->vInBody1->SliceCount,realslice1);
							body1 = this->m_body1->GetSlice(realslice1);

							this->vInBody2->GetUpsreamSlice(i % this->vInBody2->SliceCount,realslice2);
							b2Body* body2 = this->m_body2->GetSlice(realslice2);
						
							this->vInPosition1->GetValue2D(i,b1x,b1y);
							this->vInPosition2->GetValue2D(i,b2x,b2y);
							
							b2DistanceJointDef jointDef;
							jointDef.frequencyHz = freq;
							jointDef.dampingRatio = dr;
							jointDef.Initialize(body1, body2, b2Vec2(b1x,b1y), b2Vec2(b2x,b2y));

							jointDef.collideConnected = cc >= 0.5;

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
