using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using VVVV.Core.Model;

namespace VVVV.Core.Helper
{
    public class Traverser
    {
        #region Dispatcher
        class Dispatcher : IVisitor
        {
            private readonly Traverser FTraverser;
            
            public Dispatcher(Traverser traverser)
            {
                FTraverser = traverser;
            }
            
            public void Visit(IIDItem idItem)
            {
                Contract.Assume(false);
            }
            
            public void Visit(ISolution solution)
            {
                FTraverser.Traverse(solution);
            }
            
            public void Visit(IProject project)
            {
                FTraverser.Traverse(project);
            }
            
            public void Visit(IDocument document)
            {
                FTraverser.Traverse(document);
            }
            
            public void Visit(IReference reference)
            {
                FTraverser.Traverse(reference);
            }
            
            public void Visit(AssemblyReference assemblyReference)
            {
                FTraverser.Traverse(assemblyReference);
            }
            
            public void Visit(MsBuildProjectReference projectReference)
            {
                FTraverser.Traverse(projectReference);
            }
        }
        #endregion
        
        private readonly Dispatcher FDispatcher;
        
        public Traverser()
        {
            FDispatcher = new Dispatcher(this);
        }
        
        public void Traverse(ISolution solution)
        {
            TraverseChildren(solution);
        }
        
        public void Traverse(IProject project)
        {
            TraverseChildren(project);
        }
        
        public void Traverse(IDocument document)
        {
            TraverseChildren(document);
        }
        
        public void Traverse(IReference reference)
        {
            TraverseChildren(reference);
        }
        
        public void Traverse(AssemblyReference assemblyReference)
        {
            TraverseChildren(assemblyReference);
        }
        
        public void Traverse(MsBuildProjectReference projectReference)
        {
            TraverseChildren(projectReference);
        }
        
        public void Traverse(IEnumerable<IProject> projects)
        {
            foreach (var project in projects)
            {
                project.Dispatch(FDispatcher);
            }
        }
        
        public void Traverse(IEnumerable<IReference> references)
        {
            foreach (var reference in references)
            {
                reference.Dispatch(FDispatcher);
            }
        }
        
        public void Traverse(IEnumerable<IDocument> documents)
        {
            foreach (var document in documents)
            {
                document.Dispatch(FDispatcher);
            }
        }
        
        public virtual void TraverseChildren(ISolution solution)
        {
            Traverse(solution.Projects);
        }
        
        public virtual void TraverseChildren(IProject project)
        {
            Traverse(project.References);
            Traverse(project.Documents);
        }
        
        public virtual void TraverseChildren(IDocument document)
        {
            
        }
        
        public virtual void TraverseChildren(IReference reference)
        {
            
        }
        
        public virtual void TraverseChildren(AssemblyReference assemblyReference)
        {
            TraverseChildren((IReference) assemblyReference);
        }
        
        public virtual void TraverseChildren(MsBuildProjectReference projectReference)
        {
            TraverseChildren((IReference) projectReference);
        }
    }
}
