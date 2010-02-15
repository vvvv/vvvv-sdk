#include "StdAfx.h"
#include "Box2dCreateShapeNode.h"

namespace VVVV 
{
	namespace Nodes 
	{
		Box2dCreateShapeNode::Box2dCreateShapeNode(void)
		{
		}

		void Box2dCreateShapeNode::SetPluginHost(IPluginHost^ Host) 
		{
			this->FHost = Host;

			//World input
			this->FHost->CreateNodeInput("World",TSliceMode::Single,TPinVisibility::True,this->vInWorld);
			this->vInWorld->SetSubType(ArrayUtils::SingleGuidArray(WorldDataType::GUID),WorldDataType::FriendlyName);

			this->FHost->CreateNodeInput("Bodies",TSliceMode::Dynamic,TPinVisibility::True,this->vInBodies);
			this->vInBodies->SetSubType(ArrayUtils::SingleGuidArray(BodyDataType::GUID),BodyDataType::FriendlyName);

			this->FHost->CreateNodeInput("Shapes",TSliceMode::Dynamic,TPinVisibility::True,this->vInShapes);
			this->vInShapes->SetSubType(ArrayUtils::SingleGuidArray(ShapeDefDataType::GUID),ShapeDefDataType::FriendlyName);

			this->FHost->CreateValueInput("Shape Count",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInShapeCount);
			this->vInShapeCount->SetSubType(1,Double::MaxValue,1,1,false,false,true);

			this->FHost->CreateValueInput("Do Create",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInDoCreate);
			this->vInDoCreate->SetSubType(Double::MinValue,Double::MaxValue,0.01,0.0,true,false,false);

			//this->FHost->CreateValueOutput("Can Create",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutCanCreate);
			//this->vOutCanCreate->SetSubType(0,1,1,0,true,false,false);


			//this->FHost->CreateNodeOutput("Body",TSliceMode::Dynamic,TPinVisibility::True,this->vOutBodies);
			//this->vOutBodies->SetSubType(ArrayUtils::SingleGuidArray(BodyDataType::GUID),BodyDataType::FriendlyName);
			//this->vOutBodies->SetInterface(this->mBodies);

			
		}

		void Box2dCreateShapeNode::Evaluate(int SpreadMax)
		{
			double dblcreate;
			this->vInDoCreate->GetValue(0,dblcreate);


			if (dblcreate >= 0.5 && this->vInBodies->IsConnected && this->vInShapes->IsConnected && this->vInWorld->IsConnected) 
			{
				if (this->mWorld->GetIsValid()) 
				{
					int cnt = 0;
					for (int i = 0; i < vInBodies->SliceCount; i++) 
					{
						double dblcount;

						this->vInShapeCount->GetValue(i, dblcount);
						int icount = System::Convert::ToInt32(dblcount);
						
						b2Body* body;

						int realslicebody;
						this->vInBodies->GetUpsreamSlice(i % this->vInBodies->SliceCount,realslicebody);
						body = this->mBodies->GetSlice(realslicebody);
								
						for (int j = 0; j < icount ; j++)
						{
							int realsliceshape;
							this->vInShapes->GetUpsreamSlice(cnt % this->vInShapes->SliceCount,realsliceshape);
							b2FixtureDef* shapedef = this->mShapes->GetSlice(realsliceshape);
							String^ shapecust = this->mShapes->GetCustom(realsliceshape);

							/*
							bool testcount;
							if (shapedef->type == e_edgeShape)
							{
								b2EdgeChainDef* chain = (b2EdgeChainDef*)shapedef;
								int vcount = chain->vertexCount;
								if (chain->isALoop)
								{
									vcount++;
								}

								testcount = this->mWorld->GetWorld()->GetProxyCount() + vcount <= b2_maxProxies;
	
							}
							else
							{
								testcount = this->mWorld->GetWorld()->GetProxyCount() < b2_maxProxies;
							}*/

							if (true)
							{
								b2Fixture* shape = body->CreateFixture(shapedef);
								ShapeCustomData* sdata = new ShapeCustomData();
								sdata->Id = this->mWorld->GetNewShapeId();
								sdata->Custom = (char*)(void*)Marshal::StringToHGlobalAnsi(shapecust);
								shape->SetUserData(sdata);
							}

							cnt++;
						}

					}
				}

				
			} 

		}



		void Box2dCreateShapeNode::Configurate(IPluginConfig^ Input)
		{

		}

		void Box2dCreateShapeNode::ConnectPin(IPluginIO^ Pin)
		{
			//cache a reference to the upstream interface when the NodeInput pin is being connected
			if (Pin == this->vInWorld)
        	{
				INodeIOBase^ usI;
				this->vInWorld->GetUpstreamInterface(usI);
				this->mWorld = (WorldDataType^)usI;
        	}
        	if (Pin == this->vInBodies)
        	{
				INodeIOBase^ usI;
				this->vInBodies->GetUpstreamInterface(usI);
				this->mBodies = (BodyDataType^)usI;
        	}
			if (Pin == this->vInShapes) 
			{
				INodeIOBase^ usI;
				this->vInShapes->GetUpstreamInterface(usI);
				this->mShapes = (ShapeDefDataType^)usI;
			}
		}

		void Box2dCreateShapeNode::DisconnectPin(IPluginIO^ Pin)
		{
			if (Pin == this->vInWorld)
        	{
        		this->mWorld = nullptr;
        	}
			if (Pin == this->vInBodies)
        	{
        		this->mBodies = nullptr;
        	}
			if (Pin == this->vInShapes)
        	{
        		this->mShapes = nullptr;
        	}
		}
	}
}
