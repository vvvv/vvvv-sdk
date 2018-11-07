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
				Version = "Dynamic", 
				Help = "Template with a dynamic set of gui elements", 
				Tags = "c#", 
				AutoEvaluate = true)]
	#endregion PluginInfo
	public class DynamicGUITemplateNode : UserControl, IPluginEvaluate
	{
		#region fields & pins
		[Input("Button Names", DefaultString = "Bang")]
        public IDiffSpread<string> FButtonNames;

		[Input("Check Box Names", DefaultString = "Toggle")]
        public IDiffSpread<string> FCheckBoxNames;

		[Input("Text Box Count", DefaultValue = 1, IsSingle = true)]
        public IDiffSpread<int> FTextBoxCount;

		[Input("Slider Count", DefaultValue = 1, IsSingle = true)]
        public IDiffSpread<int> FSliderCount;

		[Output("Buttons")]
        public ISpread<bool> FButtonOut;

		[Output("Check Boxes")]
        public ISpread<bool> FCheckBoxOut;

		[Output("Text Boxes")]
        public ISpread<string> FTextBoxOut;

		[Output("Sliders")]
        public ISpread<double> FSlidersOut;

		[Import()]
        public ILogger FLogger;

		//layout panels
		TableLayoutPanel FMainPanel = new TableLayoutPanel();
		Dictionary<string, TableLayoutPanel> FTabPanels = new Dictionary<string, TableLayoutPanel>();

		//gui controls lists
		bool[] FButtonClick = new bool[1];
		List<CheckBox> FCheckBoxes = new List<CheckBox>();
		List<TextBox> FTextBoxes = new List<TextBox>();
		List<TrackBar> FSliders = new List<TrackBar>();

		#endregion fields & pins

		#region constructor and init

		public DynamicGUITemplateNode()
		{
			//setup the gui
			InitializeComponent();
		}

		void InitializeComponent()
		{
			//create tab panels for the UI controls
			FTabPanels["buttons"] = new TableLayoutPanel();
			FTabPanels["checkboxes"] = new TableLayoutPanel();
			FTabPanels["textboxes"] = new TableLayoutPanel();
			FTabPanels["sliders"] = new TableLayoutPanel();

			//config table panel 4x1
			FMainPanel.ColumnCount = 4;
			FMainPanel.RowCount = 1;
			FMainPanel.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

			//define size of the columns
			FMainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
			FMainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
			FMainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
			FMainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

			//table panel fills the whole window
			FMainPanel.Dock = DockStyle.Fill;

			//config table panels and add them to the main table panel
			int i = 0;
			foreach (var tabPanel in FTabPanels.Values) {
				tabPanel.Dock = DockStyle.Fill;
				FMainPanel.Controls.Add(tabPanel, i++, 0);
			}

			//add the main table panel to the window
			Controls.Add(FMainPanel);
		}

		#endregion constructor and init

		#region dynamic control layout

		//buttons
		void LayoutButtons()
		{
			var c = FTabPanels["buttons"].Controls;
			c.Clear();
			FButtonClick = new bool[FButtonNames.SliceCount];

			int i = 0;
			foreach (var name in FButtonNames) {
				var button = new Button();
				button.Text = name;
				button.FlatStyle = FlatStyle.Flat;
				button.Dock = DockStyle.Top;
				//listen to click event
				button.Click += ButtonClicked;
				//tag it with its index
				button.Tag = i++;
				c.Add(button);
			}
		}

		//called if a button is clicked
		void ButtonClicked(object sender, EventArgs e)
		{
			if (sender is Button) {
				var b = sender as Button;

				//set button click to true to read it in Evaluate()
				FButtonClick[(int)b.Tag] = true;
			}
		}

		//check boxes
		void LayoutCheckBoxes()
		{
			var c = FTabPanels["checkboxes"].Controls;

			c.Clear();
			FCheckBoxes.Clear();

			foreach (var name in FCheckBoxNames) {
				var box = new CheckBox();
				box.Text = name;
				box.FlatStyle = FlatStyle.Flat;
				box.Dock = DockStyle.Top;
				box.Height = 18;
				c.Add(box);
				FCheckBoxes.Add(box);
			}
		}

		//text boxes
		void LayoutTextBoxes()
		{
			var c = FTabPanels["textboxes"].Controls;

			c.Clear();
			FTextBoxes.Clear();

			for (int i = 0; i < FTextBoxCount[0]; i++) {
				var box = new TextBox();
				box.BorderStyle = BorderStyle.FixedSingle;
				box.Dock = DockStyle.Top;
				c.Add(box);
				FTextBoxes.Add(box);
			}

		}

		//sliders
		void LayoutSliders()
		{
			var c = FTabPanels["sliders"].Controls;

			c.Clear();
			FSliders.Clear();

			for (int i = 0; i < FSliderCount[0]; i++) {
				var slider = new TrackBar();
				slider.Maximum = 1000;
				slider.TickFrequency = 20;
				slider.Dock = DockStyle.Top;
				c.Add(slider);
				FSliders.Add(slider);
			}
		}
		#endregion dynamic control layout

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			//button setup
			if (FButtonNames.IsChanged) {
				LayoutButtons();
				FButtonOut.SliceCount = FButtonNames.SliceCount;
			}

			//checkbox setup
			if (FCheckBoxNames.IsChanged) {
				LayoutCheckBoxes();
				FCheckBoxOut.SliceCount = FCheckBoxNames.SliceCount;
			}

			//textbox setup
			if (FTextBoxCount.IsChanged) {
				LayoutTextBoxes();
				FTextBoxOut.SliceCount = FTextBoxCount[0];
			}

			//slider setup
			if (FSliderCount.IsChanged) {
				LayoutSliders();
				FSlidersOut.SliceCount = FSliderCount[0];
			}

			//set outputs
			for (int i = 0; i < FButtonOut.SliceCount; i++) {
				FButtonOut[i] = FButtonClick[i];
				FButtonClick[i] = false;
			}

			for (int i = 0; i < FCheckBoxOut.SliceCount; i++) {
				FCheckBoxOut[i] = FCheckBoxes[i].Checked;
			}

			for (int i = 0; i < FTextBoxOut.SliceCount; i++) {
				FTextBoxOut[i] = FTextBoxes[i].Text;
			}

			for (int i = 0; i < FSlidersOut.SliceCount; i++) {
				FSlidersOut[i] = FSliders[i].Value / 1000.0;
			}
		}
	}
}
