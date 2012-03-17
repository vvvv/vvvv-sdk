using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Core.Commands;
using System.Windows.Forms;

namespace VVVV.Core.Menu
{
    public class AddItemMenuEntry<T> : MenuEntry where T : IIDItem
    {
        protected IEditableIDList<T> OwnerList;
        protected Func<T> FValueFactory;

        public AddItemMenuEntry(IEditableIDList<T> ownerlist, string newModelTypeName, Keys shortcutKeys, Func<T> valueFactory)
            : base(ownerlist.Mapper.Map<ICommandHistory>(), newModelTypeName, shortcutKeys)
        {
            OwnerList = ownerlist;
            FValueFactory = valueFactory;
        }
        
        public override void Click()
        {
            if (FValueFactory != null)
            {
                var item = FValueFactory();
                if (item != null)
                {
                    var command = Command.Add(OwnerList, item);
                    CommandHistory.Insert(command);
                }
            }
        }
    }
}
