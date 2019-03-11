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

namespace VDS.RDF.Shacl
{
    using System.Collections.Generic;
    using VDS.RDF;

    public class Shacl
    {
        private static NodeFactory factory = new NodeFactory();

        public const string BaseUri = "http://www.w3.org/ns/shacl#";

        public static IUriNode Path => factory.CreateUriNode(UriFactory.Create($"{BaseUri}path"));

        public static IUriNode TargetClass => factory.CreateUriNode(UriFactory.Create($"{BaseUri}targetClass"));
        public static IUriNode TargetNode => factory.CreateUriNode(UriFactory.Create($"{BaseUri}targetNode"));
        public static IUriNode TargetObjectsOf => factory.CreateUriNode(UriFactory.Create($"{BaseUri}targetObjectsOf"));
        public static IUriNode TargetSubjectsOf => factory.CreateUriNode(UriFactory.Create($"{BaseUri}targetSubjectsOf"));

        public static IUriNode Class => factory.CreateUriNode(UriFactory.Create($"{BaseUri}class"));
        public static IUriNode Node => factory.CreateUriNode(UriFactory.Create($"{BaseUri}node"));
        public static IUriNode Property => factory.CreateUriNode(UriFactory.Create($"{BaseUri}property"));

        public static IUriNode NodeShape => factory.CreateUriNode(UriFactory.Create($"{BaseUri}NodeShape"));
        public static IUriNode PropertyShape => factory.CreateUriNode(UriFactory.Create($"{BaseUri}PropertyShape"));

        public static IUriNode AlternativePath => factory.CreateUriNode(UriFactory.Create($"{BaseUri}alternativePath"));
        public static IUriNode InversePath => factory.CreateUriNode(UriFactory.Create($"{BaseUri}inversePath"));
        public static IUriNode OneOrMorePath => factory.CreateUriNode(UriFactory.Create($"{BaseUri}oneOrMorePath"));
        public static IUriNode ZeroOrMorePath => factory.CreateUriNode(UriFactory.Create($"{BaseUri}zeroOrMorePath"));
        public static IUriNode ZeroOrOnePath => factory.CreateUriNode(UriFactory.Create($"{BaseUri}zeroOrOnePath"));

        public static IUriNode Conforms => factory.CreateUriNode(UriFactory.Create($"{BaseUri}conforms"));

        public static IEnumerable<IUriNode> Shapes
        {
            get
            {
                yield return NodeShape;
                yield return PropertyShape;
            }
        }

        public static IEnumerable<IUriNode> Targets
        {
            get
            {
                yield return TargetClass;
                yield return TargetNode;
                yield return TargetObjectsOf;
                yield return TargetSubjectsOf;
            }
        }

        public static IEnumerable<IUriNode> Constraints
        {
            get
            {
                yield return Class;
                yield return Node;
                yield return Property;
            }
        }
    }
}
