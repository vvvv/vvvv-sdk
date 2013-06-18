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
using System.Diagnostics.SymbolStore;
using System.Reflection;
using System.Reflection.Emit;

namespace Microsoft.Cci.ReflectionEmitter {

  /// <summary>
  /// 
  /// </summary>
  public class DynamicLoader {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sourceLocationProvider"></param>
    /// <param name="localScopeProvider"></param>
    public DynamicLoader(ISourceLocationProvider/*?*/ sourceLocationProvider, ILocalScopeProvider/*?*/ localScopeProvider) {
      this.sourceLocationProvider = sourceLocationProvider;
      this.localScopeProvider = localScopeProvider;
      this.emitter = new Emitter(this, sourceLocationProvider, localScopeProvider);
      this.initializingTraverser = new MetadataTraverser() { PostorderVisitor = this.emitter, TraverseIntoMethodBodies = true };
      this.typeBuilderAllocator = new TypeBuilderAllocater(this);
      this.typeCreator = new TypeCreator(this);
      this.memberBuilderAllocator = new MemberBuilderAllocator(this);
      this.mapper = new ReflectionMapper();
      this.builderMap = new Dictionary<object, object>();
    }

    ISourceLocationProvider/*?*/ sourceLocationProvider;
    ILocalScopeProvider/*?*/ localScopeProvider;
    Emitter emitter;
    MetadataTraverser initializingTraverser; //TODO: need the traverser to traverse private helper types/members
    TypeBuilderAllocater typeBuilderAllocator;
    TypeCreator typeCreator;
    MemberBuilderAllocator memberBuilderAllocator;
    ReflectionMapper mapper;
    Dictionary<object, object> builderMap;

    internal AssemblyBuilder AssemblyBuilder {
      get {
        if (this.assemblyBuilder == null)
          this.assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
            new System.Reflection.AssemblyName("CCI generated dynamic assembly "+this.GetHashCode()),
            AssemblyBuilderAccess.RunAndCollect);
        return this.assemblyBuilder;
      }
    }
    AssemblyBuilder/*?*/ assemblyBuilder;

    /// <summary>
    /// 
    /// </summary>
    public ModuleBuilder ModuleBuilder {
      get {
        if (this.moduleBuilder == null)
          this.moduleBuilder = this.AssemblyBuilder.DefineDynamicModule(this.AssemblyBuilder.GetName().Name+".manifest module", true);
        return this.moduleBuilder;
      }
      set {
        this.moduleBuilder = value;
        this.assemblyBuilder = (AssemblyBuilder)value.Assembly;
      }
    }
    ModuleBuilder/*?*/ moduleBuilder;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public AssemblyBuilder Load(IAssembly assembly) {
      //first create (but do not initialize) all typeBuilder builders, since they are needed to create member builders.
      this.typeBuilderAllocator.Traverse(assembly);
      //next create (but do not initialize) builder for all other kinds of typeBuilder members, since there may be forward references during initialization
      this.memberBuilderAllocator.Traverse(assembly);
      //now initialize all the builders
      this.initializingTraverser.TraverseChildren(assembly);
      //create all of the types
      this.typeCreator.Traverse(assembly.GetAllTypes());
      //set entry point on assembly if defined
      if (!(assembly.EntryPoint is Dummy)) this.AssemblyBuilder.SetEntryPoint((MethodInfo)this.mapper.GetMethod(assembly.EntryPoint));
      //now the assembly is ready for action
      return this.AssemblyBuilder;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="namespaceTypeDefinition"></param>
    /// <returns></returns>
    public Type Load(INamespaceTypeDefinition namespaceTypeDefinition) {
      //first create (but do not initialize) all typeBuilder builders, since they are needed to create member builders.
      this.typeBuilderAllocator.Traverse(namespaceTypeDefinition);
      //next create (but do not initialize) builder for all other kinds of typeBuilder members, since there may be forward references during initialization
      this.memberBuilderAllocator.Traverse(namespaceTypeDefinition);
      //now initialize all the builders
      this.initializingTraverser.TraverseChildren(namespaceTypeDefinition);
      //finally create the type and return it
      this.typeCreator.Traverse(namespaceTypeDefinition);
      return this.mapper.GetType(namespaceTypeDefinition);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="method"></param>
    /// <param name="skipVisibility"></param>
    /// <returns></returns>
    public DynamicMethod Load(IMethodDefinition method, bool skipVisibility) {
      var savedLocalScopeProvider = this.localScopeProvider;
      var savedSourceLocationProvider = this.sourceLocationProvider;
      this.localScopeProvider = null; //dynamic methods do not support debug info.
      this.sourceLocationProvider = null;
      var attributes = MemberBuilderAllocator.GetMethodAttributes(method);
      var callingConvention = MemberBuilderAllocator.GetCallingConvention(method.CallingConvention);
      var returnType = this.mapper.GetType(method.Type);
      var parameterTypes = new Type[method.ParameterCount];
      int i = 0;
      foreach (var parameter in method.Parameters)
        parameterTypes[i++] = this.mapper.GetType(parameter.Type);
      DynamicMethod dm = new DynamicMethod(method.Name.Value, attributes, callingConvention, returnType, parameterTypes, this.ModuleBuilder, skipVisibility);
      dm.InitLocals = method.Body.LocalsAreZeroed;
      this.emitter.EmitIL(dm.GetILGenerator(), method.Body);
      this.localScopeProvider = savedLocalScopeProvider;
      this.sourceLocationProvider = savedSourceLocationProvider;
      return dm;
    }

    class TypeBuilderAllocater : MetadataTraverser {

      internal TypeBuilderAllocater(DynamicLoader loader) {
        this.loader = loader;
      }

      DynamicLoader loader;

      public override void TraverseChildren(INamespaceTypeDefinition namespaceTypeDefinition) {
        var name = TypeHelper.GetTypeName(namespaceTypeDefinition, NameFormattingOptions.UseGenericTypeNameSuffix);
        var attributes = GetTypeAttributes(namespaceTypeDefinition);
        if (namespaceTypeDefinition.IsPublic) attributes |= TypeAttributes.Public;
        var typeBuilder = this.loader.ModuleBuilder.DefineType(name, attributes);
        this.AllocateGenericParametersIfNecessary(namespaceTypeDefinition, typeBuilder);
        this.loader.builderMap.Add(namespaceTypeDefinition, typeBuilder);
        this.loader.mapper.DefineMapping(namespaceTypeDefinition, typeBuilder); //so that typeBuilder references can be treated uniformly later on
        foreach (var nestedType in namespaceTypeDefinition.NestedTypes)
          this.TraverseChildren(nestedType);
        //TODO: also look at private helper members and private helper types
      }

      public override void TraverseChildren(INestedTypeDefinition nestedTypeDefinition) {
        var name = nestedTypeDefinition.Name.Value;
        if (nestedTypeDefinition.IsGeneric) name = name + "`" + nestedTypeDefinition.GenericParameterCount;
        var attributes = GetTypeAttributes(nestedTypeDefinition);
        attributes |= GetNestedTypeVisibility(nestedTypeDefinition);
        var containingType = (TypeBuilder)this.loader.mapper.GetType(nestedTypeDefinition.ContainingTypeDefinition);
        var typeBuilder = containingType.DefineNestedType(name);
        this.AllocateGenericParametersIfNecessary(nestedTypeDefinition, typeBuilder);
        this.loader.builderMap.Add(nestedTypeDefinition, typeBuilder); //so that typeBuilder references can be treated uniformly later on
        this.loader.mapper.DefineMapping(nestedTypeDefinition, typeBuilder);
        foreach (var nestedType in nestedTypeDefinition.NestedTypes)
          this.TraverseChildren(nestedType);
      }

      private void AllocateGenericParametersIfNecessary(ITypeDefinition typeDefinition, TypeBuilder typeBuilder) {
        if (!typeDefinition.IsGeneric) return;
        //We don't need these to be allocated in this pass, but it is more convenient to do it here.
        var names = new string[typeDefinition.GenericParameterCount];
        foreach (var genericParameter in typeDefinition.GenericParameters)
          names[genericParameter.Index] = genericParameter.Name.Value;
        var genericParameterBuilders = typeBuilder.DefineGenericParameters(names);
        foreach (var genericParameter in typeDefinition.GenericParameters) {
          var genericParameterBuilder = genericParameterBuilders[genericParameter.Index];
          this.loader.builderMap.Add(genericParameter, genericParameterBuilder);
          this.loader.mapper.DefineMapping(genericParameter, genericParameterBuilder);
        }
      }

      private static TypeAttributes GetTypeAttributes(ITypeDefinition typeDefinition) {
        var attributes = (TypeAttributes)0;
        if (typeDefinition.Layout == LayoutKind.Sequential) attributes |= TypeAttributes.SequentialLayout;
        if (typeDefinition.Layout == LayoutKind.Explicit) attributes |= TypeAttributes.ExplicitLayout;
        if (typeDefinition.IsInterface) attributes |= TypeAttributes.Interface;
        if (typeDefinition.IsAbstract) attributes |= TypeAttributes.Abstract;
        if (typeDefinition.IsSealed) attributes |= TypeAttributes.Sealed;
        if (typeDefinition.IsSpecialName) attributes |= TypeAttributes.SpecialName;
        if (typeDefinition.IsRuntimeSpecial) attributes |= TypeAttributes.RTSpecialName;
        if (typeDefinition.IsComObject) attributes |= TypeAttributes.Import;
        if (typeDefinition.IsSerializable) attributes |= TypeAttributes.Serializable;
        if (typeDefinition.StringFormat == StringFormatKind.Unicode) attributes |= TypeAttributes.UnicodeClass;
        if (typeDefinition.StringFormat == StringFormatKind.AutoChar) attributes |= TypeAttributes.AutoClass;
        if (typeDefinition.HasDeclarativeSecurity) attributes |= TypeAttributes.HasSecurity;
        if (typeDefinition.IsBeforeFieldInit) attributes |= TypeAttributes.BeforeFieldInit;
        return attributes;
      }

      private static TypeAttributes GetNestedTypeVisibility(ITypeDefinitionMember typeDefinitionMember) {
        switch (typeDefinitionMember.Visibility) {
          case TypeMemberVisibility.Assembly: return TypeAttributes.NestedAssembly;
          case TypeMemberVisibility.Family: return TypeAttributes.NestedFamily;
          case TypeMemberVisibility.FamilyAndAssembly: return TypeAttributes.NestedFamANDAssem;
          case TypeMemberVisibility.FamilyOrAssembly: return TypeAttributes.NestedFamORAssem;
          case TypeMemberVisibility.Private: return TypeAttributes.NestedPrivate;
          case TypeMemberVisibility.Public: return TypeAttributes.NestedPublic;
        }
        return 0;
      }

    }

    class MemberBuilderAllocator : MetadataTraverser {
      internal MemberBuilderAllocator(DynamicLoader loader) {
        this.loader = loader;
      }

      DynamicLoader loader;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="eventDefinition"></param>
      public override void TraverseChildren(IEventDefinition eventDefinition) {
        EventAttributes attributes = EventAttributes.None;
        if (eventDefinition.IsSpecialName) attributes |= EventAttributes.SpecialName;
        if (eventDefinition.IsRuntimeSpecial) attributes |= EventAttributes.RTSpecialName;
        var containingType = (TypeBuilder)this.loader.mapper.GetType(eventDefinition.ContainingTypeDefinition);
        var eventType = this.loader.mapper.GetType(eventDefinition.Type);
        var eventBuilder = containingType.DefineEvent(eventDefinition.Name.Value, attributes, eventType);
        this.loader.builderMap.Add(eventDefinition, eventBuilder);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="fieldDefinition"></param>
      public override void TraverseChildren(IFieldDefinition fieldDefinition) {
        var containingType = (TypeBuilder)this.loader.mapper.GetType(fieldDefinition.ContainingTypeDefinition);
        var fieldType = this.loader.mapper.GetType(fieldDefinition.Type);
        FieldAttributes attributes = this.GetAttributes(fieldDefinition);
        Type[] reqMods = null;
        Type[] optMods = null;
        if (fieldDefinition.IsModified)
          this.GetModifierTypes(fieldDefinition.CustomModifiers, out reqMods, out optMods);
        FieldBuilder fieldBuilder;
        if (fieldDefinition.IsMapped) {
          var data = new List<byte>(fieldDefinition.FieldMapping.Data).ToArray();
          if (data.Length > 0)
            fieldBuilder = containingType.DefineInitializedData(fieldDefinition.Name.Value, data, attributes);
          else
            fieldBuilder = containingType.DefineUninitializedData(fieldDefinition.Name.Value, (int)fieldDefinition.FieldMapping.Size, attributes);
        } else
          fieldBuilder = containingType.DefineField(fieldDefinition.Name.Value, fieldType, reqMods, optMods, attributes);
        this.loader.builderMap.Add(fieldDefinition, fieldBuilder);
        this.loader.mapper.DefineMapping(fieldDefinition, fieldBuilder); //so that all field references can be treated uniformly later.
      }

      private FieldAttributes GetAttributes(IFieldDefinition fieldDefinition) {
        var attributes = (FieldAttributes)0;
        switch (fieldDefinition.Visibility) {
          case TypeMemberVisibility.Assembly: attributes = FieldAttributes.Assembly; break;
          case TypeMemberVisibility.Family: attributes = FieldAttributes.Family; break;
          case TypeMemberVisibility.FamilyAndAssembly: attributes = FieldAttributes.FamANDAssem; break;
          case TypeMemberVisibility.FamilyOrAssembly: attributes = FieldAttributes.FamORAssem; break;
          case TypeMemberVisibility.Private: attributes = FieldAttributes.Private; break;
          case TypeMemberVisibility.Public: attributes = FieldAttributes.Public; break;
        }
        if (fieldDefinition.IsStatic) attributes |= FieldAttributes.Static;
        if (fieldDefinition.IsReadOnly) attributes |= FieldAttributes.InitOnly;
        if (fieldDefinition.IsCompileTimeConstant) attributes |= FieldAttributes.Literal;
        if (fieldDefinition.IsNotSerialized) attributes |= FieldAttributes.NotSerialized;
        if (fieldDefinition.IsMapped) attributes |= FieldAttributes.HasFieldRVA;
        if (fieldDefinition.IsSpecialName) attributes |= FieldAttributes.SpecialName;
        if (fieldDefinition.IsRuntimeSpecial) attributes |= FieldAttributes.RTSpecialName;
        if (fieldDefinition.IsMarshalledExplicitly) attributes |= FieldAttributes.HasFieldMarshal;
        if (!(fieldDefinition.CompileTimeValue is Dummy)) attributes |= FieldAttributes.HasDefault;
        return attributes;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="genericParameter"></param>
      public override void TraverseChildren(IGenericParameter genericParameter) {
        //Setting the constraints here seems a little out of place since no new builders are being allocated
        //and setting the constraints of generic parameter really is initializing its builder, not allocating a builder.
        //However, the code for allocating a generic method builder is just a little bit saner when combined with the code for
        //allocating non generic mehods and constructors. Making all that hang together requires us to initialize the generic method parameters
        //in this pass so that we can set the signature of the generic method builder without inviting an exception from Reflection.Emit.

        var genericTypeParameterBuilder = (GenericTypeParameterBuilder)this.loader.builderMap[genericParameter];
        var genericParameterAttributes = GetAttributes(genericParameter);
        genericTypeParameterBuilder.SetGenericParameterAttributes(genericParameterAttributes);
        List<Type> interfaceConstraints;
        Type classConstraint;
        this.GetConstraints(genericParameter, out interfaceConstraints, out classConstraint);
        if (classConstraint != null)
          genericTypeParameterBuilder.SetBaseTypeConstraint(classConstraint);
        if (interfaceConstraints != null)
          genericTypeParameterBuilder.SetInterfaceConstraints(interfaceConstraints.ToArray());
      }

      private void GetConstraints(IGenericParameter genericParameter, out List<Type> interfaceConstraints, out Type classConstraint) {
        interfaceConstraints = null;
        classConstraint = null;
        foreach (var constraint in genericParameter.Constraints) {
          var constraintType = this.loader.mapper.GetType(constraint);
          if (constraintType == null) continue;
          if (constraintType.IsClass)
            classConstraint = constraintType;
          else {
            if (interfaceConstraints == null) interfaceConstraints = new List<Type>();
            interfaceConstraints.Add(constraintType);
          }
        }
      }

      private static GenericParameterAttributes GetAttributes(IGenericParameter genericParameter) {
        var result = GenericParameterAttributes.None;
        if (genericParameter.Variance == TypeParameterVariance.Covariant)
          result |= GenericParameterAttributes.Covariant;
        else if (genericParameter.Variance == TypeParameterVariance.Contravariant)
          result |= GenericParameterAttributes.Contravariant;
        if (genericParameter.MustBeReferenceType)
          result |= GenericParameterAttributes.ReferenceTypeConstraint;
        if (genericParameter.MustBeValueType)
          result |= GenericParameterAttributes.NotNullableValueTypeConstraint;
        if (genericParameter.MustHaveDefaultConstructor)
          result |= GenericParameterAttributes.DefaultConstructorConstraint;
        return result;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="propertyDefinition"></param>
      public override void TraverseChildren(IPropertyDefinition propertyDefinition) {
        var containingType = (TypeBuilder)this.loader.mapper.GetType(propertyDefinition.ContainingTypeDefinition);
        PropertyAttributes attributes = PropertyAttributes.None;
        if (propertyDefinition.IsSpecialName) attributes |= PropertyAttributes.SpecialName;
        if (propertyDefinition.IsRuntimeSpecial) attributes |= PropertyAttributes.RTSpecialName;
        if (propertyDefinition.HasDefaultValue) attributes |= PropertyAttributes.HasDefault;
        var returnType = this.loader.mapper.GetType(propertyDefinition.Type);
        Type[] rtReqMods = null;
        Type[] rtOptMods = null;
        if (propertyDefinition.ReturnValueIsModified)
          this.GetModifierTypes(propertyDefinition.ReturnValueCustomModifiers, out rtReqMods, out rtOptMods);
        Type[][] ptReqMods = null;
        Type[][] ptOptMods = null;
        Type[] parameterTypes = this.GetParameterTypes(propertyDefinition, out ptReqMods, out ptOptMods);
        var propertyBuilder = containingType.DefineProperty(propertyDefinition.Name.Value, attributes, returnType, rtReqMods, rtOptMods,
          parameterTypes, ptReqMods, ptOptMods);
        this.loader.builderMap.Add(propertyDefinition, propertyBuilder);
      }

      /// <summary>
      /// Traverses the children of the method definition.
      /// </summary>
      /// <param name="method"></param>
      public override void TraverseChildren(IMethodDefinition method) {
        var containingType = (TypeBuilder)this.loader.mapper.GetType(method.ContainingTypeDefinition);
        var attributes = GetMethodAttributes(method);
        var callingConvention = GetCallingConvention(method.CallingConvention);

        MethodBuilder/*?*/ genericMethodBuilder = null;
        if (method.IsGeneric) {
          //We need to establish mappings from IGenericMethodParameter values to GenericTypeParameterBuilder values
          //before the return type and parameter types of the method are mapped.
          genericMethodBuilder = containingType.DefineMethod(method.Name.Value, attributes, callingConvention);
          string[] genericParameterNames = new string[method.GenericParameterCount];
          foreach (var genPar in method.GenericParameters)
            genericParameterNames[genPar.Index] = genPar.Name.Value;
          var genParBuilders = genericMethodBuilder.DefineGenericParameters(genericParameterNames);
          foreach (var genPar in method.GenericParameters) {
            var genParBuilder = genParBuilders[genPar.Index];
            this.loader.builderMap.Add(genPar, genParBuilder);
            this.loader.mapper.DefineMapping(genPar, genParBuilder);
          }
          this.loader.builderMap.Add(method, genericMethodBuilder);
          this.loader.mapper.DefineMapping(method, genericMethodBuilder);
          this.Traverse(method.GenericParameters);
        }

        var returnType = this.loader.mapper.GetType(method.Type);
        Type[] rtReqMods = null;
        Type[] rtOptMods = null;
        if (method.ReturnValueIsModified)
          this.GetModifierTypes(method.ReturnValueCustomModifiers, out rtReqMods, out rtOptMods);
        Type[][] ptReqMods = null;
        Type[][] ptOptMods = null;
        Type[] parameterTypes = this.GetParameterTypes(method, out ptReqMods, out ptOptMods);
        MethodBase builder;
        if (method.IsConstructor)
          builder = containingType.DefineConstructor(attributes, callingConvention, parameterTypes, ptReqMods, ptOptMods);
        else if (method.IsStaticConstructor)
          builder = containingType.DefineTypeInitializer();
        else if (method.IsPlatformInvoke)
          builder = containingType.DefinePInvokeMethod(method.Name.Value, method.PlatformInvokeData.ImportModule.Name.Value,
            method.PlatformInvokeData.ImportName.Value, attributes, callingConvention, returnType, rtReqMods, rtOptMods,
            parameterTypes, ptReqMods, ptOptMods, GetNativeCallingConvention(method), GetNativeCharset(method));
        else {
          if (genericMethodBuilder != null) {
            genericMethodBuilder.SetSignature(returnType, rtReqMods, rtOptMods, parameterTypes, ptReqMods, ptOptMods);
            return;
          }
          builder = containingType.DefineMethod(method.Name.Value, attributes, callingConvention, returnType, rtReqMods, rtOptMods,
            parameterTypes, ptReqMods, ptOptMods);
        }
        this.loader.builderMap.Add(method, builder);
        this.loader.mapper.DefineMapping(method, builder);
      }

      private System.Runtime.InteropServices.CallingConvention GetNativeCallingConvention(IMethodDefinition method) {
        switch (method.PlatformInvokeData.PInvokeCallingConvention) {
          case PInvokeCallingConvention.CDecl: return System.Runtime.InteropServices.CallingConvention.Cdecl;
          case PInvokeCallingConvention.FastCall: return System.Runtime.InteropServices.CallingConvention.FastCall;
          case PInvokeCallingConvention.StdCall: return System.Runtime.InteropServices.CallingConvention.StdCall;
          case PInvokeCallingConvention.ThisCall: return System.Runtime.InteropServices.CallingConvention.ThisCall;
          case PInvokeCallingConvention.WinApi: return System.Runtime.InteropServices.CallingConvention.Winapi;
        }
        return 0;
      }

      private System.Runtime.InteropServices.CharSet GetNativeCharset(IMethodDefinition method) {
        switch (method.PlatformInvokeData.StringFormat) {
          case StringFormatKind.Ansi: return System.Runtime.InteropServices.CharSet.Ansi;
          case StringFormatKind.AutoChar: return System.Runtime.InteropServices.CharSet.Auto;
          case StringFormatKind.Unicode: return System.Runtime.InteropServices.CharSet.Unicode;
        }
        return System.Runtime.InteropServices.CharSet.None;
      }

      private void GetModifierTypes(IEnumerable<ICustomModifier> customModifiers, out Type[] reqMods, out Type[] optMods) {
        reqMods = null;
        optMods = null;
        int reqModCounter = 0;
        int optModCounter = 0;
        foreach (var customModifier in customModifiers) {
          if (customModifier.IsOptional) optModCounter++; else reqModCounter++;
        }
        if (reqModCounter > 0) reqMods = new Type[reqModCounter];
        if (optModCounter > 0) optMods = new Type[optModCounter];
        reqModCounter = 0;
        optModCounter = 0;
        foreach (var customModifier in customModifiers) {
          var modifierType = this.loader.mapper.GetType(customModifier.Modifier);
          if (modifierType == null) {
            //TODO: error
            modifierType = typeof(object);
          }
          if (customModifier.IsOptional)
            optMods[optModCounter++] = modifierType;
          else
            reqMods[reqModCounter++] = modifierType;
        }
      }

      private Type[] GetParameterTypes(ISignature signature, out Type[][] ptReqMods, out Type[][] ptOptMods) {
        uint numPars = IteratorHelper.EnumerableCount(signature.Parameters); //signature.ParameterCount;
        Type[] parameterTypes = new Type[numPars];
        ptReqMods = null;
        ptOptMods = null;
        int parameterCounter = 0;
        foreach (var parameter in signature.Parameters) {
          var pType = this.loader.mapper.GetType(parameter.Type);
          if (pType == null) {
            //TODO: error
            pType = typeof(object);
          }
          parameterTypes[parameterCounter] = pType;
          if (parameter.IsModified) {
            if (ptReqMods == null) {
              ptReqMods = new Type[numPars][];
              ptOptMods = new Type[numPars][];
            }
            this.GetModifierTypes(parameter.CustomModifiers, out ptReqMods[parameterCounter], out ptOptMods[parameterCounter]);
          }
          parameterCounter++;
        }
        return parameterTypes;
      }

      internal static MethodAttributes GetMethodAttributes(IMethodDefinition method) {
        MethodAttributes attributes = (MethodAttributes)0;
        switch (method.Visibility) {
          case TypeMemberVisibility.Assembly: attributes = MethodAttributes.Assembly; break;
          case TypeMemberVisibility.Family: attributes = MethodAttributes.Family; break;
          case TypeMemberVisibility.FamilyAndAssembly: attributes = MethodAttributes.FamANDAssem; break;
          case TypeMemberVisibility.FamilyOrAssembly: attributes = MethodAttributes.FamORAssem; break;
          case TypeMemberVisibility.Private: attributes = MethodAttributes.Private; break;
          case TypeMemberVisibility.Public: attributes = MethodAttributes.Public; break;
        }
        if (method.IsStatic) attributes |= MethodAttributes.Static;
        if (method.IsSealed) attributes |= MethodAttributes.Final;
        if (method.IsVirtual) attributes |= MethodAttributes.Virtual;
        if (method.IsHiddenBySignature) attributes |= MethodAttributes.HideBySig;
        if (method.IsAccessCheckedOnOverride) attributes |= MethodAttributes.CheckAccessOnOverride;
        if (method.IsAbstract) attributes |= MethodAttributes.Abstract;
        if (method.IsSpecialName) attributes |= MethodAttributes.SpecialName;
        if (method.IsRuntimeSpecial) attributes |= MethodAttributes.RTSpecialName;
        if (method.IsPlatformInvoke) attributes |= MethodAttributes.PinvokeImpl;
        if (method.HasDeclarativeSecurity) attributes |= MethodAttributes.HasSecurity;
        return attributes;
      }

      internal static CallingConventions GetCallingConvention(CallingConvention callingConvention) {
        var result = (CallingConventions)0;
        switch (callingConvention & (CallingConvention)0x7) {
          case CallingConvention.Standard: result = CallingConventions.Standard; break;
          case CallingConvention.ExtraArguments: result = CallingConventions.VarArgs; break;
        }
        if ((callingConvention & CallingConvention.HasThis) != 0) result |= CallingConventions.HasThis;
        if ((callingConvention & CallingConvention.ExplicitThis) != 0) result |= CallingConventions.ExplicitThis;
        return result;
      }

    }

    class Emitter : MetadataVisitor {

      internal Emitter(DynamicLoader loader, ISourceLocationProvider sourceLocationProvider, ILocalScopeProvider localScopeProvider) {
        this.loader = loader;
        this.sourceLocationProvider = sourceLocationProvider;
        this.localScopeProvider = localScopeProvider;
      }

      DynamicLoader loader;
      Dictionary<uint, Label> labelFor;
      Dictionary<ILocalDefinition, LocalBuilder> localFor;
      System.Reflection.Emit.ILGenerator ilGenerator;
      IMethodBody methodBody;
      ISourceLocationProvider/*?*/ sourceLocationProvider;
      ILocalScopeProvider/*?*/ localScopeProvider;
      IDocument currentDocument;
      ISymbolDocumentWriter currentDocumentWriter;
      Dictionary<IDocument, ISymbolDocumentWriter> documentMap = new Dictionary<IDocument, ISymbolDocumentWriter>();

      /// <summary>
      /// Performs some computation with the given event definition.
      /// </summary>
      /// <param name="eventDefinition"></param>
      public override void Visit(IEventDefinition eventDefinition) {
        var eventBuilder = (EventBuilder)this.loader.builderMap[eventDefinition];
        foreach (var accessor in eventDefinition.Accessors) {
          var accessorMethod = (MethodBuilder)this.loader.builderMap[accessor];
          if (accessor == eventDefinition.Adder)
            eventBuilder.SetAddOnMethod(accessorMethod);
          else if (accessor == eventDefinition.Remover)
            eventBuilder.SetRemoveOnMethod(accessorMethod);
          else if (accessor == eventDefinition.Caller)
            eventBuilder.SetRaiseMethod(accessorMethod);
          else
            eventBuilder.AddOtherMethod(accessorMethod);
        }
        foreach (var customAttribute in eventDefinition.Attributes) {
          var customAttributeBuilder = this.GetCustomAttributeBuilder(customAttribute);
          eventBuilder.SetCustomAttribute(customAttributeBuilder);
        }
      }

      private CustomAttributeBuilder GetCustomAttributeBuilder(ICustomAttribute customAttribute) {
        var type = this.loader.mapper.GetType(customAttribute.Type);
        var constructor = (ConstructorInfo)this.loader.mapper.GetMethod(customAttribute.Constructor);
        var arguments = new object[customAttribute.Constructor.ParameterCount];
        int i = 0;
        foreach (var argument in customAttribute.Arguments) {
          argument.Dispatch(this);
          arguments[i++] = this.value;
        }
        if (customAttribute.NumberOfNamedArguments > 0) {
          int propertyCounter = 0;
          int fieldCounter = 0;
          foreach (var namedArgument in customAttribute.NamedArguments)
            if (namedArgument.IsField) fieldCounter++; else propertyCounter++;
          var properties = new PropertyInfo[propertyCounter];
          var propertyValues = new object[propertyCounter];
          var fields = new FieldInfo[fieldCounter];
          var fieldValues = new object[fieldCounter];
          propertyCounter = 0;
          fieldCounter = 0;
          foreach (var namedArgument in customAttribute.NamedArguments) {
            if (namedArgument.IsField) {
              fields[fieldCounter] = type.GetField(namedArgument.ArgumentName.Value);
              namedArgument.ArgumentValue.Dispatch(this);
              fieldValues[fieldCounter++] = this.value;
            } else {
              properties[propertyCounter] = type.GetProperty(namedArgument.ArgumentName.Value);
              namedArgument.ArgumentValue.Dispatch(this);
              propertyValues[propertyCounter++] = this.value;
            }
          }
          return new CustomAttributeBuilder(constructor, arguments, properties, propertyValues, fields, fieldValues);
        } else
          return new CustomAttributeBuilder(constructor, arguments);
      }

      object value;

      /// <summary>
      /// Performs some computation with the given metadata constant.
      /// </summary>
      /// <param name="constant"></param>
      public override void Visit(IMetadataConstant constant) {
        this.value = constant.Value;
      }

      /// <summary>
      /// Performs some computation with the given metadata array creation expression.
      /// </summary>
      /// <param name="createArray"></param>
      public override void Visit(IMetadataCreateArray createArray) {
        object[] vector = new object[IteratorHelper.EnumerableCount(createArray.Initializers)];
        int i = 0;
        foreach (var element in createArray.Initializers) {
          element.Dispatch(this);
          vector[i++] = this.value;
        }
        this.value = vector;
      }

      /// <summary>
      /// Performs some computation with the given metadata typeof expression.
      /// </summary>
      /// <param name="typeOf"></param>
      public override void Visit(IMetadataTypeOf typeOf) {
        this.value = this.loader.mapper.GetType(typeOf.TypeToGet);
      }

      /// <summary>
      /// Performs some computation with the given field definition.
      /// </summary>
      /// <param name="fieldDefinition"></param>
      public override void Visit(IFieldDefinition fieldDefinition) {
        var fieldBuilder = (FieldBuilder)this.loader.builderMap[fieldDefinition];
        if (!(fieldDefinition.CompileTimeValue is Dummy)) {
          fieldDefinition.CompileTimeValue.Dispatch(this);
          fieldBuilder.SetConstant(this.value);
        }
        foreach (var customAttribute in fieldDefinition.Attributes) {
          var customAttributeBuilder = this.GetCustomAttributeBuilder(customAttribute);
          fieldBuilder.SetCustomAttribute(customAttributeBuilder);
        }
        if (fieldDefinition.IsMarshalledExplicitly) {
          fieldBuilder.SetCustomAttribute(GetMarshalAsAttribute(fieldDefinition.MarshallingInformation));
        }
        if (fieldDefinition.ContainingTypeDefinition.Layout == LayoutKind.Explicit)
          fieldBuilder.SetOffset((int)fieldDefinition.Offset);
      }

      private CustomAttributeBuilder GetMarshalAsAttribute(IMarshallingInformation marshallingInformation) {
        var marshalAsAttributeType = typeof(System.Runtime.InteropServices.MarshalAsAttribute);
        var constructor = marshalAsAttributeType.GetConstructor(new Type[] { typeof(System.Runtime.InteropServices.UnmanagedType) });
        var arguments = new object[] { marshallingInformation.UnmanagedType };
        List<FieldInfo> fields = new List<FieldInfo>();
        List<object> fieldValues = new List<object>();
        if (marshallingInformation.UnmanagedType == System.Runtime.InteropServices.UnmanagedType.ByValArray ||
            marshallingInformation.UnmanagedType == System.Runtime.InteropServices.UnmanagedType.LPArray) {
          fields.Add(marshalAsAttributeType.GetField("ArraySubType"));
          fieldValues.Add(marshallingInformation.ElementType);
        }
        if (marshallingInformation.UnmanagedType == System.Runtime.InteropServices.UnmanagedType.Interface) {
          fields.Add(marshalAsAttributeType.GetField("IidParameterIndex"));
          fieldValues.Add((int)marshallingInformation.IidParameterIndex);
        }
        if (marshallingInformation.UnmanagedType == System.Runtime.InteropServices.UnmanagedType.CustomMarshaler) {
          fields.Add(marshalAsAttributeType.GetField("MarshalCookie"));
          fieldValues.Add(marshallingInformation.CustomMarshallerRuntimeArgument);
          fields.Add(marshalAsAttributeType.GetField("MarshalTypeRef"));
          fieldValues.Add(this.loader.mapper.GetType(marshallingInformation.CustomMarshaller));
        }
        if (marshallingInformation.UnmanagedType == System.Runtime.InteropServices.UnmanagedType.SafeArray) {
          fields.Add(marshalAsAttributeType.GetField("SafeArraySubType"));
          fieldValues.Add(marshallingInformation.SafeArrayElementSubtype);
          if (marshallingInformation.SafeArrayElementSubtype == System.Runtime.InteropServices.VarEnum.VT_DISPATCH ||
          marshallingInformation.SafeArrayElementSubtype == System.Runtime.InteropServices.VarEnum.VT_UNKNOWN ||
          marshallingInformation.SafeArrayElementSubtype == System.Runtime.InteropServices.VarEnum.VT_RECORD) {
            fields.Add(marshalAsAttributeType.GetField("SafeArrayUserDefinedSubType"));
            fieldValues.Add(this.loader.mapper.GetType(marshallingInformation.SafeArrayElementUserDefinedSubtype));
          }
        }
        if (marshallingInformation.UnmanagedType == System.Runtime.InteropServices.UnmanagedType.ByValArray ||
        marshallingInformation.UnmanagedType == System.Runtime.InteropServices.UnmanagedType.ByValTStr ||
        marshallingInformation.UnmanagedType == System.Runtime.InteropServices.UnmanagedType.LPArray) {
          fields.Add(marshalAsAttributeType.GetField("SizeConst"));
          fieldValues.Add((int)marshallingInformation.NumberOfElements);
        }
        if (marshallingInformation.UnmanagedType == System.Runtime.InteropServices.UnmanagedType.LPArray &&
        marshallingInformation.ParamIndex != null) {
          fields.Add(marshalAsAttributeType.GetField("SizeParamIndex"));
          fieldValues.Add((short)marshallingInformation.ParamIndex.Value);
        }
        return new CustomAttributeBuilder(constructor, arguments, fields.ToArray(), fieldValues.ToArray());
      }

      /// <summary>
      /// Performs some computation with the given method definition.
      /// </summary>
      /// <param name="method"></param>
      public override void Visit(IMethodDefinition method) {
        if (method.IsConstructor || method.IsStaticConstructor) {
          var constructorBuilder = (ConstructorBuilder)this.loader.builderMap[method];
          if (method.HasDeclarativeSecurity) {
            foreach (var securityAttribute in method.SecurityAttributes)
              constructorBuilder.AddDeclarativeSecurity(GetSecurityAction(securityAttribute), GetPermissionSet(securityAttribute));
          }
          foreach (var parDef in method.Parameters) {
            if (parDef.HasDefaultValue || parDef.IsOptional || parDef.IsOut || parDef.IsMarshalledExplicitly ||
            IteratorHelper.EnumerableIsNotEmpty(parDef.Attributes) || parDef.Name.Value != string.Empty) {
              var parameterAttributes = GetAttributes(parDef);
              var parameterBuilder = constructorBuilder.DefineParameter(parDef.Index+1, parameterAttributes, parDef.Name.Value);
              this.Visit(parDef, parameterBuilder);
            }
          }
          constructorBuilder.SetImplementationFlags(GetImplAttributes(method));
          foreach (var customAttribute in method.Attributes) {
            var customAttributeBuilder = this.GetCustomAttributeBuilder(customAttribute);
            constructorBuilder.SetCustomAttribute(customAttributeBuilder);
          }
          constructorBuilder.InitLocals = method.Body.LocalsAreZeroed;
          this.EmitIL(constructorBuilder.GetILGenerator(), method.Body);
        } else {
          var methodBuilder = (MethodBuilder)this.loader.builderMap[method];
          if (method.HasDeclarativeSecurity) {
            foreach (var securityAttribute in method.SecurityAttributes)
              methodBuilder.AddDeclarativeSecurity(GetSecurityAction(securityAttribute), GetPermissionSet(securityAttribute));
          }
          if (method.IsGeneric) {
            foreach (var genPar in method.GenericParameters)
              this.Visit(genPar);
          }
          foreach (var parDef in method.Parameters) {
            if (parDef.HasDefaultValue || parDef.IsOptional || parDef.IsOut || parDef.IsMarshalledExplicitly ||
            IteratorHelper.EnumerableIsNotEmpty(parDef.Attributes) || parDef.Name.Value != string.Empty) {
              var parameterBuilder = methodBuilder.DefineParameter(parDef.Index+1, GetAttributes(parDef), parDef.Name.Value);
              this.Visit(parDef, parameterBuilder);
            }
          }
          methodBuilder.SetImplementationFlags(GetImplAttributes(method));
          foreach (var customAttribute in method.Attributes) {
            var customAttributeBuilder = this.GetCustomAttributeBuilder(customAttribute);
            methodBuilder.SetCustomAttribute(customAttributeBuilder);
          }
          if (method.ReturnValueIsMarshalledExplicitly) {
            string returnValueName = null;
            if (!(method.ReturnValueName is Dummy)) returnValueName = method.ReturnValueName.Value;
            var returnValue = methodBuilder.DefineParameter(0, ParameterAttributes.Retval|ParameterAttributes.HasFieldMarshal, returnValueName);
            returnValue.SetCustomAttribute(GetMarshalAsAttribute(method.ReturnValueMarshallingInformation));
          }
          if (!method.IsAbstract && !method.IsExternal) {
            methodBuilder.InitLocals = method.Body.LocalsAreZeroed;
            this.EmitIL(methodBuilder.GetILGenerator(), method.Body);
          }
        }
      }

      private MethodImplAttributes GetImplAttributes(IMethodDefinition method) {
        MethodImplAttributes result = (MethodImplAttributes)0;
        if (method.IsNativeCode) result |= MethodImplAttributes.Native;
        if (method.IsRuntimeImplemented) result |= MethodImplAttributes.Runtime;
        if (method.IsUnmanaged) result |= MethodImplAttributes.Unmanaged;
        if (method.IsNeverInlined) result |= MethodImplAttributes.NoInlining;
        if (method.IsForwardReference) result |= MethodImplAttributes.ForwardRef;
        if (method.IsSynchronized) result |= MethodImplAttributes.Synchronized;
        if (method.IsNeverOptimized) result |= MethodImplAttributes.NoOptimization;
        if (method.PreserveSignature) result |= MethodImplAttributes.PreserveSig;
        if (method.IsRuntimeInternal) result |= MethodImplAttributes.InternalCall;
        return result;
      }

      private static ParameterAttributes GetAttributes(IParameterDefinition parDef) {
        ParameterAttributes result = ParameterAttributes.None;
        if (parDef.IsIn) result |= ParameterAttributes.In;
        if (parDef.IsOut) result |= ParameterAttributes.Out;
        if (parDef.IsOptional) result |= ParameterAttributes.Optional;
        if (parDef.HasDefaultValue) result |= ParameterAttributes.HasDefault;
        if (parDef.IsMarshalledExplicitly) result |= ParameterAttributes.HasFieldMarshal;
        return result;
      }

      /// <summary>
      /// Performs some computation with the given generic parameter.
      /// </summary>
      /// <param name="genericParameter"></param>
      public override void Visit(IGenericParameter genericParameter) {
        var genericTypeParameterBuilder = (GenericTypeParameterBuilder)this.loader.builderMap[genericParameter];
        foreach (var customAttribute in genericParameter.Attributes) {
          var customAttributeBuilder = this.GetCustomAttributeBuilder(customAttribute);
          genericTypeParameterBuilder.SetCustomAttribute(customAttributeBuilder);
        }
      }

      private void Visit(IParameterDefinition parameterDefinition, ParameterBuilder parameterBuilder) {
        if (parameterDefinition.HasDefaultValue) {
          parameterDefinition.DefaultValue.Dispatch(this);
          parameterBuilder.SetConstant(this.value);
        }
        foreach (var customAttribute in parameterDefinition.Attributes) {
          var customAttributeBuilder = this.GetCustomAttributeBuilder(customAttribute);
          parameterBuilder.SetCustomAttribute(customAttributeBuilder);
        }
        if (parameterDefinition.IsMarshalledExplicitly)
          parameterBuilder.SetCustomAttribute(GetMarshalAsAttribute(parameterDefinition.MarshallingInformation));
      }

      private static System.Security.Permissions.SecurityAction GetSecurityAction(ISecurityAttribute securityAttribute) {
        return (System.Security.Permissions.SecurityAction)securityAttribute.Action;
      }

      private System.Security.PermissionSet GetPermissionSet(ISecurityAttribute securityAttribute) {
        var result = new System.Security.PermissionSet(System.Security.Permissions.PermissionState.None);
        foreach (var attribute in securityAttribute.Attributes) {
          var permission = this.GetPermission(securityAttribute.Action, attribute);
          if (permission == null) continue; //not a trusted permission
          result.AddPermission(permission);
        }
        return result;
      }

      private System.Security.IPermission/*?*/ GetPermission(SecurityAction securityAction, ICustomAttribute attribute) {
        var constructor = this.loader.mapper.GetMethod(attribute.Constructor);
        if (constructor.Module.Assembly != typeof(object).Module.Assembly) {
          //if the permission attribute constructor is not defined in mscorlib, we are not going to trust it,
          //since the code calling the emitter may be running with elevated permissions that are not intended
          //for the code being emitted here.
          return null;
        }
        var n = attribute.Constructor.ParameterCount;
        if (n == 0) return null;
        var arguments = new object[n];
        arguments[0] = securityAction;
        int i = 1;
        foreach (var argument in attribute.Arguments) {
          if (i >= n) return null;
          argument.Dispatch(this);
          arguments[i++] = this.value;
        }
        try {
          var secAttribute = (System.Security.Permissions.SecurityAttribute)constructor.Invoke(null, arguments);
          foreach (var namedArgument in attribute.NamedArguments) {
            namedArgument.ArgumentValue.Dispatch(this);
            if (namedArgument.IsField) {
              var fieldInfo = constructor.ReflectedType.GetField(namedArgument.ArgumentName.Value);
              if (fieldInfo == null) return null;
              fieldInfo.SetValue(secAttribute, this.value);
            } else {
              var propertyInfo = constructor.ReflectedType.GetProperty(namedArgument.ArgumentName.Value);
              if (propertyInfo == null) return null;
              propertyInfo.SetValue(secAttribute, this.value, null);
            }
          }
          return (System.Security.IPermission)secAttribute.CreatePermission();
        } catch (ArgumentException) {
          return null;
        } catch (AmbiguousMatchException) {
          return null;
        } catch (TargetInvocationException) {
          return null;
        } catch (MethodAccessException) {
          return null;
        } catch (InvalidOperationException) {
          return null;
        } catch (NotSupportedException) {
          return null;
        } catch (FieldAccessException) {
          return null;
        }
      }

      /// <summary>
      /// Performs some computation with the given property definition.
      /// </summary>
      /// <param name="propertyDefinition"></param>
      public override void Visit(IPropertyDefinition propertyDefinition) {
        var propertyBuilder = (PropertyBuilder)this.loader.builderMap[propertyDefinition];
        foreach (var accessor in propertyDefinition.Accessors) {
          var accessorMethod = (MethodBuilder)this.loader.builderMap[accessor];
          if (accessor == propertyDefinition.Getter)
            propertyBuilder.SetGetMethod(accessorMethod);
          else if (accessor == propertyDefinition.Setter)
            propertyBuilder.SetSetMethod(accessorMethod);
          else
            propertyBuilder.AddOtherMethod(accessorMethod);
        }
        if (propertyDefinition.HasDefaultValue) {
          propertyDefinition.DefaultValue.Dispatch(this);
          propertyBuilder.SetConstant(this.value);
        }
        foreach (var customAttribute in propertyDefinition.Attributes) {
          var customAttributeBuilder = this.GetCustomAttributeBuilder(customAttribute);
          propertyBuilder.SetCustomAttribute(customAttributeBuilder);
        }
      }

      /// <summary>
      /// Performs some computation with the given namespace type definition.
      /// </summary>
      /// <param name="namespaceTypeDefinition"></param>
      public override void Visit(INamespaceTypeDefinition namespaceTypeDefinition) {
        var builder = (TypeBuilder)this.loader.builderMap[namespaceTypeDefinition];
        this.Visit(namespaceTypeDefinition, builder);
      }

      /// <summary>
      /// Performs some computation with the given nested type definition.
      /// </summary>
      /// <param name="nestedTypeDefinition"></param>
      public override void Visit(INestedTypeDefinition nestedTypeDefinition) {
        var builder = (TypeBuilder)this.loader.builderMap[nestedTypeDefinition];
        this.Visit(nestedTypeDefinition, builder);
      }

      private void Visit(ITypeDefinition typeDefinition, TypeBuilder typeBuilder) {
        if (typeDefinition.HasDeclarativeSecurity) {
          foreach (var securityAttribute in typeDefinition.SecurityAttributes)
            typeBuilder.AddDeclarativeSecurity(GetSecurityAction(securityAttribute), GetPermissionSet(securityAttribute));
        }
        foreach (var implementedInterface in typeDefinition.Interfaces)
          typeBuilder.AddInterfaceImplementation(this.loader.mapper.GetType(implementedInterface));
        foreach (var explicitOverride in typeDefinition.ExplicitImplementationOverrides) {
          typeBuilder.DefineMethodOverride((MethodInfo)this.loader.mapper.GetMethod(explicitOverride.ImplementingMethod),
            (MethodInfo)this.loader.mapper.GetMethod(explicitOverride.ImplementedMethod));
        }
        if (typeDefinition.IsGeneric) {
          foreach (var genericParameter in typeDefinition.GenericParameters)
            this.Visit(genericParameter);
        }
        foreach (var customAttribute in typeDefinition.Attributes) {
          var customAttributeBuilder = this.GetCustomAttributeBuilder(customAttribute);
          typeBuilder.SetCustomAttribute(customAttributeBuilder);
        }
        foreach (var baseType in typeDefinition.BaseClasses) {
          typeBuilder.SetParent(this.loader.mapper.GetType(baseType));
          break;
        }
      }

      internal void EmitIL(System.Reflection.Emit.ILGenerator ilGenerator, IMethodBody methodBody) {
        //this is painful and slow, but it seems to be the only way to get debugging information into a dynamic assembly.
        this.ilGenerator = ilGenerator;
        this.methodBody = methodBody;
        this.CreateLabelsForBranchTargets();
        this.CreateLocalBuilders();
        this.EmitNamespaceScopes();
        foreach (IOperation operation in methodBody.Operations) {
          this.CallAppropriateBeginsAndEnds(operation.Offset);
          this.EmitPdbInformationFor(operation);
          Label label;
          if (labelFor.TryGetValue((uint)operation.Offset, out label)) ilGenerator.MarkLabel(label);
          switch (operation.OperationCode) {
            case OperationCode.Array_Addr:
              ilGenerator.Emit(OpCodes.Call, this.loader.mapper.GetArrayAddrMethod((IArrayTypeReference)operation.Value, this.loader.ModuleBuilder));
              continue;
            case OperationCode.Array_Get:
              ilGenerator.Emit(OpCodes.Call, this.loader.mapper.GetArrayGetMethod((IArrayTypeReference)operation.Value, this.loader.ModuleBuilder));
              continue;
            case OperationCode.Array_Set:
              ilGenerator.Emit(OpCodes.Call, this.loader.mapper.GetArraySetMethod((IArrayTypeReference)operation.Value, this.loader.ModuleBuilder));
              continue;
            case OperationCode.Array_Create:
              ilGenerator.Emit(OpCodes.Newobj, this.loader.mapper.GetArrayCreateMethod((IArrayTypeReference)operation.Value, this.loader.ModuleBuilder));
              continue;
            case OperationCode.Array_Create_WithLowerBound:
              ilGenerator.Emit(OpCodes.Newobj, this.loader.mapper.GetArrayCreateWithLowerBoundsMethod((IArrayTypeReference)operation.Value, this.loader.ModuleBuilder));
              continue;
            case OperationCode.Beq:
            case OperationCode.Bge:
            case OperationCode.Bge_Un:
            case OperationCode.Bgt:
            case OperationCode.Bgt_Un:
            case OperationCode.Ble:
            case OperationCode.Ble_Un:
            case OperationCode.Blt:
            case OperationCode.Blt_Un:
            case OperationCode.Bne_Un:
            case OperationCode.Br:
            case OperationCode.Brfalse:
            case OperationCode.Brtrue:
            case OperationCode.Leave:
            case OperationCode.Beq_S:
            case OperationCode.Bge_S:
            case OperationCode.Bge_Un_S:
            case OperationCode.Bgt_S:
            case OperationCode.Bgt_Un_S:
            case OperationCode.Ble_S:
            case OperationCode.Ble_Un_S:
            case OperationCode.Blt_S:
            case OperationCode.Blt_Un_S:
            case OperationCode.Bne_Un_S:
            case OperationCode.Br_S:
            case OperationCode.Brfalse_S:
            case OperationCode.Brtrue_S:
            case OperationCode.Leave_S:
              //^ assume operation.Value is uint;
              ilGenerator.Emit(OpCodeFor(operation.OperationCode), labelFor[(uint)operation.Value]);
              continue;
            case OperationCode.Box:
            case OperationCode.Castclass:
            case OperationCode.Constrained_:
            case OperationCode.Cpobj:
            case OperationCode.Initobj:
            case OperationCode.Isinst:
            case OperationCode.Ldelem:
            case OperationCode.Ldelema:
            case OperationCode.Ldobj:
            case OperationCode.Mkrefany:
            case OperationCode.Refanyval:
            case OperationCode.Sizeof:
            case OperationCode.Stelem:
            case OperationCode.Stobj:
            case OperationCode.Unbox:
            case OperationCode.Unbox_Any:
              //^ assume operation.Value is ITypeReference;
              ilGenerator.Emit(OpCodeFor(operation.OperationCode), this.loader.mapper.GetType((ITypeReference)operation.Value));
              continue;
            case OperationCode.Call:
            case OperationCode.Callvirt:
            case OperationCode.Jmp:
            case OperationCode.Ldftn:
            case OperationCode.Ldvirtftn:
              //TODO: if the reference has extra arguments, use EmitCall
              //^ assume operation.Value is IMethodReference;
              var methodBase = this.loader.mapper.GetMethod((IMethodReference)operation.Value);
              if (methodBase.IsConstructor)
                ilGenerator.Emit(OpCodeFor(operation.OperationCode), (ConstructorInfo)methodBase);
              else
                ilGenerator.Emit(OpCodeFor(operation.OperationCode), (MethodInfo)methodBase);
              break;
            case OperationCode.Newobj:
              //^ assume operation.Value is IMethodReference;
              ilGenerator.Emit(OpCodes.Newobj, (ConstructorInfo)this.loader.mapper.GetMethod((IMethodReference)operation.Value));
              break;
            case OperationCode.Calli:
              //^ assume operation.Value is IFunctionPointerTypeReference;
              var functionPointer = (IFunctionPointerTypeReference)operation.Value;
              var callingConvention = MemberBuilderAllocator.GetCallingConvention(functionPointer.CallingConvention);
              var returnType = loader.mapper.GetType(functionPointer.Type);
              var parameterTypes = new Type[IteratorHelper.EnumerableCount(functionPointer.Parameters)];
              int i = 0; foreach (var parameter in functionPointer.Parameters) parameterTypes[i++] = loader.mapper.GetType(parameter.Type);
              var optionalParameterTypes = new Type[IteratorHelper.EnumerableCount(functionPointer.ExtraArgumentTypes)];
              i = 0; foreach (var parameter in functionPointer.ExtraArgumentTypes) parameterTypes[i++] = loader.mapper.GetType(parameter.Type);
              ilGenerator.EmitCalli(OpCodes.Calli, callingConvention, returnType, parameterTypes, optionalParameterTypes);
              continue;
            case OperationCode.Ldarg:
            case OperationCode.Ldarga:
            case OperationCode.Starg:
              if (operation.Value == null) //it's the this arg, which does not have an IParameterDefinition
                ilGenerator.Emit(OpCodeFor(operation.OperationCode), (short)0);
              else
                ilGenerator.Emit(OpCodeFor(operation.OperationCode), GetParameterIndex((IParameterDefinition)operation.Value));
              continue;
            case OperationCode.Ldarg_S:
            case OperationCode.Ldarga_S:
            case OperationCode.Starg_S:
              if (operation.Value == null) //it's the this arg, which does not have an IParameterDefinition
                ilGenerator.Emit(OpCodeFor(operation.OperationCode), (byte)0);
              else
                ilGenerator.Emit(OpCodeFor(operation.OperationCode), (byte)GetParameterIndex((IParameterDefinition)operation.Value));
              continue;
            case OperationCode.Ldc_I4:
              //^ assume operation.Value is int;
              ilGenerator.Emit(OpCodes.Ldc_I4, (int)operation.Value);
              continue;
            case OperationCode.Ldc_I4_S:
              //^ assume operation.Value is int;
              ilGenerator.Emit(OpCodes.Ldc_I4_S, (sbyte)(int)operation.Value);
              continue;
            case OperationCode.Ldc_I8:
              //^ assume operation.Value is long;
              ilGenerator.Emit(OpCodes.Ldc_I8, (long)operation.Value);
              continue;
            case OperationCode.Ldc_R4:
              //^ assume operation.Value is float;
              ilGenerator.Emit(OpCodes.Ldc_R4, (float)operation.Value);
              continue;
            case OperationCode.Ldc_R8:
              //^ assume operation.Value is double;
              ilGenerator.Emit(OpCodes.Ldc_R8, (double)operation.Value);
              continue;
            case OperationCode.Ldfld:
            case OperationCode.Ldflda:
            case OperationCode.Ldsfld:
            case OperationCode.Ldsflda:
            case OperationCode.Stfld:
            case OperationCode.Stsfld:
              //^ assume operation.Value is IFieldReference;
              ilGenerator.Emit(OpCodeFor(operation.OperationCode), this.loader.mapper.GetField((IFieldReference)operation.Value));
              continue;
            case OperationCode.Ldloc:
            case OperationCode.Ldloca:
            case OperationCode.Stloc:
            case OperationCode.Ldloc_S:
            case OperationCode.Ldloca_S:
            case OperationCode.Stloc_S:
              //^ assume operation.Value is ILocalDefinition;
              ilGenerator.Emit(OpCodeFor(operation.OperationCode), localFor[(ILocalDefinition)operation.Value]);
              continue;
            case OperationCode.Ldstr:
              //^ assume operation.Value is string;
              ilGenerator.Emit(OpCodes.Ldstr, (string)operation.Value);
              continue;
            case OperationCode.Ldtoken:
              IFieldReference/*?*/ fieldRef = operation.Value as IFieldReference;
              if (fieldRef != null) goto case OperationCode.Ldfld;
              IMethodReference/*?*/ methodRef = operation.Value as IMethodReference;
              if (methodRef != null) goto case OperationCode.Call;
              ilGenerator.Emit(OpCodes.Ldtoken, this.loader.mapper.GetType((ITypeReference)operation.Value));
              continue;
            case OperationCode.Newarr:
              //^ assume operation.Value is IArrayTypeReference;
              ilGenerator.Emit(OpCodes.Newarr, this.loader.mapper.GetType(((IArrayTypeReference)operation.Value).ElementType));
              continue;
            //case OperationCode.No_:
            //  //^ assume operation.Value is OperationCheckFlags;
            //  writer.WriteByte((byte)(OperationCheckFlags)operation.Value); break;
            case OperationCode.Switch:
              //^ assume operation.Value is uint[];
              uint[] targets = (uint[])operation.Value;
              Label[] labels = new Label[targets.Length];
              for (int j = 0; j < targets.Length; j++) labels[j] = labelFor[targets[j]];
              ilGenerator.Emit(OpCodes.Switch, labels);
              continue;
            case OperationCode.Unaligned_:
              //^ assume operation.Value is byte;
              ilGenerator.Emit(OpCodes.Unaligned, (byte)operation.Value);
              continue;
            default:
              ilGenerator.Emit(OpCodeFor(operation.OperationCode));
              break;
          }
        }
        this.CallAppropriateBeginsAndEnds((uint)ilGenerator.ILOffset);
        this.ilGenerator = null;
        this.methodBody = null;
        this.labelFor = null;
        this.localFor = null;
      }

      private void EmitNamespaceScopes() {
        if (this.localScopeProvider != null) {
          foreach (INamespaceScope namespaceScope in this.localScopeProvider.GetNamespaceScopes(this.methodBody)) {
            foreach (var usedNamespace in namespaceScope.UsedNamespaces) {
              if (string.IsNullOrEmpty(usedNamespace.Alias.Value))
                this.ilGenerator.UsingNamespace(usedNamespace.NamespaceName.Value);
              //it is not clear that there is a way to emit information about namespace aliases using Reflection.Emit.
            }
          }
          this.scopeOffsets = new List<uint>();
          foreach (ILocalScope scope in this.localScopeProvider.GetLocalScopes(this.methodBody)) {
            this.scopeOffsets.Add(scope.Offset);
            this.scopeOffsets.Add(scope.Offset+scope.Length);
          }
        }
      }

      List<uint> exceptionOffsets;
      List<uint>/*?*/ scopeOffsets;

      private void CallAppropriateBeginsAndEnds(uint offset) {
        if (this.exceptionOffsets.Contains(offset)) {
          foreach (var exceptionInformation in this.methodBody.OperationExceptionInformation) {
            if (exceptionInformation.TryStartOffset == offset) {
              this.ilGenerator.BeginExceptionBlock(); //TODO: do we need to do anything with the label?
            } else if (exceptionInformation.FilterDecisionStartOffset == offset) {
              this.ilGenerator.BeginExceptFilterBlock();
            } else if (exceptionInformation.HandlerStartOffset == offset) {
              switch (exceptionInformation.HandlerKind) {
                case HandlerKind.Catch: this.ilGenerator.BeginCatchBlock(this.loader.mapper.GetType(exceptionInformation.ExceptionType)); break;
                case HandlerKind.Fault: this.ilGenerator.BeginFaultBlock(); break;
                case HandlerKind.Filter: this.ilGenerator.BeginCatchBlock(null); break;
                case HandlerKind.Finally: this.ilGenerator.BeginFinallyBlock(); break;
              }
            } else if (exceptionInformation.HandlerEndOffset == offset) {
              this.ilGenerator.EndExceptionBlock();
            }
          }
        }
        if (this.scopeOffsets != null && this.scopeOffsets.Contains(offset)) {
          foreach (ILocalScope scope in this.localScopeProvider.GetLocalScopes(this.methodBody)) {
            if (scope.Offset == offset)
              this.ilGenerator.BeginScope();
            else if (scope.Offset+scope.Length == offset)
              this.ilGenerator.EndScope();
          }
        }
      }

      private void CreateLabelsForBranchTargets() {
        this.exceptionOffsets = new List<uint>();
        foreach (var exceptionInformation in this.methodBody.OperationExceptionInformation) {
          this.exceptionOffsets.Add(exceptionInformation.TryStartOffset);
          this.exceptionOffsets.Add(exceptionInformation.HandlerStartOffset);
          if (exceptionInformation.HandlerKind == HandlerKind.Filter)
            this.exceptionOffsets.Add(exceptionInformation.FilterDecisionStartOffset);
          this.exceptionOffsets.Add(exceptionInformation.HandlerEndOffset);
        }
        this.labelFor = new Dictionary<uint, Label>();
        foreach (var operation in this.methodBody.Operations) {
          switch (operation.OperationCode) {
            case OperationCode.Beq:
            case OperationCode.Bge:
            case OperationCode.Bge_Un:
            case OperationCode.Bgt:
            case OperationCode.Bgt_Un:
            case OperationCode.Ble:
            case OperationCode.Ble_Un:
            case OperationCode.Blt:
            case OperationCode.Blt_Un:
            case OperationCode.Bne_Un:
            case OperationCode.Br:
            case OperationCode.Brfalse:
            case OperationCode.Brtrue:
            case OperationCode.Leave:
            case OperationCode.Beq_S:
            case OperationCode.Bge_S:
            case OperationCode.Bge_Un_S:
            case OperationCode.Bgt_S:
            case OperationCode.Bgt_Un_S:
            case OperationCode.Ble_S:
            case OperationCode.Ble_Un_S:
            case OperationCode.Blt_S:
            case OperationCode.Blt_Un_S:
            case OperationCode.Bne_Un_S:
            case OperationCode.Br_S:
            case OperationCode.Brfalse_S:
            case OperationCode.Brtrue_S:
            case OperationCode.Leave_S:
              this.labelFor[(uint)operation.Value] = this.ilGenerator.DefineLabel();
              continue;
          }
        }
      }

      private void CreateLocalBuilders() {
        Dictionary<ILocalDefinition, ILocalScope> scopeFor = null;
        if (this.localScopeProvider != null) {
          scopeFor = new Dictionary<ILocalDefinition, ILocalScope>();
          foreach (var localScope in this.localScopeProvider.GetLocalScopes(this.methodBody)) {
            foreach (var localDefinition in this.localScopeProvider.GetVariablesInScope(localScope)) {
              scopeFor[localDefinition] = localScope;
            }
          }
        }
        this.localFor = new Dictionary<ILocalDefinition, LocalBuilder>();
        foreach (var localDefinition in this.methodBody.LocalVariables) {
          var type = this.loader.mapper.GetType(localDefinition.Type);
          if (localDefinition.IsReference) type = type.MakeByRefType();
          var localBuilder = this.ilGenerator.DeclareLocal(type, localDefinition.IsPinned); //there seems to be no way to emit modifiers
          if (this.sourceLocationProvider != null) {
            bool isCompilerGenerated; //no way to communicate this to Reflection.Emit, it seems.
            var localName = this.sourceLocationProvider.GetSourceNameFor(localDefinition, out isCompilerGenerated);
            ILocalScope scope = null;
            if (scopeFor != null && scopeFor.TryGetValue(localDefinition, out scope))
              localBuilder.SetLocalSymInfo(localName, (int)scope.Offset, (int)(scope.Offset+scope.Length));
            else
              localBuilder.SetLocalSymInfo(localName);
          }
          this.localFor[localDefinition] = localBuilder;
        }
      }

      private void EmitPdbInformationFor(IOperation operation) {
        if (this.sourceLocationProvider != null) {
          foreach (var sloc in this.sourceLocationProvider.GetPrimarySourceLocationsFor(operation.Location)) {
            ISymbolDocumentWriter document = this.GetDocument(sloc.Document);
            this.ilGenerator.MarkSequencePoint(document, sloc.StartLine, sloc.StartColumn, sloc.EndLine, sloc.EndColumn);
            return;
          }
        }
      }

      private ISymbolDocumentWriter GetDocument(IDocument document) {
        if (this.currentDocument == document) return this.currentDocumentWriter;
        this.currentDocument = document;
        if (this.documentMap.TryGetValue(document, out this.currentDocumentWriter)) return this.currentDocumentWriter;
        var language = SymLanguageType.CSharp;
        var vendor = SymLanguageVendor.Microsoft;
        var type = SymDocumentType.Text;
        var sourceDocument = document as IPrimarySourceDocument;
        if (document != null) {
          language = sourceDocument.Language;
          vendor = sourceDocument.LanguageVendor;
          type = sourceDocument.DocumentType;
        }
        this.currentDocumentWriter = this.loader.ModuleBuilder.DefineDocument(document.Location, language, vendor, type);
        return this.currentDocumentWriter;
      }

      private OpCode OpCodeFor(OperationCode operationCode) {
        switch (operationCode) {
          case OperationCode.Add: return OpCodes.Add;
          case OperationCode.Add_Ovf: return OpCodes.Add_Ovf;
          case OperationCode.Add_Ovf_Un: return OpCodes.Add_Ovf_Un;
          case OperationCode.And: return OpCodes.And;
          case OperationCode.Arglist: return OpCodes.Arglist;
          //case OperationCode.Array_Addr: return OpCodes.Array_Addr;
          //case OperationCode.Array_Create: return OpCodes.Array_Create;
          //case OperationCode.Array_Create_WithLowerBound: return OpCodes.Array_Create_WithLowerBound;
          //case OperationCode.Array_Get: return OpCodes.Array_Get;
          //case OperationCode.Array_Set: return OpCodes.Array_Set;
          case OperationCode.Beq: return OpCodes.Beq;
          case OperationCode.Beq_S: return OpCodes.Beq_S;
          case OperationCode.Bge: return OpCodes.Bge;
          case OperationCode.Bge_S: return OpCodes.Bge_S;
          case OperationCode.Bge_Un: return OpCodes.Bge_Un;
          case OperationCode.Bge_Un_S: return OpCodes.Bge_Un_S;
          case OperationCode.Bgt: return OpCodes.Bgt;
          case OperationCode.Bgt_S: return OpCodes.Bgt_S;
          case OperationCode.Bgt_Un: return OpCodes.Bgt_Un;
          case OperationCode.Bgt_Un_S: return OpCodes.Bgt_Un_S;
          case OperationCode.Ble: return OpCodes.Ble;
          case OperationCode.Ble_S: return OpCodes.Ble_S;
          case OperationCode.Ble_Un: return OpCodes.Ble_Un;
          case OperationCode.Ble_Un_S: return OpCodes.Ble_Un_S;
          case OperationCode.Blt: return OpCodes.Blt;
          case OperationCode.Blt_S: return OpCodes.Blt_S;
          case OperationCode.Blt_Un: return OpCodes.Blt_Un;
          case OperationCode.Blt_Un_S: return OpCodes.Blt_Un_S;
          case OperationCode.Bne_Un: return OpCodes.Bne_Un;
          case OperationCode.Bne_Un_S: return OpCodes.Bne_Un_S;
          case OperationCode.Box: return OpCodes.Box;
          case OperationCode.Br: return OpCodes.Br;
          case OperationCode.Br_S: return OpCodes.Br_S;
          case OperationCode.Break: return OpCodes.Break;
          case OperationCode.Brfalse: return OpCodes.Brfalse;
          case OperationCode.Brfalse_S: return OpCodes.Brfalse_S;
          case OperationCode.Brtrue: return OpCodes.Brtrue;
          case OperationCode.Brtrue_S: return OpCodes.Brtrue_S;
          case OperationCode.Call: return OpCodes.Call;
          case OperationCode.Calli: return OpCodes.Calli;
          case OperationCode.Callvirt: return OpCodes.Callvirt;
          case OperationCode.Castclass: return OpCodes.Castclass;
          case OperationCode.Ceq: return OpCodes.Ceq;
          case OperationCode.Cgt: return OpCodes.Cgt;
          case OperationCode.Cgt_Un: return OpCodes.Cgt_Un;
          case OperationCode.Ckfinite: return OpCodes.Ckfinite;
          case OperationCode.Clt: return OpCodes.Clt;
          case OperationCode.Clt_Un: return OpCodes.Clt_Un;
          case OperationCode.Constrained_: return OpCodes.Constrained;
          case OperationCode.Conv_I: return OpCodes.Conv_I;
          case OperationCode.Conv_I1: return OpCodes.Conv_I1;
          case OperationCode.Conv_I2: return OpCodes.Conv_I2;
          case OperationCode.Conv_I4: return OpCodes.Conv_I4;
          case OperationCode.Conv_I8: return OpCodes.Conv_I8;
          case OperationCode.Conv_Ovf_I: return OpCodes.Conv_Ovf_I;
          case OperationCode.Conv_Ovf_I_Un: return OpCodes.Conv_Ovf_I_Un;
          case OperationCode.Conv_Ovf_I1: return OpCodes.Conv_Ovf_I1;
          case OperationCode.Conv_Ovf_I1_Un: return OpCodes.Conv_Ovf_I1_Un;
          case OperationCode.Conv_Ovf_I2: return OpCodes.Conv_Ovf_I2;
          case OperationCode.Conv_Ovf_I2_Un: return OpCodes.Conv_Ovf_I2_Un;
          case OperationCode.Conv_Ovf_I4: return OpCodes.Conv_Ovf_I4;
          case OperationCode.Conv_Ovf_I4_Un: return OpCodes.Conv_Ovf_I4_Un;
          case OperationCode.Conv_Ovf_I8: return OpCodes.Conv_Ovf_I8;
          case OperationCode.Conv_Ovf_I8_Un: return OpCodes.Conv_Ovf_I8_Un;
          case OperationCode.Conv_Ovf_U: return OpCodes.Conv_Ovf_U;
          case OperationCode.Conv_Ovf_U_Un: return OpCodes.Conv_Ovf_U_Un;
          case OperationCode.Conv_Ovf_U1: return OpCodes.Conv_Ovf_U1;
          case OperationCode.Conv_Ovf_U1_Un: return OpCodes.Conv_Ovf_U1_Un;
          case OperationCode.Conv_Ovf_U2: return OpCodes.Conv_Ovf_U2;
          case OperationCode.Conv_Ovf_U2_Un: return OpCodes.Conv_Ovf_U2_Un;
          case OperationCode.Conv_Ovf_U4: return OpCodes.Conv_Ovf_U4;
          case OperationCode.Conv_Ovf_U4_Un: return OpCodes.Conv_Ovf_U4_Un;
          case OperationCode.Conv_Ovf_U8: return OpCodes.Conv_Ovf_U8;
          case OperationCode.Conv_Ovf_U8_Un: return OpCodes.Conv_Ovf_U8_Un;
          case OperationCode.Conv_R_Un: return OpCodes.Conv_R_Un;
          case OperationCode.Conv_R4: return OpCodes.Conv_R4;
          case OperationCode.Conv_R8: return OpCodes.Conv_R8;
          case OperationCode.Conv_U: return OpCodes.Conv_U;
          case OperationCode.Conv_U1: return OpCodes.Conv_U1;
          case OperationCode.Conv_U2: return OpCodes.Conv_U2;
          case OperationCode.Conv_U4: return OpCodes.Conv_U4;
          case OperationCode.Conv_U8: return OpCodes.Conv_U8;
          case OperationCode.Cpblk: return OpCodes.Cpblk;
          case OperationCode.Cpobj: return OpCodes.Cpobj;
          case OperationCode.Div: return OpCodes.Div;
          case OperationCode.Div_Un: return OpCodes.Div_Un;
          case OperationCode.Dup: return OpCodes.Dup;
          case OperationCode.Endfilter: return OpCodes.Endfilter;
          case OperationCode.Endfinally: return OpCodes.Endfinally;
          case OperationCode.Initblk: return OpCodes.Initblk;
          case OperationCode.Initobj: return OpCodes.Initobj;
          case OperationCode.Isinst: return OpCodes.Isinst;
          case OperationCode.Jmp: return OpCodes.Jmp;
          case OperationCode.Ldarg: return OpCodes.Ldarg;
          case OperationCode.Ldarg_0: return OpCodes.Ldarg_0;
          case OperationCode.Ldarg_1: return OpCodes.Ldarg_1;
          case OperationCode.Ldarg_2: return OpCodes.Ldarg_2;
          case OperationCode.Ldarg_3: return OpCodes.Ldarg_3;
          case OperationCode.Ldarg_S: return OpCodes.Ldarg_S;
          case OperationCode.Ldarga: return OpCodes.Ldarga;
          case OperationCode.Ldarga_S: return OpCodes.Ldarga_S;
          case OperationCode.Ldc_I4: return OpCodes.Ldc_I4;
          case OperationCode.Ldc_I4_0: return OpCodes.Ldc_I4_0;
          case OperationCode.Ldc_I4_1: return OpCodes.Ldc_I4_1;
          case OperationCode.Ldc_I4_2: return OpCodes.Ldc_I4_2;
          case OperationCode.Ldc_I4_3: return OpCodes.Ldc_I4_3;
          case OperationCode.Ldc_I4_4: return OpCodes.Ldc_I4_4;
          case OperationCode.Ldc_I4_5: return OpCodes.Ldc_I4_5;
          case OperationCode.Ldc_I4_6: return OpCodes.Ldc_I4_6;
          case OperationCode.Ldc_I4_7: return OpCodes.Ldc_I4_7;
          case OperationCode.Ldc_I4_8: return OpCodes.Ldc_I4_8;
          case OperationCode.Ldc_I4_M1: return OpCodes.Ldc_I4_M1;
          case OperationCode.Ldc_I4_S: return OpCodes.Ldc_I4_S;
          case OperationCode.Ldc_I8: return OpCodes.Ldc_I8;
          case OperationCode.Ldc_R4: return OpCodes.Ldc_R4;
          case OperationCode.Ldc_R8: return OpCodes.Ldc_R8;
          case OperationCode.Ldelem: return OpCodes.Ldelem;
          case OperationCode.Ldelem_I: return OpCodes.Ldelem_I;
          case OperationCode.Ldelem_I1: return OpCodes.Ldelem_I1;
          case OperationCode.Ldelem_I2: return OpCodes.Ldelem_I2;
          case OperationCode.Ldelem_I4: return OpCodes.Ldelem_I4;
          case OperationCode.Ldelem_I8: return OpCodes.Ldelem_I8;
          case OperationCode.Ldelem_R4: return OpCodes.Ldelem_R4;
          case OperationCode.Ldelem_R8: return OpCodes.Ldelem_R8;
          case OperationCode.Ldelem_Ref: return OpCodes.Ldelem_Ref;
          case OperationCode.Ldelem_U1: return OpCodes.Ldelem_U1;
          case OperationCode.Ldelem_U2: return OpCodes.Ldelem_U2;
          case OperationCode.Ldelem_U4: return OpCodes.Ldelem_U4;
          case OperationCode.Ldelema: return OpCodes.Ldelema;
          case OperationCode.Ldfld: return OpCodes.Ldfld;
          case OperationCode.Ldflda: return OpCodes.Ldflda;
          case OperationCode.Ldftn: return OpCodes.Ldftn;
          case OperationCode.Ldind_I: return OpCodes.Ldind_I;
          case OperationCode.Ldind_I1: return OpCodes.Ldind_I1;
          case OperationCode.Ldind_I2: return OpCodes.Ldind_I2;
          case OperationCode.Ldind_I4: return OpCodes.Ldind_I4;
          case OperationCode.Ldind_I8: return OpCodes.Ldind_I8;
          case OperationCode.Ldind_R4: return OpCodes.Ldind_R4;
          case OperationCode.Ldind_R8: return OpCodes.Ldind_R8;
          case OperationCode.Ldind_Ref: return OpCodes.Ldind_Ref;
          case OperationCode.Ldind_U1: return OpCodes.Ldind_U1;
          case OperationCode.Ldind_U2: return OpCodes.Ldind_U2;
          case OperationCode.Ldind_U4: return OpCodes.Ldind_U4;
          case OperationCode.Ldlen: return OpCodes.Ldlen;
          case OperationCode.Ldloc: return OpCodes.Ldloc;
          case OperationCode.Ldloc_0: return OpCodes.Ldloc_0;
          case OperationCode.Ldloc_1: return OpCodes.Ldloc_1;
          case OperationCode.Ldloc_2: return OpCodes.Ldloc_2;
          case OperationCode.Ldloc_3: return OpCodes.Ldloc_3;
          case OperationCode.Ldloc_S: return OpCodes.Ldloc_S;
          case OperationCode.Ldloca: return OpCodes.Ldloca;
          case OperationCode.Ldloca_S: return OpCodes.Ldloca_S;
          case OperationCode.Ldnull: return OpCodes.Ldnull;
          case OperationCode.Ldobj: return OpCodes.Ldobj;
          case OperationCode.Ldsfld: return OpCodes.Ldsfld;
          case OperationCode.Ldsflda: return OpCodes.Ldsflda;
          case OperationCode.Ldstr: return OpCodes.Ldstr;
          case OperationCode.Ldtoken: return OpCodes.Ldtoken;
          case OperationCode.Ldvirtftn: return OpCodes.Ldvirtftn;
          case OperationCode.Leave: return OpCodes.Leave;
          case OperationCode.Leave_S: return OpCodes.Leave_S;
          case OperationCode.Localloc: return OpCodes.Localloc;
          case OperationCode.Mkrefany: return OpCodes.Mkrefany;
          case OperationCode.Mul: return OpCodes.Mul;
          case OperationCode.Mul_Ovf: return OpCodes.Mul_Ovf;
          case OperationCode.Mul_Ovf_Un: return OpCodes.Mul_Ovf_Un;
          case OperationCode.Neg: return OpCodes.Neg;
          case OperationCode.Newarr: return OpCodes.Newarr;
          case OperationCode.Newobj: return OpCodes.Newobj;
          //case OperationCode.No_: return OpCodes.No_;
          case OperationCode.Nop: return OpCodes.Nop;
          case OperationCode.Not: return OpCodes.Not;
          case OperationCode.Or: return OpCodes.Or;
          case OperationCode.Pop: return OpCodes.Pop;
          case OperationCode.Readonly_: return OpCodes.Readonly;
          case OperationCode.Refanytype: return OpCodes.Refanytype;
          case OperationCode.Refanyval: return OpCodes.Refanyval;
          case OperationCode.Rem: return OpCodes.Rem;
          case OperationCode.Rem_Un: return OpCodes.Rem_Un;
          case OperationCode.Ret: return OpCodes.Ret;
          case OperationCode.Rethrow: return OpCodes.Rethrow;
          case OperationCode.Shl: return OpCodes.Shl;
          case OperationCode.Shr: return OpCodes.Shr;
          case OperationCode.Shr_Un: return OpCodes.Shr_Un;
          case OperationCode.Sizeof: return OpCodes.Sizeof;
          case OperationCode.Starg: return OpCodes.Starg;
          case OperationCode.Starg_S: return OpCodes.Starg_S;
          case OperationCode.Stelem: return OpCodes.Stelem;
          case OperationCode.Stelem_I: return OpCodes.Stelem_I;
          case OperationCode.Stelem_I1: return OpCodes.Stelem_I1;
          case OperationCode.Stelem_I2: return OpCodes.Stelem_I2;
          case OperationCode.Stelem_I4: return OpCodes.Stelem_I4;
          case OperationCode.Stelem_I8: return OpCodes.Stelem_I8;
          case OperationCode.Stelem_R4: return OpCodes.Stelem_R4;
          case OperationCode.Stelem_R8: return OpCodes.Stelem_R8;
          case OperationCode.Stelem_Ref: return OpCodes.Stelem_Ref;
          case OperationCode.Stfld: return OpCodes.Stfld;
          case OperationCode.Stind_I: return OpCodes.Stind_I;
          case OperationCode.Stind_I1: return OpCodes.Stind_I1;
          case OperationCode.Stind_I2: return OpCodes.Stind_I2;
          case OperationCode.Stind_I4: return OpCodes.Stind_I4;
          case OperationCode.Stind_I8: return OpCodes.Stind_I8;
          case OperationCode.Stind_R4: return OpCodes.Stind_R4;
          case OperationCode.Stind_R8: return OpCodes.Stind_R8;
          case OperationCode.Stind_Ref: return OpCodes.Stind_Ref;
          case OperationCode.Stloc: return OpCodes.Stloc;
          case OperationCode.Stloc_0: return OpCodes.Stloc_0;
          case OperationCode.Stloc_1: return OpCodes.Stloc_1;
          case OperationCode.Stloc_2: return OpCodes.Stloc_2;
          case OperationCode.Stloc_3: return OpCodes.Stloc_3;
          case OperationCode.Stloc_S: return OpCodes.Stloc_S;
          case OperationCode.Stobj: return OpCodes.Stobj;
          case OperationCode.Stsfld: return OpCodes.Stsfld;
          case OperationCode.Sub: return OpCodes.Sub;
          case OperationCode.Sub_Ovf: return OpCodes.Sub_Ovf;
          case OperationCode.Sub_Ovf_Un: return OpCodes.Sub_Ovf_Un;
          case OperationCode.Switch: return OpCodes.Switch;
          case OperationCode.Tail_: return OpCodes.Tailcall;
          case OperationCode.Throw: return OpCodes.Throw;
          case OperationCode.Unaligned_: return OpCodes.Unaligned;
          case OperationCode.Unbox: return OpCodes.Unbox;
          case OperationCode.Unbox_Any: return OpCodes.Unbox_Any;
          case OperationCode.Volatile_: return OpCodes.Volatile;
          case OperationCode.Xor: return OpCodes.Xor;
          default: return OpCodes.Prefix1; //a dummy
        }
      }

      private static ushort GetParameterIndex(IParameterDefinition parameterDefinition) {
        ushort parameterIndex = (ushort)parameterDefinition.Index;
        if (!parameterDefinition.ContainingSignature.IsStatic) parameterIndex++;
        return parameterIndex;
      }

    }

    /// <summary>
    /// If given a type definition that has an uncreated type builder associated with it, 
    /// this traverser first attempts to create all types referenced by the type in its base
    /// class and interfaces, then it creates the type. This does not work when there are
    /// cyclic dependencies.
    /// </summary>
    class TypeCreator : MetadataTraverser {

      internal TypeCreator(DynamicLoader loader) {
        this.loader = loader;
      }

      DynamicLoader loader;

      private void CreateTypesThatNeedToBeLoadedBeforeLoading(ITypeDefinition typeDefinition) {
        if (typeDefinition.IsGeneric) {
          foreach (var genPar in typeDefinition.GenericParameters)
            this.Traverse(genPar);
        }
        foreach (var baseClass in typeDefinition.BaseClasses)
          this.Traverse(baseClass);
        foreach (var interf in typeDefinition.Interfaces)
          this.Traverse(interf);
        foreach (var field in typeDefinition.Fields) {
          if (field.Type.IsValueType)
            this.Traverse(field.Type);
        }
      }

      /// <summary>
      /// Traverses the children of the namespace type definition.
      /// </summary>
      /// <param name="namespaceTypeDefinition"></param>
      public override void TraverseChildren(INamespaceTypeDefinition namespaceTypeDefinition) {
        object builder;
        if (!this.loader.builderMap.TryGetValue(namespaceTypeDefinition, out builder)) return;
        this.loader.builderMap.Remove(namespaceTypeDefinition);
        var typeBuilder = builder as TypeBuilder;
        if (typeBuilder == null) return;
        this.CreateTypesThatNeedToBeLoadedBeforeLoading(namespaceTypeDefinition);
        var type = typeBuilder.CreateType();
        this.loader.mapper.DefineMapping(namespaceTypeDefinition, type);
        this.Traverse(namespaceTypeDefinition.NestedTypes);
      }

      /// <summary>
      /// Traverses the children of the namespace type reference.
      /// </summary>
      /// <param name="namespaceTypeReference"></param>
      public override void TraverseChildren(INamespaceTypeReference namespaceTypeReference) {
        this.TraverseChildren(namespaceTypeReference.ResolvedType);
      }

      /// <summary>
      /// Traverses the children of the nested type definition.
      /// </summary>
      /// <param name="nestedTypeDefinition"></param>
      public override void TraverseChildren(INestedTypeDefinition nestedTypeDefinition) {
        object builder;
        if (!this.loader.builderMap.TryGetValue(nestedTypeDefinition, out builder)) return;
        this.loader.builderMap.Remove(nestedTypeDefinition);
        var typeBuilder = builder as TypeBuilder;
        if (typeBuilder == null) return;
        this.CreateTypesThatNeedToBeLoadedBeforeLoading(nestedTypeDefinition);
        var type = typeBuilder.CreateType();
        this.loader.mapper.DefineMapping(nestedTypeDefinition, type);
        this.Traverse(nestedTypeDefinition.NestedTypes);
      }

      /// <summary>
      /// Traverses the children of the nested type reference.
      /// </summary>
      /// <param name="nestedTypeReference"></param>
      public override void TraverseChildren(INestedTypeReference nestedTypeReference) {
        this.Traverse(nestedTypeReference.ContainingType);
        this.TraverseChildren(nestedTypeReference.ResolvedType);
      }

    }

  }

}