#region usings
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;

using VVVV.Core.Logging;
using VVVV.Core.Model;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

#endregion usings

namespace VVVV.HDE.CodeEditor
{
	#region PluginInfo
	[PluginInfo(Name = "CodeEditor",
	            Category = "HDE",
	            Shortcut = "Ctrl+K",
	            Author = "vvvv group",
	            Help = "The Code Editor",
	            InitialBoxWidth = 200,
	            InitialBoxHeight = 100,
	            InitialWindowWidth = 800,
	            InitialWindowHeight = 600,
	            InitialComponentMode = TComponentMode.InAWindow)]
	#endregion PluginInfo
	public class CodeEditorPlugin : ManagedVCL.TopControl, IPluginHDE, IDisposable
	{
		private CodeEditorForm FCodeEditorForm;
		
		public static readonly ImageList CompletionIcons = new ImageList();
		
		[ImportingConstructor]
		public CodeEditorPlugin(IHDEHost host, ISolution solution, ILogger logger)
		{
			if (CompletionIcons.Images.Count == 0)
			{
				var resources = new ComponentResourceManager(typeof(CodeEditorForm));
				CompletionIcons.TransparentColor = System.Drawing.Color.Transparent;
				CompletionIcons.Images.Add((System.Drawing.Bitmap) resources.GetObject("Icons.16x16.Class"));
				CompletionIcons.Images.Add((System.Drawing.Bitmap) resources.GetObject("Icons.16x16.Method"));
				CompletionIcons.Images.Add((System.Drawing.Bitmap) resources.GetObject("Icons.16x16.Property"));
				CompletionIcons.Images.Add((System.Drawing.Bitmap) resources.GetObject("Icons.16x16.Field"));
				CompletionIcons.Images.Add((System.Drawing.Bitmap) resources.GetObject("Icons.16x16.Enum"));
				CompletionIcons.Images.Add((System.Drawing.Bitmap) resources.GetObject("Icons.16x16.NameSpace"));
				CompletionIcons.Images.Add((System.Drawing.Bitmap) resources.GetObject("Icons.16x16.Event"));
			}
			
			SuspendLayout();
			
			FCodeEditorForm = new CodeEditorForm(host, solution, logger);
			FCodeEditorForm.Location = new Point(0, 0);
			FCodeEditorForm.TopLevel = false;
			FCodeEditorForm.TopMost = false;
			FCodeEditorForm.Dock = DockStyle.Fill;
			FCodeEditorForm.Show();
			Controls.Add(FCodeEditorForm);
			
			ResumeLayout(false);
			PerformLayout();
		}
	}
}
