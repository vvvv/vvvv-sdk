using System;
using VVVV.PluginInterfaces.V2;
using VVVV.Nodes.Generic;

namespace VVVV.Nodes
{
    //1.) do a 'replace all' for REPLACEME_CLASS with the name of your own type

    //2.) do a 'replace all' for NODECATEGORY to set the version and the class name prefix for all node (e.g. Value, Color...)

    //3.) you have to override the Distance method for your type
    
    [PluginInfo(
        Name = "Length",
        Category = "NODECATEGORY Spectral",
        Help = "Calculates the length of a path consisting of a spread of REPLACEME_CLASS",
        Tags = "",
        Author = "motzi"
    )]
    public class REPLACEME_CLASSLengthSpectralNode : LengthSpectral<REPLACEME_CLASS>
    {
        override protected double Distance(REPLACEME_CLASS t1, REPLACEME_CLASS t2)
        {
            // calculate the length of a path between t1 and t2
            throw new NotImplementedException("You need to implement the Distance method");
        }
    }
}

