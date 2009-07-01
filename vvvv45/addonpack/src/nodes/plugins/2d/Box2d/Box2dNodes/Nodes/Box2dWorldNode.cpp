#include "StdAfx.h"
#include "Box2dWorldNode.h"

#include "../Internals/Data/BodyCustomData.h"

namespace VVVV 
{
	namespace Nodes 
	{
		Box2dWorldNode::Box2dWorldNode(void) 
		{
			this->mWorld = gcnew WorldDataType();
			this->mBodies = gcnew BodyDataType();
			this->mGround = gcnew GroundDataType();
			this->contacts = new vector<b2ContactPoint*>();
			this->MyListener = new ContactListener(this->contacts);
			this->mWorld->Contacts = this->contacts;
		}

		Box2dWorldNode::~Box2dWorldNode() 
		{
			if (this->internalworld != nullptr) 
			{
				delete this->internalworld;
			}
		}

		void Box2dWorldNode::SetPluginHost(IPluginHost^ Host)
		{
			this->FHost = Host;

			//Bounds
			this->FHost->CreateValueInput("Lower Bound",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInLowerBound);
			this->vInLowerBound->SetSubType2D(Double::MinValue,Double::MaxValue,0.01,-100.0,-100.0,false,false,false);

			this->FHost->CreateValueInput("Upper Bound",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInUpperBound);
			this->vInUpperBound->SetSubType2D(Double::MinValue,Double::MaxValue,0.01,100.0,100.0,false,false,false);

			//Gravity
			this->FHost->CreateValueInput("Gravity",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInGravity);
			this->vInGravity->SetSubType2D(Double::MinValue,Double::MaxValue,0.01,0,-10.0,false,false,false);

			this->FHost->CreateValueFastInput("Time Step",1,ArrayUtils::Array1D(),TSliceMode::Single,TPinVisibility::True,this->vInTimeStep);
			this->vInTimeStep->SetSubType(0,Double::MaxValue,0.01,0.01,false,false,false);

			this->FHost->CreateValueFastInput("Position Iterations",1,ArrayUtils::Array1D(),TSliceMode::Single,TPinVisibility::True,this->vInPosIterations);
			this->vInPosIterations->SetSubType(1,Double::MaxValue,1,8,false,false,true);

			this->FHost->CreateValueFastInput("Velocity Iterations",1,ArrayUtils::Array1D(),TSliceMode::Single,TPinVisibility::True,this->vInVelIterations);
			this->vInVelIterations->SetSubType(1,Double::MaxValue,1,10,false,false,true);

			//Is World Enabled
			this->FHost->CreateValueInput("Enabled",1,ArrayUtils::Array1D(),TSliceMode::Single,TPinVisibility::True,this->vInEnabled);
			this->vInEnabled->SetSubType(0,1,1,0,false,true,false);

			this->FHost->CreateValueInput("Reset",1,ArrayUtils::Array1D(),TSliceMode::Single,TPinVisibility::True,this->vInReset);
			this->vInReset->SetSubType(0,1,1,0,true,false,false);

			//Allow to put objects in sleep mode
			this->FHost->CreateValueInput("Allow Sleep",1,ArrayUtils::Array1D(),TSliceMode::Single,TPinVisibility::True,this->vInAllowSleep);
			this->vInAllowSleep->SetSubType(0,1,1,1,false,true,false);

			//World output
			this->FHost->CreateNodeOutput("World",TSliceMode::Single,TPinVisibility::True,this->vOutWorldNode);
			this->vOutWorldNode->SetSubType(ArrayUtils::SingleGuidArray(WorldDataType::GUID),WorldDataType::FriendlyName);
			this->vOutWorldNode->SetInterface(this->mWorld);

			this->FHost->CreateValueOutput("World Valid",1,ArrayUtils::Array1D(),TSliceMode::Single,TPinVisibility::True,this->vOutWorldValid);
			this->vOutWorldValid->SetSubType(0,1,1,0,false,true,false);

			this->FHost->CreateNodeOutput("Ground",TSliceMode::Single,TPinVisibility::True,this->vOutGround);
			this->vOutGround->SetSubType(ArrayUtils::SingleGuidArray(GroundDataType::GUID),GroundDataType::FriendlyName);
			this->vOutGround->SetInterface(this->mGround);

			this->FHost->CreateNodeOutput("Bodies",TSliceMode::Dynamic,TPinVisibility::True,this->vOutBodies);
			this->vOutBodies->SetSubType(ArrayUtils::SingleGuidArray(BodyDataType::GUID),BodyDataType::FriendlyName);
			this->vOutBodies->SetInterface(this->mBodies);
		}


		void Box2dWorldNode::Configurate(IPluginConfig^ Input) 
		{
			
		}

		
		void Box2dWorldNode::Evaluate(int SpreadMax) 
		{			
			double reset;
			this->vInReset->GetValue(0,reset);

			this->mWorld->Reset = false;

			//Reset World
			if (this->vInAllowSleep->PinIsChanged 
				|| this->vInLowerBound->PinIsChanged
				|| this->vInUpperBound->PinIsChanged
				|| reset >= 0.5) 
			{
				if (this->internalworld != nullptr) 
				{
					delete this->internalworld;
				}

				b2AABB worldAABB;
				double lbx,lby,ubx,uby,gx,gy,allowsleep;
				this->vInAllowSleep->GetValue(0, allowsleep);
				this->vInGravity->GetValue2D(0,gx,gy);
				this->vInLowerBound->GetValue2D(0,lbx,lby);
				this->vInUpperBound->GetValue2D(0,ubx,uby);

				worldAABB.lowerBound.Set(Convert::ToSingle(lbx),Convert::ToSingle(lby));
				worldAABB.upperBound.Set(Convert::ToSingle(ubx),Convert::ToSingle(uby));

				this->mWorld->SetIsValid(worldAABB.IsValid());

				if (this->mWorld->GetIsValid()) 
				{
					b2Vec2 gravity(Convert::ToSingle(gx),Convert::ToSingle(gy));
					bool dosleep = allowsleep >= 0.5;
					this->internalworld  = new b2World(worldAABB, gravity, dosleep);
					this->mWorld->SetWorld(this->internalworld);
					
					this->internalworld->SetContactListener(this->MyListener);
					this->mGround->SetGround(this->internalworld->GetGroundBody());
					this->mGround->SetIsValid(true);
					
				} 
				else 
				{
					this->internalworld = nullptr;
					this->mWorld->SetWorld(this->internalworld);
					this->mGround->SetIsValid(false);
					this->mGround->SetGround(nullptr);
				}

				this->mWorld->Reset = true;

				this->vOutWorldValid->SetValue(0, Convert::ToDouble(this->mWorld->GetIsValid()));

			}

			if (this->vInGravity->PinIsChanged) 
			{
				double gx,gy;
				this->vInGravity->GetValue2D(0,gx,gy);

				b2Vec2 gravity(Convert::ToSingle(gx),Convert::ToSingle(gy));
				this->internalworld->SetGravity(gravity);
			}

			//Enabled Change
			if (this->vInEnabled->PinIsChanged) 
			{
				double enabled;
				this->vInEnabled->GetValue(0, enabled);

				this->mWorld->SetIsEnabled(enabled >= 0.5);
			}

			//Process if enabled
			this->mBodies->Reset();
			this->contacts->clear();

			//Delete bodies marked as such
			if (this->mWorld->GetIsValid()) 
			{
				b2Body* node = this->mWorld->GetWorld()->GetBodyList();
				while (node)
				{
					b2Body* b = node;
					node = node->GetNext();
					if (b != this->mWorld->GetWorld()->GetGroundBody()) 
					{
						BodyCustomData* bdata = (BodyCustomData*)b->GetUserData();
						if (bdata->MarkedForDeletion) 
						{

							this->mWorld->GetWorld()->DestroyBody(b);
						}
					}    
				}

				if (this->mWorld->GetIsEnabled()) 
				{
					double ts,pit,vit;

					this->vInPosIterations->GetValue(0,pit);
					this->vInVelIterations->GetValue(0,vit);
					this->vInTimeStep->GetValue(0,ts);
					this->internalworld->Step(Convert::ToSingle(ts),Convert::ToInt32(vit),Convert::ToInt32(pit));
				}

				this->vOutBodies->MarkPinAsChanged();
				this->vOutBodies->SliceCount = this->mWorld->GetWorld()->GetBodyCount() -1;
				for (b2Body* b = this->mWorld->GetWorld()->GetBodyList(); b; b = b->GetNext())
				{
					if (b != this->mWorld->GetWorld()->GetGroundBody()) 
					{
						this->mBodies->Add(b);
					}
				}
			} 
			else 
			{
				this->vOutBodies->SliceCount = 0;
			}
		}


	}
}