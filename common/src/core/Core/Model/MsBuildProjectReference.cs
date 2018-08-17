using System;

namespace VVVV.Core.Model
{
    /// <summary>
    /// Represents a reference to a loaded project.
    /// </summary>
    public class MsBuildProjectReference: ProjectItem, IReference
    {
    	public MsBuildProjectReference(MsBuildProject project)
            : base(project.Name)
        {
            ReferencedProject = project;
        }
    	
        /// <summary>
        /// The path to the assembly created by the referenced project.
        /// </summary>
        public string AssemblyLocation
        {
            get
            {
                return ReferencedProject.AssemblyLocation;
            }
        }
        
        /// <summary>
        /// The MsBuildProject this reference points to.
        /// </summary>
        public MsBuildProject ReferencedProject 
        { 
            get; 
            protected set; 
        }

		public bool IsGlobal 
		{
			get 
			{
				// TODO: Handle global project references.
				return false;
			}
		}
    }
}
