using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Core.Commands;

namespace VVVV.Core.Menu
{
    public class DefaultAddMenuEntry : AddMenuEntry
    {
        IServiceProvider FServiceProvider;
        IIDItem FModel;

        public DefaultAddMenuEntry(IServiceProvider serviceProvider, IIDItem model)
            : base(serviceProvider.GetService(typeof(ICommandHistory)) as ICommandHistory)
        {
            var addMenuProvider = serviceProvider.GetService(typeof(IAddMenuProvider)) as IAddMenuProvider;
            if (addMenuProvider != null)
            {
                foreach (var a in addMenuProvider.GetEnumerator())
                {
                    AddEntry(a);
                }
            }
            
            FServiceProvider = serviceProvider;
            FModel = model;                       
        }
    }
}
