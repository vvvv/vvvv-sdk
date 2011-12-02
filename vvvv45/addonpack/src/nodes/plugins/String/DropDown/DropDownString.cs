#region licence/info

//////project name
//DropDown (String)

//////description
//DropDown Menu for strings

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//C# sharpdevelop 

//////dependencies
//VVVV.PluginInterfaces.V1;

//////initial author
//woei

#endregion licence/info

//use what you need
using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V1;

//the vvvv node namespace
namespace VVVV.Nodes
{
	
	//class definition, inheriting from UserControl for the GUI stuff
	public class DropDownString: UserControl, IPlugin, IDisposable
    {
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;
   		
    	//input pin declaration
    	private IValueConfig FListCount;
    	private IValueConfig FColumns;
    	private IStringIn FItems;
    	private IValueIn FBinSize;
    	private IValueIn FDefault;
    	private IValueIn FSelectItem;
    	private IValueIn FDoSelect;
    	
    	//output pin declaration
    	private IValueOut FSelectIndex;
    	private IStringOut FItem;
    	
    	//
        List<ComboBoxSpread> FComboList = new List<ComboBoxSpread>();
        List<double> selectedIndex = new List<double>();
        List<string> selectedItem = new List<string>();
        int FWindowWidth = 0;
        ComboBoxSpread startCombo;
    	
    	#endregion field declaration
        
    	#region constructor/destructor
    	
        public DropDownString()
        {
        	// The InitializeComponent() call is required for Windows Forms designer support.
			InitializeComponent();
		}
        
        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected override void Dispose(bool disposing)
        {
        	// Check to see if Dispose has already been called.
        	if(!FDisposed)
        	{
        		if(disposing)
        		{
        			// Dispose managed resources.
        			FComboList.Clear();
        			selectedIndex.Clear();
        			selectedItem.Clear();
        		}
        		// Release unmanaged resources. If disposing is false,
        		// only the following code is executed.
	        	
        		FHost.Log(TLogType.Debug, "DropDownString is being deleted");
        		
        		// Note that this is not thread safe.
        		// Another thread could start disposing the object
        		// after the managed resources are disposed,
        		// but before the disposed flag is set to true.
        		// If thread safety is necessary, it must be
        		// implemented by the client.
        	}
        	FDisposed = true;
        }
    
        #endregion constructor/destructor
                
        #region node name and infos
       
        //provide node infos 
        private static IPluginInfo FPluginInfo;
        public static IPluginInfo PluginInfo
	    {
	        get 
	        {
	        	if (FPluginInfo == null)
				{
					//fill out nodes info
					//see: http://www.vvvv.org/tiki-index.php?page=vvvv+naming+conventions
					FPluginInfo = new PluginInfo();
					
					//the nodes main name: use CamelCaps and no spaces
					FPluginInfo.Name = "DropDown";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "String";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "";
					
					//the nodes author: your sign
					FPluginInfo.Author = "woei";
					//describe the nodes function
					FPluginInfo.Help = "DropDown Menu for strings";
					//specify a comma separated list of tags that describe the node
					FPluginInfo.Tags = "";
					
					//give credits to thirdparty code used
					FPluginInfo.Credits = "";
					//any known problems?
					FPluginInfo.Bugs = "";
					//any known usage of the node that may cause troubles?
					FPluginInfo.Warnings = "";
					
					//define the nodes initial size in box-mode
					FPluginInfo.InitialBoxSize = new Size(200, 100);
					//define the nodes initial size in window-mode
					FPluginInfo.InitialWindowSize = new Size(200, 100);
					//define the nodes initial component mode
					FPluginInfo.InitialComponentMode = TComponentMode.InABox;
					
					//leave below as is
					System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
					System.Diagnostics.StackFrame sf = st.GetFrame(0);
					System.Reflection.MethodBase method = sf.GetMethod();
					FPluginInfo.Namespace = method.DeclaringType.Namespace;
					FPluginInfo.Class = method.DeclaringType.Name;
					//leave above as is
				}
				return FPluginInfo;
	        }
		}
        
        public bool AutoEvaluate
        {
        	//return true if this node needs to calculate every frame even if nobody asks for its output
        	get {return true;}
        }
        
        #endregion node name and infos
        
        private void InitializeComponent()
		{
        	this.startCombo = new ComboBoxSpread(FComboList.Count, 150, 1);
        	this.SuspendLayout();
        	// 
        	// GUITemplateWrk
        	// 
        	this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
        	this.Controls.Add(startCombo);
        	this.DoubleBuffered = true;
        	this.Name = "DropDownString";
        	this.Size = new System.Drawing.Size(150, 25);
        	this.Load += new System.EventHandler(this.GUITemplatePluginLoad);
        	this.ResumeLayout(false);
        	FWindowWidth = this.Width;
        }
        
        #region pin creation
        
        //this method is called by vvvv when the node is created
        public void SetPluginHost(IPluginHost Host)
	    {
        	//assign host
	    	FHost = Host;

	    	//create inputs
	    	FHost.CreateValueConfig("List Count", 1, null, TSliceMode.Single, TPinVisibility.OnlyInspector, out FListCount);
	    	FListCount.SetSubType(1, double.MaxValue, 1, 1, false, false, true);
	    	
	    	FHost.CreateValueConfig("Columns", 1, null, TSliceMode.Single, TPinVisibility.OnlyInspector, out FColumns);
	    	FColumns.SetSubType(1, double.MaxValue, 1, 1, false, false, true);
	    	
	    	FHost.CreateStringInput("List Item", TSliceMode.Dynamic, TPinVisibility.True, out FItems);
	    	FItems.SetSubType("Item", false);
	    	
	    	FHost.CreateValueInput("Bin Size", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FBinSize);
	    	FBinSize.SetSubType(-1, double.MaxValue, 1, -1, false, false, true);
	    	
	    	FHost.CreateValueInput("Default Slice", 1, null, TSliceMode.Single, TPinVisibility.Hidden, out FDefault);
	    	FDefault.SetSubType(0, double.MaxValue, 1, 0, false, false, true);
	    	
	    	FHost.CreateValueInput("Select Item", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FSelectItem);
	    	FSelectItem.SetSubType(0, double.MaxValue, 1, 0, false, false, true);
	    	
	    	FHost.CreateValueInput("Do Select", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FDoSelect);
	    	FDoSelect.SetSubType(0, 1, 1, 0, true, false, false);	

	    	
	    	//create outputs	    	
	    	FHost.CreateValueOutput("Selected Index", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FSelectIndex);
	    	FSelectIndex.SetSubType(0, double.MaxValue, 0.01, 0, false, false, true);
	    	
	    	FHost.CreateStringOutput("Selected Item", TSliceMode.Dynamic, TPinVisibility.True, out FItem);
	    	FItem.SetSubType("", false);
	    	
	    	FComboList.Add(startCombo);
        }

        #endregion pin creation
        
        #region mainloop
        
        public void Configurate(IPluginConfig Input)
        {
        	//nothing to configure in this plugin
        	//only used in conjunction with inputs of type cmpdConfigurate
        	if (Input == FListCount)
        	{
        		FWindowWidth = this.Width;
        		double count;
        		FListCount.GetValue(0, out count);
        		int diff = FComboList.Count - (int)Math.Round(count);
        		
        		if (diff > 0) //delete pins
        		{
        			for(int i = 0; i < diff; i++)
        			{
        				Controls.RemoveAt(FComboList.Count-1);
        				FComboList.RemoveAt(FComboList.Count-1);
        			}
        		}
        		
        		else if (diff < 0) //create pins
        		{
        			double cols, newWidth;
        			FColumns.GetValue(0, out cols);
        			newWidth=FWindowWidth/cols;
        			for(int i = 0; i > diff; i--)
        			{
        				ComboBoxSpread newCombo = new ComboBoxSpread(FComboList.Count, (int)newWidth, (int)cols);
						Controls.Add(newCombo);
						FComboList.Add(newCombo);
        			}
        		}
        		Refresh();
        	}
        	
        	if (Input == FColumns)
        	{
        		FWindowWidth = this.Width;
        		double cols, newWidth;
        		FColumns.GetValue(0, out cols);
        		newWidth=FWindowWidth/cols;
        		foreach (ComboBoxSpread c in FComboList)
        		{
        			c.SetBox((int)cols, (int)newWidth);
        		}
        		Refresh();
        	}        	
        }
        
        //here we go, thats the method called by vvvv each frame
        //all data handling should be in here
        public void Evaluate(int SpreadMax)
        {
        	if (FWindowWidth!=this.Width)
        	{
        		FWindowWidth=this.Width;
        		double columns;
        		FColumns.GetValue(0, out columns);
        		foreach (ComboBoxSpread cb in FComboList)
        		{
        			cb.SetBox((int)columns, (int)(FWindowWidth/columns));
        		}
        		Refresh();
        	}
        	
        	//set output Slicecount	
        	FSelectIndex.SliceCount=FComboList.Count;
        	FItem.SliceCount=FComboList.Count;
        	
        	if (selectedIndex.Count>FComboList.Count)
        	{
        		int diff = selectedIndex.Count-FComboList.Count;
        		selectedIndex.RemoveRange(FComboList.Count, diff);
        		selectedItem.RemoveRange(FComboList.Count, diff);
        	}
        	
        	//if any of the inputs has changed
        	//recompute the outputs
        	int binIncrement = 0;
        	for (int i=0; i<FComboList.Count; i++)
        	{
        		double tmpBinSize;
        		FBinSize.GetValue(i, out tmpBinSize);
        		int curBinSize = (int)tmpBinSize;
        		if (curBinSize < 0)
        		{
        			curBinSize=FItems.SliceCount;
        		}
        		
        		if (FItems.PinIsChanged || FComboList[i].Items.Count!=curBinSize)
        		{
        			List<string> curStringList = new List<string>();
        			for (int j=0; j<curBinSize; j++)
        			{
        				string currentStringSlice;
        				FItems.GetString(binIncrement+j, out currentStringSlice);
        				curStringList.Add(currentStringSlice);
        			}
        			
        			if (!FComboList[i].Items.Equals(curStringList))
        			    {
        			    	FComboList[i].Items.Clear();
        			    	foreach (string s in curStringList)
        			    	{
        			    		FComboList[i].Items.Add(s);
        			    	}
        			    	curStringList.Clear();
        			    }
        		}
        		binIncrement+=curBinSize;
        		
        		if (selectedIndex.Count<i+1)
        		{
        			selectedIndex.Add(0);
        			selectedItem.Add("");
        		}
        		
        		double curBangSelect;
        		FDoSelect.GetValue(i, out curBangSelect);
        		if (curBangSelect>0)
        		{
        			double currentValueSlice;
        			FSelectItem.GetValue(i, out currentValueSlice);
        			int selectIndex=(int)(currentValueSlice%FComboList[i].Items.Count);
        			FComboList[i].SelectedItem=FComboList[i].Items[selectIndex];
        		}
        		
        		if (FComboList[i].SelectedItem!=null && FComboList[i].isDropDown==false)
        		{
        			selectedIndex[i] = FComboList[i].SelectedIndex;
        			selectedItem[i] = FComboList[i].SelectedItem.ToString();
        		}
        		
        		if (FComboList[i].SelectedItem==null)
        		{
        			double curDefault;
        			FDefault.GetValue(i, out curDefault);
        			FComboList[i].SelectedItem=FComboList[i].Items[(int)curDefault];
        			selectedIndex[i] = FComboList[i].SelectedIndex;
        			selectedItem[i] = FComboList[i].SelectedItem.ToString();
        		}
        		
        		FSelectIndex.SetValue(i, selectedIndex[i]);
        		FItem.SetString(i, selectedItem[i]);
        	}
        }
             
        #endregion mainloop  
         
        protected override void OnPaint(PaintEventArgs e)
        {
        }
        
        void GUITemplatePluginLoad(object sender, EventArgs e)
        {	
        }
    }
	
	class ComboBoxSpread : System.Windows.Forms.ComboBox
	{
		public int id;
		public bool isDropDown = false;
		int width;
		int height;
		
		public ComboBoxSpread(int myId, int myWidth, int columns)
		{
			id = myId;
			width = myWidth;
			height = 20;
			this.AllowDrop = true;
        	this.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        	this.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
        	this.Font = new System.Drawing.Font("Lucida Sans Unicode", 7.00F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.ForeColor = System.Drawing.Color.Black;
        	this.FormattingEnabled = true;
        	this.Margin = new System.Windows.Forms.Padding(0);
        	this.Name = "comboBox"+id;
        	this.TabIndex = id;
        	this.Size = new System.Drawing.Size(width, height);
        	this.Location = new System.Drawing.Point((width*(id%columns)), (int)Math.Floor((double)id/columns)*height);
		}
		
		public void SetBox(int columns, int newWidth)
		{
			width=newWidth;
			this.Size = new System.Drawing.Size(width, height);
			this.Location = new System.Drawing.Point((width*(id%columns)), (int)Math.Floor((double)id/columns)*height);
		}
	
		protected override void OnDropDownClosed(EventArgs e)
		{
			base.OnDropDownClosed(e);
			isDropDown=false;
		}
		
		protected override void OnDropDown(EventArgs e)
		{
			base.OnDropDown(e);
			isDropDown=true;
		}
	}
}
