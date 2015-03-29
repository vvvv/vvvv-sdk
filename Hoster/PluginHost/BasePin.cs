using System;
using System.Collections;
using System.Drawing;

using VVVV.PluginInterfaces.V1;

namespace Hoster
{
	public delegate void TOnConfigurate(IPluginConfig Input);
	
	abstract public class TBasePin: IPluginIO, IPluginIn, IPluginOut
	{
		public int FSliceCount = 0;
		protected IPluginHost FParent;
		protected string FName;
		protected int FOrder;
		protected int FDimension;
		protected bool FAutoValidate;
		protected bool FPinIsChanged;
		protected bool FSliceCountIsChanged;
		private TSliceMode FSliceMode;
		private TPinVisibility FVisibility;
		private TPinDirection FPinDirection;

		protected const int FSliceHeight = 15;
		
		abstract protected void ChangeSliceCount();
		abstract protected string AsString(int index);
				
		public TOnConfigurate FOnConfigurate;
		
		static int GlobalPinCount = 0;
		
		public TBasePin(IPluginHost Parent, string PinName, int Dimension, TPinDirection PinDirection, TOnConfigurate Callback, TSliceMode SliceMode, TPinVisibility Visibility)
		{
			FParent = Parent;
			FName = PinName;
			FDimension = Dimension;
			FSliceMode = SliceMode;
			FVisibility = Visibility;
			FPinDirection = PinDirection;
			FOnConfigurate = Callback;
			FPinIsChanged = true;
			
			SliceCount = 1;
			
			//initialize with an arbitrary order so that the sortby-order in the gui is not flickering
			FOrder = GlobalPinCount++;
		}

		public string Name
	    {
	        get {return FName;}
	        set {FName = value;}
		}
		
		public TSliceMode SliceMode
	    {
	        get {return FSliceMode;}
	        set {FSliceMode = value;}
		}
		
		public int Order
	    {
	        get {return FOrder;}
	        set {FOrder = value;}
		}
		
		public int Dimension
	    {
	        get {return FDimension;}
	        set {FDimension = value;}
		}
		
		public TPinDirection Direction
	    {
	        get {return FPinDirection;}
	        set {FPinDirection = value;}
		}
		
		public TPinVisibility Visibility
	    {
	        get {return FVisibility;}
	        set {FVisibility = value;}
		}
		
		public bool AutoValidate
	    {
	        get {return FAutoValidate;}
	        set {FAutoValidate = value;}
		}
		
		public int SliceCount
		{
			get {return FSliceCount;}
			set 
			{
				if (FSliceCount != value)
				{
					FSliceCount = value;
					
					ChangeSliceCount();
					
					FSliceCountIsChanged = true;
					FPinIsChanged = true;
					
					//don't call configurate here for vvvv doesn't do it either
					//if (FOnConfigurate != null)
					//	FOnConfigurate(this);
				}
			}
		}
		
//		public void SetPinUpdater(IPinUpdater pinUpdater)
//		{
//			
//		}

		public bool SliceCountIsChanged
		{
			get {return FSliceCountIsChanged;}
		}
			
		public bool IsConnected
		{
			get {return false;}
		}
		
		public IPluginHost PluginHost
		{
			get {return null;}
		}
		
		public bool PinIsChanged
		{
			get {return FPinIsChanged || FSliceCountIsChanged;}
		}
		
		public void Invalidate()
		{
			FPinIsChanged = false;
			FSliceCountIsChanged = false;
		}
		
		public string SpreadAsString
		{
			get
			{
				string spread = "";
			
				for (int i=0; i<FSliceCount*FDimension; i++)
					spread += AsString(i) + ",";
				
				char[] t = {','};
				return spread.TrimEnd(t);
			}
			set
			{
				SetSpreadAsString(value);					
			}
		}
		
		protected virtual string AsDisplayString(int Index)
		{
			return AsString(Index);
		}
		
		abstract public void SetSpreadAsString(string Spread);
		
		virtual public void Draw(Graphics g, Font f, Brush b, Pen p, Rectangle r)
		{
			g.DrawRectangle(p, r);
			
			g.DrawString(Name, f, b, r.X+2, 2);

			for (int i=0; i<SliceCount*Dimension; i++)
			{
				g.DrawString(AsDisplayString(i), f, b, r.X+2, FSliceHeight + 2 + i * FSliceHeight);
			}
		}
		
		public bool AllowFeedback 
		{
		    //TODO: not implmented
		    get;
			set;
		}
		
		public bool Validate()
		{
		    //TODO: not implmented
		    return false;
		}
	}	
}
