#include "StdAfx.h"
#include "Box2dApplyTorqueNode.h"

#include "../../Internals/Data/BodyCustomData.h"

namespace VVVV 
{
	namespace Nodes 
	{
		Box2dApplyTorqueNode::Box2dApplyTorqueNode(void)
		{
		}

		void Box2dApplyTorqueNode::SetPluginHost(IPluginHost^ Host) 
		{
			this->FHost = Host;

			this->FHost->CreateNodeInput("Bodies",TSliceMode::Dynamic,TPinVisibility::True,this->vInBodies);
			this->vInBodies->SetSubType(ArrayUtils::SingleGuidArray(BodyDataType::GUID),BodyDataType::FriendlyName);

			this->FHost->CreateValueInput("Torque",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInTorque);
			this->vInTorque->SetSubType2D(Double::MinValue,Double::MaxValue,0.01,0.0,0.0,false,false,false);

			this->FHost->CreateValueInput("Apply",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInApply);
			this->vInApply->SetSubType(0,1,1,0.0,true,false,false);	

		}

		void Box2dApplyTorqueNode::Configurate(IPluginConfig^ Input)
		{

		}

		
		void Box2dApplyTorqueNode::Evaluate(int SpreadMax)
		{
			this->vInBodies->PinIsChanged;
			if (this->vInBodies->IsConnected) 
			{
				double dblapply,torque;
				for (int i = 0; i < this->vInBodies->SliceCount; i++) 
				{
					int realslice;
					this->vInBodies->GetUpsreamSlice(i % this->vInBodies->SliceCount,realslice);
					b2Body* body = this->m_bodies->GetSlice(realslice);

					this->vInApply->GetValue(i,dblapply);
					if (dblapply >= 0.5) 
					{
						this->vInTorque->GetValue(i,torque);
						body->ApplyTorque(torque);
					}
				}
			}
		}


		void Box2dApplyTorqueNode::ConnectPin(IPluginIO^ Pin)
		{
			if (Pin == this->vInBodies) 
			{
				INodeIOBase^ usI;
				this->vInBodies->GetUpstreamInterface(usI);
				this->m_bodies = (BodyDataType^)usI;
			}
		}


		void Box2dApplyTorqueNode::DisconnectPin(IPluginIO^ Pin)
		{
			if (Pin == this->vInBodies)
        	{
        		this->m_bodies = nullptr;
        	}
		}
	}
}
