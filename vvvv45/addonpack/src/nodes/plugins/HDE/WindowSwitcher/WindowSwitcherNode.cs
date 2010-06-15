#region licence/info

//////project name
//vvvv plugin template with gui

//////description
//basic vvvv plugin template with gui.
//Copy this an rename it, to write your own plugin node.

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//C# sharpdevelop

//////dependencies
//VVVV.PluginInterfaces.V1;

//////initial author
//vvvv group

#endregion licence/info

//use what you need
using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V1;

//the vvvv node namespace
namespace VVVV.Nodes.WindowSwitcher
{
    //class definition, inheriting from UserControl for the GUI stuff
    public class WindowSwitcherPluginNode: UserControl, IHDEPlugin, IWindowSwitcher, IWindowListener
    {
        #region field declaration
        
        //the host (mandatory)
        private IPluginHost FPluginHost;
        private IHDEHost FHDEHost;
        private IWindowSwitcherHost FWindowSwitcherHost;
        // Track whether Dispose has been called.
        private bool FDisposed = false;
        
        private List<WindowListControl> FWindowList = new List<WindowListControl>();
        private int FWindowWidth;
        private int FSelectedWindowIndex;
        
        #endregion field declaration
        
        #region constructor/destructor
        public WindowSwitcherPluginNode()
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
                }
                // Release unmanaged resources. If disposing is false,
                // only the following code is executed.
                
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
                    //see: http://www.vvvv.org/tiki-index.php?page=Conventions.NodeAndPinNaming
                    FPluginInfo = new PluginInfo();
                    
                    //the nodes main name: use CamelCaps and no spaces
                    FPluginInfo.Name = "WindowSwitcher";
                    //the nodes category: try to use an existing one
                    FPluginInfo.Category = "HDE";
                    //the nodes version: optional. leave blank if not
                    //needed to distinguish two nodes of the same name and category
                    FPluginInfo.Version = "";
                    
                    //the nodes author: your sign
                    FPluginInfo.Author = "vvvv group";
                    //describe the nodes function
                    FPluginInfo.Help = "Window Switcher";
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
                    FPluginInfo.InitialWindowSize = new Size(400, 300);
                    //define the nodes initial component mode
                    FPluginInfo.InitialComponentMode = TComponentMode.InAWindow;
                    
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
        	this.textBoxDummy = new System.Windows.Forms.TextBox();
        	this.SuspendLayout();
        	// 
        	// textBoxDummy
        	// 
        	this.textBoxDummy.Location = new System.Drawing.Point(73, 30);
        	this.textBoxDummy.Name = "textBoxDummy";
        	this.textBoxDummy.Size = new System.Drawing.Size(100, 20);
        	this.textBoxDummy.TabIndex = 0;
        	this.textBoxDummy.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TextBoxDummyKeyUp);
        	// 
        	// WindowSwitcherPluginNode
        	// 
        	this.BackColor = System.Drawing.Color.Silver;
        	this.Controls.Add(this.textBoxDummy);
        	this.DoubleBuffered = true;
        	this.Name = "WindowSwitcherPluginNode";
        	this.Size = new System.Drawing.Size(288, 180);
        	this.ResumeLayout(false);
        	this.PerformLayout();
        }
        private System.Windows.Forms.TextBox textBoxDummy;
        
        #region initialization
        
        //this method is called by vvvv when the node is created
        public void SetPluginHost(IPluginHost host)
        {
            //assign host
            FPluginHost = host;
        }
        
        public void SetHDEHost(IHDEHost host)
        {
            //assign host
            FHDEHost = host;
            FHDEHost.AddListener(this);
        }
        #endregion initialization

        #region IWindowListener
        public void WindowAddedCB(IWindow window)
        {
            TWindowType t = window.GetWindowType();
            WindowListControl wlc = new WindowListControl(window);
            wlc.Click += new EventHandler(WindowListControlClick);
            wlc.MouseEnter += new EventHandler(WindowListControlMouseEnter);
            wlc.MouseLeave += new EventHandler(WindowListControlMouseLeave);
            FWindowList.Add(wlc);
            FWindowList.Sort(delegate(WindowListControl w1, WindowListControl w2)
                             {
                                 if (w1.Window.GetWindowType() > w2.Window.GetWindowType())
                                     return 1;
                                 else if (w1.Window.GetWindowType() < w2.Window.GetWindowType())
                                     return -1;
                                 else
                                     return string.Compare(w1.Window.GetCaption(), w2.Window.GetCaption());
                             });
        }
        
        public void WindowRemovedCB(IWindow window)
        {
            WindowListControl windowToRemove = FWindowList.Find(delegate (WindowListControl wlc) {return wlc.Window == window;});
            FWindowList.Remove(windowToRemove);
        }
        #endregion IWindowListener
        
        #region IWindowSwitcher
        public void SetWindowSwitcherHost(IWindowSwitcherHost host)
        {
            FWindowSwitcherHost = host;
        }
        
        public void Initialize(IWindow window, out int width, out int height)
        {
            //mark current window
            WindowListControl currentWindow = FWindowList.Find(delegate (WindowListControl wlc) {return wlc.Window == window;});
            FWindowList[FSelectedWindowIndex].Selected = false;
            FSelectedWindowIndex = FWindowList.IndexOf(currentWindow);
            FWindowList[FSelectedWindowIndex].Selected = true;
            
            foreach(WindowListControl wlc in FWindowList)
            {
                wlc.UpdateCaption();
                FWindowWidth = Math.Max(FWindowWidth, wlc.CaptionWidth);
            }
            
            UpdateList();
            
            //return dimensions of this listing
            width = FWindowWidth;
            height = this.Controls[0].Top + this.Controls[0].Height;
        }
        
        public void AfterShow()
        {
            //the dummy textbox gets the focus to trigger on CTRL key up
            textBoxDummy.Focus();
        }
        
        public void Up()
        {
            FWindowList[FSelectedWindowIndex].Selected = false;
            FSelectedWindowIndex -= 1;
            if (FSelectedWindowIndex == -1)
                FSelectedWindowIndex = FWindowList.Count - 1;
            FWindowList[FSelectedWindowIndex].Selected = true;
        }
        
        public void Down()
        {
            FWindowList[FSelectedWindowIndex].Selected = false;
            FSelectedWindowIndex = (FSelectedWindowIndex + 1) % FWindowList.Count;
            FWindowList[FSelectedWindowIndex].Selected = true;
        }
        #endregion IWindowSwitcher
        
        private void UpdateList()
        {
            this.SuspendLayout();
            this.Controls.Clear();
            //the dummy has keyboardfocus and triggers on CTRL key up
            this.Controls.Add(textBoxDummy);
            
            CaptionControl title;
            //add patches
            List<WindowListControl> patches = FWindowList.FindAll(delegate (WindowListControl wlc) {return wlc.Window.GetWindowType() == TWindowType.Patch;});
            if (patches.Count > 0)
            {
                title = new CaptionControl("Patches");
                this.Controls.Add(title);
                title.Dock = DockStyle.Top;
                title.BringToFront();
                
                foreach (WindowListControl wlc in patches)
                {
                    this.Controls.Add(wlc);
                    wlc.Dock = DockStyle.Top;
                    wlc.BringToFront();
                }
            }
            
            //add modules
            List<WindowListControl> modules = FWindowList.FindAll(delegate (WindowListControl wlc) {return wlc.Window.GetWindowType() == TWindowType.Module;});
            if (modules.Count > 0)
            {
                title = new CaptionControl("Modules");
                this.Controls.Add(title);
                title.Dock = DockStyle.Top;
                title.BringToFront();
                
                foreach (WindowListControl wlc in modules)
                {
                    this.Controls.Add(wlc);
                    wlc.Dock = DockStyle.Top;
                    wlc.BringToFront();
                }
            }
            
            //add editors
            List<WindowListControl> editors = FWindowList.FindAll(delegate (WindowListControl wlc) {return wlc.Window.GetWindowType() == TWindowType.Editor;});
            if (editors.Count > 0)
            {
                title = new CaptionControl("Editors");
                this.Controls.Add(title);
                title.Dock = DockStyle.Top;
                title.BringToFront();
                
                foreach (WindowListControl wlc in editors)
                {
                    this.Controls.Add(wlc);
                    wlc.Dock = DockStyle.Top;
                    wlc.BringToFront();
                }
            }
            
            //add renderer
            List<WindowListControl> renderer = FWindowList.FindAll(delegate (WindowListControl wlc) {return wlc.Window.GetWindowType() == TWindowType.Renderer;});
            if (renderer.Count > 0)
            {
                title = new CaptionControl("Renderer");
                this.Controls.Add(title);
                title.Dock = DockStyle.Top;
                title.BringToFront();
                
                foreach (WindowListControl wlc in renderer)
                {
                    this.Controls.Add(wlc);
                    wlc.Dock = DockStyle.Top;
                    wlc.BringToFront();
                }
            }
            
            //add plugins
            List<WindowListControl> plugins = FWindowList.FindAll(delegate (WindowListControl wlc) {return wlc.Window.GetWindowType() == TWindowType.Plugin;});
            if (plugins.Count > 0)
            {
                title = new CaptionControl("Plugin");
                this.Controls.Add(title);
                title.Dock = DockStyle.Top;
                title.BringToFront();
                
                foreach (WindowListControl wlc in plugins)
                {
                    this.Controls.Add(wlc);
                    wlc.Dock = DockStyle.Top;
                    wlc.BringToFront();
                }
            }
            
            //add HDEs
            List<WindowListControl> hdes = FWindowList.FindAll(delegate (WindowListControl wlc) {return wlc.Window.GetWindowType() == TWindowType.HDE;});
            if (hdes.Count > 0)
            {
                title = new CaptionControl("HDE");
                this.Controls.Add(title);
                title.Dock = DockStyle.Top;
                title.BringToFront();
                
                foreach (WindowListControl wlc in hdes)
                {
                    this.Controls.Add(wlc);
                    wlc.Dock = DockStyle.Top;
                    wlc.BringToFront();
                }
            }

            this.ResumeLayout(true);
        }
      
        void TextBoxDummyKeyUp(object sender, KeyEventArgs e)
        {
            if ((e.KeyData == Keys.ControlKey) || (e.KeyData == Keys.Control))
                FWindowSwitcherHost.HideMe(FWindowList[FSelectedWindowIndex].Window);
        }
        
        void WindowListControlClick(object sender, EventArgs e)
        {
            FWindowSwitcherHost.HideMe(((WindowListControl) sender).Window);
        }
        
        void WindowListControlMouseEnter(object sender, EventArgs e)
        {
            //deselect previously selected 
            FWindowList[FSelectedWindowIndex].Selected = false;
                
            //select sender
            (sender as WindowListControl).Selected = true;
            FSelectedWindowIndex = FWindowList.IndexOf(sender as WindowListControl);
        }
        
        void WindowListControlMouseLeave(object sender, EventArgs e)
        {
            //deselect sender
            (sender as WindowListControl).Selected = false;
        }
    }
}
