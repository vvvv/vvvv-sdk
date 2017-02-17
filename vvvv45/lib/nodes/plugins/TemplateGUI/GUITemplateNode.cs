#region usings
using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Template",
				Category = "GUI",
				Help = "Template with some gui elements",
				Tags = "c#",
				AutoEvaluate = true)]
	#endregion PluginInfo
	public class GUITemplateNode : UserControl, IPluginEvaluate
	{
		#region fields & pins
		
		[Output("Button")]
        public ISpread<bool> FButtonOut;
		
		[Output("Check Box")]
        public ISpread<bool> FCheckBoxOut;
		
		[Output("Text Box")]
        public ISpread<string> FTextBoxOut;
		
		[Output("Slider")]
        public ISpread<double> FSliderOut;
		
		[Import()]
        public ILogger FLogger;

		//gui controls
		bool FButtonClick = false;
		CheckBox FCheckBox = new CheckBox();
		TextBox FTextBox = new TextBox();
		TrackBar FSlider = new TrackBar();
		
		#endregion fields & pins
		
		#region constructor and init
		
		public GUITemplateNode()
		{
			//setup the gui
			InitializeComponent();
		}
		
		void InitializeComponent()
		{
			//clear controls in case init is called multiple times
			Controls.Clear();
			
			//add a button
			var button = new Button();
			button.Text = "Bang";
			//set position and size
			button.Bounds = new Rectangle(10, 10, 100, 25);
			button.FlatStyle = FlatStyle.Flat;
			//listen to click event
			button.Click += ButtonClicked;
			
			//add a checkbox
			FCheckBox.Text = "Toggle";
			FCheckBox.Bounds = new Rectangle(15, 50, 100, 25);
			FCheckBox.FlatStyle = FlatStyle.Flat;
			
			//add a textbox
			FTextBox.Bounds = new Rectangle(130, 10, 150, 25);
			FTextBox.BorderStyle = BorderStyle.FixedSingle;
			
			//add a slider
			FSlider.Maximum = 1000;
			FSlider.TickFrequency = 20;
			FSlider.Bounds = new Rectangle(125, 50, 160, 25);
			
			//add to controls
			Controls.Add(button);
			Controls.Add(FCheckBox);
			Controls.Add(FTextBox);
			Controls.Add(FSlider);
		}
		
		//called if the button is clicked
		void ButtonClicked(object sender, EventArgs e)
		{
			if (sender is Button) 
			{
				//set button click to true to read it in Evaluate()
				FButtonClick = true;
			}
		}

		
		#endregion constructor and init
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			//set outputs
			FButtonOut[0] = FButtonClick;
			FButtonClick = false;
			
			FCheckBoxOut[0] = FCheckBox.Checked;
			
			FTextBoxOut[0] = FTextBox.Text;
			
			FSliderOut[0] = FSlider.Value / 1000.0;
		}
	}
}