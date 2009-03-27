#include "StdAfx.h"
#include "Box2dUpdateBodyNode.h"

#include "../../Internals/Data/BodyCustomData.h"

namespace VVVV 
{
	namespace Nodes 
	{
		Box2dUpdateBodyNode::Box2dUpdateBodyNode(void)
		{
		}

		void Box2dUpdateBodyNode::SetPluginHost(IPluginHost^ Host) 
		{
			this->FHost = Host;

			this->FHost->CreateNodeInput("Bodies",TSliceMode::Dynamic,TPinVisibility::True,this->vInBodies);
			this->vInBodies->SetSubType(ArrayUtils::SingleGuidArray(BodyDataType::GUID),BodyDataType::FriendlyName);

			this->FHost->CreateValueInput("Position",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInPosition);
			this->vInPosition->SetSubType2D(Double::MinValue,Double::MaxValue,0.01,0.0,0.0,false,false,false);

			this->FHost->CreateValueInput("Set Position",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInSetPosition);
			this->vInSetPosition->SetSubType(Double::MinValue,Double::MaxValue,0.01,0.0,true,false,false);	

			this->FHost->CreateValueInput("Velocity",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInVelocity);
			this->vInVelocity->SetSubType2D(Double::MinValue,Double::MaxValue,0.01,0.0,0.0,false,false,false);

			this->FHost->CreateValueInput("Set Velocity",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInSetVelocity);
			this->vInSetVelocity->SetSubType(Double::MinValue,Double::MaxValue,0.01,0.0,true,false,false);	

			this->FHost->CreateValueInput("Angular Velocity",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInAngularVelocity);
			this->vInAngularVelocity->SetSubType(Double::MinValue,Double::MaxValue,0.01,0.0,false,false,false);

			this->FHost->CreateValueInput("Set Angular Velocity",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInSetAngularVelocity);
			this->vInSetAngularVelocity->SetSubType(Double::MinValue,Double::MaxValue,0.01,0.0,true,false,false);	
		}

		void Box2dUpdateBodyNode::Configurate(IPluginConfig^ Input)
		{

		}

		
		void Box2dUpdateBodyNode::Evaluate(int SpreadMax)
		{
			this->vInBodies->PinIsChanged;
			if (this->vInBodies->IsConnected) 
			{
				double dblsp,dblsv,dblsav;
				for (int i = 0; i < this->vInBodies->SliceCount; i++) 
				{
					int realslice;
					this->vInBodies->GetUpsreamSlice(i,realslice);
					b2Body* body = this->m_bodies->GetSlice(realslice);
					BodyCustomData* bdata = (BodyCustomData*)body->GetUserData();

					int id = bdata->Id;

					this->vInSetPosition->GetValue(i,dblsp);
					if (dblsp >= 0.5) 
					{
						double x,y;
						this->vInPosition->GetValue2D(i,x,y);
						body->SetXForm(b2Vec2(x,y),body->GetAngle());
					}

					this->vInSetVelocity->GetValue(i,dblsv);
					if (dblsv >= 0.5) 
					{
						double x,y;
						this->vInVelocity->GetValue2D(i,x,y);
						body->SetLinearVelocity(b2Vec2(x,y));
					}

					this->vInSetAngularVelocity->GetValue(i,dblsav);
					if (dblsav >= 0.5) 
					{
						double a;
						this->vInAngularVelocity->GetValue(i,a);
						body->SetAngularVelocity(a);
					}
				}
			}
		}


		void Box2dUpdateBodyNode::ConnectPin(IPluginIO^ Pin)
		{
			if (Pin == this->vInBodies) 
			{
				INodeIOBase^ usI;
				this->vInBodies->GetUpstreamInterface(usI);
				this->m_bodies = (BodyDataType^)usI;
			}
		}

		void Box2dUpdateBodyNode::DisconnectPin(IPluginIO^ Pin)
		{
			if (Pin == this->vInBodies)
        	{
        		this->m_bodies = nullptr;
        	}
		}
	}
}
