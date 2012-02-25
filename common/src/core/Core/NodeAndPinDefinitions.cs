using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Cci;
using System.Drawing;

namespace VVVV.Core
{
    /// <summary>
    /// a node definition returns basic information about a node. it also knows how to create a reference onto it.
    /// </summary>
    public interface INodeDefinition
    {
        /// <summary>
        /// The nodes main visible name. Use CamelCaps and no spaces.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// The main category of the node. Try to use an existing one.
        /// </summary>
        string Category { get; }
        /// <summary>
        /// Optional. Leave blank if not needed to distinguish two nodes of the same name and category.
        /// </summary>
        string Version { get; }
        /// <summary>
        /// Describes the nodes function in a few words.
        /// </summary>
        string Help { get; }
        /// <summary>
        /// The nodes unique username in the form of: Name (Category Version) where the Name can be a symbol
        /// </summary>
        string Username { get; }
        /// <summary>
        /// The nodes unique systemname in the form of: Name (Category Version)
        /// </summary>
        string Systemname { get; }

        /// <summary>
        /// Creates a reference on the node definition, typically used in a patch.
        /// </summary>
        INodeReference CreateReference(string name);
    }

    /// <summary>
    /// a rich node definition defines a node that is not primitive thus was either programmed visually or textually
    /// </summary>
    public interface IRichNodeDefinition : INodeDefinition
    {
        /// <summary>
        /// A comma separated list of tags that describe the node. Name, category and Version don't need to be duplicated here.
        /// </summary>
        string Tags { get; }
        /// <summary>
        /// The nodes author.
        /// </summary>
        string Author { get; }
        /// <summary>
        /// Credits for anybody that helped developing the node. E.g. those that wrote code that is called by the node.
        /// </summary>
        string Credits { get; }
        /// <summary>
        /// Specify known problems.
        /// </summary>
        string Bugs { get; }
        /// <summary>
        /// Specify any usage of the node that may cause troubles.
        /// </summary>
        string Warnings { get; }
        /// <summary>
        /// Name of the file that this node was extracted from. (e.g. an assembly or a (visual/textual) source code file)
        /// </summary>
        string Filename { get; }

        /// <summary>
        /// the Name of the Node (when referenced in textual code)
        /// </summary>
        string NameInTextualCode { get; }
        /// <summary>
        /// the Namespace
        /// </summary>
        string Namespace { get; }
    }

    /// <summary>
    /// dataflow node defintions can result from a patch or from an assembly
    /// </summary>
    public interface IDataflowNodeDefinition : IRichNodeDefinition
    {
        ///// <summary>
        ///// pins defined by the node
        ///// </summary>
        //IEnumerable<IDataflowPinDefinition> Pins { get; }

        /// <summary>
        /// inputs defined by the node
        /// </summary>
        IEnumerable<IInputPinDefinition> Inputs { get; }
        
        /// <summary>
        /// outputs defined by the node
        /// </summary>
        IEnumerable<IOutputPinDefinition> Outputs { get; }

        /// <summary>
        /// the method that either initially was extracted from an assembly to form the dataflow node or
        /// the outcome from a compilation of the visual source code of a patch that is a dataflow node definition
        /// </summary>
        IMethodDefinition MethodDefinition { get; }
        
        /// <summary>
        /// the type of the state
        /// </summary>
        ITypeReference StateType { get; }

        /// <summary>
        /// Creates a node reference defined by this node definition; typically used to build up patches
        /// </summary>
        /// <returns> a fresh node reference pointing to this definition </returns>
        IDataflowNodeReference CreateReference(string name, IEnumerable<IInputPinReference> inputs, IEnumerable<IOutputPinReference> outputs);
    }

    /// <summary>
    /// a pin as defined by source code (textually via an method parameter or visually via a hub (inlet,outlet))
    /// </summary>
    public interface IDataflowPinDefinition
    {
        /// <summary>
        /// the type of values that can be stored in that pin
        /// </summary>
        ITypeReference Type { get; }

        /// <summary>
        /// the Name of the Pin (can include spaces)
        /// </summary>
        string Name { get; }

        /// <summary>
        /// the Name of the Pin (when referenced in textual code)
        /// </summary>
        string NameInTextualCode { get; }

        /// <summary>
        /// the node that defined the pin as one of its pins
        /// </summary>
        IDataflowNodeDefinition Node { get; }
        
        /// <summary>
        /// Creates a reference on the pin definition, typically used by a node definition that needs to create a node reference. 
        /// </summary>
        IDataflowPinReference CreateReference(ITypeReference type);
    }


    /// <summary>
    /// defines an input pin
    /// </summary>
    public interface IInputPinDefinition : IDataflowPinDefinition
    {
        /// <summary>
        /// the method parameter definition
        /// </summary>
        IParameterDefinition ParameterDefinition { get; }
        
        /// <summary>
        /// returns if the pin has a default value
        /// </summary>
        bool HasDefaultValue { get; }

        /// <summary>
        /// the default value 
        /// </summary>
        IMetadataConstant DefaultValue { get; }

        /// <summary>
        /// if this is true the resulting pin reference will be striked out on creation
        /// </summary>
        bool StrikedOutByDefault { get; }

        /// <summary>
        /// Creates a reference on the pin definition, typically used by a node definition that needs to create a node reference. 
        /// </summary>
        IInputPinReference CreateReference(ITypeReference type, IMetadataConstant defaultValue, bool strikedOut);
    }

    /// <summary>
    /// defines an output pin
    /// </summary>
    public interface IOutputPinDefinition : IDataflowPinDefinition
    {
        /// <summary>
        /// Creates a reference on the pin definition, typically used by a node definition that needs to create a node reference. 
        /// </summary>
        IOutputPinReference CreateReference(ITypeReference type);
    }




    //class InletConstructor : INodeDefinition  //can be visualized in a node browser, can create the inlet
    //{
    //    public string Name // "Inlet"
    //    public string Category // "Primitives"
    //    public string Version // ""
    //    public string Help // "defines an input pin for the node defined ba this patch"
    //    public string Username // "Inlet (Primitives)"
    //    public string Systemname // "Inlet (Primitives)"
    //    public INodeReference CreateReference() // creates an Inlet
    //}

    //class Inlet : INodeReference, IInputPinDefinition, IDataflowSource //is an inlet in a patch with name, type and links, but also defines a pin
    //{
    //    public string Name //the name the user gave to the inlet
    //    public INodeDefinition Definition //the inlet constructor
    //    public bool HasDefaultValue //we could always return true, its not like in code where default values of parameters are something special
    //    public IMetadataConstant DefaultValue //a value specified by the user, initialized with default(T)
    //    public ITypeReference Type //T, can even by a generic type parameter
    //    public string NameInTextualCode //the name of the pin definition without spaces, camelcapsed
    //    public IDataFlowNodeDefinition Node // the patch
    //    public IDataFlowPinReference CreateReference() //typically used by a node reference to get its pins, will create a pin that reflects the properties of this inlet
    //    public IEnumerable<IDataFlowLink> Links //outgoing links
    //    public IDataflowPatch Patch // the patch
    //    public bool StrikedOutByDefault //false for now
    //    public IEnumerable<IDataflowLocation> AllUpstreamLocations //returns an empty list
    //}

    //class Outlet : INodeReference, IOutputPinDefinition, IDataflowSink
    //{ }

}
