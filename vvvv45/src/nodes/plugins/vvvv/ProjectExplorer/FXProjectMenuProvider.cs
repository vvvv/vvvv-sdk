
using System;
using System.Windows.Forms;
using VVVV.Core.Commands;
using VVVV.Core.Logging;
using VVVV.Core.Menu;
using VVVV.Core.Model.FX;

namespace VVVV.HDE.ProjectExplorer
{
	public class FXProjectMenuProvider : IMenuEntry
	{
		private FXProject FProject;
		private ILogger FLogger;
		
		public FXProjectMenuProvider(FXProject project, ILogger logger)
		{
			FProject = project;
			FLogger = logger;
		}
		
		public string Name
		{
			get;
			private set;
		}
		
		public Keys ShortcutKeys
		{
			get
			{
				return Keys.None;
			}
		}
		
		public bool Enabled
		{
			get
			{
				return true;
			}
		}
		
		public void Click()
		{
			// We act as the root menu entry -> Click will never be called on us.
		}
		
		public System.Collections.Generic.IEnumerator<IMenuEntry> GetEnumerator()
		{
			var commandHistory = FProject.Mapper.Map<ICommandHistory>();
			if (!FProject.IsLoaded)
				yield return new LoadMenuEntry(commandHistory, FProject, FLogger);
			else
				yield return new UnloadMenuEntry(commandHistory, FProject, FLogger);
		}
		
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
