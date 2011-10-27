using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Microsoft.Cci;

namespace VVVV.Core
{
    /// <summary>
    /// a node reference can be used inside patches. it is defined by its node definition.
    /// </summary>
    public interface INodeReference
    {
        /// <summary>
        /// The name of the node refernce. either given by the user or computed on creation (e.g. "damper7")
        /// </summary>
        string Name { get; }

        /// <summary>
        /// the 2d position within the patch.
        /// </summary>
        PointF Position { get; }

        /// <summary>
        /// gets the node definition that this refernce was made of
        /// </summary>
        INodeDefinition Definition { get; }
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

    /// <summary>
    /// a dataflow node reference can be used in patches
    /// </summary>
    public interface IDataFlowNodeReference : IRichNodeReference
    {
        /// <summary>
        /// inputs of the node
        /// </summary>
        IEnumerable<IInputPinReference> Inputs { get; }

        /// <summary>
        /// outputs of the node
        /// </summary>
        IEnumerable<IOutputPinReference> Outputs { get; }

        /// <summary>
        /// gets the node definition that this refernce was made of
        /// </summary>
        new IDataflowNodeDefinition Definition { get; }
    }







    /// <summary>
    /// used by dataflow nodes in a patch (which are of type IDataFlowNodeReference)
    /// </summary>
    public interface IDataFlowPinReference : IDataflowLocation
    {
        /// <summary>
        /// gets the pin definition that this refernce was made of
        /// </summary>
        IDataflowPinDefinition Definition { get; }
    }

    /// <summary>
    /// used by dataflow nodes in a patch (which are of type IDataFlowNodeReference)
    /// </summary>
    public interface IInputPinReference : IDataFlowPinReference, IDataflowSink
    {
        /// <summary>
        /// input is cancelled out, thus no value is given, value needs to be specified otherwise (e.g. a sink somewhere downstream)
        /// </summary>
        bool IsStrikedOut { get; }

        /// <summary>
        /// value that defines the pins value, when it's not connected and not striked out. On creation of the refernce it is equal to the default value
        /// of the pins definition. The default value of the pin reference however can be changed by the user.
        /// </summary>
        IMetadataConstant DefaultValue { get; }

        /// <summary>
        /// gets the pin definition that this refernce was made of
        /// </summary>
        new IInputPinDefinition Definition { get; }
    }

    /// <summary>
    /// used by dataflow nodes in a patch (which are of type IDataFlowNodeReference)
    /// </summary>
    public interface IOutputPinReference : IDataFlowPinReference, IDataflowSource
    {
        /// <summary>
        /// gets the pin definition that this refernce was made of
        /// </summary>
        new IOutputPinDefinition Definition { get; }
    }

}
