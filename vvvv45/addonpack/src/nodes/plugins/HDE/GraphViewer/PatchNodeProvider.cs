using System;
using System.ComponentModel;

using VVVV.HDE.Viewer.Model;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.Event;

namespace VVVV.Nodes.GraphViewer
{
	/// <summary>
	/// Description of DocumentContentProvider.
	/// </summary>
	public class PatchNodeProvider: ITreeContentProvider, ILabelProvider, ISelectionProvider, ISubscriber<EventSubject<PropertyChangedEventArgs>>
	{
	    public event EventHandler ContentChanged;
        public event EventHandler LabelChanged;
        public event EventHandler SelectionChanged;
        
		public PatchNodeProvider(IEventHub eventHub)
		{
		    eventHub.Subscribe<EventSubject<PropertyChangedEventArgs>>(this, (x) => { return x.Sender is PatchNode; });
		}
		
		public void Dispose()
		{
		}
				
		System.Collections.IEnumerable ITreeContentProvider.GetChildren(object element)
		{
			PatchNode self = element as PatchNode;
			return self.GetChildren();
		}
		
		public string GetText(object element)
		{
		    PatchNode self = element as PatchNode;
		    return self.Text;
		}
		
		public string GetToolTip(object element)
        {
            throw new NotImplementedException();
        }
	    
        public void Receive(EventSubject<PropertyChangedEventArgs> subject)
        {
            if (subject.Args.PropertyName == "Children")
				OnContentChanged(subject.Sender);
            else if (subject.Args.PropertyName == "Selection")
				OnSelectionChanged(subject.Sender);
        }
        
        protected virtual void OnContentChanged(object sender)
		{
			if (ContentChanged != null)
				ContentChanged(sender, EventArgs.Empty);
		}
        
        protected virtual void OnSelectionChanged(object sender)
		{
			if (SelectionChanged != null)
				SelectionChanged(sender, EventArgs.Empty);
		}
	    
        public bool IsSelected(object element)
        {
            return (element as PatchNode).Selected;
        }
	}
}
