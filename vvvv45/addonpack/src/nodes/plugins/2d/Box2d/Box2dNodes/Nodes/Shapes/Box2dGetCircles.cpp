#include "StdAfx.h"
#include "Box2dGetCircles.h"

using namespace System::Collections::Generic;
using namespace VVVV::Utils::VMath;

namespace VVVV 
{
	namespace Nodes 
	{
		Box2dGetCircles::Box2dGetCircles(void)
		{
		}

		void Box2dGetCircles::SetPluginHost(IPluginHost^ Host) 
		{
			this->FHost = Host;

			this->FHost->CreateNodeInput("Shapes",TSliceMode::Dynamic,TPinVisibility::True,this->vInShapes);
			this->vInShapes->SetSubType(ArrayUtils::SingleGuidArray(ShapeDataType::GUID),ShapeDataType::FriendlyName);

			this->FHost->CreateValueOutput("Position",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutPosition);
			this->vOutPosition->SetSubType2D(Double::MinValue,Double::MaxValue,0.01,0.0,0.0,false,false,false);

			this->FHost->CreateValueOutput("Radius",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutRadius);
			this->vOutRadius->SetSubType(Double::MinValue,Double::MaxValue,0.01,0.0,false,false,false);
		}


		void Box2dGetCircles::Configurate(IPluginConfig^ Input)
		{

		}

		
		void Box2dGetCircles::Evaluate(int SpreadMax)
		{
			if (this->vInShapes->IsConnected) 
			{
				List<Vector2D> pos = gcnew List<Vector2D>();
				List<double>^ radius = gcnew List<double>();
				int cnt = 0;
				for (int i = 0; i < this->vInShapes->SliceCount ; i++) 
				{
					b2Shape* shape = this->m_circles->GetSlice(i);
					if (shape->GetType() == e_circleShape) 
					{
						b2CircleShape* circle = (b2CircleShape*)shape;

						b2Vec2 local = circle->GetLocalPosition();
						b2Vec2 world = circle->GetBody()->GetWorldPoint(local);
						Vector2D vec = Vector2D(world.x,world.y);
						pos.Add(vec);

						radius->Add(circle->GetRadius());
						cnt++;
					}
				}

				this->vOutPosition->SliceCount = cnt;
				this->vOutRadius->SliceCount = cnt;
				for (int i = 0; i < cnt ; i++) 
				{
					this->vOutPosition->SetValue2D(i,pos[i].x,pos[i].y);
					this->vOutRadius->SetValue(i,radius[i]);
				}
			}
		}

		void Box2dGetCircles::ConnectPin(IPluginIO^ Pin)
		{
			if (Pin == this->vInShapes) 
			{
				INodeIOBase^ usI;
				this->vInShapes->GetUpstreamInterface(usI);
				this->m_circles = (ShapeDataType^)usI;
			}
		}

		void Box2dGetCircles::DisconnectPin(IPluginIO^ Pin)
		{
			if (Pin == this->vInShapes)
        	{
        		this->m_circles = nullptr;
        	}
		}
	}
}
