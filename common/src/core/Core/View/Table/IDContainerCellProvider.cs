using System;
using System.Collections;
using System.Collections.Generic;

using VVVV.Core.Model;

namespace VVVV.Core.View.Table
{
    /// <summary>
    /// Registered for IDContainer.
    /// </summary>
    public class IDContainerCellProvider : IEnumerable<ICell>
    {
        protected IDContainer FIDContainer;
        
        public IDContainerCellProvider(IDContainer idContainer)
        {
            FIDContainer = idContainer;
        }
        
        public IEnumerator<ICell> GetEnumerator()
        {
            foreach (IIDItem idItem in FIDContainer) 
            {
                if (idItem is IViewableProperty)
                {
                    var property = idItem as IViewableProperty;
                    yield return new ViewablePropertyCell(property);
                }
            }
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
