/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

namespace VDS.RDF.Writing
{
    /// <summary>
    /// A helper class containing GraphML name and URI constants
    /// </summary>
    public static class GraphMLSpecsHelper
    {
        /// <summary>
        /// The namespace URI for GraphML XML elements
        /// </summary>
        public const string NS = "http://graphml.graphdrawing.org/xmlns";

        /// <summary>
        /// The URL of the GraphML XML schema
        /// </summary>
        public const string XsdUri = "http://graphml.graphdrawing.org/xmlns/1.1/graphml.xsd";

        /// <summary>
        /// The name of the GraphML XML root element
        /// </summary>
        public const string GraphML = "graphml";

        /// <summary>
        /// The name of the GraphML XML element representing a graph
        /// </summary>
        public const string Graph = "graph";

        /// <summary>
        /// The name of the GraphML XML attribute representing the default directedness of and edge
        /// </summary>
        public const string Edgedefault = "edgedefault";

        /// <summary>
        /// The value representing a directed edge
        /// </summary>
        public const string Directed = "directed";

        /// <summary>
        /// The name of the GraphML XML element representing an edge
        /// </summary>
        public const string Edge = "edge";

        /// <summary>
        /// The name of the GraphML attribute representing the source of an edge
        /// </summary>
        public const string Source = "source";

        /// <summary>
        /// The name of the GraphML attribute representing the target of an edge
        /// </summary>
        public const string Target = "target";

        /// <summary>
        /// The name of the GraphML element representing the source of a node
        /// </summary>
        public const string Node = "node";

        /// <summary>
        /// The name of the GraphML element representing custom attributes for nodes and edges
        /// </summary>
        public const string Data = "data";

        /// <summary>
        /// The name of the GraphML attribute representing the domain of a key
        /// </summary>
        public const string Domain = "for";

        /// <summary>
        /// The name of the GraphML attribute representing the type of an attribute
        /// </summary>
        public const string AttributeTyte = "attr.type";

        /// <summary>
        /// The value representing the string type
        /// </summary>
        public const string String = "string";

        /// <summary>
        /// The value representing a node label attribute id
        /// </summary>
        public const string NodeLabel = "label";

        /// <summary>
        /// The value representing an edge label attribute id
        /// </summary>
        public const string EdgeLabel = "edgelabel";

        /// <summary>
        /// The name of the GraphML attribute representing the id of a node or edge
        /// </summary>
        public const string Id = "id";

        /// <summary>
        /// The name of the GraphML element representing a key
        /// </summary>
        public const string Key = "key";
    }
}
