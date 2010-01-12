#include "StdAfx.h"
#include "Box2dApplyForceNode.h"

#include "../../Internals/Data/BodyCustomData.h"

namespace VVVV 
{
	namespace Nodes 
	{
		Box2dApplyForceNode::Box2dApplyForceNode(void)
		{
		}

		void Box2dApplyForceNode::SetPluginHost(IPluginHost^ Host) 
		{
			this->FHost = Host;

			this->FHost->CreateNodeInput("Bodies",TSliceMode::Dynamic,TPinVisibility::True,this->vInBodies);
			this->vInBodies->SetSubType(ArrayUtils::SingleGuidArray(BodyDataType::GUID),BodyDataType::FriendlyName);

			this->FHost->CreateValueInput("Force",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInForce);
			this->vInForce->SetSubType2D(Double::MinValue,Double::MaxValue,0.01,0.0,0.0,false,false,false);

			this->FHost->CreateValueInput("Position",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInPosition);
			this->vInPosition->SetSubType2D(Double::MinValue,Double::MaxValue,0.01,0.0,0.0,false,false,false);

			this->FHost->CreateValueInput("Apply",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInApply);
			this->vInApply->SetSubType(0,1,1,0.0,true,false,false);	

		}

		void Box2dApplyForceNode::Configurate(IPluginConfig^ Input)
		{

		}

		
		void Box2dApplyForceNode::Evaluate(int SpreadMax)
		{
			this->vInBodies->PinIsChanged;
			if (this->vInBodies->IsConnected) 
			{
				double dblapply,fx,fy,px,py;
				for (int i = 0; i < this->vInBodies->SliceCount; i++) 
				{
					int realslice;
					this->vInBodies->GetUpsreamSlice(i % this->vInBodies->SliceCount,realslice);
					b2Body* body = this->m_bodies->GetSlice(realslice);

					this->vInApply->GetValue(i,dblapply);
					if (dblapply >= 0.5) 
					{
						this->vInPosition->GetValue2D(i,px,py);
						this->vInForce->GetValue2D(i,fx,fy);
						body->ApplyForce(b2Vec2(fx,fy),b2Vec2(px,py));
					}
				}
			}
		}


		void Box2dApplyForceNode::ConnectPin(IPluginIO^ Pin)
		{
			if (Pin == this->vInBodies) 
			{
				INodeIOBase^ usI;
				this->vInBodies->GetUpstreamInterface(usI);
				this->m_bodies = (BodyDataType^)usI;
			}
		}


		void Box2dApplyForceNode::DisconnectPin(IPluginIO^ Pin)
		{
			if (Pin == this->vInBodies)
        	{
        		this->m_bodies = nullptr;
        	}
		}
	}
}
