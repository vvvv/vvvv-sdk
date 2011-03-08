using System;
using System.Collections.Generic;
//using Microsoft.FSharp.Reflection;

namespace VVVV.Utils.Reflection
{
    public static class TypeExtensions
    {
        private static string GetName(Type t)
        {
            //simple type
            var ret = t.Name;

            if (t.IsArray)
            {
                //set array element type as name
                ret = GetName(t.GetElementType()) + "[]";
            }
            //else if (FSharpType.IsTuple(t))
            //{
            //    var elems = FSharpType.GetTupleElements(t);
            //    var elemNames = new List<string>();

            //    foreach (var tt in elems)
            //    {
            //        elemNames.Add(GetName(tt));
            //    }

            //    ret += "(" + String.Join(", ", elemNames.ToArray()) + ")";
            //}
            else if (t.IsGenericType) //tuples are generics too
            {
                var elems = t.GetGenericArguments();
                var elemNames = new List<string>();

                //get generic type names
                foreach (var gt in elems)
                {
                    elemNames.Add(GetName(gt));
                }

                //join name with generic type names
                ret += "<" + String.Join(", ", elemNames.ToArray()) + ">";
            }

            return ret;
        }

        /// <summary>
        /// Builds the complete name of a type, including generics, arrays and tuples
        /// </summary>
        /// <param name="t">A type</param>
        /// <returns>Full name of a type</returns>
        public static string GetExpandedName(this Type t)
        {
            return GetName(t);
        }
    }
}
