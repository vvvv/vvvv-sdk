#include "StdAfx.h"
#include "Box2dCreatePulleyJointNode.h"

namespace VVVV 
{
	namespace Nodes 
	{
		Box2dCreatePulleyJointNode::Box2dCreatePulleyJointNode(void)
		{
		}

		void Box2dCreatePulleyJointNode::OnPluginHostSet() 
		{
			this->FHost->CreateValueInput("Position 1",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInPosition1);
			this->vInPosition1->SetSubType2D(Double::MinValue,Double::MaxValue,0.01,0.0,0.0,false,false,false);

			this->FHost->CreateValueInput("Anchor 1",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInAnchor1);
			this->vInAnchor1->SetSubType2D(Double::MinValue,Double::MaxValue,0.01,0.0,0.0,false,false,false);

			this->FHost->CreateValueInput("Max Length 1",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInMaxLength1);
			this->vInMaxLength1->SetSubType(0,Double::MaxValue,0.01,0.0,false,false,false);

			this->FHost->CreateValueInput("Position 2",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInPosition2);
			this->vInPosition2->SetSubType2D(Double::MinValue,Double::MaxValue,0.01,0.0,0.0,false,false,false);

			this->FHost->CreateValueInput("Anchor 2",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInAnchor2);
			this->vInAnchor2->SetSubType2D(Double::MinValue,Double::MaxValue,0.01,0.0,0.0,false,false,false);

			this->FHost->CreateValueInput("Max Length 2",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInMaxLength2);
			this->vInMaxLength2->SetSubType(0,Double::MaxValue,0.01,0.0,false,false,false);

			this->FHost->CreateValueInput("Ratio",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInRatio);
			this->vInRatio->SetSubType(0,Double::MaxValue,0.01,0.0,false,false,false);
		}

		void Box2dCreatePulleyJointNode::Evaluate(int SpreadMax)
		{
			this->vInBody1->PinIsChanged;
			this->vInBody2->PinIsChanged;

			if (this->vInBody1->IsConnected && this->vInBody2->IsConnected 
				&& this->vInBody1->SliceCount > 0 && this->vInBody2->SliceCount > 0 && this->vInWorld->IsConnected) 
			{
				if (this->mWorld->GetIsValid()) 
				{
					for (int i = 0; i < SpreadMax; i++) 
					{
						double dblcreate;
						this->vInDoCreate->GetValue(i,dblcreate);
						if (dblcreate >= 0.5) 
						{
							double b1x,b1y,b2x,b2y,a1x,a1y,a2x,a2y,ml1,ml2,ratio,cc;
							int realslice1,realslice2;
							String^ cust;

							this->vInBody1->GetUpsreamSlice(i % this->vInBody1->SliceCount,realslice1);
							b2Body* body1 = this->m_body1->GetSlice(realslice1);

							this->vInBody2->GetUpsreamSlice(i % this->vInBody2->SliceCount,realslice2);
							b2Body* body2 = this->m_body2->GetSlice(realslice2);
						
							this->vInPosition1->GetValue2D(i,b1x,b1y);
							this->vInPosition2->GetValue2D(i,b2x,b2y);
							this->vInAnchor1->GetValue2D(i,a1x,a1y);
							this->vInAnchor2->GetValue2D(i,a2x,a2y);
							this->vInMaxLength1->GetValue(i,ml1);
							this->vInMaxLength2->GetValue(i,ml2);
							this->vInRatio->GetValue(i,ratio);
							this->vInCollideConnected->GetValue(i, cc);
							this->vInCustom->GetString(i,cust);

							b2PulleyJointDef jointDef;
							b2Vec2 anchor1(b1x,b1y);
							b2Vec2 anchor2(b2x,b2y);
							b2Vec2 groundAnchor1(a1x,a1y);
							b2Vec2 groundAnchor2(a2x,a2y);

							jointDef.Initialize(body1, body2, groundAnchor1, groundAnchor2, anchor1, anchor2, ratio);
							jointDef.maxLengthA = ml1;
							jointDef.maxLengthB = ml2;
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

