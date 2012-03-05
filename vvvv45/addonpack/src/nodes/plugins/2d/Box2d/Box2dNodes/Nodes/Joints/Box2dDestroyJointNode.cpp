#include "StdAfx.h"
#include "Box2dDestroyJointNode.h"

#include "../../Internals/Data/JointCustomData.h"

namespace VVVV 
{
	namespace Nodes 
	{
		void Box2dDestroyJointNode::SetPluginHost(IPluginHost^ Host) 
		{
			this->FHost = Host;

			this->FHost->CreateNodeInput("Joints",TSliceMode::Dynamic,TPinVisibility::True,this->vInJoints);
			this->vInJoints->SetSubType(ArrayUtils::SingleGuidArray(JointDataType::GUID),JointDataType::FriendlyName);

			this->FHost->CreateValueInput("Do Destroy",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInDoDestroy);
			this->vInDoDestroy->SetSubType(Double::MinValue,Double::MaxValue,0.01,0.0,true,false,false);	

			

		}

		void Box2dDestroyJointNode::Configurate(IPluginConfig^ Input)
		{

		}

		void Box2dDestroyJointNode::Evaluate(int SpreadMax)
		{
			this->vInJoints->PinIsChanged;
			if (this->vInJoints->IsConnected) 
			{
				double dbldelete;
				

					//double dblsp,dblsv,dblsav;
					for (int i = 0; i < this->vInJoints->SliceCount; i++) 
					{
						this->vInDoDestroy->GetValue(i, dbldelete);
						if (dbldelete >= 0.5)
						{
							int realslice;
							this->vInJoints->GetUpsreamSlice(i % this->vInJoints->SliceCount,realslice);
							b2Joint* joint = this->m_joints->GetSlice(realslice);

							JointCustomData* bdata = (JointCustomData*)joint->GetUserData();
							bdata->MarkedForDeletion = true;
						}
					}
				
			}
		}



		void Box2dDestroyJointNode::ConnectPin(IPluginIO^ Pin)
		{
			if (Pin == this->vInJoints) 
			{
				INodeIOBase^ usI;
				this->vInJoints->GetUpstreamInterface(usI);
				this->m_joints = (JointDataType^)usI;
			}
		}


		void Box2dDestroyJointNode::DisconnectPin(IPluginIO^ Pin)
		{
			if (Pin == this->vInJoints)
        	{
        		this->m_joints = nullptr;
        	}
		}

	}
}
