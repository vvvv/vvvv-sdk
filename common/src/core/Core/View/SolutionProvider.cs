using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

using VVVV.Utils;
using VVVV.Utils.Event;
using VVVV.HDE.Model;
using VVVV.HDE.Model.CS;
using VVVV.HDE.Viewer.Model;

namespace VVVV.HDE.Model.Provider
{
	public class SolutionProvider: Disposable, ITreeContentProvider, ILabelProvider, ISubscriber<EventSubject<PropertyChangedEventArgs>>
    {
		public event EventHandler ContentChanged;
        public event EventHandler LabelChanged;
        
        public SolutionProvider(IEventHub eventHub)
        {
        	eventHub.Subscribe<EventSubject<PropertyChangedEventArgs>>(this, (x) => { return x.Sender is ISolution; });
        }
        
		public void Receive(EventSubject<PropertyChangedEventArgs> subject)
		{
			if (subject.Args.PropertyName == "Name")
				OnLabelChanged(subject.Sender);
			else
				OnContentChanged(subject.Sender);
		}
		
		public virtual IEnumerable GetChildren(object element)
		{
			var solution = element as ISolution;
			foreach (var project in solution.Projects)
			{
				if (!(project is AssemblyProject))
					yield return project;
			}
		}
		
		public virtual string GetText(object element)
		{
			var solution = element as ISolution;
			return solution.Name;
		}
		
		public string GetToolTip(object element)
		{
			return "";
		}
    	
		protected virtual void OnContentChanged(object sender)
		{
			if (ContentChanged != null)
				ContentChanged(sender, EventArgs.Empty);
		}
		
		protected virtual void OnLabelChanged(object sender)
		{
			if (LabelChanged != null)
				LabelChanged(sender, EventArgs.Empty);
		}
    }
}
