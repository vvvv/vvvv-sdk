#include "StdAfx.h"
#include "Box2dContactDetailsNode.h"

namespace VVVV 
{
	namespace Nodes 
	{
		Box2dContactDetailsNode::Box2dContactDetailsNode(void)
		{
			this->m_shapes1 = gcnew ShapeDataType();
			this->m_shapes2 = gcnew ShapeDataType();
		}

		void Box2dContactDetailsNode::SetPluginHost(IPluginHost^ Host) 
		{
			this->FHost = Host;
	
			this->FHost->CreateNodeInput("World",TSliceMode::Dynamic,TPinVisibility::True,this->vInWorld);
			this->vInWorld->SetSubType(ArrayUtils::SingleGuidArray(WorldDataType::GUID),WorldDataType::FriendlyName);

			this->FHost->CreateValueOutput("X",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutPositionX);
			this->vOutPositionX->SetSubType(Double::MinValue,Double::MaxValue,0.01,0.0,false,false,false);

			this->FHost->CreateValueOutput("Y",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutPositionY);
			this->vOutPositionY->SetSubType(Double::MinValue,Double::MaxValue,0.01,0.0,false,false,false);

			this->FHost->CreateNodeOutput("Shapes 1",TSliceMode::Dynamic,TPinVisibility::True,this->vOutShapes1);
			this->vOutShapes1->SetSubType(ArrayUtils::SingleGuidArray(ShapeDataType::GUID),ShapeDataType::FriendlyName);
			this->vOutShapes1->SetInterface(this->m_shapes1);

			this->FHost->CreateNodeOutput("Shapes 2",TSliceMode::Dynamic,TPinVisibility::True,this->vOutShapes2);
			this->vOutShapes2->SetSubType(ArrayUtils::SingleGuidArray(ShapeDataType::GUID),ShapeDataType::FriendlyName);
			this->vOutShapes2->SetInterface(this->m_shapes2);
		}


		void Box2dContactDetailsNode::Configurate(IPluginConfig^ Input)
		{

		}

		
		void Box2dContactDetailsNode::Evaluate(int SpreadMax)
		{
			if (this->vInWorld->IsConnected) 
			{		
				if (m_world->GetIsValid()) 
				{
					this->m_shapes1->Reset();
					this->m_shapes2->Reset();

					this->vOutPositionX->SliceCount = this->m_world->Contacts->size();
					this->vOutPositionY->SliceCount = this->m_world->Contacts->size();
					this->vOutShapes1->SliceCount = this->m_world->Contacts->size();
					this->vOutShapes2->SliceCount = this->m_world->Contacts->size();
	
					for (int i = 0; i < this->m_world->Contacts->size(); i++) 
					{
						b2ContactPoint* contact = this->m_world->Contacts->at(i);

						this->vOutPositionX->SetValue(i,contact->position.x);
						this->vOutPositionY->SetValue(i,contact->position.y);
						this->m_shapes1->Add(contact->shape1);
						this->m_shapes2->Add(contact->shape2);
					}
				}
				else 
				{
					this->vOutPositionX->SliceCount = 0;
					this->vOutPositionY->SliceCount = 0;
					this->vOutShapes1->SliceCount = 0;
					this->vOutShapes2->SliceCount = 0;
				}
			} 
			else 
			{
				this->vOutPositionX->SliceCount = 0;
				this->vOutPositionY->SliceCount = 0;
				this->vOutShapes1->SliceCount = 0;
				this->vOutShapes2->SliceCount = 0;
			}
		}


		void Box2dContactDetailsNode::ConnectPin(IPluginIO^ Pin)
		{
			if (Pin == this->vInWorld) 
			{
				INodeIOBase^ usI;
				this->vInWorld->GetUpstreamInterface(usI);
				this->m_world = (WorldDataType^)usI;
			}
		}

		void Box2dContactDetailsNode::DisconnectPin(IPluginIO^ Pin)
		{
			if (Pin == this->vInWorld)
        	{
        		this->m_world = nullptr;
        	}
		}
	}
}
