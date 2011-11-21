using System;

namespace VVVV.Core.Viewer
{
    public delegate void LabelEditEventHandler(object sender, LabelEditEventArgs args);
    
    public class LabelEditEventArgs : EventArgs
    {
        public bool CancelEdit
        {
            get;
            set;
        }
        
        public string Label
        {
            get;
            private set;
        }
        
        public object Model
        {
            get;
            private set;
        }
        
        public LabelEditEventArgs(object model, string label)
        {
            Model = model;
            Label = label;
        }
    }
    
    /// <summary>
    /// Description of ILabelEditor.
    /// </summary>
    public interface ILabelEditor
    {
        event LabelEditEventHandler AfterLabelEdit;
        event LabelEditEventHandler BeforeLabelEdit;
        
        bool BeginEdit(object model);
    }
}
