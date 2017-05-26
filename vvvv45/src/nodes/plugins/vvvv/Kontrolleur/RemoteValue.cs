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
		private IPin2 FNamePin, FValuePin;
		private List<string> FPrefixes = new List<string>();
		private string FName;
		
		public INode2 Node;
		public string SourceNodePath;
		public string RuntimeNodePath;
		public string Type;
		public float Default;
		public float Minimum;
		public float Maximum;
		public float Stepsize;
		public float Value;
		public RemoteValueState State;
		public static System.Globalization.NumberFormatInfo FNumberFormat = new System.Globalization.NumberFormatInfo();
		
		public IPin2 Pin
		{
			get
			{
				return FValuePin;
			}
		}
		
		public string Name
		{
			get
			{
				return FName;
			}
			set
			{
				FName = value;
			}
		}
		
		public RemoteValue(INode2 node, List<string> prefixes)
		{
			FNumberFormat.NumberDecimalSeparator = ".";
			Node = node;
			FPrefixes = prefixes;
			
			//note: this will break if patches are renamed!
			RuntimeNodePath = Node.GetNodePath(false); 
			SourceNodePath = Node.Parent.NodeInfo.Filename + "/" + node.ID;
			
			FNamePin = node.LabelPin;
			FNamePin.Changed += ValueChangedCB;
			Name = FNamePin[0];
			if (string.IsNullOrEmpty(Name))
				Name = RuntimeNodePath;
			
			FValuePin = Node.FindPin("Y Input Value");
			FValuePin.Changed += ValueChangedCB;
			FValuePin.SubtypeChanged += SubtypeChangedCB;
			
			Value = float.Parse(FValuePin[0], FNumberFormat);

			var subtype = FValuePin.SubType.Split(',');
			Type = subtype[0];
			Default = float.Parse(subtype[2], FNumberFormat);
			Minimum = float.Parse(subtype[3], FNumberFormat);
			Maximum = float.Parse(subtype[4], FNumberFormat);
			
			if (Type == "Slider")
				Stepsize = 1;
			else
				Stepsize = 0.01f;
			
			State = RemoteValueState.Add;
		}
		
		public void Kill()
		{
			FValuePin.Changed -= ValueChangedCB;
			FValuePin.SubtypeChanged -= SubtypeChangedCB;
		}
		
		private void ValueChangedCB(object sender, EventArgs e)
		{
			if (State == RemoteValueState.Remove)
				return;
			
			var pin = sender as IPin2;
			if (pin == FValuePin)
				Value = float.Parse(FValuePin[0], FNumberFormat);
			else if (pin == FNamePin)
				Name = FNamePin[0];
			
			State = RemoteValueState.Update;
		}
		
		private void SubtypeChangedCB(object sender, EventArgs e)
		{
			if (State == RemoteValueState.Remove)
				return;
			
			var subtype = FValuePin.SubType.Split(',');
			Type = subtype[0];
			Default = float.Parse(subtype[2], FNumberFormat);
			Minimum = float.Parse(subtype[3], FNumberFormat);
			Maximum = float.Parse(subtype[4], FNumberFormat);
			
			State = RemoteValueState.Update;
		}
		
		public void InvalidateState()
		{
			State = RemoteValueState.Idle;
		}
	}
}