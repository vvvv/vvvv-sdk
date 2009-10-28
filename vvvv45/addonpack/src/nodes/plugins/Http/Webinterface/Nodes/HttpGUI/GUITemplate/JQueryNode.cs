using System;
using System.Collections.Generic;
using System.Text;

using VVVV.PluginInterfaces.V1;
using VVVV.Webinterface.Utilities;

namespace VVVV.Nodes.HttpGUI
{
	public abstract class JQueryNode : POSTReceiverNode, IJQueryIO
	{
		protected INodeIn FInputJQueryNodeInput;
		protected IJQueryIO FUpstreamJQueryNodeInput;

		protected INodeOut FOutputJQueryNodeOutput;

		protected override void CreateBasePins()
		{
			FHost.CreateNodeInput("Input JQuery", TSliceMode.Single, TPinVisibility.True, out FInputJQueryNodeInput);
			FInputJQueryNodeInput.SetSubType(new Guid[1] { JQueryIO.GUID }, JQueryIO.FriendlyName);

			FHost.CreateNodeOutput("Output JQuery", TSliceMode.Single, TPinVisibility.True, out FOutputJQueryNodeOutput);
			FOutputJQueryNodeOutput.SetSubType(new Guid[1] { JQueryIO.GUID }, JQueryIO.FriendlyName);
			FOutputJQueryNodeOutput.SetInterface(this);
		}

		protected override void BaseEvaluate(int SpreadMax)
		{
			if (FChangedSpreadSize)
			{
				if (FSliceId.Count > SpreadMax)
				{
					FSliceId.RemoveRange(SpreadMax, FSliceId.Count - SpreadMax);
				}
				else
				{
					for (int i = FSpreadMax; i < SpreadMax; i++)
					{
						FSliceId.Add(HTMLToolkit.CreateSliceID(FNodePath, i));
					}
				}
			}

			OnEvaluate(SpreadMax, FChangedSpreadSize, FNodeId, FSliceId, FReceivedNewString, FReceivedString);
		}

		protected abstract void OnEvaluate(int SpreadMax, bool changedSpreadSize, string NodeId, List<string> SlideId, bool ReceivedNewString, List<string> ReceivedString);

		#region IJQueryIO Members

		public bool PinIsChanged()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		#endregion
	}
}
