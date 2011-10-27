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
        IEnumerable<IDataFlowLink> Links { get; }

        /// <summary>
        /// returns the patch that holds the specific location
        /// </summary>
        IDataflowPatch Patch { get; }
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
    public interface IDataFlowLink
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

    public interface IDataflowPatch : IDataflowNodeDefinition
    {
        IEnumerable<IDataFlowLink> Links { get; }
    }








    public static class DataflowHelpers
    {
        //IMetadataHost FHost;

        static DataflowHelpers()
        {
            //FHost = new MetadataHostEnvironment
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

        public static bool IsClonable(this ITypeReference type, IPlatformType plattformtype)
        {
            return (type.IsValueType || TypeHelper.Type1ImplementsType2(type.ResolvedType, plattformtype.SystemICloneable));                
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

        public static bool IsConnectable(this IDataflowSource source, IDataflowSink sink, IPlatformType plattformtype)
        {
            // we can connect 
            // a) if values are welcome by the sink type and 
            //  b1) the source is either clonable OR
            //  b2) there is no other sink connected to the source
            // c) no cyclic dependency is created
            // we don't need to check if the sink is free: in the next snapshot of the source code the new source would just replace the old
            return (
                source.Type.CanFlowTo(sink.Type) &&
                (source.Type.IsClonable(plattformtype) || !source.HasSinks()) &&
                !source.AllUpstreamLocations.Contains(sink)
                );
        }

        public static bool IsConnectable(this IDataflowSink sink, IDataflowSource source, IPlatformType plattformtype)
        {
            return source.IsConnectable(sink, plattformtype);
        }

        public static bool IsConnectable(this IDataflowLocation a, IDataflowLocation b, IPlatformType plattformtype)
        {
            return SelectSource(a, b).IsConnectable(SelectSink(a, b), plattformtype);
        }

    }
}