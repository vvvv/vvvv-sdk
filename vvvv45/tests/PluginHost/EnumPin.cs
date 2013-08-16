using System;
using System.Collections;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;

namespace Hoster
{
	public class TEnumPin: TBasePin, IEnumIn, IEnumConfig, IEnumOut
	{
		private string[] FValues = new string[0];
		private string FDefault;
		private PluginHost FPluginHost;
		
		public TEnumPin(PluginHost Parent, string PinName, TPinDirection PinDirection, TOnConfigurate Callback, TSliceMode SliceMode, TPinVisibility Visibility)
			: base(Parent, PinName, 1, PinDirection, Callback, SliceMode, Visibility)
		{
			FPluginHost = Parent;
			
			base.Initialize();
		}
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
			Value = FValues[VMath.Zmod(Index, FSliceCount)];
		}
		
		public void SetString(int Index, string Value)
		{
			var entries = FPluginHost.GetEnumEntries(FDefault);
			if (entries.Contains(Value))
				FValues[VMath.Zmod(Index, FSliceCount)] = Value;
		}
		
		public void SetOrd(int Index, int Value)
		{
			var entries = FPluginHost.GetEnumEntries(FDefault);
			if (entries.Count > Value)
				FValues[VMath.Zmod(Index, FSliceCount)] = entries[Value];
		}
		
		public void GetOrd(int Index, out int Value)
		{
			var entries = FPluginHost.GetEnumEntries(FDefault);
			Value = entries.IndexOf(FValues[VMath.Zmod(Index, FSliceCount)]);
		}

		public void SetSubType(string Default)
		{
			FDefault = Default;
			for (int i = 0; i < FValues.Length; i++)
			{
				string entry;
				FPluginHost.GetEnumEntry(FDefault, 0, out entry);
				FValues[i] = entry;
			}
		}
		
		override protected void ChangeSliceCount()
		{
			var oldValues = FValues;
			FValues = new string[FSliceCount];
			
			int minLength = Math.Min(oldValues.Length, FValues.Length);
			Array.Copy(oldValues, FValues, minLength);
			
			if (!string.IsNullOrEmpty(FDefault))
			{
				for (int i = minLength; i < FValues.Length; i++)
				{
					string entry;
					FPluginHost.GetEnumEntry(FDefault, 0, out entry);
					FValues[i] = entry;
				}
			}
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
