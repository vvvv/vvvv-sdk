using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Microsoft.Cci
{
    public class SpecializedNestedTypeReference : ISpecializedNestedTypeReference
    {

        /// <summary>
        /// A reference to a nested type of a generic type instance. It is specialized because any occurrences of the type parameters have been replaced with the
        /// corresponding type arguments from the instance.
        /// </summary>
        /// <param name="unspecializedVersion"></param>
        /// <param name="containingType"></param>
        /// <param name="internFactory"></param>
        public SpecializedNestedTypeReference(INestedTypeReference unspecializedVersion, ITypeReference containingType, IInternFactory internFactory)
        {
            Contract.Requires(!(unspecializedVersion is ISpecializedNestedTypeReference));
            Contract.Requires(!(unspecializedVersion.ContainingType is ISpecializedNestedTypeReference));
            Contract.Requires(!(unspecializedVersion.ContainingType is IGenericTypeInstanceReference));
            this.unspecializedVersion = unspecializedVersion;
            this.containingType = containingType;
            this.internFactory = internFactory;
        }

        IInternFactory internFactory;

        /// <summary>
        /// The corresponding (unspecialized) member from the generic type (template) that was instantiated to obtain the containing type
        /// of this member.
        /// </summary>
        public INestedTypeReference/*!*/ UnspecializedVersion
        {
            get { return this.unspecializedVersion; }
        }
        readonly INestedTypeReference/*!*/ unspecializedVersion;

        #region INestedTypeReference Members

        public ushort GenericParameterCount
        {
            get { return this.UnspecializedVersion.GenericParameterCount; }
        }

        public INestedTypeDefinition ResolvedType
        {
            get
            {
                if (this.resolvedType == null)
                {
                    this.resolvedType = TypeHelper.GetNestedType(this.ContainingType.ResolvedType, this.Name, this.GenericParameterCount);
                    // Also look in private helper types
                    if (this.resolvedType == Dummy.NestedTypeDefinition)
                    {
                        var privateHelperTypes = this.ContainingType.ResolvedType.Methods.SelectMany(m => m.Body.PrivateHelperTypes);
                        foreach (var privateHelperType in privateHelperTypes)
                        {
                            var nestedType = privateHelperType as INestedTypeDefinition;
                            if (nestedType == null) continue;
                            if (nestedType.Name != this.Name) continue;
                            if (nestedType.GenericParameterCount != this.GenericParameterCount) continue;
                            this.resolvedType = nestedType;
                            break;
                        }
                    }
                }
                return this.resolvedType;
            }
        }
        INestedTypeDefinition resolvedType;

        public override string ToString()
        {
            return TypeHelper.GetTypeName(this);
        }

        #endregion

        #region INamedTypeReference Members


        public bool MangleName
        {
            get { return this.UnspecializedVersion.MangleName; }
        }

        INamedTypeDefinition INamedTypeReference.ResolvedType
        {
            get
            {
                if (this.ResolvedType is Dummy) return Dummy.NamedTypeDefinition;
                return this.ResolvedType;
            }
        }

        #endregion

        #region ITypeMemberReference Members

        /// <summary>
        /// A reference to the containing type of the referenced type member.
        /// </summary>
        /// <value></value>
        public ITypeReference ContainingType
        {
            get { return this.containingType; }
        }
        ITypeReference containingType;

        /// <summary>
        /// The type definition member this reference resolves to.
        /// </summary>
        /// <value></value>
        public ITypeDefinitionMember ResolvedTypeDefinitionMember
        {
            get
            {
                return this.ResolvedType;
            }
        }
        #endregion

        #region ITypeReference Members

        public IAliasForType AliasForType
        {
            get { return Dummy.AliasForType; }
        }

        public uint InternedKey
        {
            get
            {
                if (this.internedKey == 0)
                    this.internedKey = this.internFactory.GetNestedTypeReferenceInternedKey(this.ContainingType, this.Name, this.GenericParameterCount);
                return this.internedKey;
            }
        }
        uint internedKey;

        public bool IsAlias
        {
            get { return false; }
        }

        public bool IsEnum
        {
            get { return this.UnspecializedVersion.IsEnum; }
        }

        public bool IsValueType
        {
            get { return this.UnspecializedVersion.IsValueType; }
        }

        public IPlatformType PlatformType
        {
            get { return this.UnspecializedVersion.PlatformType; }
        }

        ITypeDefinition ITypeReference.ResolvedType
        {
            get
            {
                if (this.ResolvedType is Dummy) return Dummy.TypeDefinition;
                return this.ResolvedType;
            }
        }

        public PrimitiveTypeCode TypeCode
        {
            get { return PrimitiveTypeCode.NotPrimitive; }
        }

        #endregion

        #region IReference Members

        /// <summary>
        /// A collection of metadata custom attributes that are associated with this definition.
        /// </summary>
        /// <value></value>
        public IEnumerable<ICustomAttribute> Attributes
        {
            get { return this.UnspecializedVersion.Attributes; }
        }

        /// <summary>
        /// Calls visitor.Visit(ISpecializedNestedTypeReference).
        /// </summary>
        public void Dispatch(IMetadataVisitor visitor)
        {
            visitor.Visit(this);
        }

        /// <summary>
        /// Calls visitor.Visit(ISpecializedNestedTypeReference).
        /// </summary>
        public void DispatchAsReference(IMetadataVisitor visitor)
        {
            visitor.Visit(this);
        }

        /// <summary>
        /// A potentially empty collection of locations that correspond to this instance.
        /// </summary>
        /// <value></value>
        public IEnumerable<ILocation> Locations
        {
            get { return this.UnspecializedVersion.Locations; }
        }

        #endregion

        #region INamedEntity Members

        /// <summary>
        /// The name of the entity.
        /// </summary>
        public IName Name
        {
            get { return this.UnspecializedVersion.Name; }
        }

        #endregion
    }
}
