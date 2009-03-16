#include "StdAfx.h"
#include "Box2dPolygonDefNode.h"

namespace VVVV 
{
	namespace Nodes 
	{
		Box2dPolygonDefNode::Box2dPolygonDefNode(void) 
		{

		}

		void Box2dPolygonDefNode::Evaluate(int SpreadMax)
		{
			if (this->vInVertices->PinIsChanged
				|| this->vInVerticesCount->PinIsChanged
				|| this->vInFriction->PinIsChanged
				|| this->vInDensity->PinIsChanged
				|| this->vInRestitution->PinIsChanged) 
			{

				double x,y,count,friction,restitution,density;

				int max = Math::Max(this->vInVerticesCount->SliceCount,this->vInFriction->SliceCount);
				max = Math::Max(max,this->vInDensity->SliceCount);
				max = Math::Max(max,this->vInRestitution->SliceCount);

				this->vOutShapes->SliceCount = max;

				this->m_shapes->Reset();
				
				int vertidx = 0;
				for (int i = 0; i < SpreadMax;i++) 
				{		
					this->vInVerticesCount->GetValue(i,count);
					this->vInFriction->GetValue(i,friction);
					this->vInDensity->GetValue(i,density);
					this->vInRestitution->GetValue(i,restitution);

					b2PolygonDef* shapeDef = this->m_shapes->AddPolygon();

					count = Math::Min(count,8.0);
					count = Math::Max(count,3.0);

					shapeDef->vertexCount = count;

					for (int j = 0; j < count; j++) 
					{
						this->vInVertices->GetValue2D(vertidx,x,y);
						shapeDef->vertices[j].Set(x,y);
						vertidx++;
						if (vertidx == this->vInVertices->SliceCount) 
						{
							vertidx = 0;
						}
					}
					shapeDef->density = density;
					shapeDef->friction = friction;
					shapeDef->restitution = restitution;

					
				}
			}

		}
		
		void Box2dPolygonDefNode::OnPluginHostSet() 
		{
			this->FHost->CreateValueInput("Vertices",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInVertices);
			this->vInVertices->SetSubType2D(0,Double::MaxValue,0.01,0.0,0.0,false,false,false);

			this->FHost->CreateValueInput("Vertices Count",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInVerticesCount);
			this->vInVerticesCount->SetSubType(3,8,1,1.0,false,false,true);

		}
	}
}
