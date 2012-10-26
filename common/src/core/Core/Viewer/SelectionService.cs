using System;
using System.Diagnostics;

namespace VVVV.Core.Viewer
{
    /// <summary>
    /// Default implementation of ISelectionService.
    /// </summary>
    public class SelectionService : ISelectionService
    {
        private ISelectionProvider FSelectionProvider;
        
        public SelectionService()
        {
        }
        
        public event EventHandler<SelectionProviderChangedEventArgs> SelectionProviderChanged;
        
        protected virtual void OnSelectionProviderChanged(SelectionProviderChangedEventArgs args)
        {
            if (SelectionProviderChanged != null) 
            {
                SelectionProviderChanged(this, args);
            }
        }
        
        public ISelectionProvider SelectionProvider 
        {
            get 
            {
                return FSelectionProvider;
            }
            set 
            {
                if (FSelectionProvider != value)
                {
                    FSelectionProvider = value;
                    
                    OnSelectionProviderChanged(new SelectionProviderChangedEventArgs(FSelectionProvider));
                }
            }
        }
    }
}
