//Version 1 of the VVVV PluginInterface
//
//to convert this to a typelib make sure AssemblyInfo.cs states: ComVisible(true) 
//then on a commandline type:
// regasm _PluginInterfaces.dll /tlb
//this generates and registers the typelib which can then be imported e.g. via Delphi:Components:Import Component:Import Typelib


using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;
using VVVV.Utils.VMath;
using VVVV.Utils.VColor;


namespace VVVV.PluginInterfaces.V1
{
	//enum types
	public enum TSliceMode {Single, Dynamic};
	public enum TComponentMode {Hidden, InABox, InAWindow};	
	public enum TPinVisibility {False, OnlyInspector, Hidden, True};
	public enum TPinDirection {Configuration, Input, Output};
	public enum TLogType {Debug, Message, Warning, Error};	
	
	#region basic pins
	
	 //basic pin
	[Guid("D3C5CB5C-C054-4AB6-AC04-6BDB34692B25"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginIO								   
	{
		string Name{get; set;}
		int Order{get; set;}
		bool IsConnected{get;}
	}	
	
	//basic config pin
	[Guid("11FDCEBD-FFC0-415D-90D5-DA4DBBDB5B67"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginConfig: IPluginIO								   
	{
		int SliceCount{get; set;}
		string SpreadAsString{get;}
	}
	
	//basic input pin 
	[Guid("68C6F37B-1D45-4683-9FC2-BC2580187D44"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginIn: IPluginIO					
	{
		int SliceCount{get;}
		string SpreadAsString{get;}
		bool PinIsChanged{get;}
		//bool SliceIsChanged[int index]{get;}
	}
	
	//basic fast input pin 
	[Guid("9AFAD289-7C11-4296-B232-8B33FAC3E27D"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginFastIn: IPluginIO					
	{
		int SliceCount{get;}
		string SpreadAsString{get;}
	}
	
	//basic output pin 
	[Guid("67FB9F25-0579-495C-8535-28CC15F54C55"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginOut: IPluginIO					
	{
		int SliceCount{set;}
		string SpreadAsString{set;}
	}	
	
	#endregion basic pins
	
	#region value pins

	[Guid("46154821-A76F-4258-846D-8524957F98D4"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	unsafe public interface IValueConfig: IPluginConfig		//value configuration pin
	{
		//double this[int index]{get; set;}
		void SetValue(int Index, double Value);
		void SetValue2D(int Index, double Value1, double Value2);
		void SetValue3D(int Index, double Value1, double Value2, double Value3);
		void SetValue4D(int Index, double Value1, double Value2, double Value3, double Value4);
		void SetMatrix(int Index, Matrix4x4 Value);
		
		void GetValue(int Index, out double Value);
		void GetValue2D(int Index, out double Value1, out double Value2);
		void GetValue3D(int Index, out double Value1, out double Value2, out double Value3);
		void GetValue4D(int Index, out double Value1, out double Value2, out double Value3, out double Value4);
		void GetMatrix(int Index, out Matrix4x4 Value);			
		void GetValuePointer(out int SliceCount, out double* Value);  
		
		void SetSubType  (double Min, double Max, double StepSize, double Default, bool IsBang, bool IsToggle, bool IsInteger);
		void SetSubType2D(double Min, double Max, double StepSize, double Default1, double Default2, bool IsBang, bool IsToggle, bool IsInteger);
		void SetSubType3D(double Min, double Max, double StepSize, double Default1, double Default2, double Default3, bool IsBang, bool IsToggle, bool IsInteger);
		void SetSubType4D(double Min, double Max, double StepSize, double Default1, double Default2, double Default3, double Default4, bool IsBang, bool IsToggle, bool IsInteger);
	}
	
	[Guid("40137258-9CDE-49F4-93BA-DE7D91007809"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	unsafe public interface IValueIn: IPluginIn				//value input pin
	{	
		void GetValue(int Index, out double Value);
		void GetValue2D(int Index, out double Value1, out double Value2);
		void GetValue3D(int Index, out double Value1, out double Value2, out double Value3);
		void GetValue4D(int Index, out double Value1, out double Value2, out double Value3, out double Value4);
		void GetMatrix(int Index, out Matrix4x4 Value);
		//attention: don't write values into buffer!
		void GetValuePointer(out int SliceCount, out double* Value); 
		
		void SetSubType  (double Min, double Max, double StepSize, double Default, bool IsBang, bool IsToggle, bool IsInteger);
		void SetSubType2D(double Min, double Max, double StepSize, double Default1, double Default2, bool IsBang, bool IsToggle, bool IsInteger);
		void SetSubType3D(double Min, double Max, double StepSize, double Default1, double Default2, double Default3, bool IsBang, bool IsToggle, bool IsInteger);
		void SetSubType4D(double Min, double Max, double StepSize, double Default1, double Default2, double Default3, double Default4, bool IsBang, bool IsToggle, bool IsInteger);
	}

	[Guid("095081B7-D929-4459-83C0-18AA809E6635"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	unsafe public interface IValueFastIn: IPluginFastIn		//fast value input pin
	{	
		void GetValue(int Index, out double Value);
		void GetValue2D(int Index, out double Value1, out double Value2);
		void GetValue3D(int Index, out double Value1, out double Value2, out double Value3);
		void GetValue4D(int Index, out double Value1, out double Value2, out double Value3, out double Value4);
		void GetMatrix(int Index, out Matrix4x4 Value);			
		//attention: pointer goes to output of other node. don't write values into buffer!
		void GetValuePointer(out int SliceCount, out double* Value);
		
		void SetSubType  (double Min, double Max, double StepSize, double Default, bool IsBang, bool IsToggle, bool IsInteger);
		void SetSubType2D(double Min, double Max, double StepSize, double Default1, double Default2, bool IsBang, bool IsToggle, bool IsInteger);
		void SetSubType3D(double Min, double Max, double StepSize, double Default1, double Default2, double Default3, bool IsBang, bool IsToggle, bool IsInteger);
		void SetSubType4D(double Min, double Max, double StepSize, double Default1, double Default2, double Default3, double Default4, bool IsBang, bool IsToggle, bool IsInteger);
	}
	
	[Guid("B55B70E8-9C3D-408D-B9F9-A90CF8288FC7"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	unsafe public interface IValueOut: IPluginOut			//value output pin
	{
		//double this[int index]{set;}
		void SetValue(int Index, double Value);		
		void SetValue2D(int Index, double Value1, double Value2);
		void SetValue3D(int Index, double Value1, double Value2, double Value3);
		void SetValue4D(int Index, double Value1, double Value2, double Value3, double Value4);
		void SetMatrix(int Index, Matrix4x4 Value);
		void GetValuePointer(out double* Value);
		
		void SetSubType(double Min, double Max, double StepSize, double Default, bool IsBang, bool IsToggle, bool IsInteger);
		void SetSubType2D(double Min, double Max, double StepSize, double Default1, double Default2, bool IsBang, bool IsToggle, bool IsInteger);
		void SetSubType3D(double Min, double Max, double StepSize, double Default1, double Default2, double Default3, bool IsBang, bool IsToggle, bool IsInteger);
		void SetSubType4D(double Min, double Max, double StepSize, double Default1, double Default2, double Default3, double Default4, bool IsBang, bool IsToggle, bool IsInteger);
	}
	
	#endregion value pins
	
	#region string pins
	
	//string configuration pin
	[Guid("1FF25AD1-FBAB-4B29-8BAC-82CE53135868"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IStringConfig: IPluginConfig	 
	{
		void SetString(int Index, string Value);
		void GetString(int Index, out string Value);
		void SetSubType(string Default, bool IsFilename);
	}
	
	//string input pin
	[Guid("E329D418-20DE-4D91-B060-60EF2D73A7A6"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IStringIn: IPluginIn			
	{
		//string this[int index]{get;}
		void GetString(int Index, out string Value);
		void SetSubType(string Default, bool IsFilename);
	}

	//string output pin
	[Guid("EC32C616-A85F-42AC-B7D1-630E1F739D1D"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IStringOut: IPluginOut			
	{
		//string this[int index]{set;}
		void SetString(int Index, string Value);
		void SetSubType(string Default, bool IsFilename);
	}
	
	#endregion string pins
	
	#region color pins
	
	//color configuration pin
	[Guid("BAA49637-29FA-426A-9188-86906E660D30"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IColorConfig: IPluginConfig				
	{
		void SetColor(int Index, RGBAColor Color);
		void GetColor(int Index, out RGBAColor Color);
		void SetSubType(RGBAColor Default, bool HasAlpha);
	}
	
	//color input pin
	[Guid("CB6289A8-28BD-4A52-9B7A-BC1092EA2FA5"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IColorIn: IPluginIn				    
	{
		void GetColor(int Index, out RGBAColor Color);
		void SetSubType(RGBAColor Default, bool HasAlpha);
	}
	
	//color output pin
	[Guid("432CE6BA-6F57-4387-A223-D2DAFA8125F0"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IColorOut: IPluginOut		        
	{
		void SetColor(int Index, RGBAColor Color);
		void SetSubType(RGBAColor Default, bool HasAlpha);
	}
	
	#endregion color pins
	
	#region node pins
	
	//transform input pin
	[Guid("605FD0B2-AD68-40B4-92E5-819599544CF2"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface ITransformIn: IPluginIn				
	{
		void GetMatrix(int Index, out Matrix4x4 Value);
	}
	
	//transform output pin
	[Guid("AA8D6410-36E5-4EA2-AF70-66CD6321FF36"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface ITransformOut: IPluginOut			
	{
		void SetMatrix(int Index, Matrix4x4 Value);
	}
	
	#endregion node pins
	
	#region host, plugin
	
	//host
	[Guid("E72C5CF0-4738-4F20-948E-83E96D4E7843"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPluginHost							
    {
        void CreateValueConfig      (string Name, int Dimension, string[] DimensionNames, TSliceMode SliceMode, TPinVisibility Visibility, out IValueConfig Pin);
        void CreateValueInput       (string Name, int Dimension, string[] DimensionNames, TSliceMode SliceMode, TPinVisibility Visibility, out IValueIn Pin);
        void CreateValueFastInput   (string Name, int Dimension, string[] DimensionNames, TSliceMode SliceMode, TPinVisibility Visibility, out IValueFastIn Pin);
        void CreateValueOutput      (string Name, int Dimension, string[] DimensionNames, TSliceMode SliceMode, TPinVisibility Visibility, out IValueOut Pin);
        
        void CreateStringConfig		(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IStringConfig Pin);
        void CreateStringInput      (string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IStringIn Pin);
        void CreateStringOutput     (string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IStringOut Pin);
        
        void CreateColorConfig		(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IColorConfig Pin);
        void CreateColorInput       (string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IColorIn Pin);
        void CreateColorOutput      (string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IColorOut Pin);
        
        void CreateTransformInput   (string Name, TSliceMode SliceMode, TPinVisibility Visibility, out ITransformIn Pin);
        void CreateTransformOutput  (string Name, TSliceMode SliceMode, TPinVisibility Visibility, out ITransformOut Pin);
        
        void DeletePin(IPluginIO Pin);
        void GetCurrentTime(out double CurrentTime);
        void GetHostPath(out string Path);
        
        void Log(TLogType Type, string Message);
    }
    
    //plugin
    [Guid("7F813C89-4EDE-4087-A626-4320BE41C87F"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPlugin								
    {
        void SetPluginHost(IPluginHost Host);
        void Configurate(IPluginConfig Input);
        void Evaluate(int SpreadMax);
        bool AutoEvaluate {get;}
    }
    
    #endregion host, plugin
    
	#region plugin info
    
    //plugin info interface
    [Guid("16EE5CF9-0D75-4ECF-9440-7D2909E8F7DC"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginInfo
	{
		string Name {get; set;}
		string Category {get; set;}
		string Version {get; set;}
		string Help {get; set;}
		string Tags {get; set;}
		string Author {get; set;}
		string Credits {get; set;}
		string Bugs {get; set;}
		string Warnings {get; set;}
		string Namespace {get; set;}
		string Class {get; set;}
		Size InitialWindowSize {get; set;}
		Size InitialBoxSize {get; set;}
		TComponentMode InitialComponentMode {get; set;}
	}
	
	//plugin info inplementation
	[Guid("FE1216D6-5439-416D-8FB7-16E9A29EF67B")]
	public class PluginInfo: MarshalByRefObject, IPluginInfo
	{
		private string FName = "";
		private string FCategory = "";
		private string FVersion = "";
		private string FAuthor = "";
		private string FHelp = "";
		private string FTags = "";
		private string FBugs = "";
		private string FCredits = "";
		private string FWarnings = "";
		private string FNamespace = "";
		private string FClass = "";
		private Size FInitialWindowSize = new Size(0, 0);
		private Size FInitialBoxSize = new Size(0, 0);
		private TComponentMode FInitialComponentMode = TComponentMode.Hidden;
		
	    public string Name
	    {
	        get {return FName;}
	        set {FName = value;}
		}
	    public string Category
	    {
	        get {return FCategory;}
	        set {FCategory = value;}
		}
	    public string Version
	    {
	        get {return FVersion;}
	        set {FVersion = value;}
		}
	    public string Author
	    {
	        get {return FAuthor;}
	        set {FAuthor = value;}
		}
	    public string Help
	    {
	        get {return FHelp;}
	        set {FHelp = value;}
		}
	    public string Tags
	    {
	        get {return FTags;}
	        set {FTags = value;}
		}
	    public string Bugs
	    {
	        get {return FBugs;}
	        set {FBugs = value;}
		}
	    public string Credits
	    {
	        get {return FCredits;}
	        set {FCredits = value;}
		}
	    public string Warnings
	    {
	        get {return FWarnings;}
	        set {FWarnings = value;}
		}
	    public string Namespace
	    {
	        get {return FNamespace;}
	        set {FNamespace = value;}
		}
	    public string Class
	    {
	        get {return FClass;}
	        set {FClass = value;}
		}
	    public Size InitialWindowSize
	    {
	        get {return FInitialWindowSize;}
	        set {FInitialWindowSize = value;}
		}
	    public Size InitialBoxSize
	    {
	        get {return FInitialBoxSize;}
	        set {FInitialBoxSize = value;}
		}
	    public TComponentMode InitialComponentMode
	    {
	        get {return FInitialComponentMode;}
	        set {FInitialComponentMode = value;}
		}
	}
	
	#endregion plugin info
	
}
