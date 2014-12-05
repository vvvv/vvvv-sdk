using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Core.Commands;

namespace VVVV.Core.Menu
{
    public class DefaultAddMenuEntry : AddMenuEntry
    {
        ModelMapper FMapper;
        IIDItem FModel;

        public DefaultAddMenuEntry(ModelMapper mapper, IIDItem model)
            :base(model.Mapper.Map<ICommandHistory>())
        {
            if (mapper.CanMap<IAddMenuProvider>())
                foreach (var a in mapper.Map<IAddMenuProvider>().GetEnumerator())
                    Add(a);
            
            FMapper = mapper;
            FModel = model;                       
        }
    }
}
