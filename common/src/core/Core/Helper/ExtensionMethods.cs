using System;
using System.IO;
using System.Drawing;
using System.Xml.Linq;
using System.Collections;
using VVVV.Core.Serialization;
using VVVV.Core.Logging;

namespace VVVV.Core
{
    /// <summary>
    /// Provides extension methods for various .NET classes.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Returns the local directory name of the specified URI.
        /// For example 'file://c:/foo/bar/doc.txt' becomes 'C:\foo\bar'.
        /// </summary>
        public static string GetLocalDir(this Uri uri)
        {
            return Path.GetDirectoryName(uri.LocalPath);
        }
        
        /// <summary>
        /// Concatinates this string representation of a path with path2.
        /// </summary>
        public static string ConcatPath(this string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }

        /// <summary>
        /// Serializes the items in the list and adds the xml to the XElement
        /// </summary>
        public static void SerializeAndAddList(this XElement x, IEnumerable list, Serializer serializer)
        {
            foreach (var item in list)
            {
                x.Add(serializer.Serialize(item));
            }
        }


        /// <summary>
        /// Deserializes all childs of the XElement
        /// </summary>
        /// <typeparam name="T">The type to deserialize and add to the list</typeparam>
        public static void DeserializeList<T>(this XElement x, Serializer serializer)
        {
            foreach (var item in x.Elements())
            {
                serializer.Deserialize<T>(item);
            }
        }

        /// <summary>
        /// Deserializes the content oft the XElement and adds the objects to the list
        /// </summary>
        /// <typeparam name="T">The type to deserialize and add to the list</typeparam>
        public static void DeserializeAndAddToList<T>(this XElement x, IEditableCollection<T> list, Serializer serializer)
        {
            if(x != null)
                foreach (var item in x.Elements())
                {
                    DebugHelpers.CatchAndLog(() =>
                        {
                            list.Add(serializer.Deserialize<T>(item));
                        }, "deserializing an XElement", (e) =>
                            {
                            });
                }
        }

        /// <summary>
        /// Creates a XElement and adds the ID as name attribute
        /// </summary>
        /// <param name="tagName">The name of the XML tag to create</param>
        /// <returns></returns>
        public static XElement GetXML(this IIDItem item, string tagName)
        {
            var x = new XElement(tagName);
            x.Add(new XAttribute("Name", item.Name));
            return x;
        }
    }
}
