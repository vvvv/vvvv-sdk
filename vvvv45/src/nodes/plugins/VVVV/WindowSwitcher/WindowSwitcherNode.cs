#region usings
using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;
#endregion usings

//the vvvv node namespace
namespace VVVV.Nodes.WindowSwitcher
{
    [PluginInfo(Name = "WindowSwitcher",
                Category = "VVVV",
                Ignore = true,
                Author = "vvvv group",
                Help = "The Window Switcher")]
    public class WindowSwitcherPluginNode: UserControl, IWindowSwitcher, IWindowListener
    {
        #region field declaration
        private IHDEHost FHDEHost;
        [Import]
        protected IWindowSwitcherHost FWindowSwitcherHost;
        // Track whether Dispose has been called.
        private bool FDisposed = false;
        
        private List<WindowListControl> FFullWindowList = new List<WindowListControl>();
        private int FWindowWidth;
        private int FSelectedWindowIndex;
        
        #endregion field declaration
        
        #region constructor/destructor
        [ImportingConstructor]
        public WindowSwitcherPluginNode(IHDEHost host)
        {
            // The InitializeComponent() call is required for Windows Forms designer support.
            InitializeComponent();
            
            FHDEHost = host;
            FHDEHost.AddListener(this);
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
                    FHDEHost.RemoveListener(this);
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
 
        private void InitializeComponent()
        {
        	this.FDummyTextBox = new System.Windows.Forms.TextBox();
        	this.SuspendLayout();
        	// 
        	// FDummyTextBox
        	// 
        	this.FDummyTextBox.Location = new System.Drawing.Point(63, 25);
        	this.FDummyTextBox.Name = "FDummyTextBox";
        	this.FDummyTextBox.Size = new System.Drawing.Size(100, 20);
        	this.FDummyTextBox.TabIndex = 0;
        	this.FDummyTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.FDummyTextBoxKeyUp);
        	// 
        	// WindowSwitcherPluginNode
        	// 
        	this.BackColor = System.Drawing.Color.Silver;
        	this.Controls.Add(this.FDummyTextBox);
        	this.DoubleBuffered = true;
        	this.Name = "WindowSwitcherPluginNode";
        	this.Size = new System.Drawing.Size(288, 180);
        	this.ResumeLayout(false);
        	this.PerformLayout();
        }
        private System.Windows.Forms.TextBox FDummyTextBox;
        #endregion constructor/destructor

        #region IWindowListener
        public void WindowAddedCB(IWindow window)
        {
        	//deselect current index
        	if (FFullWindowList.Count > FSelectedWindowIndex)
        		FFullWindowList[FSelectedWindowIndex].Selected = false;
        	//add new window
            WindowListControl wlc = new WindowListControl(window);
            wlc.Click += new EventHandler(WindowListControlClick);
            wlc.MouseEnter += new EventHandler(WindowListControlMouseEnter);
            wlc.MouseLeave += new EventHandler(WindowListControlMouseLeave);
            FFullWindowList.Add(wlc);
        }
        
        public void WindowRemovedCB(IWindow window)
        {
        	//deselect current index
        	FFullWindowList[FSelectedWindowIndex].Selected = false;
            //remove window from list
        	WindowListControl windowToRemove = FFullWindowList.Find(delegate (WindowListControl wlc) {return wlc.Window == window;});
            FFullWindowList.Remove(windowToRemove);
        }
        #endregion IWindowListener
        
        #region IWindowSwitcher
        public void Initialize(IWindow window, out int width, out int height)
        {
            UpdateList();

            //unmark current window
            WindowListControl currentWindow = FFullWindowList.Find(delegate (WindowListControl wlc){return wlc.Window == window;});
            FFullWindowList[FSelectedWindowIndex].Selected = false;
            //mark current window
            FSelectedWindowIndex = FFullWindowList.IndexOf(currentWindow);
            FFullWindowList[FSelectedWindowIndex].Selected = true;
            
            foreach(WindowListControl wlc in FFullWindowList)
            {
                wlc.UpdateCaption();
                FWindowWidth = Math.Max(FWindowWidth, wlc.CaptionWidth);
            }          
            
            //return dimensions of this listing
            width = FWindowWidth;
            height = this.Controls[0].Top + this.Controls[0].Height;
        }
        
        public void AfterShow()
        {
            //the dummy textbox gets the focus to trigger on CTRL key up
            FDummyTextBox.Focus();
        }
        
        public void Up()
        {
            FFullWindowList[FSelectedWindowIndex].Selected = false;
            FSelectedWindowIndex -= 1;
            
            WindowListControl kommunikator = FFullWindowList.Find(delegate (WindowListControl wlc) {return wlc.Window.GetCaption() == "Kommunikator";});
            
            if (FSelectedWindowIndex == -1)
            	if (kommunikator.Window.IsVisible())
	                FSelectedWindowIndex = FFullWindowList.Count - 1;
            	else
            		FSelectedWindowIndex = FFullWindowList.Count - 2;
            
            FFullWindowList[FSelectedWindowIndex].Selected = true;
        }
        
        public void Down()
        {
            FFullWindowList[FSelectedWindowIndex].Selected = false;
            WindowListControl kommunikator = FFullWindowList.Find(delegate (WindowListControl wlc) {return wlc.Window.GetCaption() == "Kommunikator";});
            if (kommunikator.Window.IsVisible())
	            FSelectedWindowIndex = (FSelectedWindowIndex + 1) % FFullWindowList.Count;
            else
            	FSelectedWindowIndex = (FSelectedWindowIndex + 1) % (FFullWindowList.Count - 1);
            FFullWindowList[FSelectedWindowIndex].Selected = true;
        }
        #endregion IWindowSwitcher
        
        private void UpdateList()
        {
        	//sort them windows
        	FFullWindowList.Sort(delegate(WindowListControl w1, WindowListControl w2)
                             {
                                 if (w1.Window.GetWindowType() > w2.Window.GetWindowType())
                                     return 1;
                                 else if (w1.Window.GetWindowType() < w2.Window.GetWindowType())
                                     return -1;
                                 else
                                     return string.Compare(w1.Window.GetCaption(), w2.Window.GetCaption());
                             });
        	
        	//redraw the list
            this.SuspendLayout();
            this.Controls.Clear();
            //the dummy has keyboardfocus and triggers on CTRL key up
            this.Controls.Add(FDummyTextBox);
            
            CaptionControl title;
            //add patches
            List<WindowListControl> patches = FFullWindowList.FindAll(delegate (WindowListControl wlc) {return wlc.Window.GetWindowType() == WindowType.Patch;});
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
            List<WindowListControl> modules = FFullWindowList.FindAll(delegate (WindowListControl wlc) {return wlc.Window.GetWindowType() == WindowType.Module;});
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
            List<WindowListControl> editors = FFullWindowList.FindAll(delegate (WindowListControl wlc) {return wlc.Window.GetWindowType() == WindowType.Editor;});
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
            List<WindowListControl> renderer = FFullWindowList.FindAll(delegate (WindowListControl wlc) {return wlc.Window.GetWindowType() == WindowType.Renderer;});
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
            List<WindowListControl> plugins = FFullWindowList.FindAll(delegate (WindowListControl wlc) {return wlc.Window.GetWindowType() == WindowType.Plugin;});
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
            List<WindowListControl> hdes = FFullWindowList.FindAll(delegate (WindowListControl wlc) {return ((wlc.Window.GetWindowType() == WindowType.HDE) && (wlc.Window.GetCaption() != "Kommunikator")) || ((wlc.Window.GetCaption() == "Kommunikator") && (wlc.Window.IsVisible()));});
            if (hdes.Count > 0)
            {
                title = new CaptionControl("vvvv");
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
        
        void WindowListControlClick(object sender, EventArgs e)
        {
            FWindowSwitcherHost.HideMe(((WindowListControl) sender).Window);
        }
        
        void WindowListControlMouseEnter(object sender, EventArgs e)
        {
            //deselect previously selected
            FFullWindowList[FSelectedWindowIndex].Selected = false;
            
            //select sender
            (sender as WindowListControl).Selected = true;
            FSelectedWindowIndex = FFullWindowList.IndexOf(sender as WindowListControl);
        }
        
        void WindowListControlMouseLeave(object sender, EventArgs e)
        {
            //deselect sender
            (sender as WindowListControl).Selected = false;
        }
        
        void FDummyTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            if ((e.KeyData == Keys.ControlKey) || (e.KeyData == Keys.Control))
                FWindowSwitcherHost.HideMe(FFullWindowList[FSelectedWindowIndex].Window);
        }
    }
}
