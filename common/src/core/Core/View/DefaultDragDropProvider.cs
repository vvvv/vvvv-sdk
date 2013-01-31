using System;
using System.Collections.Generic;
using VVVV.Core.Commands;
using System.Drawing;

namespace VVVV.Core.View
{
    public class DefaultDragDropProvider : IDraggable, IDroppable
    {
        protected IIDItem FIdItem;
        protected IDraggable FDraggable;
        protected IDroppable FDroppable;
        
        public DefaultDragDropProvider(ModelMapper mapper)
        {
            var model = mapper.Model;
            
            if (model is IIDItem)
                FIdItem = model as IIDItem;
            
            if (model is IDraggable)
                FDraggable = model as IDraggable;
            
            if (model is IDroppable)
                FDroppable = model as IDroppable;
        }
        
        public bool AllowDrag()
        {
            if (FDraggable != null)
                return FDraggable.AllowDrag();
            
            if (FIdItem != null)
                return FIdItem.Owner is IEditableCollection;
            
            return false;
        }
        
        public object ItemToDrag()
        {
            if (FDraggable != null)
                return FDraggable.ItemToDrag();
            return FIdItem;
        }
        
        public bool AllowDrop(Dictionary<string, object> items)
        {
            if (FDroppable != null)
                return FDroppable.AllowDrop(items);
            
            if (FIdItem != null)
                if (FIdItem is IEditableCollection)
            {
                object targetItem;
                var destination = FIdItem as IEditableCollection;
                var converter = FIdItem.ServiceProvider.GetService<IConverter>();
                
                foreach (var item in items.Values)
                {
                    if (item is string[] && converter != null)
                    {
                        var entries = item as string[];
                        foreach (var entry in entries)
                        {
                            if (converter.Convert(entry, out targetItem))
                                if (destination.CanAdd(targetItem))
                                    return true;
                        }
                    }
                    else if (converter != null)
                    {
                        if (converter.Convert(item, out targetItem))
                            return destination.CanAdd(targetItem);
                    }
                }
            }
            
            return false;
        }
        
        public void DropItems(Dictionary<string, object> items, Point pt)
        {
            if (FDroppable != null)
                FDroppable.DropItems(items, pt);
            
            if (FIdItem != null)
            {
                if (FIdItem is IEditableIDList)
                {
                    IIDItem targetItem;
                    var destination = FIdItem as IEditableIDList;
                    var commandHistory = FIdItem.GetCommandHistory();
                    var converter = FIdItem.ServiceProvider.GetService<IConverter>();

                    foreach (var item in items.Values)
                    {
                        if (item is string[] && converter != null)
                        {
                            var entries = item as string[];
                            var command = new CompoundCommand();
                            
                            foreach (var entry in entries)
                            {
                                if (converter.Convert(entry, out targetItem))
                                    if (destination.CanAdd(targetItem))
                                        command.Append(CreateAddCommand(destination, targetItem));
                            }
                            
                            commandHistory.Insert(command);
                            break;
                        }
                        else if (converter != null)
                        {
                            if (converter.Convert(item, out targetItem))
                            {
                                if (destination.CanAdd(targetItem))
                                {
                                    var command = CreateAddCommand(destination, targetItem);
                                    commandHistory.Insert(command);
                                }
                            }
                        }
                    }
                }
            }
        }
        
        protected Command CreateAddCommand(IEditableIDList destination, IIDItem item)
        {
            IEditableIDList source;
            if (IsContained(item, out source))
                return Command.Move(source, destination, item);
            else
                return Command.Add(destination, item);
        }
        
        protected bool IsContained(IIDItem item, out IEditableIDList source)
        {
            source = null;
            
            var iid = item as IIDItem;
            if (iid.Owner is IEditableIDList)
            {
                source = iid.Owner as IEditableIDList;
                return true;
            }
            
            return false;
        }
    }
}
