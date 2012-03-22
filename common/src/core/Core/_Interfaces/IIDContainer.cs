using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using VVVV.Core.Model;

namespace VVVV.Core
{
    public interface IIDContainer : IIDItem
    {
        IIDItem this[string name]
        {
            get;
        }
        
        event RenamedHandler ItemRenamed;
    }

    public interface IIDContainer<out T> : IIDItem, IIDContainer where T : IIDItem
    {
        new T this[string name]
        {
            get;
        }        
    }

    public static class IDContainerExtensions
    {
        public static IIDItem GetIDItem(this IIDItem item, IEnumerable<string> relPath, List<string> donePath)
        {
            if (relPath.Count() == 0)
                return item;
            
            if (item is IIDContainer)
                return ((IIDContainer)item).GetIDItem(relPath, donePath);
                        
            throw new Exception(
                String.Format("could not find ID {0}. made it up to {1}, which is no container.",
                    donePath.Concat(relPath).Aggregate((acc, next) => acc + "/" + next),
                    donePath.Aggregate((acc, next) => acc + "/" + next)
                ));            
        }

        public static IIDItem GetIDItem(this IIDContainer container, IEnumerable<string> relPath, List<string> donePath)
        {
            if (relPath.Count() == 0)
                return container;

            string first = relPath.First();            

            var childItem = container[first];
            if (childItem != null)
            {
                donePath.Add(first);
                return childItem.GetIDItem(relPath.Skip(1), donePath);
            }

            throw new Exception(
                String.Format("no {0} found within {1}.",
                    first,
                    donePath.Aggregate((acc, next) => acc + "/" + next)
                ));
        }

        /// <summary>
        /// gets the ID Item in the model, starting from another ID item. a relpath of "" return the relToItem
        /// </summary>        
        public static IIDItem GetIDItem(this IIDItem relToItem, string relPath)
        {
            return relToItem.GetIDItem(relPath.Split('/'), new List<string>());
        }

        public static IIDItem GetIDItem(this IDItem container, string relPath)
        {
            return ((IIDItem)container).GetIDItem(relPath);
        }

        public static IIDItem GetIDItem(this IDContainer container, string relPath)
        {
            return ((IIDContainer)container).GetIDItem(relPath);
        }

    }
}
