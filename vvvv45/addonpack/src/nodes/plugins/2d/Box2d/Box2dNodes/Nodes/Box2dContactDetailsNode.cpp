#include "StdAfx.h"
#include "Box2dContactDetailsNode.h"
#include "../Internals/Data/ShapeCustomData.h"

namespace VVVV 
{
	namespace Nodes 
	{
		Box2dContactDetailsNode::Box2dContactDetailsNode(void)
		{
		}

		void Box2dContactDetailsNode::SetPluginHost(IPluginHost^ Host) 
		{
			this->FHost = Host;
	
			this->FHost->CreateNodeInput("World",TSliceMode::Dynamic,TPinVisibility::True,this->vInWorld);
			this->vInWorld->SetSubType(ArrayUtils::SingleGuidArray(WorldDataType::GUID),WorldDataType::FriendlyName);

			this->FHost->CreateValueOutput("Position",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutPosition);
			this->vOutPosition->SetSubType(Double::MinValue,Double::MaxValue,0.01,0.0,false,false,false);

			this->FHost->CreateValueOutput("Shapes 1",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutShapes1);
			this->vOutShapes1->SetSubType(Double::MinValue,Double::MaxValue,1,0.0,false,false,true);

			this->FHost->CreateValueOutput("Shapes 2",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutShapes2);
			this->vOutShapes2->SetSubType(Double::MinValue,Double::MaxValue,1,0.0,false,false,true);

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
					this->vOutPosition->SliceCount = this->m_world->Contacts->size();
					this->vOutShapes1->SliceCount = this->m_world->Contacts->size();
					this->vOutShapes2->SliceCount = this->m_world->Contacts->size();
	
					for (int i = 0; i < this->m_world->Contacts->size(); i++) 
					{
						b2ContactPoint* contact = this->m_world->Contacts->at(i);

						this->vOutPosition->SetValue2D(i,contact->position.x,contact->position.y);

						ShapeCustomData* sdata = (ShapeCustomData*)contact->shape1->GetUserData();
						this->vOutShapes1->SetValue(i,sdata->Id);
						sdata = (ShapeCustomData*)contact->shape2->GetUserData();
						this->vOutShapes2->SetValue(i,sdata->Id);
					}
				}
				else 
				{
					this->vOutPosition->SliceCount = 0;
					this->vOutShapes1->SliceCount = 0;
					this->vOutShapes2->SliceCount = 0;
				}
			} 
			else 
			{
				this->vOutPosition->SliceCount = 0;
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
