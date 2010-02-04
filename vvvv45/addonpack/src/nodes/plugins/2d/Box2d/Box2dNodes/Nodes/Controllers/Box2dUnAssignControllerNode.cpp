#include "StdAfx.h"
#include "Box2dUnAssignControllerNode.h"

namespace VVVV 
{
	namespace Nodes 
	{

		Box2dUnAssignControllerNode::Box2dUnAssignControllerNode(void)
		{
		}

		void Box2dUnAssignControllerNode::SetPluginHost(IPluginHost^ Host)
		{
			this->FHost = Host;

			this->FHost->CreateNodeInput("Controller",TSliceMode::Dynamic,TPinVisibility::True,this->vInController);
			this->vInController->SetSubType(ArrayUtils::SingleGuidArray(ControllerDataType::GUID),ControllerDataType::FriendlyName);

			this->FHost->CreateNodeInput("Body",TSliceMode::Dynamic,TPinVisibility::True,this->vInBody);
			this->vInBody->SetSubType(ArrayUtils::SingleGuidArray(BodyDataType::GUID),BodyDataType::FriendlyName);

			this->FHost->CreateValueInput("Do UnAssign",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInDoCreate);
			this->vInDoCreate->SetSubType(Double::MinValue,Double::MaxValue,0.01,0.0,true,false,false);

		}

		
		void Box2dUnAssignControllerNode::Configurate(IPluginConfig^ Input) 
		{
		}

		
		void Box2dUnAssignControllerNode::ConnectPin(IPluginIO^ Pin)
		{
			if (Pin == this->vInBody) 
			{
				INodeIOBase^ usI;
				this->vInBody->GetUpstreamInterface(usI);
				this->m_body = (BodyDataType^)usI;
			}
			if (Pin == this->vInController) 
			{
				INodeIOBase^ usI;
				this->vInController->GetUpstreamInterface(usI);
				this->m_controller = (ControllerDataType^)usI;
			}
		}
	
		void Box2dUnAssignControllerNode::DisconnectPin(IPluginIO^ Pin)
		{
			if (Pin == this->vInBody)
        	{
        		this->m_body = nullptr;
        	}
			if (Pin == this->vInController)
			{
				this->m_controller = nullptr;
			}
		}

		void Box2dUnAssignControllerNode::Evaluate(int SpreadMax)
		{
			this->vInBody->PinIsChanged;
			this->vInController->PinIsChanged;

			if (this->vInBody->IsConnected && this->vInController->IsConnected)
			{

					for (int i = 0; i < SpreadMax; i++)
					{
						double dbldoassign;
						int realslice;
						this->vInDoCreate->GetValue(i, dbldoassign);

						if (dbldoassign >= 0.5)
						{
							b2Body* body;
							this->vInBody->GetUpsreamSlice(i % this->vInBody->SliceCount,realslice);
							body = this->m_body->GetSlice(realslice);

							int realslicectrl;
							this->vInController->GetUpsreamSlice(i % this->vInController->SliceCount,realslicectrl);
							b2Controller* ctrl = this->m_controller->GetSlice(realslicectrl);



							bool found = false;
							b2ControllerEdge* edge = body->GetControllerList();
							while(edge && (!found))
							{
								if (edge->controller == ctrl)
								{
									found = true;
								}
								else
								{
									edge = edge->nextController;
								}
							}
								
							if (found)
							{
								ctrl->RemoveBody(body);
							}
						}
						
					}
				
			}
		}
	}
}