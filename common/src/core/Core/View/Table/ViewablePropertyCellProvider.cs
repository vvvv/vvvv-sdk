
using System;
using System.Collections.Generic;

namespace VVVV.Core.View.Table
{
    /// <summary>
    /// Must be registered for IViewableProperty.
    /// </summary>
    public class ViewablePropertyCellProvider : IEnumerable<ICell>, IDisposable
    {
        private readonly Cell FNameCell;
        private readonly ViewablePropertyCell FValueCell;
        
        public ViewablePropertyCellProvider(IViewableProperty property)
        {
            FNameCell = new Cell(property.Name, typeof(string), false);
            FValueCell = new ViewablePropertyCell(property);
        }

        IEnumerator<ICell> IEnumerable<ICell>.GetEnumerator()
        {
            yield return FNameCell;
            yield return FValueCell;
        }
        
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
        
        public void Dispose()
        {
            FValueCell.Dispose();
        }
    }
}
