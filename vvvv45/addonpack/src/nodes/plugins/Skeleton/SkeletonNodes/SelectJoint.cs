//use what you need
using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Collections;

using VVVV.PluginInterfaces.V1;
using VVVV.SkeletonInterfaces;
using VVVV.Utils.VMath;

//the vvvv node namespace
namespace VVVV.Nodes
{
	//class definition, inheriting from UserControl for the GUI stuff
	public class SelectJoint: UserControl, IPlugin
	{
		#region field declaration
		
		//the host (mandatory)
		private IPluginHost FHost;
		// Track whether Dispose has been called.
		private bool FDisposed = false;
		
		private INodeIn FSkeletonInput;
		private IStringConfig FSelectionInput; // only for saving purposes
		
		private IStringOut FJointNameOutput;
	
		private List<IJoint> selectedJoints;
		private IJoint selectedJoint = null;
		private IJoint hoveredJoint = null;
		private IJoint rootJoint;
		private Skeleton skeleton;
		private float zoom = 10;
		private string[] configSelectedNames;
		
		private Vector2D dragStartPos = new Vector2D();
		private Vector2D dragDelta = new Vector2D();
		private Vector2D panOffset = new Vector2D(0);
		private Vector3D camRotation = new Vector3D(0);
		private Dictionary<string, Vector3D> jointPositions;
		private bool isDragging = false;
		
		private bool initialized = false;
		
		#endregion field declaration
		
		#region constructor/destructor
		public SelectJoint()
		{
			selectedJoints = new List<IJoint>();
			jointPositions = new Dictionary<string, Vector3D>();
			
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
					// Dispose managed resources
				}
				// Release unmanaged resources. If disposing is false,
				// only the following code is executed.
				
				FHost.Log(TLogType.Debug, "PluginGUITemplate is being deleted");
				
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
					FPluginInfo.Name = "SelectJoint";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "Skeleton";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "GUI";
					
					//the nodes author: your sign
					FPluginInfo.Author = "Matthias Zauner";
					//describe the nodes function
					FPluginInfo.Help = "SelectJoint";
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
					FPluginInfo.InitialWindowSize = new Size(400, 400);
					//define the nodes initial component mode
					FPluginInfo.InitialComponentMode = TComponentMode.Hidden;
					
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
			this.SuspendLayout();
			// 
			// GUITemplatePlugin
			// 
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.DoubleBuffered = true;
			this.Name = "SelectJoint";
			this.Size = new System.Drawing.Size(400, 400);
			this.MouseClick += new MouseEventHandler(this.mouseClicked);
			this.MouseWheel += new MouseEventHandler(this.mouseWheel);
			this.MouseDown += new MouseEventHandler(this.mouseDown);
			this.MouseUp += new MouseEventHandler( this.mouseUp);
			this.MouseMove += new MouseEventHandler( this.mouseMove);
			this.ResumeLayout(false);
			
			initializeToolbar();
		}
		
		private void initializeToolbar()
		{
			Panel axisSelection = new Panel();
			axisSelection.Location = new Point(0,0);
			
			string[] axes = new String[]{"z", "y", "x"};
			string[] labels = new String[]{"FRONT", "TOP", "SIDE"};
			int buttonWidth = 50;
			int buttonHeight = 25;
			int i;
			for (i=0; i<axes.Length; i++)
			{
				RadioButton axisButton = new RadioButton();
				axisButton.Text = labels[i];
				axisButton.Name = axes[i];
				axisButton.Location = new Point(i*buttonWidth,0);
				axisButton.Size = new Size(buttonWidth,buttonHeight);
				axisButton.Parent = axisSelection;
				axisButton.Appearance = Appearance.Button;
				axisButton.FlatStyle = FlatStyle.Flat;
				axisButton.TextAlign = ContentAlignment.MiddleCenter;
				axisButton.Font = new Font("Lucida Sans", 6);
				axisButton.CheckedChanged += new EventHandler(this.switchAxis);
				if (i==0)
					axisButton.Checked = true;
			}
			
			axisSelection.Size = new Size(i*buttonWidth, buttonHeight);
			axisSelection.Parent = this;
		}
		
		private void switchAxis(object sender, EventArgs e)
		{
			RadioButton b = (RadioButton)sender;
			if (!b.Checked)
			{
				b.BackColor = Color.LightGray;
				return;
			}
			if (b.Name=="x")
				camRotation = new Vector3D(0, -3.14/2, 0);
			else if (b.Name=="y")
				camRotation = new Vector3D(-3.14/2, 0, 0);
			else
				camRotation = new Vector3D(0, 0, 0);
			b.BackColor = Color.Black;
			Invalidate();
		}
		
		#region pin creation
		
		//this method is called by vvvv when the node is created
		public void SetPluginHost(IPluginHost Host)
		{
			//assign host
			FHost = Host;

            var guids = new System.Guid[1];
            guids[0] = SkeletonNodeIO.GUID;

            FHost.CreateNodeInput("Skeleton", TSliceMode.Single, TPinVisibility.True, out FSkeletonInput);
	    	FSkeletonInput.SetSubType(guids, "Skeleton");
	    	FHost.CreateStringConfig("Selection", TSliceMode.Dynamic, TPinVisibility.Hidden, out FSelectionInput);
	    	
		    //create outputs
	    	FHost.CreateStringOutput("Joint Name", TSliceMode.Dynamic, TPinVisibility.True, out FJointNameOutput);
		}

		#endregion pin creation
		
		#region mainloop
		
		public void Configurate(IPluginConfig Input)
		{
			if (Input.Name=="Selection" && selectedJoints.Count==0)
			{
				string[] separator = new string[1];
        		separator[0] = ",";
        		configSelectedNames = Input.SpreadAsString.Split(separator, System.StringSplitOptions.None);
			}
		}
		
		//here we go, thats the method called by vvvv each frame
		//all data handling should be in here
		public void Evaluate(int SpreadMax)
		{
			//if any of the inputs has changed
			//recompute the outputs
			
			if (FSkeletonInput.PinIsChanged || !initialized)
			{
				jointPositions = new Dictionary<string, Vector3D>();
				object currInterface;
				FSkeletonInput.GetUpstreamInterface(out currInterface);
				try
				{
					skeleton = (Skeleton)currInterface;
                    rootJoint = (IJoint)skeleton.Root;
                }
                catch (Exception e)
                {
					FHost.Log(TLogType.Error, e.Message);
				}
				
				if (!initialized && configSelectedNames!=null)
				{
					selectedJoints.Clear();
					IJoint currJoint;
					for (int i=0; i<configSelectedNames.Length; i++)
	        		{
	        			if (string.IsNullOrEmpty(configSelectedNames[i]))
							break;
						if (skeleton.JointTable.ContainsKey(configSelectedNames[i]))
						{
							currJoint = skeleton.JointTable[configSelectedNames[i]];
							if (currJoint!=null)
								selectedJoints.Add(currJoint);
						}
					}
				}
				
				//redraw gui only if anything changed
				Invalidate();
			}

            if (selectedJoints.Count>0)
			{
				FJointNameOutput.SliceCount = selectedJoints.Count;
				for (int i=0; i<selectedJoints.Count; i++)
				{
					FJointNameOutput.SetString(i, selectedJoints[i].Name);
				}
			}
			else
				FJointNameOutput.SetString(0, "");
			
			buildConfig();
			
			initialized = true;
		}
		
		#endregion mainloop
		
		private void mouseClicked(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				Vector3D mousePos = new Vector3D(e.X, e.Y, 0);
				hasClicked(rootJoint, mousePos);
			}
			
		}
		
		private void mouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				dragStartPos = new Vector2D(e.X, e.Y);
				dragDelta = new Vector2D(0);
				isDragging = true;
			}
		}
		
		private void mouseMove(object sender, MouseEventArgs e)
		{
			if (isDragging)
			{
				dragDelta.x = e.X - dragStartPos.x;
				dragDelta.y = e.Y - dragStartPos.y;
				Invalidate();
			}
			else
			{
				Vector3D mousePos = new Vector3D(e.X, e.Y, 0);
				if (hasHovered(skeleton.Root, mousePos))
					Invalidate();
				else
				{
					if (hoveredJoint!=null)
					{
						hoveredJoint = null;
						Invalidate();
					}
				}
			}
		}
		
		private void mouseUp(object sender, MouseEventArgs e)
		{
			if (isDragging)
			{
				panOffset.x += dragDelta.x;
				panOffset.y += dragDelta.y;
				dragDelta = new Vector2D(0);
				isDragging = false;
			}
		}
		
		private bool hasHovered(IJoint currJoint, Vector3D mousePos)
		{
			if (!jointPositions.ContainsKey(currJoint.Name))
				return false;
			if (VMath.Dist(mousePos, jointPositions[currJoint.Name])<5) {
				
				hoveredJoint = currJoint;
				return true;
			}
			
			for (int i=0; i<currJoint.Children.Count; i++)
			{
				if (hasHovered(currJoint.Children[i], mousePos))
					return true;
			}
			return false;
		}
		
		private bool hasClicked(IJoint currJoint, Vector3D mousePos)
		{
            if (!jointPositions.ContainsKey(currJoint.Name))
                return false;
            if (VMath.Dist(mousePos, jointPositions[currJoint.Name])<5)
            {
				bool alreadySelected = false;
				for (int i=0; i<selectedJoints.Count; i++)
				{
					if (selectedJoints[i] == currJoint)
					{
						alreadySelected = true;
						break;
					}
				}
				if (Control.ModifierKeys != Keys.Control)
					selectedJoints.Clear();
				if (alreadySelected && Control.ModifierKeys == Keys.Control)
				{
					selectedJoints.Remove(currJoint);
					selectedJoint = null;
				}
				else
				{
					selectedJoints.Add(currJoint);
					selectedJoint = currJoint;
				}
				Invalidate();
				return true;
			}
			for (int i=0; i<currJoint.Children.Count; i++)
			{
				if (hasClicked(currJoint.Children[i], mousePos))
					return true;
			}
			return false;
		}
		
		private void mouseWheel(object sender, MouseEventArgs e)
		{
			float offset = 0.1f;
			if (e.Delta>0)
				zoom *= 1+offset;
			if (e.Delta<0)
				zoom *= 1-offset;
			Invalidate();
		}
		
		protected override void OnPaint(PaintEventArgs e)
		{
			System.Drawing.Graphics g = e.Graphics;
			
			
			SolidBrush b = new SolidBrush(Color.Black);
			Pen p = new Pen(Color.Black);

			
			Stack transformStack = new Stack();
			Matrix4x4 mat = new Matrix4x4(VMath.IdentityMatrix);
			mat = VMath.RotateY(camRotation.y) *   VMath.RotateX(camRotation.x) * VMath.Scale(zoom, -zoom, zoom) /** this.Perspective(2*3.14/8, 1, 0, 100)*/ * VMath.Translate(200.0+panOffset.x+dragDelta.x, 200.0+panOffset.y+dragDelta.y, 0.0) * mat;
			transformStack.Push(mat);
			
			if (rootJoint!=null)
				drawJoint(g, rootJoint, transformStack, Color.Black);
		}
		
		private Vector3D drawJoint(Graphics g, IJoint currJoint, Stack transformStack, Color c)
		{
			int listIndex = -1;
			for (int i=0; i<selectedJoints.Count; i++)
			{
				if (selectedJoints[i] == currJoint)
				{
					c = Color.Red;
					listIndex = i;
				}
			}
			
			Pen jointPen = new Pen(c, 1f);
			Pen bonePen = new Pen(c, 1f);
			Matrix4x4 t = currJoint.AnimationTransform * currJoint.BaseTransform * (Matrix4x4)transformStack.Peek();
			
			Vector3D v = t * (new Vector3D(0));
			v.z = 0;
            var bounds = new RectangleF((float)v.x - 5.0f, (float)v.y - 5.0f, 10.0f, 10.0f);
            // Draw circles for joints which take part in skinning and rectangles for those who don't
            if (currJoint.Id >= 0)
                g.DrawEllipse(jointPen, bounds);
            else
                g.DrawRectangle(jointPen, bounds.X, bounds.Y, bounds.Width, bounds.Height);
			if (listIndex>=0)
			{
				Font f = new Font("Verdana", 7);
				g.FillRectangle(new SolidBrush(Color.Black), (float)v.x+5, (float)v.y-10, (float)(10* (Math.Floor(listIndex/10.0)+1)), 10.0f);
				SolidBrush b = new SolidBrush(Color.White);
				g.DrawString(listIndex+"", f, b, (float)v.x+5, (float)v.y-10);
			}
			c = Color.Black;
			
			if (currJoint == hoveredJoint)
			{
				Font f = new Font("Verdana", 7);
				g.FillRectangle(new SolidBrush(Color.Black), (float)v.x+15, (float)v.y-10, (float)(7* (currJoint.Name.Length+1)), 13.0f);
				SolidBrush b = new SolidBrush(Color.White);
				g.DrawString(currJoint.Name, f, b, (float)v.x+15, (float)v.y-10);
			}
			
			if (!jointPositions.ContainsKey(currJoint.Name))
				jointPositions.Add(currJoint.Name, v);
			else
				jointPositions[currJoint.Name] = v;
			
			transformStack.Push(t);
			
			Vector3D v2;
			for (int i=0; i<currJoint.Children.Count; i++)
			{
				v2 = drawJoint(g, currJoint.Children[i], transformStack, c);
				g.DrawLine(bonePen, (float)v.x, (float)v.y, (float)v2.x, (float)v2.y);
			}
			
			transformStack.Pop();
			
			return v;
		}
		
		private void buildConfig()
		{
			FSelectionInput.SliceCount = selectedJoints.Count;
			for (int i=0; i<selectedJoints.Count; i++)
			{
				FSelectionInput.SetString(i, selectedJoints[i].Name);
			}
		}
		
		private Matrix4x4 Perspective(double fovy, double aspect, double zNear, double zFar)
		{
			Matrix4x4 ret = new Matrix4x4();
			
			ret.m11 = System.Math.Atan(fovy/2)/aspect;
			ret.m12 = 0.0;
			ret.m13 = 0.0;
			ret.m14 = 0.0;
			
			ret.m21 = 0.0;
			ret.m22 = System.Math.Atan(fovy/2);
			ret.m23 = 0.0;
			ret.m24 = 0.0;
			
			ret.m31 = 0.0;
			ret.m32 = 0.0;
			ret.m33 = (zFar+zNear)/(zNear-zFar);
			ret.m34 = -1.0;
			
			ret.m41 = 0.0;
			ret.m42 = 0.0;
			ret.m43 = (2*zFar*zNear)/(zNear-zFar);
			ret.m44 = -1.0;
			
			return ret;
		}
	}
}
