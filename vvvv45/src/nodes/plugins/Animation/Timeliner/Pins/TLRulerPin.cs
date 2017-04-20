using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml;

using VVVV.PluginInterfaces.V1;

namespace VVVV.Nodes.Timeliner
{
	public class TLRulerPin: TLBasePin
	{
		private double FStart;
		private double FEnd;
		
		public double Start
		{
			get{return FStart;}
		}
		
		public double End
		{
			get{return FEnd;}
		}
		
		public double VisibleTimeRange
		{
			get{return FEnd - FStart;}
		}
		
		private TLTime FTimer;
		public TLTime Timer
		{
			set {FTimer = value;}
		}
		
		public TLRulerPin():base(){InitializeComponent();}
		
		public TLRulerPin(TLTransformer Transformer, int Order, XmlNode PinSettings, bool ShowRemoveButton):base(Transformer, Order, PinSettings)
		{
			InitializeComponent();
			
			//initializecomponent overrides the name! reset it here
			XmlAttribute attr = PinSettings.Attributes.GetNamedItem("Name") as XmlAttribute;
        	this.Name = attr.Value;
        	
			this.RemoveButton.Visible = ShowRemoveButton;
		}
		
		protected override void InitializeHeight()
		{
			FMinimalHeight = DIP(25);
			
			if (FUncollapsedHeight == 0)
				FUncollapsedHeight = DIP(25);
			
			base.InitializeHeight();
		}
		
		private void InitializeComponent()
		{
            this.label1 = new System.Windows.Forms.Label();
            this.RemoveButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(3, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "label1";
            // 
            // RemoveButton
            // 
            this.RemoveButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RemoveButton.ForeColor = System.Drawing.Color.White;
            this.RemoveButton.Location = new System.Drawing.Point(0, 0);
            this.RemoveButton.Name = "RemoveButton";
            this.RemoveButton.Size = new System.Drawing.Size(18, 20);
            this.RemoveButton.TabIndex = 2;
            this.RemoveButton.Text = "X";
            this.RemoveButton.UseVisualStyleBackColor = true;
            this.RemoveButton.Click += new System.EventHandler(this.Button1Click);
            // 
            // TLRulerPin
            // 
            this.BackColor = System.Drawing.Color.Gray;
            this.Controls.Add(this.RemoveButton);
            this.Controls.Add(this.label1);
            this.Name = "TLRulerPin";
            this.ResumeLayout(false);
            this.PerformLayout();

		} 
		private System.Windows.Forms.Button RemoveButton;
		private System.Windows.Forms.Label label1;
		
		public override void DrawSlices(Graphics g, double From, double To)
		{
			RectangleF r = g.ClipBounds;
			if (r.Width == 0 || r.Height == 0) 
				return;
			
			PointF[] stp = new PointF[2];
			stp[0].X = 0;
			stp[1].X = -r.Width;
			FTransformer.GTransformation.TransformPoints(stp);
			FStart = -stp[0].X / FTransformer.GTimeScale;
			FEnd = -stp[1].X / FTransformer.GTimeScale;
			
			int visibleSeconds = (int) (r.Width / FTransformer.GTimeScale);
			int firstSecond = (int) Math.Ceiling(-FStart);
			
			double startOffset = - (firstSecond + FStart) * FTransformer.GTimeScale;
			
			//pixel per second: pps
			float pps = r.Width / visibleSeconds;
			
			Pen pen = new Pen(Color.Gray);
			float s;
			int m, ss;
			string tt;

			int pixPerBar = 60;
			double visibleTime = r.Width / FTransformer.GTimeScale;
			int secPerBar = Math.Max(1, (int) (visibleTime / (r.Width / pixPerBar)));
			int maxBars = (int) r.Width / pixPerBar;
			
			float x;
			Font f = new Font("Verdana", 7);
			SolidBrush sb = new SolidBrush(Color.Gray);
			
			int t=0;
			do
			{
				x = (float) (startOffset + t*secPerBar*FTransformer.GTimeScale);
				g.DrawLine(pen, x, 0, x, r.Height);
				
				s = -firstSecond + t * secPerBar;
				m = (int) s / 60;
				ss = (int) (s % 60);
				tt = m.ToString() + ":" + ss.ToString("d2");
				g.DrawString(tt, f, sb, x, 5);
				t++;
			}
			while (x < r.Width);
			
			t=0;
			do
			{
				x = (float) (startOffset + t*secPerBar/10.0*FTransformer.GTimeScale);
				g.DrawLine(pen, x, r.Height, x, r.Height - r.Height/6);
				t++;
			}
			while (x < r.Width);
			
			//draw time triangles
			float time;
			for (int i=0; i<FTimer.TimeCount; i++)
			{
				time = FTransformer.TransformPoint(new PointF((float) FTimer.GetTime(i), 0)).X;
				g.DrawLine(new Pen(Color.Black), time, 0, time, Height);
				PointF[] tri = new PointF[3];
				tri[0] = new PointF(time, 7);
				tri[1] = new PointF(time-4, 0);
				tri[2] = new PointF(time+4, 0);
				g.FillPolygon(new SolidBrush(Color.Black), tri);
			}
		}
		
		public override void Evaluate(double CurrentTime)
		{
			bool showMinus = false;
			double time = Math.Abs(CurrentTime);
			
			if (CurrentTime < 0)
				showMinus = true;
			
			int ms = (int) ((time - Math.Floor(time)) * 1000);
			int s = (int) (time % 60);
			int m = (int) (time / 60 % 60);
			int h = (int) (time / 60 / 60 % 60);
			//int d = (int) (time / 60 / 60 / 12 % 12);
			DateTime dt = new DateTime(2008, 1, 1, h + 1, m, s, ms);
			
			if (showMinus)
				label1.Text = "-" + dt.ToString("mm:ss:fff"); 
			else
				label1.Text = " " + dt.ToString("mm:ss:fff");
		}
		
		void Button1Click(object sender, EventArgs e)
		{
			RemovePin();
		}
	}
}
