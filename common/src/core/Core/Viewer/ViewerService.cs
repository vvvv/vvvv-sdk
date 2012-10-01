using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.Core.Viewer
{
    /// <summary>
    /// Default implementation of the viewer service.
    /// </summary>
    public class ViewerService : IViewerService
    {
        private IViewer activeViewer;

        /// <summary>
        /// Gets or sets the active viewer of the application.
        /// </summary>
        public IViewer ActiveViewer 
        {
            get { return activeViewer; }
            set
            {
                if (value != activeViewer)
                {
                    activeViewer = value;
                    OnActiveViewerChanged(new ActiveViewerChangedEventArgs(activeViewer));
                }
            }
        }

        /// <summary>
        /// Raised if the active viewer of the application changed.
        /// </summary>
        public event EventHandler<ActiveViewerChangedEventArgs> ActiveViewerChanged;

        protected virtual void OnActiveViewerChanged(ActiveViewerChangedEventArgs args)
        {
            if (ActiveViewerChanged != null)
            {
                ActiveViewerChanged(this, args);
            }
        }
    }
}
