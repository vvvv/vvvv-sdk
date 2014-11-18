using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using VL.Core;
using VL.Core.Commands;
using VL.Core.Menu;
using VL.Core.Model;
using VL.Core.Serialization;
using VL.Core.Viewer;
using VL.HDE.View;
using VL.Lang;
using VL.Lang.Commands;
using VL.Lang.Model;
using VL.Lang.Symbols;
using VL.Applications;
using VL.Lang.View;

namespace VVVV.VL.Factories
{
    class MainMenuProvider : IMenuProvider
    {
        Solution FSolution;
        EditorForm FPatchEditor;
        ISelectionService FSelectionService;
        IViewerService FViewerService;
        List<IMenuEntry> FMenuEntries = new List<IMenuEntry>();
        
        ICommandHistory FCommandHistory;

        public MainMenuProvider(Solution solution, EditorForm patchEditor, ISelectionService selectionService, IViewerService viewerService)
        {
            FSolution = solution;
            FPatchEditor = patchEditor;
            FSelectionService = selectionService;
            FSelectionService.CurrentProviderSelectionChanged += HandleSelectionChanged;
            FViewerService = viewerService;
            FViewerService.ActiveViewerChanged += viewerService_ActiveViewerChanged;
            FCommandHistory = GetCommandHistoryFromViewer(viewerService.ActiveViewer);
            FCommandHistory.CommandExecuted += commandHistory_CommandExecuted;
            
            //only create menu once so that checked menu items keep state
            FMenuEntries.Add(CreateFileMenu());
            FMenuEntries.Add(CreateEditMenu());
        }

        void viewerService_ActiveViewerChanged(object sender, ActiveViewerChangedEventArgs e)
        {
        	//unsubscribe from previous viewer
        	FCommandHistory.CommandExecuted -= commandHistory_CommandExecuted;
        	
        	//subscribe to new viewer
        	FCommandHistory = GetCommandHistoryFromViewer(FViewerService.ActiveViewer);
        	FCommandHistory.CommandExecuted += commandHistory_CommandExecuted;
        }

        void commandHistory_CommandExecuted(object sender, CommandExecutedEventArgs args)
        {
        	//update endabled state of menuentries
        	OnUpdate();
        }

		void HandleSelectionChanged(object sender, SelectionChangedEventArgs args)
		{
			//update endabled state of menuentries
        	OnUpdate();
		}
		
        public Action OnUpdate {get;set;}
        
        public IEnumerable<IMenuEntry> MenuEntries
        {
            get 
            {
            	return FMenuEntries;    
            }
        }

        MenuEntry CreateFileMenu()
        {
            var assemblyName = string.Empty;
            var selectedPatch = this.FPatchEditor.SelectedType;
            if (selectedPatch != null)
            {
                var project = selectedPatch.Project;
                if (project != null)
                    assemblyName = project.AssemblyLocation;
            }

            var menuEntry = new MenuEntry("File");
            menuEntry.AddEntry(new MenuEntry("Open", Keys.Control | Keys.O, OpenDocument));
            menuEntry.AddEntry(new MenuEntry("Save", Keys.Control | Keys.S, Save));
            menuEntry.AddEntry(new MenuEntry("Save As", Keys.Control | Keys.Shift | Keys.S, SaveAs));
            return menuEntry;
        }

        MenuEntry CreateEditMenu()
        {
            var commandHistory = GetCommandHistoryFromViewer(FViewerService.ActiveViewer);
            var menuEntry = new MenuEntry("Edit");
            menuEntry.AddEntry(new UndoMenuEntry(commandHistory));
            menuEntry.AddEntry(new RedoMenuEntry(commandHistory));
            menuEntry.AddEntry(new MenuSeparator());
            menuEntry.AddEntry(new MenuEntry("Cut", Keys.Control | Keys.X, Cut, () => CurrentSelection.Cast<object>().Any()));
            menuEntry.AddEntry(new MenuEntry("Copy", Keys.Control | Keys.C, Copy, () => CurrentSelection.Cast<object>().Any()));
            menuEntry.AddEntry(new MenuEntry("Paste", Keys.Control | Keys.V, Paste, Clipboard.ContainsText));
            menuEntry.AddEntry(new MenuEntry("Duplicate", Keys.Control | Keys.D, Duplicate, () => CurrentSelection.Cast<object>().Any()));
            menuEntry.AddEntry(new MenuEntry("Delete", Keys.Delete, Delete, () => CurrentSelection.Cast<object>().Any()));
            menuEntry.AddEntry(new MenuEntry("Select All", Keys.Control | Keys.A, SelectAll));
            menuEntry.AddEntry(new MenuEntry("Deselect All", Keys.Control | Keys.Shift | Keys.A, DeselectAll));
            menuEntry.AddEntry(new MenuSeparator());
            menuEntry.AddEntry(new MenuEntry("Align", Keys.Control | Keys.L, Align, () => CurrentSelection.Cast<object>().Any()));
            menuEntry.AddEntry(new MenuEntry("Remove Errors", Keys.Control | Keys.E, RemoveErrors));
            return menuEntry;
        }

        #region File

        void OpenDocument(MenuEntry entry)
        {
            var fd = new OpenFileDialog();
            fd.DefaultExt = "vl";
            fd.Filter = "VL Documents (*.vl)|*.vl|All Files (*.*)|*.*";
            fd.ShowDialog();
            if (fd.FileName == "") return;

            //setup and load document
            var project = FSolution.Projects[0] as VLProject;
            project.AddVLDocumentFromFile(fd.FileName);
        }

        void Save(MenuEntry entry)
        {
            var doc = FPatchEditor.SelectedType.Namespace.Document;
            doc.Save();
            FPatchEditor.UpdateCaption();
        }

        void SaveAs(MenuEntry entry)
        {
            var doc = FPatchEditor.SelectedType.Namespace.Document;

            var fd = new SaveFileDialog();
            fd.FileName = doc.LocalPath;
            fd.DefaultExt = "vl";
            fd.Filter = "VL Documents (*.vl)|*.vl";
            fd.ShowDialog();
            if (!string.IsNullOrEmpty(fd.FileName))
            {
                doc.SaveTo(fd.FileName);
            }
            
            FPatchEditor.UpdateCaption();
        }

        #endregion

        #region Edit

        static ICommandHistory GetCommandHistoryFromViewer(IViewer viewer)
        {
            var idItem = viewer.Model as IIDItem;
            if (idItem != null)
            {
                return idItem.GetCommandHistory();
            }
            return null;
        }

        ISelection CurrentSelection
        {
            get
            {
                var selectionProvider = FSelectionService.SelectionProvider;
                var currentSelection = selectionProvider.CurrentSelection;
                return currentSelection ?? Selection.Empty;
            }
        }

        void Cut(MenuEntry entry)
        {
            VLCommander.Cut(CurrentSelection);
        }

        void Copy(MenuEntry entry)
        {
            VLCommander.Copy(CurrentSelection);
        }

        void Paste(MenuEntry entry)
        {
            // ??? activeViewer was of type EditorForm. casting it via as PatchView returned null. how was this supposed to work?

            //var activeViewer = viewerService.ActiveViewer as PatchView;
            //var activePatch = activeViewer.Model as VLPatch;
            
            var location = this.FPatchEditor.LastMouseLocation;
            var activeNodeDef = this.FPatchEditor.SelectedTypeView;
            
            if (activeNodeDef != null)
            {
                var patchView = activeNodeDef.GetPatchView(location);
                patchView.StartRecordNewViews();
                VLCommander.Paste(patchView.Patch, location);
                patchView.EndRecordNewViews();
            }
        }

        void Duplicate(MenuEntry entry)
        {
            var location = this.FPatchEditor.LastMouseLocation;
            var activeNodeDef = this.FPatchEditor.SelectedTypeView;

            if (activeNodeDef != null)
            {
                var patchView = activeNodeDef.GetPatchView(location);
                patchView.StartRecordNewViews();
                VLCommander.Duplicate(CurrentSelection);
                patchView.EndRecordNewViews();
            }
        }

        void Delete(MenuEntry entry)
        {
            VLCommander.Delete(CurrentSelection);
        }
        
        void SelectAll(MenuEntry entry)
        {
            FPatchEditor.SelectedTypeView.SelectedNodes.Clear();
            
            foreach (var cast in FPatchEditor.SelectedTypeView.PatchView.AllPatchCastViews)
            {
                FPatchEditor.SelectedTypeView.AddToSelection(cast);
            }

            FPatchEditor.SelectedTypeView.FireSelectionChanged();
            FPatchEditor.SelectedTypeView.RequestUpdateScene();
        }
        
        void DeselectAll(MenuEntry entry)
        {
            FPatchEditor.SelectedTypeView.ClearSelection();
            FPatchEditor.SelectedTypeView.FireSelectionChanged();
            FPatchEditor.SelectedTypeView.RequestUpdateScene();
        }
        
        void Align(MenuEntry entry)
        {
            VLCommander.Align(CurrentSelection);
        }

        void RemoveErrors(MenuEntry entry)
        {
            var patch = FPatchEditor.SelectedType.Patch;
            patch.CommandHistory.Insert(new RemoveErrorsCommand(patch));
        }

        #endregion
    }
}
