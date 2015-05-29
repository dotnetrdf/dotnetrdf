/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

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

using System.Collections.Specialized;

namespace VDS.RDF.Graphs
{
    /// <summary>
    /// Interface for graphs that support events, events are provided via implementation of the standard <see cref="INotifyCollectionChanged" /> interface
    /// </summary>
    public interface IEventedGraph
        : IGraph, INotifyCollectionChanged
    {
        /// <summary>
        /// Indicates whether a graph actually has events
        /// </summary>
        /// <remarks>
        /// While generally speaking use of this interface will be sufficient to indicate that a graph supports events in some cases where complex graph types such as decorators, unions, etc are used the availability of events may be dictated by the underlying graphs even if the wrapper is capable of providing them.  Thus users intending to consume events should check that this method returns true.
        /// </remarks>
        bool HasEvents { get; }
    }
}