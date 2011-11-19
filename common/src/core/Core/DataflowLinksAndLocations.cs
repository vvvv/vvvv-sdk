using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Cci;

namespace VVVV.Core
{
    public interface IDataflowLocation
    {
        /// <summary>
        /// the type of values that can be stored in that pin
        /// </summary>
        ITypeReference Type { get; }

        /// <summary>
        /// get all the links that start or end here
        /// </summary>
        IEnumerable<IDataflowLink> Links { get; }

        /// <summary>
        /// returns the patch that holds the specific location
        /// </summary>
        IDataflowPatchSymbol Patch { get; }
    }

    public interface IDataflowSource : IDataflowLocation
    {
        IEnumerable<IDataflowLocation> AllUpstreamLocations { get; }
    }

    public interface IDataflowSink : IDataflowLocation
    {
    }

    /// <summary>
    /// a link in a dataflow graph connects a source with a sink
    /// </summary>
    public interface IDataflowLink
    {
        /// <summary>
        /// get the source (= starting point) of the dataflow link
        /// </summary>
        IDataflowSource Source { get; }

        /// <summary>
        /// get the sink (= end point) of the dataflow link
        /// </summary>
        IDataflowSink Sink { get; }
    }

    public interface IHubSymbol : INodeReference, IDataflowPinDefinition//, IDataflowLocation
    { }

    public interface IInletSymbol : IHubSymbol, IInputPinDefinition//, IDataflowSource
    { }

    public interface IOutletSymbol : IHubSymbol, IOutputPinDefinition//, IDataflowSink
    { }

    public interface IPatchSymbol : INodeDefinition
    {
        IEnumerable<INodeReference> Nodes { get; }
    }

    public interface IDataflowPatchSymbol : IPatchSymbol, IDataflowNodeDefinition
    {
        IEnumerable<IDataflowLink> Links { get; }

        new IEnumerable<IInletSymbol> Inputs { get; }

        new IEnumerable<IOutletSymbol> Outputs { get; }

        IEnumerable<IFunctionNodeReference> FunctionNodes { get; }
    }

    public interface IFunctionSymbol : IDataflowPatchSymbol, IFunctionNodeDefinition
    {
    }

    public interface IFunctorSymbol : IDataflowPatchSymbol, IFunctorNodeDefinition
    {
    }




    public static class PatchAndDataflowHelpers
    {
        static PatchAndDataflowHelpers()
        {
        }

        public static IEnumerable<IDataflowSink> GetSinks(this IDataflowSource source)
        {
            foreach (var link in source.Links)
                // no further check needed, as long as sources can't be sinks
                yield return link.Sink;
        }

        public static bool HasSinks(this IDataflowSource source)
        {
            return source.GetSinks().Any();
        }

        public static IEnumerable<IDataflowSource> GetSources(this IDataflowSink sink)
        {
            foreach (var link in sink.Links)
                // no further check needed, as long as sources can't be sinks
                yield return link.Source;
        }

        public static IDataflowSource GetSource(this IDataflowSink sink)
        {
            return sink.GetSources().FirstOrDefault();
        }

        public static bool HasSource(this IDataflowSink sink)
        {
            return sink.GetSources().Any();
        }

        public static bool IsClonable(this ITypeReference type)
        {
            return (type.IsValueType || TypeHelper.Type1ImplementsType2(type.ResolvedType, type.PlatformType.SystemICloneable)); //type.PlatformType.SystemICloneable: oki?             
        }

        public static bool CanFlowTo(this ITypeReference sourceType, ITypeReference sinkType)
        {
            //probably only works for nongeneric types:
            if (TypeHelper.TypesAreAssignmentCompatible(sourceType.ResolvedType, sinkType.ResolvedType)) 
                return true;
            
            // covers generic types, but only if they are equivalent, lacks the cases where they are assignment compatible 
            if (TypeHelper.TypesAreEquivalentAssumingGenericMethodParametersAreEquivalentIfTheirIndicesMatch(sourceType, sinkType))
                return true;

            return false;
        }

        public static IDataflowSource SelectSource(this IDataflowLocation a, IDataflowLocation b)
        {
            return a is IDataflowSource ? a as IDataflowSource : b as IDataflowSource;
        }

        public static IDataflowSink SelectSink(this IDataflowLocation a, IDataflowLocation b)
        {
            return a is IDataflowSink ? a as IDataflowSink : b as IDataflowSink;
        }

        public static bool IsConnectable(this IDataflowSource source, IDataflowSink sink)
        {
            // we can connect 
            // a) if values are welcome by the sink type and 
            //  b1) the source is either clonable OR
            //  b2) there is no other sink connected to the source
            // c) no cyclic dependency is created
            // we don't need to check if the sink is free: in the next snapshot of the source code the new source would just replace the old
            return (
                source.Type.CanFlowTo(sink.Type) &&
                (source.Type.IsClonable() || !source.HasSinks()) &&
                !source.AllUpstreamLocations.Contains(sink)
                );
        }

        public static bool IsConnectable(this IDataflowSink sink, IDataflowSource source)
        {
            return source.IsConnectable(sink);
        }

        public static bool IsConnectable(this IDataflowLocation a, IDataflowLocation b)
        {
            return SelectSource(a, b).IsConnectable(SelectSink(a, b));
        }

    }
}