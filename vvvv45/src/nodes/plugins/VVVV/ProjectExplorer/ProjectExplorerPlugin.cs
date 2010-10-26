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
	public class ProjectExplorerPlugin : TopControl, IPluginEvaluate
	{
		public ISolution Solution
		{
			get;
			private set;
		}
		
		protected TreeViewer FTreeViewer;
		protected ILogger FLogger;
		[Import]
		protected IHDEHost FHDEHost;
		
		[ImportingConstructor]
		public ProjectExplorerPlugin(ISolution solution, ILogger logger)
		{
			try
			{
				Solution = solution;
				FLogger = logger;
				
				var mappingRegistry = new MappingRegistry();
				mappingRegistry.RegisterDefaultMapping<INamed, DefaultNameProvider>();
				mappingRegistry.RegisterDefaultMapping<IDraggable, DefaultDragDropProvider>();
				mappingRegistry.RegisterDefaultMapping<IDroppable, DefaultDragDropProvider>();
				mappingRegistry.RegisterDefaultMapping<IMenuEntry, DefaultContextMenuProvider>();
				mappingRegistry.RegisterDefaultMapping<AddMenuEntry, DefaultAddMenuEntry>();
				mappingRegistry.RegisterDefaultMapping(logger);
				
				mappingRegistry.RegisterMapping<ISolution, SolutionViewProvider>();
				mappingRegistry.RegisterMapping<IProject, ProjectViewProvider>();
				// Do not enumerate IDocument
				mappingRegistry.RegisterMapping<IDocument, IEnumerable>(Empty.Enumerable);
				mappingRegistry.RegisterMapping<MsBuildProject, MsBuildProjectViewProvider>();
				mappingRegistry.RegisterMapping<FXProject, FXProjectViewProvider>();
				mappingRegistry.RegisterMapping<IProject, IDescripted, DescriptedProjectViewProvider>();
				
				SuspendLayout();
				
				BackColor = System.Drawing.Color.Silver;
				
				FTreeViewer = new TreeViewer();
				FTreeViewer.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
				FTreeViewer.Dock = System.Windows.Forms.DockStyle.Fill;
				FTreeViewer.BackColor = System.Drawing.Color.Silver;
				FTreeViewer.ShowTooltip = true;
				
				FTreeViewer.DoubleClick += FTreeViewer_DoubleClick;
				
				FTreeViewer.Registry = mappingRegistry;
				FTreeViewer.Input = Solution;
				
				Controls.Add(FTreeViewer);
				
				ResumeLayout(false);
				PerformLayout();
			}
			catch (Exception e)
			{
				logger.Log(e);
				throw e;
			}
		}

		void FTreeViewer_DoubleClick(IModelMapper sender, MouseEventArgs e)
		{
			var doc = sender.Model as IDocument;
			if (doc != null)
			{
				// We only want the EffectFactory to answer.
				var addonFactories = new List<IAddonFactory>(FHDEHost.AddonFactories);
				var editorFactory = addonFactories.Find(factory => factory is EditorFactory);
				
				try
				{
					FHDEHost.AddonFactories.Clear();
					FHDEHost.AddonFactories.Add(editorFactory);
					FHDEHost.Open(doc.Location.LocalPath, false);
				}
				finally
				{
					FHDEHost.AddonFactories.Clear();
					FHDEHost.AddonFactories.AddRange(addonFactories);
				}
			}
		}
		
		public void Evaluate(int spreadMax)
		{
			
		}
	}
}
