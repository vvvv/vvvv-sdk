using System;
using System.Diagnostics;

namespace VVVV.Core.Viewer
{
    /// <summary>
    /// Default implementation of ISelectionService.
    /// </summary>
    public class SelectionService : ISelectionService
    {
        class DefaultSelectionProvider : ISelectionProvider
        {
            public ISelection CurrentSelection
            {
                get { return Selection.Empty; }
            }

            public event SelectionChangedEventHandler SelectionChanged;
        }

        private ISelectionProvider FSelectionProvider;
        
        public SelectionService()
        {
            FSelectionProvider = new DefaultSelectionProvider();
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
