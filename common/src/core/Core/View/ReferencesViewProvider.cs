using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using VVVV.Core.Commands;
using VVVV.Core.Dialogs;
using VVVV.Core.Menu;
using VVVV.Core.Model;

namespace VVVV.Core.View
{
    public class ReferencesViewProvider : IAddMenuProvider
    {
        #region AddReferencesMenuEntry class
        
        protected class AddReferenceMenuEntry : MenuEntry
        {
            protected readonly IProject FProject;
            protected readonly IEditableIDList<IReference> FReferences;
            
            public AddReferenceMenuEntry(ICommandHistory history, IProject project, IEditableIDList<IReference> references)
                :base(history, "Reference", Keys.Control | Keys.N)
            {
                FProject = project;
                FReferences = references;
            }
            
            public override void Click()
            {
                var dialog = new ReferenceDialog();
                    
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var command = new CompoundCommand();
                    foreach (var reference in dialog.References)
                    {
                        if (!FReferences.Contains(reference))
                        {
                            command.Append(Command.Add(FReferences, reference));
                        }
                    }
                    CommandHistory.Insert(command);
                }
            }
        }
        
        #endregion
        
        protected readonly IProject FProject;
        protected readonly IEditableIDList<IReference> FReferences;
        protected readonly ICommandHistory FCommandHistory;
        
        public ReferencesViewProvider(IProject project, IEditableIDList<IReference> references)
        {
            FProject = project;
            FReferences = references;
            FCommandHistory = FReferences.Mapper.Map<ICommandHistory>();
        }
        
        public IEnumerable<IMenuEntry> GetEnumerator()
        {
            yield return new AddReferenceMenuEntry(FCommandHistory, FProject, FReferences);
        }
    }
}
