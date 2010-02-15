#include "StdAfx.h"
#include "Box2dCreateJointNode.h"

namespace VVVV 
{
	namespace Nodes 
	{
		Box2dCreateJointNode::Box2dCreateJointNode(void)
		{

		}

		void Box2dCreateJointNode::SetPluginHost(IPluginHost^ Host)
		{
			this->FHost = Host;

			this->FHost->CreateNodeInput("World",TSliceMode::Single,TPinVisibility::True,this->vInWorld);
			this->vInWorld->SetSubType(ArrayUtils::SingleGuidArray(WorldDataType::GUID),WorldDataType::FriendlyName);


			this->FHost->CreateNodeInput("Body 1",TSliceMode::Dynamic,TPinVisibility::True,this->vInBody1);
			this->vInBody1->SetSubType(ArrayUtils::SingleGuidArray(BodyDataType::GUID),BodyDataType::FriendlyName);

			if (!this->ForceBodyOneGround())
			{
				this->FHost->CreateNodeInput("Body 2",TSliceMode::Dynamic,TPinVisibility::True,this->vInBody2);
				this->vInBody2->SetSubType(ArrayUtils::SingleGuidArray(BodyDataType::GUID),BodyDataType::FriendlyName);
			}

			this->FHost->CreateValueInput("Collide Connected",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInCollideConnected);
			this->vInCollideConnected->SetSubType(Double::MinValue,Double::MaxValue,0.01,0.0,false,true,false);

		
			//Output
			this->OnPluginHostSet();

			this->FHost->CreateStringInput("Custom",TSliceMode::Dynamic,TPinVisibility::True,this->vInCustom);
			this->vInCustom->SetSubType("",false);

			this->FHost->CreateValueInput("Do Create",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInDoCreate);
			this->vInDoCreate->SetSubType(Double::MinValue,Double::MaxValue,0.01,0.0,true,false,false);

		}

		void Box2dCreateJointNode::Configurate(IPluginConfig^ Input) 
		{
		}

		void Box2dCreateJointNode::ConnectPin(IPluginIO^ Pin)
		{
			if (Pin == this->vInWorld)
        	{
				INodeIOBase^ usI;
				this->vInWorld->GetUpstreamInterface(usI);
				this->mWorld = (WorldDataType^)usI;
        	}
			if (Pin == this->vInBody1) 
			{
				INodeIOBase^ usI;
				this->vInBody1->GetUpstreamInterface(usI);
				this->m_body1 = (BodyDataType^)usI;
			}

			if (Pin == this->vInBody2) 
			{
				INodeIOBase^ usI;
				this->vInBody2->GetUpstreamInterface(usI);
				this->m_body2 = (BodyDataType^)usI;
			}
		}




		void Box2dCreateJointNode::DisconnectPin(IPluginIO^ Pin)
		{
			if (Pin == this->vInWorld)
        	{
        		this->mWorld = nullptr;
        	}
			if (Pin == this->vInBody1)
        	{
        		this->m_body1 = nullptr;
        	}
			if (Pin == this->vInBody2)
        	{
        		this->m_body2 = nullptr;
        	}
		}

	}
}

