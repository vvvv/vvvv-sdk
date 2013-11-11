using System;
using System.Collections;

using VVVV.PluginInterfaces.V1;

namespace Hoster
{
	public class TStringPin: TBasePin, IStringIn, IStringConfig, IStringOut
	{
		private string[] FValues;
		private string FDefault;
		private bool FIsFilename;
  
		public TStringPin(IPluginHost Parent, string PinName, TPinDirection PinDirection, TOnConfigurate Callback, TSliceMode SliceMode, TPinVisibility Visibility)
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
			Value = FValues[Index % FSliceCount];
		}
		
		public void SetString(int Index, string Value)
		{
			if (Value != FValues[Index])
			{
				FValues[Index] = Value;
				FPinIsChanged = true;
			}	
			
			if ((FPinIsChanged) && (FOnConfigurate != null))
				FOnConfigurate(this);
		}

		public void SetSubType(string Default, bool IsFilename)
		{
			FDefault = Default;
			FIsFilename = IsFilename;
			
			for (int i=0; i<SliceCount; i++)
				FValues[i] = Default;
				//SetString(i, Default);

			FPinIsChanged = true;
		}
		
		public void SetSubType2(string Default, int MaxCharacters, string FileMask, TStringType StringType)
		{
			//todo
		}
		
	/*	unsafe public void GetValueP(out int SliceCount, out string* ValueP)
		{
			string s = "test";
			fixed(string* p = &s)
			{ValueP = p;}
			SliceCount = FSliceCount;
		}*/
		
		override protected void ChangeSliceCount()
		{
			int oldCount = 0;
			if (FValues != null)
				oldCount = FValues.Length;
			string[] tmp = new string[oldCount];
			
			//save old values
			for (int i=0; i<oldCount; i++)
				tmp[i] = FValues[i];
			
			FValues = new string[FSliceCount];
			
			//set old values to new array
			for (int i=0; i<Math.Min(FSliceCount, oldCount); i++)
				FValues[i] =  tmp[i];
			if (oldCount > 0)
			{
				for (int i=oldCount; i<FSliceCount; i++)
					FValues[i] =  tmp[oldCount-1];
			}
			else
				for (int i=oldCount; i<FSliceCount; i++)
					FValues[i] = FDefault;
		}
		
		override protected string AsString(int index)
		{
			return FValues[index];
		}
		
		protected override string AsDisplayString(int Index)
		{
			if (FIsFilename)
				return System.IO.Path.GetFileName(FValues[Index]);
			else
				return FValues[Index];
		}
		
		override public void SetSpreadAsString(string Spread)
		{
			if (Spread == "")
			{
				if (FSliceCount > 0)
				{
					FSliceCountIsChanged = true;
					FPinIsChanged = true;
				}
				FSliceCount = 0;		
			}
			else
			{
				char[] s = {','};
				string[] slices = Spread.Split(s);
				
				int newSliceCount = (int) slices.Length / FDimension;
				if (newSliceCount != FSliceCount)
				{
					FSliceCount = newSliceCount;
					FValues = new string[FSliceCount];
					FSliceCountIsChanged = true;
				}
				
				for (int i=0; i<SliceCount; i++)
				{
					//SetString(i, slices[i]);
					if (FValues[i] != slices[i])
						FPinIsChanged = true;
					FValues[i] = slices[i];
				}
			}
			
			
			
			if (FOnConfigurate != null)
				FOnConfigurate(this);
		}
	}	
}
