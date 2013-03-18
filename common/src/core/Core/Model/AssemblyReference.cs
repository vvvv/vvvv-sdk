using System;
using System.IO;
using System.Linq;
using VVVV.Utils;

namespace VVVV.Core.Model
{
    /// <summary>
    /// Reference to an assembly.
    /// </summary>
    public class AssemblyReference : ProjectItem, IReference
    {
        protected bool? FIsGlobal;

        public AssemblyReference(string assemblyLocation, bool? isGlobal)
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
            : this(assemblyLocation, null)
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
                if (!FIsGlobal.HasValue)
                {
                    var msBuildProject = Project as MsBuildProject;
                    if (msBuildProject != null)
                    {
                        var referenceFileName = Path.GetFileName(AssemblyLocation);
                        return msBuildProject.ReferencePaths
                            .Any(p => File.Exists(Path.Combine(p, referenceFileName)));
                    }
                    return false;
                }
                return FIsGlobal.Value;
            }
        }

        public override void Dispatch(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
