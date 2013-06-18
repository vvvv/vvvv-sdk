using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

using VVVV.Core.Commands;
using VVVV.Core.Logging;
using VVVV.Core.Menu;
using VVVV.Core.Model;
using VVVV.Core.Viewer;

namespace VVVV.Core.View
{
	public class DefaultContextMenuProvider : IMenuEntry
	{
		protected ModelMapper FMapper;
		protected ILogger FLogger;
		
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
		
		public DefaultContextMenuProvider(ModelMapper mapper, ILogger logger)
		{
			FMapper = mapper;
			FLogger = logger;
		}
		
		public void Click()
		{
			// We act as the root menu entry -> Click will never be called on us.
		}
		
		public virtual IEnumerator<IMenuEntry> GetEnumerator()
		{
			var idItem = FMapper.Model as IIDItem;
			
			if (idItem == null)
				yield break;
			
			var commandHistory = idItem.Mapper.Map<ICommandHistory>();
			
			if (FMapper.CanMap<AddMenuEntry>())
			{
				var addMenuEntry = FMapper.Map<AddMenuEntry>();
				if (addMenuEntry.HasSubMenuEntries)
				{
					yield return addMenuEntry;
					yield return new MenuSeparator();
				}
			}
			
			yield return new UndoMenuEntry(commandHistory);
			yield return new RedoMenuEntry(commandHistory);
			yield return new MenuSeparator();

			if (FMapper.CanMap<IRenameable>())
			{
				var renameable = FMapper.Map<IRenameable>();
				if (FMapper.CanMap<ILabelEditor>())
					yield return new RenameMenuEntry(commandHistory, renameable, FMapper.Map<ILabelEditor>());
				else
					yield return new RenameMenuEntry(commandHistory, renameable);
				yield return new MenuSeparator();
			}

			if (FMapper.CanMap<IEditableProperty>())
			{
				var renameable = FMapper.Map<IEditableProperty>();
				yield return new SetPropertyMenuEntry(commandHistory, renameable);
				yield return new MenuSeparator();
			}

			if (idItem.Owner is IEditableIDList)
			{
				var owner = idItem.Owner as IEditableIDList;
				commandHistory = idItem.Owner.Mapper.Map<ICommandHistory>();

				if (owner.CanRemove(idItem))
					yield return new RemoveMenuEntry<IEditableIDList, IIDItem>(commandHistory, owner, idItem);
			}
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
