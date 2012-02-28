#region usings
using System;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.Graph;
#endregion usings

namespace VVVV.Nodes
{
	enum RemoteValueState {Idle, Add, Update, Remove}
	
	class RemoteValue
	{
		private IPin2 FNamePin, FMinimumPin, FMaximumPin, FTypePin, FValuePin;
		private List<string> FPrefixes = new List<string>();
		private string FName;
		
		public INode2 Node;
		public string Address;
		public string Type;
		public float Default;
		public float Minimum;
		public float Maximum;
		public float Stepsize;
		public float Value;
		public RemoteValueState State;
		
		public string Name
		{
			get
			{
				return FName;
			}
			set
			{
				FName = value;
				foreach (string prefix in FPrefixes)
					if (FName.StartsWith(prefix))
					{
						FName = FName.Substring(prefix.Length);
						break;
					}
			}
		}
		
		public RemoteValue(INode2 node, List<string> prefixes)
		{
			Node = node;
			FPrefixes = prefixes;
			Address = "/" + Node.Parent.NodeInfo.Filename + "/" + Node.ID;
			
			FNamePin = node.LabelPin;
			FNamePin.Changed += ValueChangedCB;
			Name = FNamePin.GetSlice(0);
			
			FMinimumPin = Node.FindPin("Minimum");
			FMinimumPin.Changed += ValueChangedCB;
			Minimum = float.Parse(FMinimumPin.GetSlice(0));
			
			FMaximumPin = Node.FindPin("Maximum");
			FMaximumPin.Changed += ValueChangedCB;
			Maximum = float.Parse(FMaximumPin.GetSlice(0));
			
			FTypePin = Node.FindPin("Slider Behavior");
			FTypePin.Changed += ValueChangedCB;
			Type = FTypePin.GetSlice(0);
			
			FValuePin = Node.FindPin("Y Input Value");
			FValuePin.Changed += ValueChangedCB;
			Value = float.Parse(FValuePin.GetSlice(0).Replace('.', ','));
			
			Default = 0;
			if (Type == "Slider")
				Stepsize = 1;
			else
				Stepsize = 0.01f;
			
			State = RemoteValueState.Add;
		}
		
		private void ValueChangedCB(object sender, EventArgs e)
		{
			if (State == RemoteValueState.Remove)
				return;
			
			var pin = sender as IPin2;
			if (pin == FValuePin)
				Value = float.Parse(FValuePin.GetSlice(0).Replace('.', ','));
			else if (pin == FNamePin)
				Name = FNamePin.GetSlice(0);
			else if (pin == FMinimumPin)
				Minimum = float.Parse(FMinimumPin.GetSlice(0));
			else if (pin == FMaximumPin)
				Maximum = float.Parse(FMaximumPin.GetSlice(0));
			else if (pin == FTypePin)
				Type = FTypePin.GetSlice(0);
			
			State = RemoteValueState.Update;
		}
		
		public void InvalidateState()
		{
			State = RemoteValueState.Idle;
		}
	}
}