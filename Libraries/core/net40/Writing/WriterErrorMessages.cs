/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using System.Linq;
using System.Text;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Class containing constants for standardised Writer Error Messages
    /// </summary>
    public static class WriterErrorMessages
    {
        /// <summary>
        /// Error message produced when a User attempts to serialize a Graph containing Graph Literals
        /// </summary>
        private const String GraphLiteralsUnserializableError = "Graph Literal Nodes are not serializable in {0}";
        /// <summary>
        /// Error message produced when a User attempts to serialize a Graph containing Unknown Node Types
        /// </summary>
        private const String UnknownNodeTypeUnserializableError = "Unknown Node Types cannot be serialized as {0}";
        /// <summary>
        /// Error message produced when a User attempts to serialize a Graph containing Triples with Literal Subjects
        /// </summary>
        private const String LiteralSubjectsUnserializableError = "Triples with a Literal Subject are not serializable in {0}";
        /// <summary>
        /// Error message produced when a User attempts to serialize a Graph containing Triples with Literal Predicates
        /// </summary>
        private const String LiteralPredicatesUnserializableError = "Triples with a Literal Predicate are not serializable in {0}";
        /// <summary>
        /// Error message produced when a User attempts to serialized a Graph containing Triples with Graph Literal Predicates
        /// </summary>
        private const String GraphLiteralPredicatesUnserializableError = "Triples with a Graph Literal Predicate are not serializable in {0}";
        /// <summary>
        /// Error message produced when a User attempts to serialize a Graph containing Triples with Blank Node Predicates
        /// </summary>
        private const String BlankPredicatesUnserializableError = "Triples with a Blank Node Predicate are not serializable in {0}";
        /// <summary>
        /// Error message produced when a User attempts to serialize a Graph containing URIs which cannot be reduced to a URIRef or QName as required by the serialization
        /// </summary>
        public const String UnreducablePropertyURIUnserializable = "Unable to serialize this Graph since a Property has an unreducable URI";
        /// <summary>
        /// Error message produced when a User attempts to serialize a Graph containing collections where a collection item has more than one rdf:first triple
        /// </summary>
        public const String MalformedCollectionWithMultipleFirsts = "This RDF Graph contains more than one rdf:first Triple for an Item in a Collection which means the Graph is not serializable";
        /// <summary>
        /// Error messages produced when errors occur in a multi-threaded writing process
        /// </summary>
        public const String ThreadedOutputError = "One/more errors occurred while outputting RDF in {0} using a multi-threaded writing process";
        /// <summary>
        /// Error message produced when a User attempts to serialize a Variable Node in a format which does not support it
        /// </summary>
        public const String VariableNodesUnserializableError = "Variable Nodes cannot be serialized as {0}";

        /// <summary>
        /// Gets an Error message indicating that Graph Literals are not serializable with the appropriate RDF format name inserted in the error
        /// </summary>
        /// <param name="format">RDF format (syntax)</param>
        /// <returns></returns>
        public static String GraphLiteralsUnserializable(String format)
        {
            return String.Format(GraphLiteralsUnserializableError, format);
        }

        /// <summary>
        /// Gets an Error message indicating that Unknown Node Types are not serializable with the appropriate RDF format name inserted in the error
        /// </summary>
        /// <param name="format">RDF format (syntax)</param>
        /// <returns></returns>
        public static String UnknownNodeTypeUnserializable(String format)
        {
            return String.Format(UnknownNodeTypeUnserializableError, format);
        }

        /// <summary>
        /// Gets an Error message indicating that Variable Nodes are not serializable with the appropriate RDF format name inserted in the error
        /// </summary>
        /// <param name="format">RDF format (syntax)</param>
        /// <returns></returns>
        public static String VariableNodesUnserializable(String format)
        {
            return String.Format(VariableNodesUnserializableError, format);
        }

        /// <summary>
        /// Gets an Error message indicating that Literal Subjects are not serializable with the appropriate RDF format name inserted in the error
        /// </summary>
        /// <param name="format">RDF format (syntax)</param>
        /// <returns></returns>
        public static String LiteralSubjectsUnserializable(String format)
        {
            return String.Format(LiteralSubjectsUnserializableError, format);
        }

        /// <summary>
        /// Gets an Error message indicating that Literal Predicates are not serializable with the appropriate RDF format name inserted in the error
        /// </summary>
        /// <param name="format">RDF format (syntax)</param>
        /// <returns></returns>
        public static String LiteralPredicatesUnserializable(String format)
        {
            return String.Format(LiteralPredicatesUnserializableError, format);
        }

        /// <summary>
        /// Gets an Error message indicating that Graph Literal Predicates are not serializable with the appropriate RDF format name inserted in the error
        /// </summary>
        /// <param name="format">RDF format (syntax)</param>
        /// <returns></returns>
        public static String GraphLiteralPredicatesUnserializable(String format)
        {
            return String.Format(GraphLiteralPredicatesUnserializableError, format);
        }

        /// <summary>
        /// Gets an Error message indicating that Blank Node Predicates are not serializable with the appropriate RDF format name inserted in the error
        /// </summary>
        /// <param name="format">RDF format (syntax)</param>
        /// <returns></returns>
        public static String BlankPredicatesUnserializable(String format)
        {
            return String.Format(BlankPredicatesUnserializableError, format);
        }

        /// <summary>
        /// Gets an Error message indicating that a multi-threading writer process failed
        /// </summary>
        /// <param name="format">RDF format (syntax)</param>
        /// <returns></returns>
        public static String ThreadedOutputFailure(String format)
        {
            return String.Format(ThreadedOutputError, format);
        }
    }
}
