using System;
using System.Collections;

using VVVV.PluginInterfaces.V1;

namespace Hoster
{
	public class TEnumPin: TBasePin, IEnumIn, IEnumConfig, IEnumOut
	{
		private string[] FValues;
		private string FDefault;
  
		public TEnumPin(IPluginHost Parent, string PinName, TPinDirection PinDirection, TOnConfigurate Callback, TSliceMode SliceMode, TPinVisibility Visibility)
		: base(Parent, PinName, 1, PinDirection, Callback, SliceMode, Visibility)
		{}
		/*		
		public string this[int i]
		{
			get { return FValues[i % FSliceCount]; }
			set 
			{ 
				if (value != FValues[i])
				{
					FValues[i] = value;
					FPinIsChanged = true;
				}
			}
		}*/
		
		public void GetString(int Index, out string Value)
		{
			Value = "";
		}
		
		public void SetString(int Index, string Value)
		{

		}
		
		public void SetOrd(int Index, int Value)
		{
			
		}
		
		public void GetOrd(int Index, out int Value)
		{
			Value = 0;
		}

		public void SetSubType(string Default)
		{
			
		}
		
		override protected void ChangeSliceCount()
		{

		}
		
		override protected string AsString(int index)
		{
			return FValues[index];
		}
		
		protected override string AsDisplayString(int Index)
		{
			return "";
		}
		
		override public void SetSpreadAsString(string Spread)
		{

		}
	}	
}
