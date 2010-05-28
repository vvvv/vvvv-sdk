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
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.IO;

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.Crypto;

//the vvvv node namespace
namespace VVVV.Nodes.Kommunikator
{
    //class definition, inheriting from UserControl for the GUI stuff
    public class KommunikatorPluginNode: UserControl, IHDEPlugin, IKommunikator
    {
        #region field declaration
        
        //the host (mandatory)
        private IPluginHost FPluginHost;
        private IHDEHost FHDEHost;
        private IKommunikatorHost FKommunikatorHost;
        // Track whether Dispose has been called.
        private bool FDisposed = false;
        
        private StringHasher FStringHasher = new StringHasher();
        private Graphics FOverlay;
        private Image FOriginal;
        private Point FMouseDownPoint, FMouseCurrentPoint;
        private bool FDrawRect;
        private Rectangle FCropRect;
        private Rectangle FZoomedImage;
        private double FOriginalAspect;
        private double FPictureBoxAspect;
        private Pen FRectPen;
        
        private const int CHeaderWidth = 900;
        private const int CHeaderHeight = 200;
        private const Uri CPictureUploadUri = new Uri("http://vvvv.org/external-api/picture-upload")
        
        #endregion field declaration
        
        #region constructor/destructor
        public KommunikatorPluginNode()
        {
            // The InitializeComponent() call is required for Windows Forms designer support.
            InitializeComponent();
            
            FRectPen = new Pen(Color.Black);
            FRectPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
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
                    FPluginInfo.Name = "Kommunikator";
                    //the nodes category: try to use an existing one
                    FPluginInfo.Category = "HDE";
                    //the nodes version: optional. leave blank if not
                    //needed to distinguish two nodes of the same name and category
                    FPluginInfo.Version = "";
                    
                    //the nodes author: your sign
                    FPluginInfo.Author = "vvvv group";
                    //describe the nodes function
                    FPluginInfo.Help = "Communicator to vvvv.org";
                    //specify a comma separated list of tags that describe the node
                    FPluginInfo.Tags = "post screenshot web blog";
                    
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
        	this.panelScreenshot = new System.Windows.Forms.Panel();
        	this.pictureBoxScreenshot = new System.Windows.Forms.PictureBox();
        	this.labelScreenshotInfo = new System.Windows.Forms.Label();
        	this.panel2 = new System.Windows.Forms.Panel();
        	this.textBoxScreenshotDescription = new System.Windows.Forms.TextBox();
        	this.textBoxScreenshotTitle = new System.Windows.Forms.TextBox();
        	this.panel1 = new System.Windows.Forms.Panel();
        	this.panel3 = new System.Windows.Forms.Panel();
        	this.buttonUpload = new System.Windows.Forms.Button();
        	this.buttonClose = new System.Windows.Forms.Button();
        	this.checkBoxUseAsHeader = new System.Windows.Forms.CheckBox();
        	this.textBoxPassword = new System.Windows.Forms.TextBox();
        	this.textBoxUsername = new System.Windows.Forms.TextBox();
        	this.textBoxConsole = new System.Windows.Forms.TextBox();
        	this.panelScreenshot.SuspendLayout();
        	((System.ComponentModel.ISupportInitialize)(this.pictureBoxScreenshot)).BeginInit();
        	this.panel2.SuspendLayout();
        	this.panel1.SuspendLayout();
        	this.panel3.SuspendLayout();
        	this.SuspendLayout();
        	// 
        	// panelScreenshot
        	// 
        	this.panelScreenshot.Controls.Add(this.pictureBoxScreenshot);
        	this.panelScreenshot.Controls.Add(this.labelScreenshotInfo);
        	this.panelScreenshot.Controls.Add(this.panel2);
        	this.panelScreenshot.Controls.Add(this.textBoxConsole);
        	this.panelScreenshot.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.panelScreenshot.Location = new System.Drawing.Point(0, 0);
        	this.panelScreenshot.Name = "panelScreenshot";
        	this.panelScreenshot.Size = new System.Drawing.Size(489, 402);
        	this.panelScreenshot.TabIndex = 2;
        	// 
        	// pictureBoxScreenshot
        	// 
        	this.pictureBoxScreenshot.BackColor = System.Drawing.Color.Black;
        	this.pictureBoxScreenshot.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
        	this.pictureBoxScreenshot.Cursor = System.Windows.Forms.Cursors.Cross;
        	this.pictureBoxScreenshot.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.pictureBoxScreenshot.InitialImage = null;
        	this.pictureBoxScreenshot.Location = new System.Drawing.Point(0, 0);
        	this.pictureBoxScreenshot.Name = "pictureBoxScreenshot";
        	this.pictureBoxScreenshot.Size = new System.Drawing.Size(489, 279);
        	this.pictureBoxScreenshot.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
        	this.pictureBoxScreenshot.TabIndex = 3;
        	this.pictureBoxScreenshot.TabStop = false;
        	this.pictureBoxScreenshot.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PictureBoxScreenshotMouseMove);
        	this.pictureBoxScreenshot.Resize += new System.EventHandler(this.PictureBoxScreenshotResize);
        	this.pictureBoxScreenshot.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PictureBoxScreenshotMouseDown);
        	this.pictureBoxScreenshot.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PictureBoxScreenshotMouseUp);
        	// 
        	// labelScreenshotInfo
        	// 
        	this.labelScreenshotInfo.Dock = System.Windows.Forms.DockStyle.Bottom;
        	this.labelScreenshotInfo.Location = new System.Drawing.Point(0, 279);
        	this.labelScreenshotInfo.Name = "labelScreenshotInfo";
        	this.labelScreenshotInfo.Size = new System.Drawing.Size(489, 18);
        	this.labelScreenshotInfo.TabIndex = 10;
        	this.labelScreenshotInfo.Text = "ScreenshotInfo";
        	// 
        	// panel2
        	// 
        	this.panel2.Controls.Add(this.textBoxScreenshotDescription);
        	this.panel2.Controls.Add(this.textBoxScreenshotTitle);
        	this.panel2.Controls.Add(this.panel1);
        	this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
        	this.panel2.Location = new System.Drawing.Point(0, 297);
        	this.panel2.Name = "panel2";
        	this.panel2.Size = new System.Drawing.Size(489, 85);
        	this.panel2.TabIndex = 9;
        	// 
        	// textBoxScreenshotDescription
        	// 
        	this.textBoxScreenshotDescription.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        	this.textBoxScreenshotDescription.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.textBoxScreenshotDescription.Location = new System.Drawing.Point(0, 20);
        	this.textBoxScreenshotDescription.Multiline = true;
        	this.textBoxScreenshotDescription.Name = "textBoxScreenshotDescription";
        	this.textBoxScreenshotDescription.Size = new System.Drawing.Size(344, 65);
        	this.textBoxScreenshotDescription.TabIndex = 10;
        	// 
        	// textBoxScreenshotTitle
        	// 
        	this.textBoxScreenshotTitle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        	this.textBoxScreenshotTitle.Dock = System.Windows.Forms.DockStyle.Top;
        	this.textBoxScreenshotTitle.Location = new System.Drawing.Point(0, 0);
        	this.textBoxScreenshotTitle.Name = "textBoxScreenshotTitle";
        	this.textBoxScreenshotTitle.Size = new System.Drawing.Size(344, 20);
        	this.textBoxScreenshotTitle.TabIndex = 11;
        	// 
        	// panel1
        	// 
        	this.panel1.Controls.Add(this.panel3);
        	this.panel1.Controls.Add(this.checkBoxUseAsHeader);
        	this.panel1.Controls.Add(this.textBoxPassword);
        	this.panel1.Controls.Add(this.textBoxUsername);
        	this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
        	this.panel1.Location = new System.Drawing.Point(344, 0);
        	this.panel1.Name = "panel1";
        	this.panel1.Size = new System.Drawing.Size(145, 85);
        	this.panel1.TabIndex = 9;
        	// 
        	// panel3
        	// 
        	this.panel3.Controls.Add(this.buttonUpload);
        	this.panel3.Controls.Add(this.buttonClose);
        	this.panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
        	this.panel3.Location = new System.Drawing.Point(0, 63);
        	this.panel3.Name = "panel3";
        	this.panel3.Size = new System.Drawing.Size(145, 22);
        	this.panel3.TabIndex = 17;
        	// 
        	// buttonUpload
        	// 
        	this.buttonUpload.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.buttonUpload.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.buttonUpload.Location = new System.Drawing.Point(0, 0);
        	this.buttonUpload.Name = "buttonUpload";
        	this.buttonUpload.Size = new System.Drawing.Size(76, 22);
        	this.buttonUpload.TabIndex = 14;
        	this.buttonUpload.Text = "Upload";
        	this.buttonUpload.UseVisualStyleBackColor = true;
        	this.buttonUpload.Click += new System.EventHandler(this.ButtonUploadClick);
        	// 
        	// buttonClose
        	// 
        	this.buttonClose.Dock = System.Windows.Forms.DockStyle.Right;
        	this.buttonClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.buttonClose.Location = new System.Drawing.Point(76, 0);
        	this.buttonClose.Name = "buttonClose";
        	this.buttonClose.Size = new System.Drawing.Size(69, 22);
        	this.buttonClose.TabIndex = 15;
        	this.buttonClose.Text = "Close";
        	this.buttonClose.UseVisualStyleBackColor = true;
        	this.buttonClose.Click += new System.EventHandler(this.ButtonCloseClick);
        	// 
        	// checkBoxUseAsHeader
        	// 
        	this.checkBoxUseAsHeader.Dock = System.Windows.Forms.DockStyle.Top;
        	this.checkBoxUseAsHeader.Enabled = false;
        	this.checkBoxUseAsHeader.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.checkBoxUseAsHeader.Location = new System.Drawing.Point(0, 40);
        	this.checkBoxUseAsHeader.Name = "checkBoxUseAsHeader";
        	this.checkBoxUseAsHeader.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
        	this.checkBoxUseAsHeader.Size = new System.Drawing.Size(145, 21);
        	this.checkBoxUseAsHeader.TabIndex = 16;
        	this.checkBoxUseAsHeader.Text = "use image as header";
        	this.checkBoxUseAsHeader.UseVisualStyleBackColor = true;
        	this.checkBoxUseAsHeader.CheckedChanged += new System.EventHandler(this.CheckBoxUseAsHeaderCheckedChanged);
        	// 
        	// textBoxPassword
        	// 
        	this.textBoxPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        	this.textBoxPassword.Dock = System.Windows.Forms.DockStyle.Top;
        	this.textBoxPassword.Location = new System.Drawing.Point(0, 20);
        	this.textBoxPassword.Name = "textBoxPassword";
        	this.textBoxPassword.PasswordChar = '*';
        	this.textBoxPassword.Size = new System.Drawing.Size(145, 20);
        	this.textBoxPassword.TabIndex = 15;
        	this.textBoxPassword.Text = "guest";
        	// 
        	// textBoxUsername
        	// 
        	this.textBoxUsername.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        	this.textBoxUsername.Dock = System.Windows.Forms.DockStyle.Top;
        	this.textBoxUsername.Location = new System.Drawing.Point(0, 0);
        	this.textBoxUsername.Name = "textBoxUsername";
        	this.textBoxUsername.Size = new System.Drawing.Size(145, 20);
        	this.textBoxUsername.TabIndex = 14;
        	this.textBoxUsername.Text = "guest";
        	// 
        	// textBoxConsole
        	// 
        	this.textBoxConsole.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        	this.textBoxConsole.Dock = System.Windows.Forms.DockStyle.Bottom;
        	this.textBoxConsole.Location = new System.Drawing.Point(0, 382);
        	this.textBoxConsole.Name = "textBoxConsole";
        	this.textBoxConsole.Size = new System.Drawing.Size(489, 20);
        	this.textBoxConsole.TabIndex = 11;
        	// 
        	// KommunikatorPluginNode
        	// 
        	this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
        	this.Controls.Add(this.panelScreenshot);
        	this.DoubleBuffered = true;
        	this.Name = "KommunikatorPluginNode";
        	this.Size = new System.Drawing.Size(489, 402);
        	this.panelScreenshot.ResumeLayout(false);
        	this.panelScreenshot.PerformLayout();
        	((System.ComponentModel.ISupportInitialize)(this.pictureBoxScreenshot)).EndInit();
        	this.panel2.ResumeLayout(false);
        	this.panel2.PerformLayout();
        	this.panel1.ResumeLayout(false);
        	this.panel1.PerformLayout();
        	this.panel3.ResumeLayout(false);
        	this.ResumeLayout(false);
        }
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.TextBox textBoxConsole;
        private System.Windows.Forms.CheckBox checkBoxUseAsHeader;
        private System.Windows.Forms.Label labelScreenshotInfo;
        private System.Windows.Forms.TextBox textBoxScreenshotTitle;
        private System.Windows.Forms.Button buttonUpload;
        private System.Windows.Forms.TextBox textBoxUsername;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox textBoxScreenshotDescription;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.PictureBox pictureBoxScreenshot;
        private System.Windows.Forms.Panel panelScreenshot;
        
        #region initialization
        //this method is called by vvvv when the node is created
        public void SetPluginHost(IPluginHost host)
        {
            FPluginHost = host;
        }
        
        public void SetHDEHost(IHDEHost host)
        {
            //assign host
            FHDEHost = host;
        }
        #endregion initialization
        
        #region IKommunikator
        public void SetIKommunikatorHost(IKommunikatorHost host)
        {
            FKommunikatorHost = host;
        }
        
        public void Initialize(string title, string description)
        {
            textBoxScreenshotTitle.Text = title;
            textBoxScreenshotDescription.Text = description;
            
            FOriginal = Clipboard.GetImage();
            pictureBoxScreenshot.BackgroundImage = FOriginal;
            
            //create overlay image that holds the crop selection 
            Image img = new Bitmap(FOriginal.Width, FOriginal.Height, PixelFormat.Format32bppArgb);
            FOverlay = Graphics.FromImage(img);
            pictureBoxScreenshot.Image = img;
            
            if ((FOriginal.Width >= CHeaderWidth) && (FOriginal.Height >= CHeaderHeight))
                checkBoxUseAsHeader.Enabled = true;
            else
                checkBoxUseAsHeader.Enabled = false;
            
            FOriginalAspect = FOriginal.Width / (double) FOriginal.Height;
            FCropRect = new Rectangle(0, 0, FOriginal.Width, FOriginal.Height);
            
            UpdateZoomedImageRect();
            UpdateScreenshotInfo();
        }
        #endregion IKommunikator
        
        #region PictureBox
        void PictureBoxScreenshotMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                FDrawRect = true;
                FMouseDownPoint = e.Location;
            }
            else
            {
                FCropRect = new Rectangle(0, 0, FOriginal.Width, FOriginal.Height);
                UpdateScreenshotInfo();
                UpdateOverlay();
            }
        }
        
        void PictureBoxScreenshotMouseMove(object sender, MouseEventArgs e)
        {
            if (FDrawRect)
            {
                FMouseCurrentPoint = e.Location;
                UpdateOverlay();
            }
        }
        
        void PictureBoxScreenshotMouseUp(object sender, MouseEventArgs e)
        {
            FDrawRect = false;
        }
        
        void PictureBoxScreenshotResize(object sender, EventArgs e)
        {
            UpdateZoomedImageRect();
        }
         
        #endregion PictureBox
        
        private void UpdateScreenshotInfo()
        {
            labelScreenshotInfo.Text = "Original: " + FOriginal.Width.ToString() + " x " + FOriginal.Height.ToString() + " Cropped: " + FCropRect.Width.ToString() + " x " + FCropRect.Height.ToString();
            labelScreenshotInfo.Invalidate();
        }
        
        private void UpdateZoomedImageRect()
        {
            int left, top, width, height;
            FPictureBoxAspect = pictureBoxScreenshot.Width / (double) pictureBoxScreenshot.Height;
            
            //aspect > 1 is landscape
            //aspect <= 1 is portrait
            
            if (FPictureBoxAspect > FOriginalAspect)
            {
                height = pictureBoxScreenshot.Height;
                width = (int)Math.Round(height * FOriginalAspect);
            }
            else
            {
                width = pictureBoxScreenshot.Width;
                height = (int)Math.Round(width / FOriginalAspect);
            }
            
            left = pictureBoxScreenshot.Width / 2 - width / 2;
            top = pictureBoxScreenshot.Height / 2 - height / 2;
            
            FZoomedImage = new Rectangle(left, top, width, height);
        }
        
        private void UpdateOverlay()
        {
            FOverlay.Clear(Color.Transparent);
            double xScale = FZoomedImage.Width / (double) FOriginal.Width;
            double yScale = FZoomedImage.Height / (double) FOriginal.Height;
            
            int left, top, width, height;
            if ((checkBoxUseAsHeader.Enabled && checkBoxUseAsHeader.Checked))
            {
                left = Math.Max(0, (int) ((FMouseCurrentPoint.X - FZoomedImage.Left) / xScale) - CHeaderWidth/2);
                left = Math.Min(left, FOriginal.Width - CHeaderWidth);
                top = Math.Max(0, (int) ((FMouseCurrentPoint.Y - FZoomedImage.Top) / yScale) - CHeaderHeight/2);
                top = Math.Min(top, FOriginal.Height - CHeaderHeight);
                width = CHeaderWidth;
                height = CHeaderHeight;
            }
            else
            {
                left = Math.Max(0, (int) ((FMouseDownPoint.X - FZoomedImage.Left) / xScale));
                top = Math.Max(0, (int) ((FMouseDownPoint.Y - FZoomedImage.Top) / yScale));
                width = Math.Max(10, (int) ((FMouseCurrentPoint.X - FMouseDownPoint.X) / xScale));
                width = Math.Min(width, FOriginal.Width - left);
                height = Math.Max(10, (int) ((FMouseCurrentPoint.Y - FMouseDownPoint.Y) / yScale));
                height = Math.Min(height, FOriginal.Height - top);
            }
            
            FCropRect = new Rectangle(left, top, width, height);
            
            FOverlay.DrawRectangle(FRectPen, FCropRect);
            UpdateScreenshotInfo();
            pictureBoxScreenshot.Invalidate();
        }
       
        void ButtonUploadClick(object sender, EventArgs e)
        {
            textBoxConsole.Text = "";
            
            //crop the image
            Bitmap target = new Bitmap(FCropRect.Width, FCropRect.Height);
            using(Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(FOriginal, new Rectangle(0, 0, target.Width, target.Height), FCropRect, GraphicsUnit.Pixel);
            }

            MemoryStream fileData = new MemoryStream();
            target.Save(fileData, System.Drawing.Imaging.ImageFormat.Png);
            fileData.Seek(0, SeekOrigin.Begin);

            //add post-header
            NameValueCollection nvc = new NameValueCollection();
            nvc.Add("name", textBoxUsername.Text);
            nvc.Add("pass", FStringHasher.ToMD5(textBoxPassword.Text));
            nvc.Add("header", (checkBoxUseAsHeader.Enabled && checkBoxUseAsHeader.Checked).ToString());
            nvc.Add("title", textBoxScreenshotTitle.Text);
            nvc.Add("description", textBoxScreenshotDescription.Text);

            using (WebResponse response = Upload.PostFile(CPictureUploadUri, nvc, fileData, "upload.png", null, null, null, null))
            {
                // the stream returned by WebResponse.GetResponseStream
                // will contain any content returned by the server after upload

                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string result = reader.ReadToEnd();
                    if (result.Contains("OK"))
                        textBoxConsole.Text = "Upload Successful.";
                    else if (result.Contains("LOGIN FAILED"))
                        textBoxConsole.Text = "Login failed!";
                    else if (result.Contains("SERVER BUSY"))
                        textBoxConsole.Text = "Server is busy, please try again later.";
                    else
                        textBoxConsole.Text = "ERROR: " + result;
                }
            }
        }
        
        void ButtonCloseClick(object sender, EventArgs e)
        {
            FKommunikatorHost.HideMe();
        }
        
        void CheckBoxUseAsHeaderCheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxUseAsHeader.Checked)
            {
                FMouseCurrentPoint = new Point(pictureBoxScreenshot.Width/2, pictureBoxScreenshot.Height/2);
                UpdateOverlay();
            }
        }
    }
}
