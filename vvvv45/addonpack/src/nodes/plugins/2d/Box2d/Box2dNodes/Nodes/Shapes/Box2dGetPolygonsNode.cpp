#include "StdAfx.h"
#include "Box2dGetPolygonsNode.h"
#include "../../Internals/Data/ShapeCustomData.h"

using namespace System::Collections::Generic;
using namespace VVVV::Utils::VMath;

namespace VVVV 
{
	namespace Nodes 
	{
		Box2dGetPolygonsNode::Box2dGetPolygonsNode(void)
		{
		}

		void Box2dGetPolygonsNode::SetPluginHost(IPluginHost^ Host) 
		{
			this->FHost = Host;

			this->FHost->CreateNodeInput("Shapes",TSliceMode::Dynamic,TPinVisibility::True,this->vInShapes);
			this->vInShapes->SetSubType(ArrayUtils::SingleGuidArray(ShapeDataType::GUID),ShapeDataType::FriendlyName);

			this->FHost->CreateValueOutput("Centers",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutCenters);
			this->vOutCenters->SetSubType2D(Double::MinValue,Double::MaxValue,0.01,0.0,0.0,false,false,false);

			this->FHost->CreateValueOutput("Vertices",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutVertices);
			this->vOutVertices->SetSubType2D(Double::MinValue,Double::MaxValue,0.01,0.0,0.0,false,false,false);

			this->FHost->CreateValueOutput("Vertices Count",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutVerticesCount);
			this->vOutVerticesCount->SetSubType(Double::MinValue,Double::MaxValue,0.01,0.0,false,false,true);

			this->FHost->CreateValueOutput("Shape Id",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutId);
			this->vOutId->SetSubType(Double::MinValue,Double::MaxValue,1,0.0,false,false,true);
		}


		void Box2dGetPolygonsNode::Configurate(IPluginConfig^ Input)
		{

		}

		
		void Box2dGetPolygonsNode::Evaluate(int SpreadMax)
		{
			if (this->vInShapes->IsConnected) 
			{
				std::vector<b2Vec2> centers;
				std::vector<b2Vec2> vertices;
				std::vector<int> vcount;
				std::vector<int> ids;
				int cnt = 0;
				for (int i = 0; i < this->vInShapes->SliceCount ; i++) 
				{
					int realslice;
					this->vInShapes->GetUpsreamSlice(i,realslice);
					b2Shape* shape = this->m_polygons->GetSlice(realslice);
					
					if (shape->GetType() == e_polygonShape) 
					{
						b2PolygonShape* poly = (b2PolygonShape*)shape;
						if (poly->GetVertexCount() > 0) 
						{
							vcount.push_back(poly->GetVertexCount()+1);
							centers.push_back(poly->GetBody()->GetWorldPoint(poly->GetCentroid()));

							const b2Vec2* verts = poly->GetVertices();
							for (int j=0; j < poly->GetVertexCount();j++) 
							{
								vertices.push_back(poly->GetBody()->GetWorldPoint(verts[j]));
							}
							vertices.push_back(poly->GetBody()->GetWorldPoint(verts[0]));

							ShapeCustomData* sdata = (ShapeCustomData*)shape->GetUserData();
							ids.push_back(sdata->Id);

							cnt++;
						}
					}
				}

				this->vOutVertices->SliceCount = vertices.size();
				this->vOutVerticesCount->SliceCount = vcount.size();
				this->vOutCenters->SliceCount = centers.size();
				this->vOutId->SliceCount = ids.size();

				for (int i = 0; i <  vertices.size() ; i++) 
				{
					this->vOutVertices->SetValue2D(i,vertices.at(i).x,vertices.at(i).y);
				}

				for (int i = 0; i < vcount.size() ; i++) 
				{
					this->vOutVerticesCount->SetValue(i,vcount.at(i));
					this->vOutCenters->SetValue2D(i,centers.at(i).x,centers.at(i).y);
					this->vOutId->SetValue(i,ids.at(i));
				}
			}
		}

		void Box2dGetPolygonsNode::ConnectPin(IPluginIO^ Pin)
		{
			if (Pin == this->vInShapes) 
			{
				INodeIOBase^ usI;
				this->vInShapes->GetUpstreamInterface(usI);
				this->m_polygons = (ShapeDataType^)usI;
			}
		}

		void Box2dGetPolygonsNode::DisconnectPin(IPluginIO^ Pin)
		{
			if (Pin == this->vInShapes)
        	{
        		this->m_polygons = nullptr;
        	}
		}
	}
}


