using System;
using System.Collections;
using System.ComponentModel.Composition;

using VVVV.Core;
using VVVV.Core.Menu;
using VVVV.Core.Model;
using VVVV.Core.View;
using VVVV.HDE.Viewer.WinFormsViewer;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.ManagedVCL;

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
		
		TreeViewer FTreeViewer;
		
		[ImportingConstructor]
		public ProjectExplorerPlugin(ISolution solution)
		{
			Solution = solution;
			
			var mappingRegistry = new MappingRegistry();
			mappingRegistry.RegisterDefaultMapping<INamed, DefaultNameProvider>();
			mappingRegistry.RegisterDefaultMapping<IDraggable, DefaultDragDropProvider>();
            mappingRegistry.RegisterDefaultMapping<IDroppable, DefaultDragDropProvider>();
            mappingRegistry.RegisterDefaultMapping<IMenuEntry, DefaultContextMenuProvider>();
            mappingRegistry.RegisterDefaultMapping<AddMenuEntry, DefaultAddMenuEntry>();
            
			mappingRegistry.RegisterMapping<ISolution, SolutionViewProvider>();
			mappingRegistry.RegisterMapping<IProject, ProjectViewProvider>();
			// Do not enumerate IDocument
			mappingRegistry.RegisterMapping<IDocument, IEnumerable>(Empty.Enumerable);
			mappingRegistry.RegisterMapping<IEditableIDList<IReference>, ReferencesViewProvider>();
			
			SuspendLayout();
			
			FTreeViewer = new TreeViewer();
			FTreeViewer.Dock = System.Windows.Forms.DockStyle.Fill;
			FTreeViewer.Registry = mappingRegistry;
			FTreeViewer.Input = Solution;
			
			Controls.Add(FTreeViewer);
			
			ResumeLayout(false);
			PerformLayout();
		}
		
		public void Evaluate(int spreadMax)
		{
			
		}
	}
}
