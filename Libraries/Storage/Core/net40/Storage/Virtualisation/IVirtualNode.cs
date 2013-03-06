/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;

namespace VDS.RDF.Storage.Virtualisation
{
    /// <summary>
    /// Interface for Virtual Nodes
    /// </summary>
    /// <typeparam name="TNodeID">Node ID Type</typeparam>
    /// <typeparam name="TGraphID">Graph ID Type</typeparam>
    public interface IVirtualNode<TNodeID, TGraphID> 
        : INode, IEquatable<IVirtualNode<TNodeID, TGraphID>>, IComparable<IVirtualNode<TNodeID, TGraphID>>
    {
        /// <summary>
        /// Gets the Node ID
        /// </summary>
        TNodeID VirtualID
        {
            get;
        }

        /// <summary>
        /// Gets the Virtual Node provider
        /// </summary>
        IVirtualRdfProvider<TNodeID, TGraphID> Provider
        {
            get;
        }

        /// <summary>
        /// Gets whether the Nodes value has been materialised
        /// </summary>
        bool IsMaterialised
        {
            get;
        }

        /// <summary>
        /// Gets the materialised value forcing it to be materialised if necessary
        /// </summary>
        INode MaterialisedValue
        {
            get;
        }
    }
}
