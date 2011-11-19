using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Core;
using Microsoft.Cci;

namespace VVVV.Core
{
    public class DefaultNodeReference : INodeReference
    {
        public string Name { get; private set; }
        public INodeDefinition Definition { get; private set; }

        public DefaultNodeReference (INodeDefinition definition, string name)
	    {
            Definition = definition;
            Name = name;
	    }
    }

    public class DefaultRichNodeReference : DefaultNodeReference, IRichNodeReference
    {
        public new IRichNodeDefinition Definition { get; private set; }

        public DefaultRichNodeReference(IRichNodeDefinition definition, string name)
            : base(definition, name)
        {
            Definition = definition;
        }
    }

    public class DefaultDataflowNodeReference : DefaultRichNodeReference, IDataflowNodeReference
    {
        public IEnumerable<IInputPinReference> Inputs { get; private set; }

        public IEnumerable<IOutputPinReference> Outputs { get; private set; }

        public new IDataflowNodeDefinition Definition { get; private set; }

        public DefaultDataflowNodeReference(IDataflowNodeDefinition definition, string name, IEnumerable<IInputPinReference> inputs, IEnumerable<IOutputPinReference> outputs)
            : base(definition, name)
        {
            Definition = definition;
            Inputs = inputs;
            Outputs = outputs;
        }
    }

    public class DefaultFunctionNodeReference : DefaultDataflowNodeReference, IFunctionNodeReference
    {
        public new IFunctionNodeDefinition Definition { get; private set; }

        public DefaultFunctionNodeReference(IFunctionNodeDefinition definition, string name, IEnumerable<IInputPinReference> inputs, IEnumerable<IOutputPinReference> outputs)
            : base(definition, name, inputs, outputs)
        {
            Definition = definition;
        }
    }

    public class DefaultDataflowPinReference : IDataflowPinReference
    {
        public IDataflowPinDefinition Definition { get; private set; }
        public ITypeReference Type { get; private set; }
        //public IEnumerable<IDataflowLink> Links { get; private set; }
        //public IDataflowPatchReference Patch { get; private set; }

        public DefaultDataflowPinReference(IDataflowPinDefinition def, ITypeReference type)
        {
            Definition = def;
            Type = type;
        }
    }

    public class DefaultInputPinReference : DefaultDataflowPinReference, IInputPinReference
    {
        public bool IsStrikedOut { get; private set; }
        public IMetadataConstant DefaultValue { get; private set; }
        public new IInputPinDefinition Definition { get; private set; }

        public DefaultInputPinReference(IInputPinDefinition def, ITypeReference type, IMetadataConstant defaultValue, bool strikedOut)
            : base(def, type)
        {
            Definition = def;
            DefaultValue = defaultValue;
            IsStrikedOut = strikedOut;
        }
    }

    public class DefaultOutputPinReference : DefaultDataflowPinReference, IOutputPinReference
    {
        public new IOutputPinDefinition Definition { get; private set; }
        //public IEnumerable<IDataflowLocation> AllUpstreamLocations { get; private set; }

        public DefaultOutputPinReference(IOutputPinDefinition def, ITypeReference type)
            : base(def, type)
        {
            Definition = def;
        }
    }
}
