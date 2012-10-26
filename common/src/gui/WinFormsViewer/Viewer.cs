using System;
using System.ComponentModel;
using System.Windows.Forms;

using VVVV.Core;
using VVVV.Core.Viewer;

namespace VVVV.HDE.Viewer.WinFormsViewer
{
    /// <summary>
    /// Base class for all Viewer classes.
    /// Should be abstract, but Windows Designer can't handle abstract base classes.
    /// TODO: Make it abstract once all work is done here.
    /// TODO: Should implement ISelectionProvider once it is abstract.
    /// </summary>
    public class Viewer : UserControl, IViewer
    {
        private MappingRegistry FRegistry;
        private object FInput;
        private delegate void ActionDelegate();
        
        [Browsable(false)]
        [ReadOnly(true)]
        public MappingRegistry Registry
        {
            get
            {
                return FRegistry;
            }
            set
            {
                FRegistry = value.CreateChildRegistry();
                InitializeMappingRegistry(FRegistry);
            }
        }
        
        [Browsable(false)]
        [ReadOnly(true)]
        public object Model
        {
            get
            {
                return FInput;
            }
            set
            {
                if (Registry == null)
                    throw new InvalidOperationException("MappingRegistry must be set before Input property can be assigned!");
                
                FInput = value;
                
                if (IsHandleCreated)
                    BeginInvoke(new ActionDelegate(Reload));
                else
                {
                    // We're in the GUI thread.
                    Reload();
                }
            }
        }
        
        public Viewer()
        {
        }
        
        /// <summary>
        /// Reloads the whole model.
        /// Should be abstract.
        /// </summary>
        public virtual void Reload()
        {
            
        }
        
        /// <summary>
        /// Hook to register mappings in the registry.
        /// </summary>
        protected virtual void InitializeMappingRegistry(MappingRegistry registry)
        {
            
        }
    }
}
