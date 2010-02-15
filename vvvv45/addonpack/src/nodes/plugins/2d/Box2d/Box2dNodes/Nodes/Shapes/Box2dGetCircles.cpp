#include "StdAfx.h"
#include "Box2dGetCircles.h"
#include "../../Internals/Data/ShapeCustomData.h"

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

			this->FHost->CreateValueInput("Local Coordinates",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInLocal);
			this->vInLocal->SetSubType(0,1,1,1,false,true,false);

			this->FHost->CreateValueOutput("Position",2,ArrayUtils::Array2D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutPosition);
			this->vOutPosition->SetSubType2D(Double::MinValue,Double::MaxValue,0.01,0.0,0.0,false,false,false);

			this->FHost->CreateValueOutput("Radius",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutRadius);
			this->vOutRadius->SetSubType(Double::MinValue,Double::MaxValue,0.01,0.0,false,false,false);

			this->FHost->CreateValueOutput("Is Sensor",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutIsSensor);
			this->vOutIsSensor->SetSubType(Double::MinValue,Double::MaxValue,0.01,0.0,false,true,false);

			this->FHost->CreateStringOutput("Custom",TSliceMode::Dynamic,TPinVisibility::True,this->vOutCustom);
			this->vOutCustom->SetSubType("",false);

			this->FHost->CreateValueOutput("Shape Id",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutId);
			this->vOutId->SetSubType(Double::MinValue,Double::MaxValue,1,0.0,false,false,true);
		}


		void Box2dGetCircles::Configurate(IPluginConfig^ Input)
		{

		}

		
		void Box2dGetCircles::Evaluate(int SpreadMax)
		{
			this->vInShapes->PinIsChanged;
			
			if (this->vInLocal->PinIsChanged)
			{
				double dbllocal;
				this->vInLocal->GetValue(0,dbllocal);
				this->m_local = dbllocal >= 0.5;
			}

			if (this->vInShapes->IsConnected) 
			{
				List<Vector2D> pos = gcnew List<Vector2D>();
				List<double>^ radius = gcnew List<double>();
				List<int>^ ids = gcnew List<int>();
				List<String^>^ custs = gcnew List<String^>(); 
				std::vector<bool> issensor;
				int cnt = 0;
				for (int i = 0; i < this->vInShapes->SliceCount ; i++) 
				{
					int realslice;
					this->vInShapes->GetUpsreamSlice(i,realslice);
					b2Fixture* fixture = this->m_circles->GetSlice(realslice);
					if (fixture->GetType() == b2Shape::Type::e_circle) 
					{
						b2CircleShape* circle = (b2CircleShape*)fixture->GetShape();

						b2Vec2 local = circle->m_p;
						if (this->m_local)
						{
							Vector2D vec = Vector2D(local.x,local.y);
							pos.Add(vec);
						}
						else
						{
							b2Vec2 world = fixture->GetBody()->GetWorldPoint(local);
							Vector2D vec = Vector2D(world.x,world.y);
							pos.Add(vec);
						}

						radius->Add(circle->m_radius);
						

						ShapeCustomData* sdata = (ShapeCustomData*)fixture->GetUserData();
						ids->Add(sdata->Id);
						issensor.push_back(fixture->IsSensor());
						String^ str = gcnew String(sdata->Custom);
						custs->Add(str);

						cnt++;
					}
				}

				this->vOutPosition->SliceCount = cnt;
				this->vOutRadius->SliceCount = cnt;
				this->vOutId->SliceCount = cnt;
				this->vOutIsSensor->SliceCount = issensor.size();
				this->vOutCustom->SliceCount = cnt;

				for (int i = 0; i < cnt ; i++) 
				{	
					this->vOutPosition->SetValue2D(i, pos[i].x,pos[i].y);
					this->vOutRadius->SetValue(i,radius[i]);
					this->vOutId->SetValue(i,ids[i]);
					this->vOutIsSensor->SetValue(i, issensor.at(i));
					this->vOutCustom->SetString(i, custs[i]);
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
