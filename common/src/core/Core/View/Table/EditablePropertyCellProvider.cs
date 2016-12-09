using System;
using System.Collections.Generic;
using VVVV.Core.Commands;

namespace VVVV.Core.View.Table
{
    /// <summary>
    /// Must be registered for IEditableProperty.
    /// </summary>
    public class EditablePropertyCellProvider : IEnumerable<ICell>, IDisposable
    {
        private readonly Cell FNameCell;
        private readonly EditablePropertyCell FValueCell;
        
        public EditablePropertyCellProvider(IEditableProperty property)
        {
            FNameCell = new Cell(property.Name, typeof(string), false);
            FValueCell = new EditablePropertyCell(property);
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
