#include "StdAfx.h"
#include "Box2dDestroyShape.h"

#include "../../Internals/Data/ShapeCustomData.h"

namespace VVVV
{
	namespace Nodes
	{
		Box2dDestroyShape::Box2dDestroyShape(void)
		{
		}

		void Box2dDestroyShape::SetPluginHost(IPluginHost^ Host) 
		{
			this->FHost = Host;

			this->FHost->CreateNodeInput("Shapes",TSliceMode::Dynamic,TPinVisibility::True,this->vInShapes);
			this->vInShapes->SetSubType(ArrayUtils::SingleGuidArray(ShapeDataType::GUID),ShapeDataType::FriendlyName);

			this->FHost->CreateValueInput("Do Destroy",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInDoDestroy);
			this->vInDoDestroy->SetSubType(Double::MinValue,Double::MaxValue,0.01,0.0,true,false,false);	

		}


		void Box2dDestroyShape::Configurate(IPluginConfig^ Input)
		{

		}

		
		void Box2dDestroyShape::Evaluate(int SpreadMax)
		{
			this->vInShapes->PinIsChanged;
			if (this->vInShapes->IsConnected) 
			{
				double dbldelete;

				for (int i = 0; i < this->vInShapes->SliceCount; i++) 
				{
					this->vInDoDestroy->GetValue(i, dbldelete);
					if (dbldelete >= 0.5)
					{
						int realslice;
						this->vInShapes->GetUpsreamSlice(i % this->vInShapes->SliceCount,realslice);
						b2Fixture* shape = this->m_shapes->GetSlice(realslice);

						ShapeCustomData* sdata = (ShapeCustomData*)shape->GetUserData();
						sdata->MarkedForDeletion = true;
					}
				}
			}
		}

		void Box2dDestroyShape::ConnectPin(IPluginIO^ Pin)
		{
			if (Pin == this->vInShapes) 
			{
				INodeIOBase^ usI;
				this->vInShapes->GetUpstreamInterface(usI);
				this->m_shapes = (ShapeDataType^)usI;
			}
		}

		void Box2dDestroyShape::DisconnectPin(IPluginIO^ Pin)
		{
			if (Pin == this->vInShapes)
        	{
        		this->m_shapes = nullptr;
        	}
		}
	}

}
