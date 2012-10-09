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
			this->vInSetPosition->SetSubType(0,1,1.0,0.0,true,false,false);	

			this->FHost->CreateValueInput("Angle",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInAngle);
			this->vInAngle->SetSubType(Double::MinValue,Double::MaxValue,0.01,0.0,false,false,false);

			this->FHost->CreateValueInput("Set Angle",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInSetAngle);
			this->vInSetAngle->SetSubType(0,1,1.0,0.0,true,false,false);

			this->FHost->CreateValueInput("Velocity",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInVelocity);
			this->vInVelocity->SetSubType2D(Double::MinValue,Double::MaxValue,0.01,0.0,0.0,false,false,false);

			this->FHost->CreateValueInput("Set Velocity",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInSetVelocity);
			this->vInSetVelocity->SetSubType(0,1,1.0,0.0,true,false,false);	

			this->FHost->CreateValueInput("Angular Velocity",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInAngularVelocity);
			this->vInAngularVelocity->SetSubType(Double::MinValue,Double::MaxValue,0.01,0.0,false,false,false);

			this->FHost->CreateValueInput("Set Angular Velocity",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInSetAngularVelocity);
			this->vInSetAngularVelocity->SetSubType(0,1,1.0,0.0,true,false,false);

			this->FHost->CreateStringInput("Custom",TSliceMode::Dynamic,TPinVisibility::True,this->vInCustom);
			this->vInCustom->SetSubType("",false);

			this->FHost->CreateValueInput("Set Custom",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInSetCustom);
			this->vInSetCustom->SetSubType(0,1,1,0.0,true,false,false);

			this->FHost->CreateValueInput("Sleeping",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInSleeping);
			this->vInSleeping->SetSubType(0,1,1.0,0.0,false,true,false);

			this->FHost->CreateValueInput("Set Sleeping",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInSetSleeping);
			this->vInSetSleeping->SetSubType(0,1,1.0,0.0,true,false,false);

		}

		
		void Box2dUpdateBodyNode::Configurate(IPluginConfig^ Input)
		{

		}

		
		void Box2dUpdateBodyNode::Evaluate(int SpreadMax)
		{
			this->vInBodies->PinIsChanged;
			if (this->vInBodies->IsConnected) 
			{
				double dblsp,dblsv,dblsav,dblsc,dblsa,dblsleep,dblsetsleep;
				for (int i = 0; i < this->vInBodies->SliceCount; i++) 
				{
					int realslice;
					this->vInBodies->GetUpsreamSlice(i % this->vInBodies->SliceCount,realslice);
					b2Body* body = this->m_bodies->GetSlice(realslice);
					BodyCustomData* bdata = (BodyCustomData*)body->GetUserData();

					int id = bdata->Id;

					this->vInSetPosition->GetValue(i,dblsp);
					this->vInSetAngle->GetValue(i, dblsa);
					if (dblsp >= 0.5 || dblsa >= 0.5) 
					{
						double x,y,a;

						if (dblsp)
						{
							this->vInPosition->GetValue2D(i,x,y);
						}
						else
						{
							x = body->GetPosition().x;
							y = body->GetPosition().y;
						}

						if (dblsa)
						{
							this->vInAngle->GetValue(i, a);
							a = a * (float)System::Math::PI;
						}
						else 
						{
							a = body->GetAngle();
						}

						body->SetTransform(b2Vec2(Convert::ToSingle(x),Convert::ToSingle(y)),Convert::ToSingle(a));
					}

					this->vInSetVelocity->GetValue(i,dblsv);
					if (dblsv >= 0.5) 
					{
						double x,y;
						this->vInVelocity->GetValue2D(i,x,y);
						body->SetLinearVelocity(b2Vec2(Convert::ToSingle(x),Convert::ToSingle(y)));
					}

					this->vInSetAngularVelocity->GetValue(i,dblsav);
					if (dblsav >= 0.5) 
					{
						double a;
						this->vInAngularVelocity->GetValue(i,a);
						body->SetAngularVelocity(Convert::ToSingle(a));
					}

					this->vInSetCustom->GetValue(i,dblsc);
					if (dblsc >= 0.5)
					{
						String^ cust;
						this->vInCustom->GetString(i,cust);
						BodyCustomData* bdata = (BodyCustomData*)body->GetUserData();
						bdata->Custom = (char*)(void*)Marshal::StringToHGlobalAnsi(cust);
					}

					this->vInSetSleeping->GetValue(i, dblsetsleep);
					if (dblsetsleep >= 0.5)
					{
						this->vInSleeping->GetValue(i,dblsleep);
						body->SetAwake(dblsleep < 0.5);
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
