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

using System;
using VDS.RDF;
using System.Runtime.CompilerServices;
using VDS.RDF.Query.Spin;
using System.IO;
using VDS.RDF.Storage;
using VDS.RDF.Query.Spin.Util;
namespace VDS.RDF.Query.Spin.LibraryOntology
{

    /**
     * Vocabulary of the SPIN Standard Modules Library (SPL).
     * 
     * @author Holger Knublauch
     */
    internal class SPL
    {

        public const String BASE_URI = "http://spinrdf.org/spl";

        public const String NS_URI = BASE_URI + "#";

        public const String PREFIX = "spl";


        public readonly static IUriNode ClassArgument = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Argument"));

        public readonly static IUriNode ClassAttribute = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Attribute"));

        public readonly static IUriNode ClassInferDefaultValue = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "InferDefaultValue"));

        public readonly static IUriNode ClassObjectCountPropertyConstraint = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "ObjectCountPropertyConstraint"));

        public readonly static IUriNode ClassRunTestCases = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "RunTestCases"));

        public readonly static IUriNode ClassSPINOverview = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "SPINOverview"));

        public readonly static IUriNode ClassTestCase = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "TestCase"));


        public readonly static IUriNode PropertyObjectCount = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "objectCount"));


        public readonly static IUriNode PropertyDefaultValue = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "defaultValue"));

        public readonly static IUriNode PropertyHasValue = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "hasValue"));

        public readonly static IUriNode PropertyMaxCount = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "maxCount"));

        public readonly static IUriNode PropertyMinCount = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "minCount"));

        public readonly static IUriNode PropertyOptional = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "optional"));

        public readonly static IUriNode PropertyPredicate = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "predicate"));

        public readonly static IUriNode PropertyValueType = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "valueType"));

        private static Graph model;
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static Graph GetModel()
        {
            if (model == null)
            {
                model = new Graph();
                model.BaseUri = UriFactory.Create(BASE_URI);
                model.LoadFromUri(UriFactory.Create(BASE_URI), new VDS.RDF.Parsing.RdfXmlParser());
            }
            return model;
        }
    }
}