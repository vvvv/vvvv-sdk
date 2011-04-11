#region usings
using System;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.Graph;
#endregion usings

namespace VVVV.Nodes
{
	enum RemoteValueState {Idle, Add, Update, Remove}
	
	class RemoteValue
	{
		private INode2 FNode;
		private IPin2 FNamePin, FMinimumPin, FMaximumPin, FTypePin, FValuePin;
		public string Address;
		public string Name;
		public string Type;
		public float Default;
		public float Minimum;
		public float Maximum;
		public float Stepsize;
		public float Value;
		public RemoteValueState State;
		
		public RemoteValue(INode2 node)
		{
			FNode = node;
			Address = "/" + FNode.Parent.NodeInfo.Filename + "/" + FNode.ID;
			
			FNamePin = node.LabelPin;
			FNamePin.Changed += ValueChangedCB;
			Name = FNamePin[0];
			
			FMinimumPin = FNode.FindPin("Minimum");
			FMinimumPin.Changed += ValueChangedCB;
			Minimum = float.Parse(FMinimumPin[0]);
			
			FMaximumPin = FNode.FindPin("Maximum");
			FMaximumPin.Changed += ValueChangedCB;
			Maximum = float.Parse(FMaximumPin[0]);
			
			FTypePin = FNode.FindPin("Slider Behavior");
			FTypePin.Changed += ValueChangedCB;
			Type = FTypePin[0];
			
			FValuePin = FNode.FindPin("Y Input Value");
			FValuePin.Changed += ValueChangedCB;
			Value = float.Parse(FValuePin[0].Replace('.', ','));
			
			Default = 0;
			if (Type == "Slider")
				Stepsize = 1;
			else
				Stepsize = 0.01f;
			
			State = RemoteValueState.Add;
		}
		
		private void ValueChangedCB(object sender, EventArgs e)
		{
			var pin = sender as IPin2;
			if (pin == FValuePin)
				Value = float.Parse(FValuePin[0].Replace('.', ','));
			else if (pin == FNamePin)
				Name = FNamePin[0];
			else if (pin == FMinimumPin)
				Minimum = float.Parse(FMinimumPin[0]);
			else if (pin == FMaximumPin)
				Maximum = float.Parse(FMaximumPin[0]);
			else if (pin == FTypePin)
				Type = FTypePin[0];
			
			State = RemoteValueState.Update;
		}
		
		public void InvalidateState()
		{
			State = RemoteValueState.Idle;
		}
	}
}