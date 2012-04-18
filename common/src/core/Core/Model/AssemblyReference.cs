using System;
using System.IO;
using VVVV.Utils;

namespace VVVV.Core.Model
{
    /// <summary>
    /// Reference to an assembly.
    /// </summary>
    public class AssemblyReference : ProjectItem, IReference
    {
        protected bool FIsGlobal;

        public AssemblyReference(string assemblyLocation, bool isGlobal)
            : base(Path.GetFileNameWithoutExtension(assemblyLocation))
        {
            AssemblyLocation = assemblyLocation;
            FIsGlobal = isGlobal;
        }

        /// <summary>
        /// Sets the IsGlobal property to false
        /// </summary>
        /// <param name="assemblyLocation">File location of the assembly</param>
        public AssemblyReference(string assemblyLocation)
            : this(assemblyLocation, false)
        {
        }

        public string AssemblyLocation
        {
            get;
            private set;
        }

        public bool IsGlobal
        {
            get
            {
                return FIsGlobal;
            }
        }

        public override void Dispatch(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
