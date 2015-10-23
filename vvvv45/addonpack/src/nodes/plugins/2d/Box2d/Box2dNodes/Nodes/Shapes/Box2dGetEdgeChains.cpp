#include "StdAfx.h"
#include "Box2dGetEdgeChains.h"
#include "../../Internals/Data/ShapeCustomData.h"

using namespace System::Collections::Generic;
using namespace VVVV::Utils::VMath;

namespace VVVV 
{
	namespace Nodes 
	{
		Box2dGetEdgeChains::Box2dGetEdgeChains(void)
		{
		}

		void Box2dGetEdgeChains::SetPluginHost(IPluginHost^ Host) 
		{
			this->FHost = Host;

			this->FHost->CreateNodeInput("Shapes",TSliceMode::Dynamic,TPinVisibility::True,this->vInShapes);
			this->vInShapes->SetSubType(ArrayUtils::SingleGuidArray(ShapeDataType::GUID),ShapeDataType::FriendlyName);

			this->FHost->CreateValueInput("Local Coordinates",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInLocal);
			this->vInLocal->SetSubType(0,1,1,1,false,true,false);


			//this->FHost->CreateValueOutput("Centers",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutCenters);
			//this->vOutCenters->SetSubType2D(Double::MinValue,Double::MaxValue,0.01,0.0,0.0,false,false,false);

			this->FHost->CreateValueOutput("Vertices 1",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutVertices1);
			this->vOutVertices1->SetSubType2D(Double::MinValue,Double::MaxValue,0.01,0.0,0.0,false,false,false);

			this->FHost->CreateValueOutput("Vertices 2",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutVertices2);
			this->vOutVertices2->SetSubType2D(Double::MinValue,Double::MaxValue,0.01,0.0,0.0,false,false,false);

			this->FHost->CreateValueOutput("Is Sensor",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutIsSensor);
			this->vOutIsSensor->SetSubType(Double::MinValue,Double::MaxValue,0.01,0.0,false,true,false);


			//this->FHost->CreateValueOutput("Shape Id",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutId);
			//this->vOutId->SetSubType(Double::MinValue,Double::MaxValue,1,0.0,false,false,true);
		}


		void Box2dGetEdgeChains::Configurate(IPluginConfig^ Input)
		{

		}

		
		void Box2dGetEdgeChains::Evaluate(int SpreadMax)
		{
			if (this->vInLocal->PinIsChanged)
			{
				double dbllocal;
				this->vInLocal->GetValue(0,dbllocal);

				this->m_local = dbllocal >= 0.5;
			}

			if (this->vInShapes->IsConnected) 
			{
				std::vector<b2Vec2> vertices1;
				std::vector<b2Vec2> vertices2;
				std::vector<bool> issensor;
				int cnt = 0;
				for (int i = 0; i < this->vInShapes->SliceCount ; i++) 
				{
					int realslice;
					this->vInShapes->GetUpsreamSlice(i,realslice);
					b2Shape* shape = this->m_polygons->GetSlice(realslice);
					
					if (shape->GetType() == e_edgeShape) 
					{
						b2EdgeShape* poly = (b2EdgeShape*)shape;

						if (this->m_local)
						{
							vertices1.push_back(poly->GetVertex1());
							vertices2.push_back(poly->GetVertex2());
						}
						else
						{
							vertices1.push_back(poly->GetBody()->GetWorldPoint(poly->GetVertex1()));
							vertices2.push_back(poly->GetBody()->GetWorldPoint(poly->GetVertex2()));
						}
							
						issensor.push_back(shape->IsSensor());
					}
				}

				this->vOutVertices1->SliceCount = vertices1.size();
				this->vOutVertices2->SliceCount = vertices2.size();
				this->vOutIsSensor->SliceCount = issensor.size();
				//this->vOutVerticesCount->SliceCount = vcount.size();
				//this->vOutCenters->SliceCount = centers.size();
				//this->vOutId->SliceCount = 0;//ids.size();

				for (int i = 0; i <  vertices1.size() ; i++) 
				{
					this->vOutVertices1->SetValue2D(i,vertices1.at(i).x,vertices1.at(i).y);
					this->vOutVertices2->SetValue2D(i,vertices2.at(i).x,vertices2.at(i).y);
					this->vOutIsSensor->SetValue(i, issensor.at(i));
				}

				/*
				for (int i = 0; i < vcount.size() ; i++) 
				{
					this->vOutVerticesCount->SetValue(i,vcount.at(i));
					this->vOutCenters->SetValue2D(i,centers.at(i).x,centers.at(i).y);
					//this->vOutId->SetValue(i,ids.at(i));
				}*/
			}
		}

		void Box2dGetEdgeChains::ConnectPin(IPluginIO^ Pin)
		{
			if (Pin == this->vInShapes) 
			{
				INodeIOBase^ usI;
				this->vInShapes->GetUpstreamInterface(usI);
				this->m_polygons = (ShapeDataType^)usI;
			}
		}

		void Box2dGetEdgeChains::DisconnectPin(IPluginIO^ Pin)
		{
			if (Pin == this->vInShapes)
        	{
        		this->m_polygons = nullptr;
        	}
		}
	}
}



