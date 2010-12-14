using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Forms;

using VVVV.Core;
using VVVV.Core.Logging;
using VVVV.Core.Menu;
using VVVV.Core.Model;
using VVVV.Core.Model.FX;
using VVVV.Core.View;
using VVVV.HDE.Viewer.WinFormsViewer;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.ManagedVCL;
using VVVV.Hosting.Factories;

namespace VVVV.HDE.ProjectExplorer
{
	#region PluginInfo
	[PluginInfo(Name = "ProjectExplorer",
	            Category = "VVVV",
	            Shortcut = "Ctrl+J",
	            Author = "vvvv group",
	            Help = "The Project Explorer",
	            InitialBoxWidth = 200,
	            InitialBoxHeight = 100,
	            InitialWindowWidth = 300,
	            InitialWindowHeight = 500,
	            InitialComponentMode = TComponentMode.InAWindow)]
	#endregion PluginInfo
	public class ProjectExplorerPlugin : TopControl, IPluginBase
	{
		protected TreeViewer FTreeViewer;
		protected CheckBox FCheckBox;
		protected ILogger FLogger;
		protected IDiffSpread<bool> FHideUnusedProjectsIn;
		protected MappingRegistry FMappingRegistry;
		
		[Import]
		protected EditorFactory FEditorFactory;
		
		[ImportingConstructor]
		public ProjectExplorerPlugin(
			[Config("Hide unused projects", IsSingle = true, DefaultValue = 1.0)] IDiffSpread<bool> showUnloadedProjectsIn,
			ISolution solution,
			ILogger logger)
		{
			try
			{
				Solution = solution;
				FLogger = logger;
				FHideUnusedProjectsIn = showUnloadedProjectsIn;
				FHideUnusedProjectsIn.Changed += new SpreadChangedEventHander<bool>(FHideUnusedProjectsIn_Changed);
				
				FMappingRegistry = new MappingRegistry();
				FMappingRegistry.RegisterDefaultMapping<INamed, DefaultNameProvider>();
				// Do not allow drag'n drop except for references.
//				FMappingRegistry.RegisterDefaultMapping<IDraggable, DefaultDragDropProvider>();
//				FMappingRegistry.RegisterDefaultMapping<IDroppable, DefaultDragDropProvider>();
				FMappingRegistry.RegisterDefaultMapping<IMenuEntry, DefaultContextMenuProvider>();
				FMappingRegistry.RegisterDefaultMapping<AddMenuEntry, DefaultAddMenuEntry>();
				FMappingRegistry.RegisterDefaultMapping(logger);
				
				if (showUnloadedProjectsIn[0])
					FMappingRegistry.RegisterMapping<ISolution, SolutionViewProvider>();
				else
					FMappingRegistry.RegisterMapping<ISolution, LoadedProjectsSolutionViewProvider>();
				FMappingRegistry.RegisterMapping<IEditableIDList<IReference>, DefaultDragDropProvider>();
				
				FMappingRegistry.RegisterMapping<IProject, ProjectViewProvider>();
				// Do not enumerate IDocument
				FMappingRegistry.RegisterMapping<IDocument, IEnumerable>(Empty.Enumerable);
				FMappingRegistry.RegisterMapping<MsBuildProject, MsBuildProjectViewProvider>();
				FMappingRegistry.RegisterMapping<FXProject, FXProjectViewProvider>();
				FMappingRegistry.RegisterMapping<FXProject, IMenuEntry, FXProjectMenuProvider>();
				FMappingRegistry.RegisterMapping<IProject, IDescripted, DescriptedProjectViewProvider>();
				
				SuspendLayout();
				
				BackColor = System.Drawing.Color.Silver;
				
				FCheckBox = new CheckBox();
				FCheckBox.Text = "Hide unused projects";
				FCheckBox.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
				FCheckBox.Dock = DockStyle.Top;
				FCheckBox.FlatStyle = FlatStyle.Flat;
				FCheckBox.BackColor = System.Drawing.Color.DarkGray;
				FCheckBox.ForeColor = System.Drawing.Color.White;
				FCheckBox.Padding = new Padding(3, 0, 0, 0);
				FCheckBox.AutoSize = true;
				FCheckBox.CheckedChanged += new EventHandler(FCheckBox_CheckedChanged);
				
				FTreeViewer = new TreeViewer();
				FTreeViewer.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
				FTreeViewer.Dock = System.Windows.Forms.DockStyle.Fill;
				FTreeViewer.BackColor = System.Drawing.Color.Silver;
				FTreeViewer.ShowTooltip = true;
				FTreeViewer.DoubleClick += FTreeViewer_DoubleClick;
				
				FTreeViewer.Registry = FMappingRegistry;
				FTreeViewer.Input = Solution;
				
				Controls.Add(FTreeViewer);
				Controls.Add(FCheckBox);
				
				
				ResumeLayout(false);
				PerformLayout();
				
				// Workaround because config pins do not send changed on reload :/
				FCheckBox.Checked = true;
			}
			catch (Exception e)
			{
				logger.Log(e);
				throw e;
			}
		}

		void FCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			this.FHideUnusedProjectsIn[0] = FCheckBox.Checked;
		}

		void FHideUnusedProjectsIn_Changed(IDiffSpread<bool> spread)
		{
			FCheckBox.Checked = FHideUnusedProjectsIn[0];
			
			if (!spread[0])
				FMappingRegistry.RegisterMapping<ISolution, SolutionViewProvider>();
			else
				FMappingRegistry.RegisterMapping<ISolution, LoadedProjectsSolutionViewProvider>();
			
			FTreeViewer.Reload();
		}
		
		public ISolution Solution
		{
			get;
			private set;
		}

		void FTreeViewer_DoubleClick(IModelMapper sender, System.Windows.Forms.MouseEventArgs e)
		{
			string file = null;
			
			var doc = sender.Model as IDocument;
			if (doc != null)
			{
				file = doc.Location.LocalPath;
			}
			else
			{
				var reference = sender.Model as IReference;
				if (reference != null)
				{
					var fxReference = reference as FXReference;
					if (fxReference != null)
					{
						file = fxReference.ReferencedDocument.Location.LocalPath;
					}
				}
			}
			
			if (file != null)
			{
				FEditorFactory.Open(file);
			}
		}
		
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (!IsDisposed)
				{
					FHideUnusedProjectsIn.Changed -= FHideUnusedProjectsIn_Changed;
				}
			}
			
			base.Dispose(disposing);
		}
	}
}
