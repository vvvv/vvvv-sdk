#include "StdAfx.h"
#include "Box2dEdgeShapeDefNode.h"

namespace VVVV 
{
	namespace Nodes 
	{
		Box2dEdgeShapeDefNode::Box2dEdgeShapeDefNode(void) 
		{

		}

		void Box2dEdgeShapeDefNode::Evaluate(int SpreadMax)
		{
			if (this->vInVertices->PinIsChanged
				|| this->vInVerticesCount->PinIsChanged
				|| this->vInFriction->PinIsChanged
				|| this->vInDensity->PinIsChanged
				|| this->vInRestitution->PinIsChanged
				|| this->vInIsSensor->PinIsChanged
				|| this->vInCustom->PinIsChanged
				|| this->vInGroupIndex->PinIsChanged) 
			{

				double x,y,count,friction,restitution,density,issensor,isloop,group;
				String^ custom;

				int max = Math::Max(this->vInVerticesCount->SliceCount,this->vInFriction->SliceCount);
				max = Math::Max(max,this->vInDensity->SliceCount);
				max = Math::Max(max,this->vInRestitution->SliceCount);
				max = Math::Max(max,this->vInLoop->SliceCount);

				this->vOutShapes->SliceCount = max;

				this->m_shapes->Reset();
				
				int vertidx = 0;
				for (int i = 0; i < max;i++) 
				{		
					this->vInVerticesCount->GetValue(i,count);
					this->vInFriction->GetValue(i,friction);
					this->vInDensity->GetValue(i,density);
					this->vInRestitution->GetValue(i,restitution);
					this->vInIsSensor->GetValue(i,issensor);
					this->vInCustom->GetString(i, custom);
					this->vInLoop->GetValue(i,isloop);
					this->vInGroupIndex->GetValue(i, group);

					if (count > 2) 
					{
						b2EdgeChainDef* shapeDef = this->m_shapes->AddEdgeChain();
						shapeDef->vertexCount = count;
						shapeDef->isALoop = isloop >= 0.5;

						b2Vec2* loop = new b2Vec2[count];
						for (int j = 0; j < count; j++) 
						{
							this->vInVertices->GetValue2D(vertidx,x,y);
							b2Vec2 vec(x,y);
							loop[j] = vec;
							vertidx++;
							if (vertidx == this->vInVertices->SliceCount) 
							{
								vertidx = 0;
							}
						}

						shapeDef->vertices = loop;
						shapeDef->density = density;
						shapeDef->friction = friction;
						shapeDef->restitution = restitution;
						shapeDef->isSensor = issensor >= 0.5;
						shapeDef->filter.groupIndex = Convert::ToInt32(group);

						this->m_shapes->AddCustom(custom);
					}		
				}
				this->vOutShapes->MarkPinAsChanged();
			}

		}
		
		void Box2dEdgeShapeDefNode::OnPluginHostSet() 
		{
			this->FHost->CreateValueInput("Vertices",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInVertices);
			this->vInVertices->SetSubType2D(0,Double::MaxValue,0.01,0.0,0.0,false,false,false);

			this->FHost->CreateValueInput("Vertices Count",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInVerticesCount);
			this->vInVerticesCount->SetSubType(2,Double::MaxValue,1,1.0,false,false,true);

			this->FHost->CreateValueInput("Loop", 1, nullptr, TSliceMode::Dynamic, TPinVisibility::True, this->vInLoop);
			this->vInLoop->SetSubType(0, 1, 1, 0, false,true, false);

		}
	}
}

