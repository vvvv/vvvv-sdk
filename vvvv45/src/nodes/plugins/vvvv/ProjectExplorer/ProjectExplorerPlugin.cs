﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Forms;

using VVVV.Core;
using VVVV.Core.Logging;
using VVVV.Core.Menu;
using VVVV.Core.Model;
using VVVV.Core.Model.FX;
using VVVV.Core.View;
using VVVV.HDE.Viewer.WinFormsViewer;
using VVVV.Hosting.Factories;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.Graph;
using VVVV.Utils.Linq;
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
	            InitialWindowWidth = 340,
	            InitialWindowHeight = 550,
	            InitialComponentMode = TComponentMode.InAWindow)]
	#endregion PluginInfo
	public partial class ProjectExplorerPlugin : TopControl, IPluginBase
	{
		private readonly INode2 FRootNode;
		
		protected ILogger FLogger;
		protected IDiffSpread<bool> FHideUnusedProjectsIn;
		protected IDiffSpread<BuildConfiguration> FBuildConfigIn;
		protected MappingRegistry FMappingRegistry;
		
		[Import]
		protected EditorFactory FEditorFactory;
		
		[ImportingConstructor]
		public ProjectExplorerPlugin(
			[Config("Hide unused projects", IsSingle = true, DefaultValue = 1.0)] IDiffSpread<bool> showUnloadedProjectsIn,
			[Config("Build configuration", IsSingle = true)] IDiffSpread<BuildConfiguration> buildConfigIn,
			ISolution solution,
			ILogger logger,
			IHDEHost hdeHost)
		{
			try
			{
				FRootNode = hdeHost.RootNode;
				
				Solution = solution;
				FLogger = logger;
				
				FHideUnusedProjectsIn = showUnloadedProjectsIn;
				FHideUnusedProjectsIn.Changed += new SpreadChangedEventHander<bool>(FHideUnusedProjectsIn_Changed);
				FBuildConfigIn = buildConfigIn;
				FBuildConfigIn.Changed += FBuildConfigIn_Changed;
				
				FMappingRegistry = new MappingRegistry();
				FMappingRegistry.RegisterDefaultMapping<INamed, DefaultNameProvider>();
				FMappingRegistry.RegisterDefaultMapping<IParent, DefaultParentProvider>();
				// Do not allow drag'n drop except for references.
//				FMappingRegistry.RegisterDefaultMapping<IDraggable, DefaultDragDropProvider>();
				FMappingRegistry.RegisterDefaultMapping<IMenuEntry, DefaultContextMenuProvider>();
				FMappingRegistry.RegisterDefaultMapping<AddMenuEntry, DefaultAddMenuEntry>();
				FMappingRegistry.RegisterDefaultInstance(FLogger);
				FMappingRegistry.RegisterDefaultInstance(FRootNode);
				
				if (showUnloadedProjectsIn[0])
					FMappingRegistry.RegisterMapping<ISolution, SolutionViewProvider>();
				else
					FMappingRegistry.RegisterMapping<ISolution, LoadedProjectsSolutionViewProvider>();
				FMappingRegistry.RegisterMapping<IEditableIDList<IReference>, DefaultDragDropProvider>();
				
				FMappingRegistry.RegisterMapping<IProject, ProjectViewProvider>();
				// Do not enumerate IDocument
				FMappingRegistry.RegisterInstance<IDocument, IEnumerable>(Empty.Enumerable);
				FMappingRegistry.RegisterMapping<MsBuildProject, MsBuildProjectViewProvider>();
				FMappingRegistry.RegisterMapping<FXProject, FXProjectViewProvider>();
				FMappingRegistry.RegisterMapping<FXProject, IMenuEntry, FXProjectMenuProvider>();
				FMappingRegistry.RegisterMapping<IProject, IDescripted, DescriptedProjectViewProvider>();
				// Allow drag drop only in MsBuildProject
				FMappingRegistry.RegisterMapping<MsBuildProject, IDroppable, DefaultDragDropProvider>();
				FMappingRegistry.RegisterMapping<IReference, ReferenceViewProvider>();
				FMappingRegistry.RegisterMapping<PersistentIDContainer, PersistentNameProvider>();
				FMappingRegistry.RegisterMapping<IReference, IDescripted, ReferenceViewProvider>();
				
				InitializeComponent();
				
				FBuildConfigComboBox.Items.AddRange((object[]) Enum.GetNames(typeof(BuildConfiguration)));
				FBuildConfigComboBox.SelectedIndex = 0;
				
				FHideUnusedProjectsCheckBox.CheckedChanged += FHideUnusedProjectsCheckBox_CheckedChanged;
				FTreeViewer.DoubleClick += FTreeViewer_DoubleClick;

                FTreeViewer.Registry = FMappingRegistry;
                FTreeViewer.Input = Solution;
                
                // Workaround because config pins do not send changed on reload :/
                FHideUnusedProjectsIn.Sync();
                FBuildConfigIn.Sync();
			}
			catch (Exception e)
			{
				logger.Log(e);
				throw e;
			}
		}
		
		protected bool IsProjectInUse(IProject project)
		{
			var query =
				from node in FRootNode.AsDepthFirstEnumerable()
				where node.NodeInfo.UserData == project
				select node;
			
			return query.Any();
		}

		void FBuildConfigIn_Changed(IDiffSpread<BuildConfiguration> spread)
		{
			FBuildConfigComboBox.SelectedIndex = (int) spread[0];
			
			var projects =
				from p in Solution.Projects
				where p is MsBuildProject
				select p as MsBuildProject;
			
			foreach (var project in projects)
			{
				project.BuildConfiguration = spread[0];
                if (IsProjectInUse(project))
                {
                    if (!project.IsLoaded)
                        project.Load();
                    project.CompileAsync();
                }
			}
		}

		void FHideUnusedProjectsCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			this.FHideUnusedProjectsIn[0] = FHideUnusedProjectsCheckBox.Checked;
			this.FHideUnusedProjectsIn.Flush();
		}

		void FHideUnusedProjectsIn_Changed(IDiffSpread<bool> spread)
		{
			FHideUnusedProjectsCheckBox.Checked = FHideUnusedProjectsIn[0];
			
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

		void FTreeViewer_DoubleClick(ModelMapper sender, System.Windows.Forms.MouseEventArgs e)
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
		
		void FBuildConfigComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			var buildConfig = (BuildConfiguration) FBuildConfigComboBox.SelectedIndex;
			FBuildConfigIn[0] = buildConfig;
			FBuildConfigIn.Flush();
		}
	}
}
