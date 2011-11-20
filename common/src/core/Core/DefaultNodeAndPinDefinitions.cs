using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Core.Collections;
using Microsoft.Cci;
using System.Drawing;

namespace VVVV.Core
{
    public class DefaultNodeDefinition : INodeDefinition
    {
        public string Name { get; set; }

        public string Category { get; set; }

        public string Version { get; set; }

        public string Help { get; set; }

        public string Username 
        { 
            get
            {
                return Systemname; //todo handle special cases (Add -> +) ...
            } 
        }

        public string Systemname 
        { 
            get
            {
                //todo: caching of get property
                if (Version != "" && Version != null)
                    return Name + " (" + Category + " " + Version + ")"; //todo: more performatant string creation
                else
                    return Name + " (" + Category + ")"; //todo: more performatant string creation
            }
        }

        public virtual INodeReference CreateReference(string name)
        {
            return new DefaultNodeReference(this, name);
        }
    }

    public class DefaultRichNodeDefinition : DefaultNodeDefinition, IRichNodeDefinition
    {
        public string Tags { get; set; }

        public string Author { get; set; }

        public string Credits { get; set; }

        public string Bugs { get; set; }

        public string Warnings { get; set; }

        public string Filename { get; set; }

        public string NameInTextualCode { get; set; }

        public string Namespace { get; set; }
    }

    public class DefaultDataflowNodeDefinition : DefaultRichNodeDefinition, IDataflowNodeDefinition
    {
        public IEnumerable<IInputPinDefinition> Inputs { get; set; }

        public IEnumerable<IOutputPinDefinition> Outputs { get; set; }

        public DefaultDataflowNodeDefinition()
        {
            Inputs = new EditableList<IInputPinDefinition>();
            Outputs = new EditableList<IOutputPinDefinition>();
        }

        public Microsoft.Cci.IMethodDefinition MethodDefinition { get; set; }


        public IDataflowNodeReference CreateReference(string name, IEnumerable<IInputPinReference> inputs, IEnumerable<IOutputPinReference> outputs)
        {
            return new DefaultDataflowNodeReference(this, name, inputs, outputs);
        }
    }

    public class DefaultFunctionNodeDefinition : DefaultDataflowNodeDefinition, IFunctionNodeDefinition
    {
        public IFunctionNodeReference CreateReference(string name, IEnumerable<IInputPinReference> inputs, IEnumerable<IOutputPinReference> outputs)
        {
            return new DefaultFunctionNodeReference(this, name, inputs, outputs);
        }
    }

    public class DefaultStepNodeDefinition : DefaultDataflowNodeDefinition, IStepNodeDefinition
    {
        public Microsoft.Cci.ITypeReference StateType { get; set; }
    }






    public class DefaultDataflowPinDefinition : IDataflowPinDefinition
    {
        public ITypeReference Type { get; set; }

        public string Name { get; set; }

        public string NameInTextualCode { get; set; }

        public IDataflowNodeDefinition Node { get; set; }

        public IDataflowPinReference CreateReference(ITypeReference type)
        {
            return new DefaultDataflowPinReference(this, type);
        }
    }

    public class DefaultInputPinDefinition : DefaultDataflowPinDefinition, IInputPinDefinition
    {
        public bool HasDefaultValue { get; set; }

        public IMetadataConstant DefaultValue { get; set; }

        public bool StrikedOutByDefault { get; set; }

        public IInputPinReference CreateReference(ITypeReference type, IMetadataConstant defaultValue, bool strikedOut)
        {
            return new DefaultInputPinReference(this, type, defaultValue, strikedOut);
        }
    }

    public class DefaultOutputPinDefinition : DefaultDataflowPinDefinition, IOutputPinDefinition
    {
        public IOutputPinReference CreateReference(ITypeReference type)
        {
            return new DefaultOutputPinReference(this, type);
        }
    }
}
