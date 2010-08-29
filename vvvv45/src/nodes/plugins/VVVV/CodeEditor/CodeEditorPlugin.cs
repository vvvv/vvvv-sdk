#region usings

using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

using VVVV.Core.Logging;
using VVVV.Core.Model;
using VVVV.HDE.CodeEditor.Gui.Dialogs;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.ManagedVCL;

#endregion usings

namespace VVVV.HDE.CodeEditor
{
	#region PluginInfo
	[PluginInfo(Name = "CodeEditor",
	            Category = "VVVV",
	            Shortcut = "Ctrl+K",
	            Author = "vvvv group",
	            Help = "The Code Editor",
	            InitialBoxWidth = 200,
	            InitialBoxHeight = 100,
	            InitialWindowWidth = 700,
	            InitialWindowHeight = 800,
	            InitialComponentMode = TComponentMode.InAWindow)]
	#endregion PluginInfo
	public class CodeEditorPlugin : TopControl, IPluginHDE, IDisposable, IQueryDelete
	{
		private CodeEditorForm FCodeEditorForm;
		private ILogger FLogger;
		
		public static readonly ImageList CompletionIcons = new ImageList();
		
		[ImportingConstructor]
		public CodeEditorPlugin(IHDEHost host, ISolution solution, ILogger logger)
		{
			FLogger = logger;
			
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
		
		protected override void Dispose(bool disposing)
		{
			try
			{
				base.Dispose(disposing);
			}
			catch (Exception e)
			{
				FLogger.Log(e);
			}
		}
		
		public bool DeleteMe()
		{
			// Save the current SynchronizationContext. ShowDialog method changes the
			// current SynchronizationContext. Later RunWorkerCompleted callbacks from
			// BackgroundWorker objects do not return to the GUI (vvvv) thread.
			var syncContext = SynchronizationContext.Current;
			
			// Check if one of opened documents needs to be saved.
			bool saveAll = false;
			foreach (var doc in FCodeEditorForm.OpenDocuments)
			{
				if (doc.IsDirty)
				{
					if (saveAll)
					{
						doc.Save();
						continue;
					}
					
					var saveDialog = new SaveDialog(doc.Location.LocalPath);
					if (saveDialog.ShowDialog(this) == DialogResult.OK)
					{
						// Resore the old SynchronizationContext.
						SynchronizationContext.SetSynchronizationContext(syncContext);
						
						var result = saveDialog.SaveOptionResult;
						
						switch (result)
						{
							case SaveOption.SaveAll:
								doc.Save();
								saveAll = true;
								break;
							case SaveOption.Save:
								doc.Save();
								break;
							case SaveOption.DontSave:
								// Do nothing
								break;
							default:
								// Cancel
								return false;
						}
					}
					else
					{
						// Cancel
						return false;
					}
				}
			}
			
			return true;
		}
	}
}
