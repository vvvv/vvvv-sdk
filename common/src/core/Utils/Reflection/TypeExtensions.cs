using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.Utils.Reflection
{
    public static class TypeExtensions
    {
        private static string GetName(Type t)
        {
            var ret = t.Name;

            if (t.IsArray)
            {
                ret = GetName(t.GetElementType()) + "[]";
            }
            else if(t.IsGenericType)
            {
                var elems = t.GetGenericArguments();
                var elemNames = new List<string>();

                foreach (var gt in elems)
                {
                    elemNames.Add(GetName(gt));
                }

                ret += "<" + String.Join(", ", elemNames.ToArray()) + ">";
            }

            return ret;
        }

        /// <summary>
        /// Creates the full name of a type, including generics 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string GetExpandedName(this Type t)
        {
            return GetName(t);
        }
    }
}
