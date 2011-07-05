using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Storage.Virtualisation
{
    public interface IVirtualNode<TNodeID, TGraphID> 
        : INode, IEquatable<IVirtualNode<TNodeID, TGraphID>>, IComparable<IVirtualNode<TNodeID, TGraphID>>
    {
        TNodeID VirtualID
        {
            get;
        }

        IVirtualRdfProvider<TNodeID, TGraphID> Provider
        {
            get;
        }

        bool IsMaterialised
        {
            get;
        }

        INode MaterialisedValue
        {
            get;
        }
    }
}
