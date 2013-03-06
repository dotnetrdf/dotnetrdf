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
using System.Collections.Generic;
using VDS.RDF.Query;
using VDS.RDF.Query.Inference.Pellet.Services;

namespace VDS.RDF
{
    //Callback Delegates for Async operations

    /// <summary>
    /// Callback for methods that return a <see cref="IGraph">IGraph</see> asynchronously
    /// </summary>
    /// <param name="g">Graph</param>
    /// <param name="state">State</param>
    public delegate void GraphCallback(IGraph g, Object state);

    /// <summary>
    /// Callback for methods that return a Namespace Map
    /// </summary>
    /// <param name="nsmap">Namespace Map</param>
    /// <param name="state">State</param>
    public delegate void NamespaceCallback(INamespaceMapper nsmap, Object state);

    /// <summary>
    /// Callbacks for methods that return a list of nodes
    /// </summary>
    /// <param name="nodes">Node List</param>
    /// <param name="state">State</param>
    public delegate void NodeListCallback(List<INode> nodes, Object state);

}