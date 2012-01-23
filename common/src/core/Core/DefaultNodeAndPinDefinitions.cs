using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Core.Collections;

namespace VVVV.Core
{
    public class BasicNodeDefinition : INodeDefinition
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
                if (Version != "")
                    return Name + " (" + Category + " " + Version + ")"; //todo: more performatant string creation
                else
                    return Name + " (" + Category + ")"; //todo: more performatant string creation
            }
        }

        public INodeReference CreateReference()
        {
            throw new NotImplementedException();
        }
    }

    public class RichNodeDefinition : BasicNodeDefinition, IRichNodeDefinition
    {
        public string Tags { get; set; }

        public string Author { get; set; }

        public string Credits { get; set; }

        public string Bugs { get; set; }

        public string Warnings { get; set; }

        public string Filename { get; set; }

        public string NameInTextualCode { get; set; }

        public string Namespace { get; set; }

        IRichNodeReference IRichNodeDefinition.CreateReference()
        {
            throw new NotImplementedException();
        }
    }

    public class DataflowNodeDefinition : RichNodeDefinition, IDataflowNodeDefinition
    {
        public IEnumerable<IInputPinDefinition> Inputs { get; set; }

        public IEnumerable<IOutputPinDefinition> Outputs { get; set; }

        public DataflowNodeDefinition()
        {
            Inputs = new EditableList<IInputPinDefinition>();
            Outputs = new EditableList<IOutputPinDefinition>();
        }

        public Microsoft.Cci.IMethodDefinition MethodDefinition { get; set; }

        public new IDataFlowNodeReference CreateReference()
        {
            throw new NotImplementedException();
        }
        
        public override string ToString()
        {
            return string.Format("[DataflowNodeDefinition MethodDefinition={0}]", MethodDefinition);
        }
    }

    public class FunctionNodeDefinition : DataflowNodeDefinition, IFunctionNodeDefinition
    {
    }

    public class FunctorNodeDefinition : DataflowNodeDefinition, IFunctorNodeDefinition
    {
        public Microsoft.Cci.ITypeReference StateType { get; set; }
    }






    public class DataflowPinDefinition : IDataflowPinDefinition
    {

        public Microsoft.Cci.ITypeReference Type { get; set; }

        public string Name { get; set; }

        public string NameInTextualCode { get; set; }

        public IDataflowNodeDefinition Node { get; set; }

        public IDataFlowPinReference CreateReference()
        {
            throw new NotImplementedException();
        }
    }

    public class InputPinDefinition : DataflowPinDefinition, IInputPinDefinition
    {

        public bool HasDefaultValue { get; set; }

        public Microsoft.Cci.IMetadataConstant DefaultValue { get; set; }

        public bool StrikedOutByDefault { get; set; }
        
        public Microsoft.Cci.IParameterDefinition ParameterDefinition { get; set; }
    }

    public class OutputPinDefinition : DataflowPinDefinition, IOutputPinDefinition
    {
      
    }
}
