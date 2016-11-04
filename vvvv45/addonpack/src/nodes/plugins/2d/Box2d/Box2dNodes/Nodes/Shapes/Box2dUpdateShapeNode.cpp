#include "StdAfx.h"
#include "Box2dUpdateShapeNode.h"

#include "../../Internals/Data/ShapeCustomData.h"

using namespace VVVV::DataTypes;

namespace VVVV 
{
	namespace Nodes 
	{

		Box2dUpdateShapeNode::Box2dUpdateShapeNode(void)
		{
		}

		void Box2dUpdateShapeNode::SetPluginHost(IPluginHost^ Host) 
		{
			this->FHost = Host;

			this->FHost->CreateNodeInput("Shapes",TSliceMode::Dynamic,TPinVisibility::True,this->vInShapes);
			this->vInShapes->SetSubType(ArrayUtils::SingleGuidArray(ShapeDataType::GUID),ShapeDataType::FriendlyName);

			this->FHost->CreateValueInput("Density",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInDensity);
			this->vInDensity->SetSubType(Double::MinValue,Double::MaxValue,0.01,0.0,false,false,false);

			this->FHost->CreateValueInput("Set Density",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInSetDensity);
			this->vInSetDensity->SetSubType(0,1,1,0.0,true,false,false);

			this->FHost->CreateValueInput("Friction",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInFriction);
			this->vInFriction->SetSubType(Double::MinValue,Double::MaxValue,0.01,0.0,false,false,false);

			this->FHost->CreateValueInput("Set Friction",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInSetFriction);
			this->vInSetFriction->SetSubType(0,1,1,0.0,true,false,false);

			this->FHost->CreateValueInput("Restitution",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInRestitution);
			this->vInRestitution->SetSubType(Double::MinValue,Double::MaxValue,0.01,0.0,false,false,false);

			this->FHost->CreateValueInput("Set Restitution",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInSetRestitution);
			this->vInSetRestitution->SetSubType(0,1,1,0.0,true,false,false);	

			this->FHost->CreateValueInput("Group Index", 1, nullptr, TSliceMode::Dynamic, TPinVisibility::True, this->vInGroup);
			this->vInGroup->SetSubType(Double::MinValue, Double::MaxValue, 1, 0, false,false, true);

			this->FHost->CreateValueInput("Set Group",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInSetGroup);
			this->vInSetGroup->SetSubType(0,1,1,0.0,true,false,false);

			this->FHost->CreateStringInput("Custom",TSliceMode::Dynamic,TPinVisibility::True,this->vInCustom);
			this->vInCustom->SetSubType("",false);

			this->FHost->CreateValueInput("Set Custom",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInSetCustom);
			this->vInSetCustom->SetSubType(0,1,1,0.0,true,false,false);

		}

		
		void Box2dUpdateShapeNode::Configurate(IPluginConfig^ Input)
		{

		}

				
		void Box2dUpdateShapeNode::ConnectPin(IPluginIO^ Pin)
		{
			if (Pin == this->vInShapes) 
			{
				INodeIOBase^ usI;
				this->vInShapes->GetUpstreamInterface(usI);
				this->m_shapes = (ShapeDataType^)usI;
			}
		}

		void Box2dUpdateShapeNode::Evaluate(int SpreadMax)
		{
			this->vInShapes->PinIsChanged;
			if (this->vInShapes->IsConnected) 
			{
				double dblsd,dblsf,dblsr,dblsc,dblsg;
				for (int i = 0; i < this->vInShapes->SliceCount; i++) 
				{
					int realslice;
					this->vInShapes->GetUpsreamSlice(i % this->vInShapes->SliceCount,realslice);
					b2Shape* shape = this->m_shapes->GetSlice(realslice);
					ShapeCustomData* sdata = (ShapeCustomData*)shape->GetUserData();

					int id = sdata->Id;

					this->vInSetFriction->GetValue(i,dblsf);
					this->vInSetDensity->GetValue(i, dblsd);
					this->vInSetRestitution->GetValue(i,dblsr);
					this->vInSetGroup->GetValue(i, dblsg);
					
					if (dblsg >= 0.5)
					{
						double g;
						this->vInGroup->GetValue(i,g);
						b2FilterData filter = shape->GetFilterData();
						filter.groupIndex = Convert::ToInt32(g);
					}

					if (dblsf >= 0.5) 
					{
						double f;
						this->vInFriction->GetValue(i, f);
						shape->SetFriction(f);
					}

					if (dblsd >= 0.5) 
					{
						double d;
						this->vInDensity->GetValue(i, d);

						shape->SetDensity(d);
						shape->GetBody()->SetMassFromShapes();
					}


					if (dblsr >= 0.5) 
					{
						double r;
						this->vInRestitution->GetValue(i, r);
						shape->SetRestitution(r);
					}

					this->vInSetCustom->GetValue(i,dblsc);
					if (dblsc >= 0.5)
					{
						String^ cust;
						this->vInCustom->GetString(i,cust);
						sdata->Custom = (char*)(void*)Marshal::StringToHGlobalAnsi(cust);
					}

				}
			}
		}





	
		void Box2dUpdateShapeNode::DisconnectPin(IPluginIO^ Pin)
		{
			if (Pin == this->vInShapes)
        	{
        		this->m_shapes = nullptr;
        	}
		}
	}
}