
using System;
using VVVV.Core.Model;

namespace VVVV.Core
{
    public interface IVisitor
    {
        void Visit(IIDItem idItem);
        void Visit(ISolution solution);
        void Visit(IProject project);
        void Visit(IDocument document);
        void Visit(IReference reference);
        void Visit(AssemblyReference assemblyReference);
        void Visit(MsBuildProjectReference projectReference);
    }
}
