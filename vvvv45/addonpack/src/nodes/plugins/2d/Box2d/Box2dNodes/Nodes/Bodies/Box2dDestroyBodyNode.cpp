#include "StdAfx.h"
#include "Box2dDestroyBodyNode.h"

#include "../../Internals/Data/BodyCustomData.h"

namespace VVVV 
{
	namespace Nodes 
	{
		void Box2dDestroyBodyNode::SetPluginHost(IPluginHost^ Host) 
		{
			this->FHost = Host;

			this->FHost->CreateNodeInput("Bodies",TSliceMode::Dynamic,TPinVisibility::True,this->vInBodies);
			this->vInBodies->SetSubType(ArrayUtils::SingleGuidArray(BodyDataType::GUID),BodyDataType::FriendlyName);

			this->FHost->CreateValueInput("Do Destroy",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInDoDestroy);
			this->vInDoDestroy->SetSubType(Double::MinValue,Double::MaxValue,1.0,0.0,true,false,false);	

			

		}

		void Box2dDestroyBodyNode::Configurate(IPluginConfig^ Input)
		{

		}

		void Box2dDestroyBodyNode::Evaluate(int SpreadMax)
		{
			this->vInBodies->PinIsChanged;
			if (this->vInBodies->IsConnected) 
			{
				double dbldelete;
				

					//double dblsp,dblsv,dblsav;
					for (int i = 0; i < this->vInBodies->SliceCount; i++) 
					{
						this->vInDoDestroy->GetValue(i, dbldelete);
						if (dbldelete >= 0.5)
						{
							int realslice;
							this->vInBodies->GetUpsreamSlice(i % this->vInBodies->SliceCount,realslice);
							b2Body* body = this->m_bodies->GetSlice(realslice);

							//Just In Case
							BodyCustomData* bdata = (BodyCustomData*)body->GetUserData();
							bdata->MarkedForDeletion = true;
						}
					}
				
			}
		}



		void Box2dDestroyBodyNode::ConnectPin(IPluginIO^ Pin)
		{
			if (Pin == this->vInBodies) 
			{
				INodeIOBase^ usI;
				this->vInBodies->GetUpstreamInterface(usI);
				this->m_bodies = (BodyDataType^)usI;
			}
		}


		void Box2dDestroyBodyNode::DisconnectPin(IPluginIO^ Pin)
		{
			if (Pin == this->vInBodies)
        	{
        		this->m_bodies = nullptr;
        	}
		}

	}
}
