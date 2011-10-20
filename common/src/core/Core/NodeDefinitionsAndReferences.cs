using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.Core
{
    /// <summary>
    /// a node definition returns basic information about a node. it also knows how to create a refernce onto it.
    /// </summary>
    public interface INodeDefinition
    {
        /// <summary>
        /// The nodes main visible name. Use CamelCaps and no spaces.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// The category in which the plugin can be found. Try to use an existing one.
        /// </summary>
        string Category { get; }
        /// <summary>
        /// Optional. Leave blank if not needed to distinguish two nodes of the same name and category.
        /// </summary>
        string Version { get; }
        /// <summary>
        /// Describe the nodes function in a few words.
        /// </summary>
        string Help { get; set; }
        /// <summary>
        /// The nodes unique username in the form of: Name (Category Version) where the Name can be a symbol
        /// </summary>
        string Username { get; }
        /// <summary>
        /// The nodes unique systemname in the form of: Name (Category Version)
        /// </summary>
        string Systemname { get; }

        /// <summary>
        /// Creates a INodeReference defined by this INodeDefinition; typically used to build up patches
        /// </summary>
        /// <returns> a fresh INodeReference pointing to this Definition </returns>
        INodeReference CreateReference();
    }

    /// <summary>
    /// a node reference can be used inside patches. it is defined by its node definition.
    /// </summary>
    public interface INodeReference
    {
        /// <summary>
        /// gets the node definition that this refernce was made of
        /// </summary>
        INodeDefinition Definition { get; }
    }

    /// <summary>
    /// a rich node definition defines a node that is not primitive thus was either programmed visually or textually
    /// </summary>
    public interface IRichNodeDefinition : INodeDefinition
    {
        /// <summary>
        /// Specify a comma separated list of tags that describe the node. Name, category and Version don't need to be duplicated here.
        /// </summary>
        string Tags { get; set; }
        /// <summary>
        /// Specify the plugins author.
        /// </summary>
        string Author { get; set; }
        /// <summary>
        /// Give credits to thirdparty code used.
        /// </summary>
        string Credits { get; set; }
        /// <summary>
        /// Specify known problems.
        /// </summary>
        string Bugs { get; set; }
        /// <summary>
        /// Specify any usage of the node that may cause troubles.
        /// </summary>
        string Warnings { get; set; }
        /// <summary>
        /// Name of the file used by the IAddonFactory to create this node.
        /// </summary>
        string Filename { get; }
    }

    /// <summary>
    /// a rich node reference can be used in patches. it was defined by a node that is not primitive thus was either programmed visually or textually
    /// </summary>
    public interface IRichNodeReference
    {
        /// <summary>
        /// gets the node definition that this refernce was made of
        /// </summary>
        new IRichNodeDefinition Definition { get; }
    }

    public interface IDataFlowPinDefinition
    {
        /// <summary>
        /// the Name of the Pin (can include spaces)
        /// </summary>
        string Name { get; }

        /// <summary>
        /// the Name of the Pin (when referenced in textual code)
        /// </summary>
        string NameInTextualCode { get; }
    }

    /// <summary>
    /// dataflow node defintions can result from a patch or from an assembly
    /// </summary>
    public interface IDataFlowNodeDefinition : IRichNodeDefinition
    {
        /// <summary>
        /// inputs defined by the node
        /// </summary>
        IEnumerable<IDataFlowPinDefinition> Inputs { get; }
        /// <summary>
        /// outputs defined by the node
        /// </summary>
        IEnumerable<IDataFlowPinDefinition> Outputs { get; }
    }

    /// <summary>
    /// used by dataflow nodes in a patch (which are of type IDataFlowNodeReference)
    /// </summary>
    public interface IDataFlowPinReference
    {
        /// <summary>
        /// gets the pin definition that this refernce was made of
        /// </summary>
        IDataFlowPinDefinition Definition { get; }
    }

    /// <summary>
    /// a dataflow node reference can be used in patches
    /// </summary>
    public interface IDataFlowNodeReference : IRichNodeReference
    {
        /// <summary>
        /// inputs of the node
        /// </summary>
        IEnumerable<IDataFlowPinReference> Inputs { get; }
        /// <summary>
        /// outputs of the node
        /// </summary>
        IEnumerable<IDataFlowPinReference> Outputs { get; }

        /// <summary>
        /// gets the node definition that this refernce was made of
        /// </summary>
        new IDataFlowNodeDefinition Definition { get; }
    }
}
