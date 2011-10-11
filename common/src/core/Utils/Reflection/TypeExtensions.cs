using System;
using System.Collections.Generic;
//using Microsoft.FSharp.Reflection;

namespace VVVV.Utils.Reflection
{
    public static class TypeExtensions
    {

        /// <summary>
        /// Generates a CSharp like type name string.
        /// </summary>
        /// <param name="type">A .Net Type</param>
        /// <returns>Type name as one would write it in CSharp</returns>
        public static string GetCSharpName(this Type type)
        {
            return type.GetCSharpName(false);
        }
        
        /// <summary>
        /// Generates a CSharp like type name string.
        /// </summary>
        /// <param name="type">A .Net Type</param>
        /// <param name="includeNamespace">If true, the return string comes with namespace prefix.</param>
        /// <returns>Type name as one would write it in CSharp</returns>
        public static string GetCSharpName(this Type type, bool includeNamespace)
        {
            var typeName = type.FullName;
            
            if(!includeNamespace)
                typeName = typeName.Replace(type.Namespace + ".", ""); //Removing the namespace

            var provider = System.CodeDom.Compiler.CodeDomProvider.CreateProvider("CSharp"); //You can also use "VisualBasic"
            var reference = new System.CodeDom.CodeTypeReference(typeName);

            return provider.GetTypeOutput(reference);
        }

        public static string GetFSharpName(this Type t)
        {
            //simple type
            var ret = t.Name;

            if (t.IsArray)
            {
                //set array element type as name
                ret = GetFSharpName(t.GetElementType()) + "[]";
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
                    elemNames.Add(GetFSharpName(gt));
                }

                //join name with generic type names
                ret += "<" + String.Join(", ", elemNames.ToArray()) + ">";
            }

            return ret;
        }
    }
}
