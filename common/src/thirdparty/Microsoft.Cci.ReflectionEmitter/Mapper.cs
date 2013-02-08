//-----------------------------------------------------------------------------
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the Microsoft Public License.
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//-----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Cci.UtilityDataStructures;
using System.Diagnostics;

namespace Microsoft.Cci.ReflectionEmitter {
    /// <summary>
    /// An object that provides methods to map CCI metadata references to corresponding System.Type and System.Reflection.* objects.
    /// The object maintains a cache of mappings and should typically be used for doing many mappings.
    /// </summary>
    public class ReflectionMapper
    {

        /// <summary>
        /// An object that provides methods to map CCI metadata references to corresponding System.Type and System.Reflection.* objects.
        /// The object maintains a cache of mappings and should typically be used for doing many mappings.
        /// </summary>
        public ReflectionMapper(IInternFactory internFactory)
        {
            this.mappingVisitor = new MappingVisitorForTypes(this);
            this.internFactory = internFactory;
        }

        Dictionary<AssemblyIdentity, Assembly> assemblyMap = new Dictionary<AssemblyIdentity, Assembly>(16);
        Dictionary<IModule, Module> moduleMap = new Dictionary<IModule, Module>(16);
        DoubleHashtable<FieldInfo> fieldMap = new DoubleHashtable<FieldInfo>(2048 * 4); //TODO: use a hashtable and the newly defined InternKey
        Hashtable<MethodBase> methodMap = new Hashtable<MethodBase>(2048 * 8);
        DoubleHashtable<MemberInfo[]> membersMap = new DoubleHashtable<MemberInfo[]>(2048 * 8);
        Dictionary<uint, Type> typeMap = new Dictionary<uint, Type>();
        MappingVisitorForTypes mappingVisitor;
        IInternFactory internFactory;
        Dictionary<uint, ITypeReference> reverseTypeMap = new Dictionary<uint, ITypeReference>();
        List<ITypeReference> temporaryTypeMappings = new List<ITypeReference>();

        /// <summary>
        /// Returns a "live" System.Reflection.Assembly instance that provides reflective access to the referenced assembly.
        /// If the assembly cannot be found or cannot be loaded, the result is null.
        /// </summary>
        public Assembly/*?*/ GetAssembly(IAssemblyReference/*?*/ assemblyReference)
        {
            if (assemblyReference == null) return null;
            var ident = assemblyReference.AssemblyIdentity;
            Assembly result = null;
            if (!this.assemblyMap.TryGetValue(ident, out result))
            {
                var name = new System.Reflection.AssemblyName();
                if (!String.IsNullOrEmpty(ident.Location))
                    name.CodeBase = ident.Location;
                name.CultureInfo = new System.Globalization.CultureInfo(ident.Culture);
                name.Name = ident.Name.Value;
                name.SetPublicKeyToken(new List<byte>(ident.PublicKeyToken).ToArray());
                name.Version = ident.Version;
                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var loadedAssem in loadedAssemblies)
                {
                    if (System.Reflection.AssemblyName.ReferenceMatchesDefinition(name, loadedAssem.GetName()))
                    {
                        result = loadedAssem;
                        break;
                    }
                }
                if (result == null)
                {
                    try
                    {
                        result = Assembly.Load(name);
                    }
                    catch (System.UriFormatException)
                    {
                    }
                    catch (System.IO.FileNotFoundException)
                    {
                    }
                    catch (System.IO.FileLoadException)
                    {
                    }
                    catch (System.BadImageFormatException)
                    {
                    }
                    this.assemblyMap.Add(ident, result);
                }
            }
            return result;
        }

        /// <summary>
        /// Returns a "live" System.Reflection.Module instance that provides reflective access to the referenced module.
        /// If the module cannot be found or cannot be loaded, the result is null.
        /// </summary>
        public Module/*?*/ GetModule(IModuleReference/*?*/ moduleReference)
        {
            if (moduleReference == null) throw new ReflectionMapperException();
            if (moduleReference.ContainingAssembly == null) throw new ReflectionMapperException();
            var assembly = this.GetAssembly(moduleReference.ContainingAssembly);
            if (assembly == null) throw new ReflectionMapperException();
            return assembly.GetModule(moduleReference.Name.Value);
        }

        /// <summary>
        /// Returns a "live" System.Reflection.FieldInfo object that provides reflective access to the referenced field.
        /// If the field cannot be found or cannot be loaded, the result is null.
        /// </summary>
        public FieldInfo/*?*/ GetField(IFieldReference/*?*/ fieldReference)
        {
            if (fieldReference == null) throw new ReflectionMapperException();
            var containingType = fieldReference.ContainingType;
            var result = this.fieldMap.Find(containingType.InternedKey, (uint)fieldReference.Name.UniqueKey);
            if (result == null)
            {
                try
                {
                    var reflectionType = this.GetType(containingType);
                    if (reflectionType.GetType().Name.Contains("TypeBuilder"))
                        result = this.GetFieldFromTypeBuilder(fieldReference);
                    else
                    {
                        var members = this.GetMembers(containingType, fieldReference.Name);
                        result = this.GetFieldFrom(fieldReference, members);
                    }
                }
                catch (NotSupportedException)
                {
                    result = this.GetFieldFromTypeBuilder(fieldReference);
                }
                if (result == null) throw new ReflectionMapperException();
                this.fieldMap.Add(containingType.InternedKey, (uint)fieldReference.Name.UniqueKey, result);
            }
            return result;
        }

        private FieldInfo GetFieldFrom(IFieldReference fieldReference, MemberInfo[] members)
        {
            var fieldType = this.GetType(fieldReference.Type);
            foreach (var member in members)
            {
                var field = (FieldInfo)member as FieldInfo;
                if (field == null) continue;
                if (field.FieldType != fieldType) continue;
                if (fieldReference.IsModified)
                {
                    if (!this.ModifiersMatch(field.GetOptionalCustomModifiers(), field.GetRequiredCustomModifiers(), fieldReference.CustomModifiers)) continue;
                }
                return field;
            }
            return null;
        }

        private MemberInfo[] GetMembers(ITypeReference typeReference, IName name)
        {
            var members = this.membersMap.Find(typeReference.InternedKey, (uint)name.UniqueKey);
            if (members == null)
            {
                var type = this.GetType(typeReference);
                var bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
                members = type.GetMember(name.Value, bindingAttr);
                this.membersMap.Add(typeReference.InternedKey, (uint)name.UniqueKey, members);
            }
            return members;
        }

        private FieldInfo GetFieldFromTypeBuilder(IFieldReference fieldReference)
        {
            var containingType = this.GetType(fieldReference.ContainingType);
            var templateFieldReference = this.GetTemplateField(fieldReference);
            var templateFieldInfo = this.GetField(templateFieldReference);
            return TypeBuilder.GetField(containingType, templateFieldInfo);
        }

        private MethodBase GetMethodFromTypeBuilder(IMethodReference methodReference)
        {
            var containingType = this.GetType(methodReference.ContainingType);
            var templateMethodReference = GetTemplateMethod(methodReference);
            var templateMethodInfo = this.GetMethod(templateMethodReference);
            if (templateMethodReference.ResolvedMethod.IsConstructor)
                return TypeBuilder.GetConstructor(containingType, (ConstructorInfo)templateMethodInfo);
            else
                return TypeBuilder.GetMethod(containingType, (MethodInfo)templateMethodInfo);
        }

        private ITypeDefinition GetTemplateType(ITypeReference typeReference)
        {
            var genericTypeInstanceReference = typeReference as IGenericTypeInstanceReference;
            if (genericTypeInstanceReference != null)
            {
                return GetTemplateType(genericTypeInstanceReference.GenericType);
            }
            var specializedNestedTypeReference = typeReference as ISpecializedNestedTypeReference;
            if (specializedNestedTypeReference != null)
            {
                return GetTemplateType(specializedNestedTypeReference.UnspecializedVersion);
            }
            var resolvedType = typeReference.ResolvedType;
            if (resolvedType is Dummy)
            {
                resolvedType = (ITypeDefinition)this.reverseTypeMap[typeReference.InternedKey];
            }
            return resolvedType;
        }

        private IMethodReference GetTemplateMethod(IMethodReference methodReference)
        {
            var specializedMethodReference = methodReference as ISpecializedMethodReference;
            if (specializedMethodReference != null)
                return specializedMethodReference.UnspecializedVersion;
            var specializedMethodDefinition = methodReference.ResolvedMethod as ISpecializedMethodDefinition;
            if (specializedMethodDefinition != null)
                return GetTemplateMethod(specializedMethodDefinition);
            var resolvedMethod = methodReference.ResolvedMethod;
            if (resolvedMethod is Dummy)
            {
                // One case where this happens is if the method is from a private helper type.
                // Look for it manually.
                var templateType = GetTemplateType(methodReference.ContainingType);
                foreach (var member in templateType.Members.Concat(templateType.PrivateHelperMembers))
                {
                    if (member.Name != methodReference.Name) continue;
                    var privateHelperMethod = member as IMethodDefinition;
                    if (privateHelperMethod != null)
                        return privateHelperMethod;
                }
            }
            throw new NotImplementedException();
        }

        private IFieldReference GetTemplateField(IFieldReference fieldReference)
        {
            var containingType = this.GetType(fieldReference.ContainingType);
            var specializedFieldReference = fieldReference as ISpecializedFieldReference;
            if (specializedFieldReference != null)
                return specializedFieldReference.UnspecializedVersion;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a "live" System.Reflection.MethodBase object that provides reflective access to the referenced method.
        /// If the method cannot be found or cannot be loaded, the result is null.
        /// </summary>
        public MethodBase/*?*/ GetMethod(IMethodReference/*?*/ methodReference)
        {
            if (methodReference == null) return null;
            var genericMethodInstanceReference = methodReference as IGenericMethodInstanceReference;
            if (genericMethodInstanceReference != null) return this.GetGenericMethodInstance(genericMethodInstanceReference);
            MethodBase result = this.methodMap.Find(methodReference.InternedKey);
            if (result == null)
            {
                var containingType = methodReference.ContainingType;
                try
                {
                    var reflectionType = this.GetType(containingType);
                    if (reflectionType.GetType().Name.Contains("TypeBuilder"))
                        result = this.GetMethodFromTypeBuilder(methodReference);
                    else
                    {
                        var members = this.GetMembers(containingType, methodReference.Name);
                        result = this.GetMethodFrom(methodReference, members);
                    }
                }
                catch (NotSupportedException)
                {
                    result = this.GetMethodFromTypeBuilder(methodReference);
                }
                if (result == null) throw new ReflectionMapperException();
                this.methodMap.Add(methodReference.InternedKey, result);
            }
            return result;
        }

        private MethodBase/*?*/ GetMethodFrom(IMethodReference methodReference, MemberInfo[] members, bool skipTypeCheck = false)
        {
            //Generic methods need special treatment because their signatures refer to their generic parameters
            //and we can't map those to types unless we can first map the generic method.
            if (methodReference.IsGeneric) return this.GetGenericMethodFrom(methodReference, members);
            var methodReturnType = this.GetType(methodReference.Type);
            if (methodReturnType == null) return null;
            if (methodReference.ReturnValueIsByRef) methodReturnType = methodReturnType.MakeByRefType();
            var parameterCount = methodReference.ParameterCount;
            var parameters = new IParameterTypeInformation[parameterCount];
            var parameterTypes = parameterCount == 0 ? null : new Type[parameterCount];
            int parameterIndex = 0;
            foreach (var parameter in methodReference.Parameters)
            {
                parameters[parameterIndex] = parameter;
                var ptype = this.GetType(parameter.Type);
                if (ptype == null) return null;
                if (parameter.IsByReference) ptype = ptype.MakeByRefType();
                parameterTypes[parameterIndex++] = ptype;
            }
            foreach (var member in members)
            {
                var methodBase = member as MethodBase;
                if (methodBase == null || methodBase.IsGenericMethodDefinition) continue;
                if (!this.CallingConventionsMatch(methodBase, methodReference)) continue;
                if (methodBase.IsConstructor)
                {
                    if (methodReference.Type.TypeCode != PrimitiveTypeCode.Void) continue;
                }
                else
                {
                    var method = (MethodInfo)methodBase;
                    if (!skipTypeCheck && (methodReturnType != method.ReturnType)) continue;
                    if (methodReference.ReturnValueIsModified)
                    {
                        if (!this.ModifiersMatch(method.ReturnParameter.GetOptionalCustomModifiers(), method.ReturnParameter.GetRequiredCustomModifiers(), methodReference.ReturnValueCustomModifiers)) continue;
                    }
                }
                var memberParameterInfos = methodBase.GetParameters();
                if (parameterCount != memberParameterInfos.Length) continue;
                bool matched = true;
                for (int i = 0; i < parameterCount; i++)
                {
                    var mparInfo = memberParameterInfos[i];
                    var ipar = parameters[i];
                    var part = parameterTypes[i];
                    if (!skipTypeCheck && (mparInfo.ParameterType != part)) { matched = false; break; }
                    if (ipar.IsModified)
                    {
                        if (!this.ModifiersMatch(mparInfo.GetOptionalCustomModifiers(), mparInfo.GetRequiredCustomModifiers(), ipar.CustomModifiers)) continue;
                    }
                }
                if (!matched) continue;
                return methodBase;
            }
            return null;
        }

        private bool CallingConventionsMatch(MethodBase methodBase, IMethodReference methodReference)
        {
            if (methodBase.IsStatic && !methodReference.IsStatic) return false;
            switch (methodBase.CallingConvention & (CallingConventions)3)
            {
                case CallingConventions.Any:
                    if ((methodReference.CallingConvention & (CallingConvention)7) != CallingConvention.Default &&
                        (methodReference.CallingConvention & (CallingConvention)7) != CallingConvention.ExtraArguments)
                        return false;
                    break;
                case CallingConventions.Standard:
                    if ((methodReference.CallingConvention & (CallingConvention)7) != CallingConvention.Default) return false;
                    break;
                case CallingConventions.VarArgs:
                    if ((methodReference.CallingConvention & (CallingConvention)7) != CallingConvention.ExtraArguments) return false;
                    break;
            }
            if ((methodBase.CallingConvention & CallingConventions.HasThis) != 0 &&
                (methodReference.CallingConvention & CallingConvention.HasThis) == 0) return false;
            if ((methodBase.CallingConvention & CallingConventions.ExplicitThis) != 0 &&
                (methodReference.CallingConvention & CallingConvention.ExplicitThis) == 0) return false;
            return true;
        }

        private MethodInfo GetGenericMethodFrom(IMethodReference methodReference, MemberInfo[] members)
        {
            MethodInfo result = null;
            var parameterCount = methodReference.ParameterCount;
            var parameters = new IParameterTypeInformation[parameterCount];
            int i = 0; foreach (var par in methodReference.Parameters) parameters[i++] = par;
            var referencedMethodIsStatic = methodReference.IsStatic;
            foreach (var member in members)
            {
                var method = member as MethodInfo;
                if (method == null || !method.IsGenericMethodDefinition) continue;
                if (methodReference.GenericParameterCount != method.GetGenericArguments().Length) continue;
                if (!this.CallingConventionsMatch(method, methodReference)) continue;
                var mrtype = method.ReturnType;
                if (methodReference.ReturnValueIsByRef) mrtype = mrtype.GetElementType();
                if (!this.TypesMatch(methodReference.Type, mrtype, method)) continue;
                if (methodReference.ReturnValueIsModified)
                {
                    if (!this.ModifiersMatch(method.ReturnParameter.GetOptionalCustomModifiers(), method.ReturnParameter.GetRequiredCustomModifiers(), methodReference.ReturnValueCustomModifiers)) continue;
                }
                var memberParameterInfos = method.GetParameters();
                if (parameterCount != memberParameterInfos.Length) continue;
                bool matched = true;
                for (i = 0; i < parameterCount; i++)
                {
                    var mparInfo = memberParameterInfos[i];
                    var mparType = mparInfo.ParameterType;
                    var ipar = parameters[i];
                    if (ipar.IsByReference) mparType = mparType.GetElementType();
                    if (!this.TypesMatch(ipar.Type, mparType, method)) { matched = false; break; }
                    if (ipar.IsModified)
                    {
                        if (!this.ModifiersMatch(mparInfo.GetOptionalCustomModifiers(), mparInfo.GetRequiredCustomModifiers(), ipar.CustomModifiers)) continue;
                    }
                }
                if (!matched) continue;
                result = method;
                break;
            }
            if (result != null)
                this.methodMap.Add(methodReference.InternedKey, result);
            return result;
        }

        private bool TypesMatch(ITypeReference typeReference, Type/*?*/ type, MethodInfo genericMethod)
        {
            if (type == null) return false;
            var arrayType = typeReference as IArrayTypeReference;
            if (arrayType != null) return this.TypesMatch(arrayType.ElementType, type.GetElementType(), genericMethod);
            var managedPointerType = typeReference as IManagedPointerTypeReference;
            if (managedPointerType != null) return this.TypesMatch(managedPointerType.TargetType, type.GetElementType(), genericMethod);
            var pointerType = typeReference as IPointerTypeReference;
            if (pointerType != null) return this.TypesMatch(pointerType.TargetType, type.GetElementType(), genericMethod);
            var modifiedType = typeReference as IModifiedTypeReference;
            if (modifiedType != null) return this.TypesMatch(modifiedType.UnmodifiedType, type, genericMethod);
            var genericMethodParameterReference = typeReference as IGenericMethodParameterReference;
            if (genericMethodParameterReference != null)
            {
                var genericMethodParameters = genericMethod.GetGenericArguments();
                if (genericMethodParameterReference.Index >= genericMethodParameters.Length) return false;
                var genPar = genericMethodParameters[genericMethodParameterReference.Index];
                return genPar == type;
            }
            var genericTypeInstance = typeReference as IGenericTypeInstanceReference;
            if (genericTypeInstance != null)
            {
                if (!type.IsGenericType) return false;
                if (!this.TypesMatch(genericTypeInstance.GenericType, type.GetGenericTypeDefinition(), genericMethod)) return false;
                var genericArguments = type.GetGenericArguments();
                if (genericArguments == null || genericArguments.Length != genericTypeInstance.GenericType.GenericParameterCount) return false;
                int i = 0;
                foreach (var genarg in genericTypeInstance.GenericArguments)
                {
                    if (!this.TypesMatch(genarg, genericArguments[i++], genericMethod)) return false;
                }
                return true;
            }
            return this.GetType(typeReference) == type;
        }

        private MethodBase GetGenericMethodInstance(IGenericMethodInstanceReference genericMethodInstanceReference)
        {
            var genericMethodReference = genericMethodInstanceReference.GenericMethod;
            var genericMethod = (MethodInfo)this.GetMethod(genericMethodReference);
            var typeArguments = new Type[genericMethodReference.GenericParameterCount];
            var i = 0; foreach (var arg in genericMethodInstanceReference.GenericArguments) typeArguments[i++] = this.GetType(arg);
            var genericMethodInstance = genericMethod.MakeGenericMethod(typeArguments);
            this.methodMap.Add(genericMethodInstanceReference.InternedKey, genericMethodInstance);
            return genericMethodInstance;
        }

        private bool ModifiersMatch(Type[] optional, Type[] required, IEnumerable<ICustomModifier> modifiers)
        {
            int optionalCount = 0;
            int requiredCount = 0;
            int modifierCount = 0;
            foreach (var modifier in modifiers)
            {
                var modifierType = this.GetType(modifier.Modifier);
                if (modifierType == null) return false;
                if (modifier.IsOptional)
                {
                    if (optionalCount == optional.Length) return false;
                    if (optional[optionalCount++] != modifierType) return false;
                }
                else
                {
                    if (requiredCount == required.Length) return false;
                    if (required[requiredCount++] != modifierType) return false;
                }
                modifierCount++;
            }
            return modifierCount == optional.Length + required.Length;
        }

        /// <summary>
        /// Returns a "live" System.Type object that provides reflective access to the referenced typeBuilder.
        /// If the typeBuilder cannot be found or cannot be loaded, the result is null.
        /// </summary>
        public Type/*?*/ GetType(ITypeReference/*?*/ type)
        {
            if (type == null) return null;
            Type result = null;
            if (!this.typeMap.TryGetValue(type.InternedKey, out result))
            {
                type.DispatchAsReference(this.mappingVisitor);
                result = this.mappingVisitor.result;
                if (result == null) throw new ReflectionMapperException();
                this.DefineMapping(type, result);
                this.temporaryTypeMappings.Add(type);
            }
            return result;
        }


        internal void DefineMapping(ITypeReference typeReference, Type type)
        {
            this.typeMap[typeReference.InternedKey] = type;
            this.reverseTypeMap[typeReference.InternedKey] = typeReference;
        }

        internal void ClearMapping(ITypeReference typeReference)
        {
            this.typeMap.Remove(typeReference.InternedKey);
            this.reverseTypeMap.Remove(typeReference.InternedKey);
        }

        internal void DefineMapping(IFieldDefinition fieldDefinition, FieldInfo fieldBuilder)
        {
            this.fieldMap.Add(fieldDefinition.ContainingType.InternedKey, (uint)fieldDefinition.Name.UniqueKey, fieldBuilder);
        }

        internal void DefineMapping(IMethodDefinition method, MethodBase methodBuilder)
        {
            this.methodMap.Add(method.InternedKey, methodBuilder);
        }

        internal void ClearTemporaryMappings()
        {
            this.fieldMap = new DoubleHashtable<FieldInfo>();
            this.methodMap.Clear();
            foreach (var t in temporaryTypeMappings)
                this.typeMap.Remove(t.InternedKey);
            temporaryTypeMappings.Clear();
        }

        internal MethodInfo GetArrayAddrMethod(IArrayTypeReference arrayTypeReference, ModuleBuilder moduleBuilder)
        {
            var type = this.GetType(arrayTypeReference);
            var parameterTypes = new Type[arrayTypeReference.Rank];
            for (int i = 0; i < arrayTypeReference.Rank; i++) parameterTypes[i] = typeof(int);
            return moduleBuilder.GetArrayMethod(type, "Address", CallingConventions.HasThis, type.GetElementType().MakeByRefType(), parameterTypes);
        }

        internal MethodInfo GetArrayGetMethod(IArrayTypeReference arrayTypeReference, ModuleBuilder moduleBuilder)
        {
            var type = this.GetType(arrayTypeReference);
            var parameterTypes = new Type[arrayTypeReference.Rank];
            for (int i = 0; i < arrayTypeReference.Rank; i++) parameterTypes[i] = typeof(int);
            return moduleBuilder.GetArrayMethod(type, "Get", CallingConventions.HasThis, type.GetElementType(), parameterTypes);
        }

        internal MethodInfo GetArraySetMethod(IArrayTypeReference arrayTypeReference, ModuleBuilder moduleBuilder)
        {
            var type = this.GetType(arrayTypeReference);
            var parameterTypes = new Type[arrayTypeReference.Rank + 1];
            for (int i = 0; i < arrayTypeReference.Rank; i++) parameterTypes[i] = typeof(int);
            parameterTypes[arrayTypeReference.Rank] = type.GetElementType();
            return moduleBuilder.GetArrayMethod(type, "Set", CallingConventions.HasThis, typeof(void), parameterTypes);
        }

        internal MethodInfo GetArrayCreateMethod(IArrayTypeReference arrayTypeReference, ModuleBuilder moduleBuilder)
        {
            var type = this.GetType(arrayTypeReference);
            var parameterTypes = new Type[arrayTypeReference.Rank];
            for (int i = 0; i < arrayTypeReference.Rank; i++) parameterTypes[i] = typeof(int);
            return moduleBuilder.GetArrayMethod(type, ".ctor", CallingConventions.HasThis, typeof(void), parameterTypes);
        }

        internal MethodInfo GetArrayCreateWithLowerBoundsMethod(IArrayTypeReference arrayTypeReference, ModuleBuilder moduleBuilder)
        {
            var type = this.GetType(arrayTypeReference);
            var parameterTypes = new Type[arrayTypeReference.Rank * 2];
            for (int i = 0; i < arrayTypeReference.Rank * 2; i++) parameterTypes[i] = typeof(int);
            return moduleBuilder.GetArrayMethod(type, ".ctor", CallingConventions.HasThis, typeof(void), parameterTypes);
        }
    }

    /// <summary>
    /// A visitor that maps CCI typeBuilder references to System.Type instances using method on System.Type.
    /// It uses a provided ReflectionMapper object to map element types to System.Type instances so
    /// that the caches maintained by ReflectionMapper can be used.
    /// </summary>
    internal class MappingVisitorForTypes : MetadataVisitor {

        internal MappingVisitorForTypes(ReflectionMapper mapper) {
            this.mapper = mapper;
        }

        private readonly ReflectionMapper mapper;
        internal Type result;

        public override void Visit(IArrayTypeReference arrayTypeReference) {
            if (arrayTypeReference.IsVector)
                this.result = this.mapper.GetType(arrayTypeReference.ElementType).MakeArrayType();
            else
                this.result = this.mapper.GetType(arrayTypeReference.ElementType).MakeArrayType((int)arrayTypeReference.Rank);
        }

        public override void Visit(IFunctionPointerTypeReference functionPointerTypeReference) {
            //System.Reflection has no way to construct an actual function pointer.
            //What it returns when reflecting on a function pointer typeBuilder is System.IntPtr.
            //We'll do the same. Since function pointers are unverifiable, the typeBuilder mismatch at the call site does
            //not actually matter.
            this.result = typeof(System.IntPtr);
        }

        public override void Visit(IGenericMethodParameterReference genericMethodParameterReference) {
            var genericMethod = this.mapper.GetMethod(genericMethodParameterReference.DefiningMethod);
            this.result = genericMethod.GetGenericArguments()[genericMethodParameterReference.Index];
        }

        public override void Visit(IGenericTypeInstanceReference genericTypeInstanceReference) {
            var template = genericTypeInstanceReference.GenericType;
            var specializedNestedType = template as ISpecializedNestedTypeReference;
            if (specializedNestedType != null) template = specializedNestedType.UnspecializedVersion;
            var templateType = this.mapper.GetType(template);
            var consolidatedArguments = new List<Type>();
            this.GetConsolidatedTypeArguments(consolidatedArguments, genericTypeInstanceReference);
            this.result = templateType.MakeGenericType(consolidatedArguments.ToArray());
        }

        private void GetConsolidatedTypeArguments(List<Type> consolidatedTypeArguments, ITypeReference typeReference) {
            var genTypeInstance = typeReference as IGenericTypeInstanceReference;
            if (genTypeInstance != null) {
                GetConsolidatedTypeArguments(consolidatedTypeArguments, genTypeInstance.GenericType);
                foreach (var genArg in genTypeInstance.GenericArguments)
                    consolidatedTypeArguments.Add(this.mapper.GetType(genArg));
                return;
            }
            var nestedTypeReference = typeReference as INestedTypeReference;
            if (nestedTypeReference != null) GetConsolidatedTypeArguments(consolidatedTypeArguments, nestedTypeReference.ContainingType);
        }

        public override void Visit(IGenericTypeParameterReference genericTypeParameterReference) {
            var definingType = genericTypeParameterReference.DefiningType;
            int index = genericTypeParameterReference.Index;
            //This index does not account for any inherited generic parameters. See if any containing types contain typeBuilder parameters and adjust the index.
            while (true) {
                var nestedType = definingType as INestedTypeReference;
                if (nestedType == null) break;
                definingType = nestedType.ContainingType;
                var genericTypeInstance = definingType as IGenericTypeInstanceReference;
                if (genericTypeInstance != null) index += (int)IteratorHelper.EnumerableCount(genericTypeInstance.GenericArguments);
            }
            //The defining typeBuilder may actually be a containing typeBuilder of the typeBuilder that contains the reference we are mapping here.
            //In that case, the System.Type object obtained below will probably be a different object than the one that would
            //be obtained by looking at the (consolidated) typeBuilder parameter list of the System.Type object that contains the reference.
            //We assume that this does not matter, however, since a Reflection typeBuilder parameter does not keep (visible) track of the typeBuilder which
            //it parameterizes.
            var genericType = this.mapper.GetType(definingType);
            this.result = genericType.GetGenericArguments()[genericTypeParameterReference.Index];
        }

        public override void Visit(IManagedPointerTypeReference managedPointerTypeReference) {
            this.result = this.mapper.GetType(managedPointerTypeReference.TargetType).MakeByRefType();
        }

        public override void Visit(IModifiedTypeReference modifiedTypeReference) {
            //Sytem.Reflection cannot model modified typeBuilder references. Just strip the modifiers.
            modifiedTypeReference.UnmodifiedType.Dispatch(this);
        }

        public override void Visit(INamespaceTypeReference namespaceTypeReference) {
            var assemblyReference = namespaceTypeReference.ContainingUnitNamespace.Unit as IAssemblyReference;
            if (assemblyReference != null) {
                var assembly = this.mapper.GetAssembly(assemblyReference);
                this.result = assembly.GetType(TypeHelper.GetTypeName(namespaceTypeReference, NameFormattingOptions.UseGenericTypeNameSuffix));
                if (this.result == null && namespaceTypeReference.GenericParameterCount > 0)
                    this.result = assembly.GetType(TypeHelper.GetTypeName(namespaceTypeReference));
            } else {
                var mod = (IModuleReference)namespaceTypeReference.ContainingUnitNamespace.Unit;
                var module = this.mapper.GetModule(mod);
                this.result = module.GetType(TypeHelper.GetTypeName(namespaceTypeReference, NameFormattingOptions.UseGenericTypeNameSuffix));
                if (this.result == null && namespaceTypeReference.GenericParameterCount > 0)
                    this.result = module.GetType(TypeHelper.GetTypeName(namespaceTypeReference));
            }
        }

        public override void Visit(INestedTypeReference nestedTypeReference) {
            var containingType = this.mapper.GetType(nestedTypeReference.ContainingType);
            var name = nestedTypeReference.Name.Value;
            // This method fails in case the containingType is generic and represented by a TypeBuilder.
            // See: https://connect.microsoft.com/VisualStudio/feedback/details/94519/there-should-be-static-typebuilder-getnestedtype-method-similar-to-typebuilder-getmethod-etc#tabs
            this.result = containingType.GetNestedType(name, BindingFlags.NonPublic|BindingFlags.DeclaredOnly);
            if (this.result == null && nestedTypeReference.GenericParameterCount > 0) {
                name = name + "'" + nestedTypeReference.GenericParameterCount;
                this.result = containingType.GetNestedType(name, BindingFlags.NonPublic|BindingFlags.DeclaredOnly);
            }
        }

        public override void Visit(IPointerTypeReference pointerTypeReference) {
            this.result = this.mapper.GetType(pointerTypeReference.TargetType).MakePointerType();
        }

    }

    public class ReflectionMapperException : Exception
    {
    }
}
