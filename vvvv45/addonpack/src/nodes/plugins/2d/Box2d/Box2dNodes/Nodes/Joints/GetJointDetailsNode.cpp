#include "StdAfx.h"
#include "GetJointDetailsNode.h"

#include "../../Internals/Data/JointCustomData.h"

namespace VVVV 
{
	namespace Nodes 
	{

		GetJointDetailsNode::GetJointDetailsNode(void)
		{
			this->m_bodies1 = gcnew BodyDataType();
			this->m_bodies2 = gcnew BodyDataType();
		}

		void GetJointDetailsNode::SetPluginHost(IPluginHost^ Host) 
		{
			this->FHost = Host;

			this->FHost->CreateNodeInput("Input",TSliceMode::Dynamic,TPinVisibility::True,this->vInJoints);
			this->vInJoints->SetSubType(ArrayUtils::SingleGuidArray(JointDataType::GUID),JointDataType::FriendlyName);

			this->FHost->CreateStringOutput("Type",TSliceMode::Dynamic,TPinVisibility::True,this->vOutType);
			this->vOutType->SetSubType("",false);

			this->FHost->CreateNodeOutput("Body 1",TSliceMode::Dynamic,TPinVisibility::True,this->vOutBody1);
			this->vOutBody1->SetSubType(ArrayUtils::SingleGuidArray(BodyDataType::GUID),BodyDataType::FriendlyName);
			this->vOutBody1->SetInterface(this->m_bodies1);

			this->FHost->CreateValueOutput("Anchor 1",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutPosition1);
			this->vOutPosition1->SetSubType2D(Double::MinValue,Double::MaxValue,0.01,0.0,0.0,false,false,false);

			this->FHost->CreateNodeOutput("Body 2",TSliceMode::Dynamic,TPinVisibility::True,this->vOutBody2);
			this->vOutBody2->SetSubType(ArrayUtils::SingleGuidArray(BodyDataType::GUID),BodyDataType::FriendlyName);
			this->vOutBody2->SetInterface(this->m_bodies2);

			this->FHost->CreateValueOutput("Anchor 2",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutPosition2);
			this->vOutPosition2->SetSubType2D(Double::MinValue,Double::MaxValue,0.01,0.0,0.0,false,false,false);

			//this->FHost->CreateValueOutput("Force",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutPosition1);
			//this->vOutPosition1->SetSubType2D(Double::MinValue,Double::MaxValue,0.01,0.0,0.0,false,false,false);

			this->FHost->CreateStringOutput("Custom",TSliceMode::Dynamic,TPinVisibility::True,this->vOutCustom);
			this->vOutCustom->SetSubType("",false);

			this->FHost->CreateValueOutput("Joint Id",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutId);
			this->vOutId->SetSubType(Double::MinValue,Double::MaxValue,1,0.0,false,false,true);

		}


		void GetJointDetailsNode::Configurate(IPluginConfig^ Input)
		{

		}

		
		void GetJointDetailsNode::Evaluate(int SpreadMax)
		{
			if (this->vInJoints->IsConnected) 
			{
				
				this->m_bodies1->Reset();
				this->m_bodies2->Reset();

				int cnt = this->vInJoints->SliceCount;
				this->vOutBody1->SliceCount = cnt;
				this->vOutPosition1->SliceCount = cnt;
				this->vOutBody2->SliceCount = cnt;
				this->vOutPosition2->SliceCount = cnt;
				this->vOutId->SliceCount = cnt;
				this->vOutCustom->SliceCount = cnt;
				this->vOutType->SliceCount = cnt;

				for (int i = 0; i < this->vInJoints->SliceCount; i++) 
				{
					b2Joint* joint;

					int realslice;
					this->vInJoints->GetUpsreamSlice(i % this->vInJoints->SliceCount,realslice);
					joint = this->m_joints->GetSlice(realslice);

					JointCustomData* jdata = (JointCustomData*)joint->GetUserData();
					String^ str = gcnew String(jdata->Custom);
					this->vOutId->SetValue(i, jdata->Id);
					this->vOutCustom->SetString(i,str);

					this->m_bodies1->Add(joint->GetBodyA());
					this->m_bodies2->Add(joint->GetBodyB());

					b2Vec2 pos1 = joint->GetAnchorA();
					this->vOutPosition1->SetValue2D(i, pos1.x,pos1.y);

					b2Vec2 pos2 = joint->GetAnchorB();
					this->vOutPosition2->SetValue2D(i, pos2.x,pos2.y);

					String^ type;
					switch (joint->GetType())
					{
					case e_distanceJoint:
						type = "Distance";
						break;	
					case e_revoluteJoint:
						type = "Revolute";
						break;
					case e_prismaticJoint:
						type = "Prismatic";
						break;
					case e_pulleyJoint:
						type = "Pulley";
						break;
					case e_mouseJoint:
						type = "Mouse";
						break;
					case e_lineJoint:
						type = "Line";
						break;
					case e_gearJoint:
						type = "Gear";
						break;
					default:
						type = "Unknown";
						break;
					}

					this->vOutType->SetString(i, type);

				}
			}
		}


		void GetJointDetailsNode::ConnectPin(IPluginIO^ Pin)
		{
			if (Pin == this->vInJoints)
        	{
				INodeIOBase^ usI;
				this->vInJoints->GetUpstreamInterface(usI);
				this->m_joints = (JointDataType^)usI;
        	}
		}

		void GetJointDetailsNode::DisconnectPin(IPluginIO^ Pin)
		{
			if (Pin == this->vInJoints)
        	{
        		this->m_joints = nullptr;
        	}
		}
	}
}
