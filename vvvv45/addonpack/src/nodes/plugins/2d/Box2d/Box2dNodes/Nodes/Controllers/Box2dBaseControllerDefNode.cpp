#include "StdAfx.h"
#include "Box2dBaseControllerDefNode.h"

namespace VVVV 
{
	namespace Nodes 
	{
		Box2dBaseControllerDefNode::Box2dBaseControllerDefNode(void)
		{
			 this->m_controller = gcnew ControllerDataType();
			 this->m_controller->SetController(nullptr);
		}

		Box2dBaseControllerDefNode::~Box2dBaseControllerDefNode() 
		{
			if (this->ctrl != 0) 
			{
				if (this->m_world->GetIsValid())
				{
					this->ctrl->Clear();
					this->m_world->GetWorld()->DestroyController(this->ctrl);
				}
			}
		}

		void Box2dBaseControllerDefNode::SetPluginHost(IPluginHost^ Host)
		{
			this->FHost = Host;

			this->FHost->CreateNodeInput("World",TSliceMode::Dynamic,TPinVisibility::True,this->vInWorld);
			this->vInWorld->SetSubType(ArrayUtils::SingleGuidArray(WorldDataType::GUID),WorldDataType::FriendlyName);

			this->OnPluginHostSet();

			this->FHost->CreateValueInput("Clear",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vInClear);
			this->vInClear->SetSubType(Double::MinValue,Double::MaxValue,0.01,0.0,true,false,false);

			this->FHost->CreateNodeOutput("Controller",TSliceMode::Single,TPinVisibility::True,this->vOutController);
			this->vOutController->SetSubType(ArrayUtils::SingleGuidArray(ControllerDataType::GUID),ControllerDataType::FriendlyName);
			this->vOutController->SetInterface(this->m_controller);

			//this->FHost->CreateValueOutput("Body Count",1,ArrayUtils::Array1D(),TSliceMode::Dynamic,TPinVisibility::True,this->vOutBodyCount);
			//this->vOutBodyCount->SetSubType(-1,Double::MaxValue,1,0.0,false,false,true);
			
		}

		void Box2dBaseControllerDefNode::Evaluate(int SpreadMax)
		{
			this->vInWorld->PinIsChanged;
			bool redo = false;
			if (this->vInWorld->IsConnected)
			{
				if (this->m_world->GetIsValid())
				{
					if (this->ctrl == nullptr)
					{
						redo = true;
					}
					else
					{
						if (this->m_world->HasReset())
						{
							redo = true;
						}
					}

					this->OnEvaluate(SpreadMax,redo);
					this->m_controller->SetController(ctrl);

					if (this->ctrl == nullptr)
					{
						this->FHost->Log(TLogType::Error,Convert::ToString(redo));
						this->FHost->Log(TLogType::Error,"Null controller");
					}

					double dblclear;
					this->vInClear->GetValue(0, dblclear);

					if (dblclear >= 0.5 && this->ctrl !=nullptr)
					{	
						this->ctrl->Clear();
					}
				}
			}


			this->vOutController->MarkPinAsChanged();
		}

		void Box2dBaseControllerDefNode::ConnectPin(IPluginIO^ Pin)
		{
			if (Pin == this->vInWorld) 
			{
				INodeIOBase^ usI;
				this->vInWorld->GetUpstreamInterface(usI);
				this->m_world = (WorldDataType^)usI;
			}
		}

		void Box2dBaseControllerDefNode::DisconnectPin(IPluginIO^ Pin)
		{
			if (Pin == this->vInWorld)
        	{
				if (this->m_world != nullptr)
				{
					if (this->m_world->GetIsValid())
					{
						this->ctrl->Clear();
						this->m_world->GetWorld()->DestroyController(this->ctrl);

					}
        			this->m_world = nullptr;
					this->ctrl = nullptr;
				}
				this->m_controller->SetController(nullptr);
        	}
		}

	}
}
