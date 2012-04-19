using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace VVVV.Core.Service
{
    public class RecoverableInferenceException : Exception { };

    public class TypeComparer : IComparable<TypeComparer>
    {
        public Type Type { get; private set; }
        public TypeComparer(Type t)
        {
            Type = t;
        }

        public int CompareTo(TypeComparer other)
        {
            if (this == other)
                return 0;

            if (SmallerType(this, other) == this)
                return -1;
            else
                return 1;
        }

        //public static bool operator <=(TypeComparer a, TypeComparer b)
        //{
        //    return SmallerType(a, b) == a;
        //}

        //public static bool operator <(TypeComparer a, TypeComparer b)
        //{
        //    return (a != b) && (a <= b);
        //}

        public static TypeComparer SmallerType(TypeComparer a, TypeComparer b)
        {
            if (a.Type == b.Type)
                return a;

            if (!a.Type.ContainsGenericParameters && b.Type.ContainsGenericParameters)
                return a;
            if (!b.Type.ContainsGenericParameters && a.Type.ContainsGenericParameters)
                return b;
            if (!a.Type.ContainsGenericParameters && !b.Type.ContainsGenericParameters)
                if (a.Type.IsAssignableFrom(b.Type))
                    return b;
                else
                    if (b.Type.IsAssignableFrom(a.Type))
                        return a;
                    else
                        throw new RecoverableInferenceException();

            if (a.Type.IsGenericParameter)
                return b;
            else
                if (b.Type.IsGenericParameter)
                    return a;

            if (a.Type.CanBeMadeOf(b.Type))
                return a;
            else
                if (b.Type.CanBeMadeOf(a.Type))
                    return b;
                else
                    throw new RecoverableInferenceException();
        }
    }

    public class TypeConstraint // : IComparable<TypeConstraint>
    {
        public Type SubType { get; private set; }
        public Type SuperType { get; private set; }
        public object UserData { get; private set; }
        public bool ExactMatch { get; private set; }

        public TypeConstraint(Type subtype, Type supertype, bool exactmatch, object userdata)
        {
            SubType = subtype;
            SuperType = supertype;
            UserData = userdata;
            ExactMatch = exactmatch;
        }

        public TypeConstraint(Type subtype, Type supertype, object userdata)
            : this(subtype, supertype, false, userdata) { }

        public TypeConstraint(Type subtype, Type supertype, bool exactmatch)
            : this(subtype, supertype, exactmatch, null) { }

        public TypeConstraint(Type subtype, Type supertype)
            : this(subtype, supertype, false, null) { }

        //public int CompareTo(TypeConstraint other)
        //{
        //    if (this==other) 
        //        return 0;

        //    if (SmallerType(this, other) == this)
        //        return -1;
        //    else
        //        return 1;
        //}

        //public static bool operator <=(TypeConstraint a, TypeConstraint b)
        //{
        //    return SmallerType(a, b) == a;
        //}

        //public static bool operator <(TypeConstraint a, TypeConstraint b)
        //{
        //    return (a != b) && (a <= b);
        //}

        //public static TypeConstraint SmallerType(TypeConstraint a, TypeConstraint b)
        //{
        //    if (a.SubType != b.SubType || !a.SubType.IsGenericParameter)
        //        throw new NotImplementedException();

        //    if (a.SuperType == b.SuperType)
        //        return a;

        //    if (!a.SuperType.ContainsGenericParameters && b.SuperType.ContainsGenericParameters)
        //        return a;
        //    if (!b.SuperType.ContainsGenericParameters && a.SuperType.ContainsGenericParameters)
        //        return b;
        //    if (!a.SuperType.ContainsGenericParameters && !b.SuperType.ContainsGenericParameters)
        //        if (a.SuperType.IsAssignableFrom(b.SuperType))
        //            return b;
        //        else
        //            if (b.SuperType.IsAssignableFrom(a.SuperType))
        //                return a;
        //            else
        //                throw new RecoverableInferenceException();

        //    if (a.SuperType.IsGenericParameter)
        //        return b;
        //    else
        //        if (b.SuperType.IsGenericParameter)
        //            return a;

        //    if (a.SuperType.CanBeMadeOf(b.SuperType))
        //        return a;
        //    else
        //        if (b.SuperType.CanBeMadeOf(a.SuperType))
        //            return b;
        //        else
        //            throw new RecoverableInferenceException();
        //}
    }

    public static class TypeExtensions
    {
        //public static bool CanBeMadeOf(this Type t, Type td, out Type[] typeParamsToMakeTfromTD)
        //{
        //    var typeParams = new List<Type>();
        //    var can = CanBeMadeOf(t, td, typeParams);
        //    typeParamsToMakeTfromTD = typeParams.ToArray();
        //    return can;
        //}

        public static bool CanBeMadeOf(this Type t, Type td, out Tuple<Type, Type>[] mappings)
        {
            var typeParams = new List<Tuple<Type, Type>>();
            var can = CanBeMadeOf(t, td, typeParams);
            mappings = typeParams.ToArray();
            return can;
        }

        public static bool CanBeMadeOf(this Type t, Type td)
        {
            var typeParams = new List<Tuple<Type, Type>>();
            var can = CanBeMadeOf(t, td, typeParams);
            return can;
        }

        private static bool CanBeMadeOf(this Type t, Type td, List<Tuple<Type, Type>> mappings)
        {
            if (td.IsGenericParameter)
            {
                mappings.Add(new Tuple<Type, Type>(td, t));
                return true;
            }

            if (!t.IsGenericType)
                return t == td;

            // If FullName != null then the type description is a well formed .NET type
            if (td.FullName != null)
            {
                // to be able to make t of td, t's type definition must be td
                if (t.GetGenericTypeDefinition() == td)
                {
                    var a = t.GetGenericArguments();
                    var p = td.GetGenericArguments();

                    for (var i = 0; i < a.Length; i++)
                    {
                        mappings.Add(new Tuple<Type,Type>(p[i], a[i]));
                    }

                    return true;
                }
                return false;
            }
            else
            // no well formed .NET type definition. Here we do the magic tricks
            {
                var a = t.GetGenericArguments(); // t's arguments
                var p = td.GetGenericArguments(); //td's parameters

                if (a.Count() == p.Count())
                {
                    for (int i = 0; i < a.Length; i++)
                    {
                        // if a[i] can be made of p[i] typeParams are added (if needed)
                        if (!CanBeMadeOf(a[i], p[i], mappings))
                        {
                            mappings.Clear();
                            return false;
                        }
                    }
                    return true;
                }
                else
                    return false;
            }
        }

        //private static bool CanBeMadeOf(this Type t, Type td, List<Type> typeParamsToMakeTfromTD)
        //{
        //    if (td.IsGenericParameter)
        //    {
        //        typeParamsToMakeTfromTD.Add(t);
        //        return true;
        //    }

        //    if (!t.IsGenericType)
        //        return t == td;

        //    // If FullName != null then the type description is a well formed .NET type
        //    if (td.FullName != null)
        //    {
        //        // to be able to make t of td, t's type definition must be td
        //        if (t.GetGenericTypeDefinition() == td)
        //        {
        //            typeParamsToMakeTfromTD.AddRange(t.GetGenericArguments());
        //            return true;
        //        }
        //        return false;
        //    }
        //    else
        //    // no well formed .NET type definition. Here we do the magic tricks
        //    {
        //        var a = t.GetGenericArguments(); // t's arguments
        //        var p = td.GetGenericArguments(); //td's parameters

        //        if (a.Count() == p.Count())
        //        {
        //            for (int i = 0; i < a.Count(); i++)
        //            {
        //                // if a[i] can be made of p[i] typeParams are added (if needed)
        //                if (!CanBeMadeOf(a[i], p[i], typeParamsToMakeTfromTD))
        //                {
        //                    typeParamsToMakeTfromTD.Clear();
        //                    return false;
        //                }
        //            }
        //            return true;
        //        }
        //        else
        //            return false;
        //    }
        //}

        private static Type CloseByParameterization(this Type td, List<Type> typeParamsToMakeTfromTD)
        {
            if (td.IsGenericParameter)
            {
                var t = typeParamsToMakeTfromTD[0];
                typeParamsToMakeTfromTD.RemoveAt(0);

                return t;
            }
            
            if (!td.ContainsGenericParameters)
                return td;

            // If FullName != null then the type description is a well formed .NET type
            if (td.FullName != null)
            {
                var p = td.GetGenericArguments();
                var a = typeParamsToMakeTfromTD.GetRange(0, p.Count());
                typeParamsToMakeTfromTD.RemoveRange(0, p.Count());
                return td.MakeGenericType(a.ToArray());
            }
            else
            // no well formed .NET type definition. Here we do the magic tricks
            {
                var p = td.GetGenericArguments(); //td's parameters
                Type[] a = new Type[p.Count()];

                for (int i = 0; i < p.Count(); i++)
                    a[i] = CloseByParameterization(p[i], typeParamsToMakeTfromTD);

                return td.GetGenericTypeDefinition().MakeGenericType(a);
            }
        }

        public static Type CloseByParameterization(this Type td, Type[] typeParamsToMakeTfromTD)
        {
            var list = typeParamsToMakeTfromTD.ToList();
            return CloseByParameterization(td, list);
        }
        
        public static Type CloseBySubstitution(this Type td, Type from, Type to)
        {
            if (td == from)
                return to;

            if (!td.ContainsGenericParameters)
                return td;

            if (!td.IsGenericParameter)
            {
                var p = td.GetGenericArguments(); //td's parameters
                Type[] a = new Type[p.Count()];

                if (p.Count() > 0)
                {
                    for (int i = 0; i < p.Count(); i++)
                        a[i] = CloseBySubstitution(p[i], from, to);

                    return td.GetGenericTypeDefinition().MakeGenericType(a);
                }
                else
                    return td;
            }
            else
            {
                if (from.IsGenericParameter && td.Name == from.Name)
                    return to;
                else
                    return td;
            }
        }

        public static Type CloseBySubstitution(this Type td, Tuple<Type, Type>[] mappings)
        {
            var t = td;
            foreach (var m in mappings)
                td = td.CloseBySubstitution(m.Item1, m.Item2);
            return td;
        }

        private static U AddAndReturn<T,U>(this Dictionary<T,U> dict, T t, U u)
        {
            dict.Add(t, u);
            return u;
        }

        public static Type CloseBySubstitution(this Type td, Dictionary<Type, Type> mappings)
        {
            if (mappings.Keys.Contains(td))
                return mappings[td];

            if (!td.ContainsGenericParameters)
                return mappings.AddAndReturn(td, td);

            if (!td.IsGenericParameter)
            {
                var p = td.GetGenericArguments(); //td's parameters
                Type[] a = new Type[p.Count()];

                if (p.Count() > 0)
                {
                    for (int i = 0; i < p.Count(); i++)
                        a[i] = CloseBySubstitution(p[i], mappings);

                    return mappings.AddAndReturn(td, td.GetGenericTypeDefinition().MakeGenericType(a));
                }
                else
                    return mappings.AddAndReturn(td, td);
            }
            else
                return mappings.AddAndReturn(td, td);
        }

        public static IEnumerable<TypeConstraint> Expand(this TypeConstraint constraint)
        {
            if (constraint.SubType.IsGenericType && constraint.SuperType.IsGenericType)
            {
                var subtypedef = constraint.SubType.GetGenericTypeDefinition();
                var supertypedef = constraint.SuperType.GetGenericTypeDefinition();

                if (subtypedef == supertypedef)
                {
                    // same type mask, now we need to check parameters and unify them
                    var p = supertypedef.GetGenericArguments();
                    var subargs = constraint.SubType.GetGenericArguments();
                    var superargs = constraint.SuperType.GetGenericArguments();

                    for (int i = 0; i < p.Length; i++)
                    {
                        var attr = p[i].GenericParameterAttributes;
                        if (!constraint.ExactMatch && (attr & GenericParameterAttributes.Covariant) != 0)
                            foreach (var c in new TypeConstraint(subargs[i], superargs[i]).Expand()) yield return c;
                        else
                            if (!constraint.ExactMatch && (attr & GenericParameterAttributes.Contravariant) != 0)
                                foreach (var c in new TypeConstraint(superargs[i], subargs[i]).Expand()) yield return c;
                            else
                                foreach (var c in new TypeConstraint(subargs[i], superargs[i], true).Expand()) yield return c;
                    }
                }
                else
                    throw new RecoverableInferenceException(); //("can't unify generic types " + subtypedef + " and " + supertypedef);
            }
            else
                yield return constraint;
        }

        public static IEnumerable<TypeConstraint> ExpandAll(this IEnumerable<TypeConstraint> cs)
        {
            foreach (var c in cs)
                foreach (var c2 in c.Expand())
                    yield return c2;
        }

        public static System.Tuple<Type, Type>[] InfereTypeMappingsFrom(this Type td, Type t)
        {
            Tuple<Type, Type>[] mappings;
            CanBeMadeOf(t, td, out mappings);
            return mappings;

            //if (t.FullName == null) 
            //{
            
            //Type[] types;

            //if (t.CanBeMadeOf(td, out types))
            //{
            //    var args = td.GetGenericArguments();                
            //    var tuples = new List<Tuple<Type,Type>>();

            //    for (int i = 0; i < args.Count(); i++)
            //        tuples.Add(new Tuple<Type, Type>(args[i], types[i]));

            //    tuples.Add(new Tuple<Type, Type>(td, t));

            //    return tuples.ToArray();
            //}
            //else
            //    return new Tuple<Type, Type>[0];
        }




        
        ///// <summary>
        ///// a number meassuring the distance to the specified base type
        ///// 
        ///// if the types are not related to each other the distance is uint.MaxValue
        ///// if the types are identical the distance is 0
        ///// 
        ///// if the type is inherited from the type Base, then each level of derivation adds 1 to the output
        ///// 
        ///// if a type in the class hierarchy implements an interface of type Base
        ///// the result will be t.DistanceToBase(object) + 1 + t.DistancetoBase(t2) where t2 implements Base
        /////      
        ///// distance gets bigger when Base Type is a GenericTypeDefinition 
        ///// 
        ///// </summary>
        ///// <param name="A"></param>
        ///// <param name="Base"></param>
        ///// <returns></returns>
        //public static uint DistanceToBase(this Type A, Type Base)
        //{
        //    return uint.MaxValue;
        //}

        //public static uint DistanceToBase<TBase>(this Type A)
        //{
        //    return A.DistanceToBase(typeof(TBase));
        //}
    }
}
