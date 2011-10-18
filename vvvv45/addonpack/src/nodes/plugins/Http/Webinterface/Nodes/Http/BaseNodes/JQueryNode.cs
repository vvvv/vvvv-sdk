using System;
using System.Collections.Generic;
using System.Text;

using VVVV.PluginInterfaces.V1;
using VVVV.Webinterface.Utilities;
using VVVV.Webinterface.jQuery;
using VVVV.Nodes.Http.BaseNodes;

namespace VVVV.Nodes.Http.BaseNodes
{
	public abstract class JQueryNode : POSTReceiverNode, IJQueryIO, IPluginConnections
	{
		protected INodeIn FInputJQueryNodeInput;
		protected IJQueryIO FUpstreamJQueryNodeInterface;

		protected INodeOut FOutputJQueryNodeOutput;
		protected IStringOut FJQueryCodeStringOutput;

		protected JQueryNodeIOData FUpstreamJQueryNodeData;
		
		protected JQueryExpression FExpression = JQueryExpression.This();
		protected bool FJQueryNodeInputEventThisFrame;
		protected bool FInputPinChangedThisFrame;

		protected abstract bool DynamicPinsAreChanged();

		protected abstract void OnSetPluginHost();

		protected override void CreateBasePins()
		{
			FHost.CreateNodeInput("Input JQuery", TSliceMode.Single, TPinVisibility.True, out FInputJQueryNodeInput);
			FInputJQueryNodeInput.SetSubType(new Guid[1] { JQueryIO.GUID }, JQueryIO.FriendlyName);

			FHost.CreateNodeOutput("Output JQuery", TSliceMode.Single, TPinVisibility.True, out FOutputJQueryNodeOutput);
			FOutputJQueryNodeOutput.SetSubType(new Guid[1] { JQueryIO.GUID }, JQueryIO.FriendlyName);
			FOutputJQueryNodeOutput.SetInterface(this);

			FHost.CreateStringOutput("JQuery Code", TSliceMode.Single, TPinVisibility.True, out FJQueryCodeStringOutput);
			FJQueryCodeStringOutput.SetSubType("", false);

			OnSetPluginHost();
		}

		protected override void BaseEvaluate(int SpreadMax, bool ReceivedNewString)
		{
			bool newDataOnJQueryInput = false;

			//if (FInputJQueryNodeInput.PinIsChanged)
			//{
			//    System.Diagnostics.Debug.WriteLine("NodePinChanged");
			//}

			if (FInputJQueryNodeInput.IsConnected && (FJQueryNodeInputEventThisFrame || FUpstreamJQueryNodeInterface.PinIsChanged))
			{
				newDataOnJQueryInput = true;
				for (int i = 0; i < SpreadMax; i++)
				{
					FUpstreamJQueryNodeData = FUpstreamJQueryNodeInterface.GetJQueryData(i);
				}

			}

			OnEvaluate(SpreadMax, FChangedSpreadSize, FNodeId, FSliceId, FReceivedNewString, FReceivedString);

			if (FInputPinChangedThisFrame = FJQueryNodeInputEventThisFrame || newDataOnJQueryInput || FChangedSpreadSize || DynamicPinsAreChanged())
			{
				for (int i = 0; i < SpreadMax; i++)
				{
					
					FJQueryCodeStringOutput.SetString(i, FExpression.Chain(FUpstreamJQueryNodeData != null ? FUpstreamJQueryNodeData.BuildChain() : new JQueryExpression()).GenerateScript(0, true, true));
				}
			}

			FJQueryNodeInputEventThisFrame = false;
		}

		protected abstract void OnEvaluate(int SpreadMax, bool changedSpreadSize, string NodeId, List<string> SlideId, bool ReceivedNewString, List<string> ReceivedString);

		#region IPluginConnections Members

		public virtual void ConnectPin(IPluginIO pin)
		{
			//cache a reference to the upstream interface when the NodeInput pin is being connected
			if (pin == FInputJQueryNodeInput)
			{
				if (FInputJQueryNodeInput != null)
				{
					INodeIOBase upstreamInterface;
					FInputJQueryNodeInput.GetUpstreamInterface(out upstreamInterface);
					FUpstreamJQueryNodeInterface = upstreamInterface as IJQueryIO;
					FJQueryNodeInputEventThisFrame = true;
				}

			}
		}

		public virtual void DisconnectPin(IPluginIO pin)
		{
			//reset the cached reference to the upstream interface when the NodeInput is being disconnected
			if (pin == FInputJQueryNodeInput)
			{
				FUpstreamJQueryNodeInterface = null;
				FUpstreamJQueryNodeData = null;
				FJQueryNodeInputEventThisFrame = true;
			}
		}

		#endregion

		#region IJQueryIO Members

		public bool PinIsChanged
		{
			get { return FInputPinChangedThisFrame; }
		}

		public JQueryNodeIOData GetJQueryData(int slice)
		{
			JQueryNodeIOData data = new JQueryNodeIOData(FExpression);
			data.UpstreamJQueryData = FUpstreamJQueryNodeData;
			return data;
		}

		#endregion

		#region IPlugin Members


		public virtual void Configurate(IPluginConfig input)
		{
			
		}

		public virtual bool AutoEvaluate
		{
			get { return false;  }
		}

		#endregion
	}
}
