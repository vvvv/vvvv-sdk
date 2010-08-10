#region usings
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using ManagedVCL;
using Microsoft.Practices.Unity;
using VVVV.Core;
using VVVV.Core.Commands;
using VVVV.Core.Logging;
using VVVV.Core.Menu;
using VVVV.Core.Model;
using VVVV.Core.Model.CS;
using VVVV.Core.View;
using VVVV.Core.View.Table;
using VVVV.Core.Viewer;
using VVVV.HDE.CodeEditor.ErrorView;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using Dom = ICSharpCode.SharpDevelop.Dom;

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
		
		[ImportingConstructor]
		public CodeEditorPlugin(IHDEHost host, ISolution solution, ILogger logger)
		{
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
