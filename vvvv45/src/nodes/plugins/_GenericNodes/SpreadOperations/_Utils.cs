using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.Generic
{
    internal static class _Utils
    {
        internal static string GetDownstreamSubType(this IPin output)
        {
            foreach (var downstreamPin in output.GetConnectedPins())
            {
                var subType = downstreamPin?.SubType.Split(new[] { ',' })[1].Trim();
                if (subType != "Empty") // IO boxes use Empty
                    return subType;
            }
            return null;
        }
    }
}
