using System;
using System.Xml.Linq;

namespace VVVV.Core.Serialization
{
    public static class XElementExtensions
    {
        public static void AddTypeAttribute(this XElement element, Type type)
        {
            var attribute = new XAttribute("Type", type.AssemblyQualifiedName);
            element.Add(attribute);
        }
        
        public static void AddTypeAttribute<T>(this XElement element)
        {
            element.AddTypeAttribute(typeof(T));
        }
        
        public static Type GetAttributedType(this XElement element)
        {
            var attribute = element.Attribute("Type");
            if (attribute != null)
            {
                return Type.GetType(attribute.Value);
            }
            
            return null;
        }
    }
}
