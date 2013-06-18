using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Core.Commands;

namespace VVVV.Core.Menu
{
    public class DefaultAddMenuEntry : AddMenuEntry
    {
        IIDItem FModel;

        public DefaultAddMenuEntry(IIDItem model, ModelMapper mapper)
            : base(model.GetCommandHistory())
        {
            if (mapper.CanMap<IAddMenuProvider>())
            {
                var addMenuProvider = mapper.Map<IAddMenuProvider>();
                foreach (var a in addMenuProvider.GetEnumerator())
                {
                    AddEntry(a);
                }
            }
            
            FModel = model;                       
        }
    }
}
