#include "StdAfx.h"
#include "Box2dContactDetailsNode.h"
#include "../Internals/Data/ShapeCustomData.h"

namespace VVVV 
{
	namespace Nodes 
	{
		Box2dContactDetailsNode::Box2dContactDetailsNode(void)
		{
			this->m_shape1 = gcnew ShapeDataType();
			this->m_shape2 = gcnew ShapeDataType();
			this->mBody1 = gcnew BodyDataType();
			this->mBody2 = gcnew BodyDataType();
		}

		void Box2dContactDetailsNode::SetPluginHost(IPluginHost^ Host) 
		{
			this->FHost = Host;
	
			this->FHost->CreateNodeInput("World",TSliceMode::Dynamic,TPinVisibility::True,this->vInWorld);
			this->vInWorld->SetSubType(ArrayUtils::SingleGuidArray(WorldDataType::GUID),WorldDataType::FriendlyName);

			this->FHost->CreateValueOutput("Position",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutPosition);
			this->vOutPosition->SetSubType2D(Double::MinValue,Double::MaxValue,0.01,0.0,0.0,false,false,false);

			this->FHost->CreateValueOutput("Normals",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutNormals);
			this->vOutNormals->SetSubType2D(Double::MinValue,Double::MaxValue,0.01,0.0,0.0,false,false,false);

			this->FHost->CreateNodeOutput("Shape 1",TSliceMode::Dynamic,TPinVisibility::True,this->vOutShape1);
			this->vOutShape1->SetSubType(ArrayUtils::SingleGuidArray(ShapeDataType::GUID),ShapeDataType::FriendlyName);
			this->vOutShape1->SetInterface(this->m_shape1);
		
			this->FHost->CreateNodeOutput("Shape 2",TSliceMode::Dynamic,TPinVisibility::True,this->vOutShape2);
			this->vOutShape2->SetSubType(ArrayUtils::SingleGuidArray(ShapeDataType::GUID),ShapeDataType::FriendlyName);
			this->vOutShape2->SetInterface(this->m_shape2);

			this->FHost->CreateNodeOutput("Body 1",TSliceMode::Dynamic,TPinVisibility::True,this->vOutBody1);
			this->vOutBody1->SetSubType(ArrayUtils::SingleGuidArray(BodyDataType::GUID),BodyDataType::FriendlyName);
			this->vOutBody1->SetInterface(this->mBody1);

			this->FHost->CreateNodeOutput("Body 2",TSliceMode::Dynamic,TPinVisibility::True,this->vOutBody2);
			this->vOutBody2->SetSubType(ArrayUtils::SingleGuidArray(BodyDataType::GUID),BodyDataType::FriendlyName);
			this->vOutBody2->SetInterface(this->mBody2);

			this->FHost->CreateValueOutput("Shapes 1",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutShapes1);
			this->vOutShapes1->SetSubType(Double::MinValue,Double::MaxValue,1,0.0,false,false,true);

			this->FHost->CreateValueOutput("Shapes 2",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutShapes2);
			this->vOutShapes2->SetSubType(Double::MinValue,Double::MaxValue,1,0.0,false,false,true);

			this->FHost->CreateValueOutput("Is New",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutNew);
			this->vOutNew->SetSubType(Double::MinValue,Double::MaxValue,0.01,0.0,true,false,false);	

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
					this->vOutNormals->SliceCount = this->m_world->Contacts->size();
					this->vOutShapes1->SliceCount = this->m_world->Contacts->size();
					this->vOutShapes2->SliceCount = this->m_world->Contacts->size();
					this->vOutShape1->SliceCount = this->m_world->Contacts->size();
					this->vOutShape2->SliceCount = this->m_world->Contacts->size();
					this->vOutBody1->SliceCount = this->m_world->Contacts->size();
					this->vOutBody2->SliceCount = this->m_world->Contacts->size();
					this->vOutNew->SliceCount = this->m_world->Contacts->size();
					this->m_shape1->Reset();
					this->m_shape2->Reset();
					this->mBody1->Reset();
					this->mBody2->Reset();
	
					for (int i = 0; i < this->m_world->Contacts->size(); i++) 
					{
						b2Contact* contact = this->m_world->Contacts->at(i);
						b2Manifold* man = contact->GetManifold();

						b2WorldManifold worldManifold;
						contact->GetWorldManifold(&worldManifold);

						this->vOutPosition->SetValue2D(i,worldManifold.localPoint.x,worldManifold.localPoint.y);
						this->vOutNormals->SetValue2D(i,worldManifold.localNormal.x,worldManifold.localNormal.y);
						

						ShapeCustomData* sdata = (ShapeCustomData*)contact->GetFixtureA()->GetUserData();
						this->vOutShapes1->SetValue(i,sdata->Id);
						sdata = (ShapeCustomData*)contact->GetFixtureB()->GetUserData();
						this->vOutShapes2->SetValue(i,sdata->Id);
						this->vOutNew->SetValue(i,this->m_world->Newcontacts->at(i));

						this->m_shape1->Add(contact->GetFixtureA());
						this->m_shape2->Add(contact->GetFixtureB());
						this->mBody1->Add(contact->GetFixtureA()->GetBody());
						this->mBody2->Add(contact->GetFixtureB()->GetBody());

						this->vOutShape1->MarkPinAsChanged();
						this->vOutShape2->MarkPinAsChanged();
						this->vOutBody1->MarkPinAsChanged();
						this->vOutBody2->MarkPinAsChanged();

					}
				}
				else 
				{
					this->vOutPosition->SliceCount = 0;
					this->vOutNormals->SliceCount = 0;
					this->vOutShapes1->SliceCount = 0;
					this->vOutShapes2->SliceCount = 0;
					this->vOutNew->SliceCount = 0;
				}
			} 
			else 
			{
				this->vOutPosition->SliceCount = 0;
				this->vOutNormals->SliceCount = 0;
				this->vOutShapes1->SliceCount = 0;
				this->vOutShapes2->SliceCount = 0;
				this->vOutNew->SliceCount = 0;
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
