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

			this->FHost->CreateValueInput("Closed Polygons",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInClosed);
			this->vInClosed->SetSubType(0,1,1,0,false,true,false);

			this->FHost->CreateValueInput("Local Coordinates",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInLocal);
			this->vInLocal->SetSubType(0,1,1,0,false,true,false);


			this->FHost->CreateValueOutput("Centers",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutCenters);
			this->vOutCenters->SetSubType2D(Double::MinValue,Double::MaxValue,0.01,0.0,0.0,false,false,false);

			this->FHost->CreateValueOutput("Vertices",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutVertices);
			this->vOutVertices->SetSubType2D(Double::MinValue,Double::MaxValue,0.01,0.0,0.0,false,false,false);

			this->FHost->CreateValueOutput("Vertices Count",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutVerticesCount);
			this->vOutVerticesCount->SetSubType(Double::MinValue,Double::MaxValue,0.01,0.0,false,false,true);

			this->FHost->CreateValueOutput("Is Sensor",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutIsSensor);
			this->vOutIsSensor->SetSubType(Double::MinValue,Double::MaxValue,0.01,0.0,false,true,false);

			this->FHost->CreateStringOutput("Custom",TSliceMode::Dynamic,TPinVisibility::True,this->vOutCustom);
			this->vOutCustom->SetSubType("",false);

			this->FHost->CreateValueOutput("Shape Id",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutId);
			this->vOutId->SetSubType(Double::MinValue,Double::MaxValue,1,0.0,false,false,true);
		}


		void Box2dGetPolygonsNode::Configurate(IPluginConfig^ Input)
		{

		}

		
		void Box2dGetPolygonsNode::Evaluate(int SpreadMax)
		{
			if (this->vInClosed->PinIsChanged || this->vInLocal->PinIsChanged)
			{
				double dblclosed,dbllocal;
				this->vInClosed->GetValue(0, dblclosed);
				this->vInLocal->GetValue(0,dbllocal);

				this->m_closed  = dblclosed >= 0.5;
				this->m_local = dbllocal >= 0.5;
			}

			if (this->vInShapes->IsConnected) 
			{
				std::vector<b2Vec2> centers;
				std::vector<b2Vec2> vertices;
				std::vector<int> vcount;
				std::vector<int> ids;
				std::vector<bool> issensor;
				List<String^>^ custs = gcnew List<String^>(); 
				int cnt = 0;
				for (int i = 0; i < this->vInShapes->SliceCount ; i++) 
				{
					int realslice;
					this->vInShapes->GetUpsreamSlice(i,realslice);
					b2Fixture* fixture = this->m_polygons->GetSlice(realslice);
					
					if (fixture->GetType() == b2Shape::Type::e_polygon) 
					{
						b2PolygonShape* poly = (b2PolygonShape*)fixture->GetShape();
						if (poly->GetVertexCount() > 0) 
						{
							if (this->m_closed)
							{
								vcount.push_back(poly->GetVertexCount() + 1);
							}
							else
							{
								vcount.push_back(poly->GetVertexCount());
							}

							
							const b2Vec2* verts = poly->m_vertices;
							for (int j=0; j < poly->GetVertexCount();j++) 
							{
								if (this->m_local)
								{
									vertices.push_back(verts[j]);
									centers.push_back(poly->m_centroid);
								}
								else
								{
									vertices.push_back(fixture->GetBody()->GetWorldPoint(verts[j]));
									centers.push_back(fixture->GetBody()->GetWorldPoint(poly->m_centroid));

								}
							}

							if (this->m_closed)
							{
								if (this->m_local)
								{
									vertices.push_back(verts[0]);
								}
								else
								{
									vertices.push_back(fixture->GetBody()->GetWorldPoint(verts[0]));
								}
							}

							ShapeCustomData* sdata = (ShapeCustomData*)fixture->GetUserData();
							ids.push_back(sdata->Id);
							issensor.push_back(fixture->IsSensor());
							String^ str = gcnew String(sdata->Custom);
							custs->Add(str);

							cnt++;
						}
					}
				}

				this->vOutVertices->SliceCount = vertices.size();
				this->vOutVerticesCount->SliceCount = vcount.size();
				this->vOutCenters->SliceCount = centers.size();
				this->vOutId->SliceCount = ids.size();
				this->vOutIsSensor->SliceCount = issensor.size();
				this->vOutCustom->SliceCount = ids.size();

				for (int i = 0; i <  vertices.size() ; i++) 
				{
					this->vOutVertices->SetValue2D(i,vertices.at(i).x,vertices.at(i).y);
				}

				for (int i = 0; i < vcount.size() ; i++) 
				{
					this->vOutVerticesCount->SetValue(i,vcount.at(i));
					this->vOutCenters->SetValue2D(i,centers.at(i).x,centers.at(i).y);
					this->vOutId->SetValue(i,ids.at(i));
					this->vOutIsSensor->SetValue(i, issensor.at(i));
					this->vOutCustom->SetString(i, custs[i]);
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


