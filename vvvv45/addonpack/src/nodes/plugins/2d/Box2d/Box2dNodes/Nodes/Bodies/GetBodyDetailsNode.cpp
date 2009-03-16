#include "StdAfx.h"
#include "GetBodyDetailsNode.h"
#include "../../Internals/Data/BodyCustomData.h"

namespace VVVV 
{
	namespace Nodes 
	{

		GetBodyDetailsNode::GetBodyDetailsNode(void)
		{
			this->m_shapes = gcnew ShapeDataType();
		}

		void GetBodyDetailsNode::SetPluginHost(IPluginHost^ Host) 
		{
			this->FHost = Host;

			this->FHost->CreateNodeInput("Bodies",TSliceMode::Dynamic,TPinVisibility::True,this->vInBodies);
			this->vInBodies->SetSubType(ArrayUtils::SingleGuidArray(BodyDataType::GUID),BodyDataType::FriendlyName);

			this->FHost->CreateValueOutput("Position",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutPosition);
			this->vOutPosition->SetSubType2D(Double::MinValue,Double::MaxValue,0.01,0.0,0.0,false,false,false);

			this->FHost->CreateValueOutput("Rotation",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutRotation);
			this->vOutRotation->SetSubType(Double::MinValue,Double::MaxValue,0.01,0.0,false,false,false);

			this->FHost->CreateValueOutput("Is Dynamic",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutIsDynamic);
			this->vOutIsDynamic->SetSubType(Double::MinValue,Double::MaxValue,1,0.0,false,true,false);

			this->FHost->CreateNodeOutput("Shapes",TSliceMode::Dynamic,TPinVisibility::True,this->vOutShapes);
			this->vOutShapes->SetSubType(ArrayUtils::SingleGuidArray(ShapeDataType::GUID),ShapeDataType::FriendlyName);
			this->vOutShapes->SetInterface(this->m_shapes);

			this->FHost->CreateValueOutput("Body Id",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutId);
			this->vOutId->SetSubType(Double::MinValue,Double::MaxValue,1,0.0,false,false,true);

			//this->FHost->CreateValueOutput("Velocity",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutVelocity);
			//this->vOutVelocity->SetSubType2D(Double::MinValue,Double::MaxValue,0.01,0.0,0.0,false,false,false);
		}


		void GetBodyDetailsNode::Configurate(IPluginConfig^ Input)
		{

		}

		
		void GetBodyDetailsNode::Evaluate(int SpreadMax)
		{
			if (this->vInBodies->IsConnected) 
			{
				
				this->m_shapes->Reset();

				int cnt = this->vInBodies->SliceCount;
				this->vOutPosition->SliceCount = cnt;
				this->vOutRotation->SliceCount = cnt;
				this->vOutIsDynamic->SliceCount = cnt;
				this->vOutId->SliceCount = cnt;

				for (int i = 0; i < this->vInBodies->SliceCount; i++) 
				{
					int realslice;
					this->vInBodies->GetUpsreamSlice(i,realslice);
					b2Body* body = this->m_bodies->GetSlice(realslice);

					b2Vec2 pos = body->GetPosition();
					this->vOutPosition->SetValue2D(i, pos.x,pos.y);
					this->vOutRotation->SetValue(i,body->GetAngle() / (Math::PI * 2.0));
					this->vOutIsDynamic->SetValue(i,Convert::ToInt32(body->IsDynamic()));

					BodyCustomData* bdata = (BodyCustomData*)body->GetUserData();
					this->vOutId->SetValue(i, bdata->Id);

					for (b2Shape* s = body->GetShapeList(); s; s = s->GetNext())
					{
						if (s->GetType() == e_circleShape || s->GetType() == e_polygonShape) 
						{
							this->m_shapes->Add(s);
						}
					}
				}

				this->vOutShapes->SliceCount = this->m_shapes->Count();
			}
		}


		void GetBodyDetailsNode::ConnectPin(IPluginIO^ Pin)
		{
			if (Pin == this->vInBodies) 
			{
				INodeIOBase^ usI;
				this->vInBodies->GetUpstreamInterface(usI);
				this->m_bodies = (BodyDataType^)usI;
			}
		}

		void GetBodyDetailsNode::DisconnectPin(IPluginIO^ Pin)
		{
			if (Pin == this->vInBodies)
        	{
        		this->m_bodies = nullptr;
        	}
		}
	}
}
