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
using VDS.RDF.Query.Spin;
using VDS.RDF.Storage;
using VDS.RDF.Query.Spin.Util;
namespace VDS.RDF.Query.Spin.LibraryOntology
{
    /**
     * Vocabulary for http://spinrdf.org/spinmap
     *
     * Automatically generated with TopBraid Composer.
     */
    internal class SPINMAP
    {

        public const String BASE_URI = "http://spinrdf.org/spinmap";

        public const String NS_URI = BASE_URI + "#";

        public const String PREFIX = "spinmap";

        public const String TARGET_PREDICATE = "targetPredicate";

        public const String SOURCE_PREDICATE = "sourcePredicate";

        public readonly static IUriNode ClassContext = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Context"));

        public readonly static IUriNode ClassMapping = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Mapping"));

        public readonly static IUriNode ClassMapping_0_1 = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Mapping-0-1"));

        public readonly static IUriNode ClassMapping_1 = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Mapping-1"));

        public readonly static IUriNode ClassMapping_1_1 = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Mapping-1-1"));

        public readonly static IUriNode ClassMapping_2_1 = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Mapping-2-1"));

        public readonly static IUriNode ClassTargetFunction = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "TargetFunction"));

        public readonly static IUriNode ClassTargetFunctions = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "TargetFunctions"));

        public readonly static IUriNode ClassTransformationFunction = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "TransformationFunction"));

        public readonly static IUriNode ClassTransformationFunctions = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "TransformationFunctions"));

        public readonly static IUriNode PropertyContext = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "context"));

        public readonly static IUriNode PropertyEquals = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "equals"));

        public readonly static IUriNode PropertyExpression = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "expression"));

        public readonly static IUriNode PropertyFunction = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "function"));

        public readonly static IUriNode PropertyInverseExpression = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "inverseExpression"));

        public readonly static IUriNode PropertyPostRule = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "postRule"));

        public readonly static IUriNode PropertyPredicate = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "predicate"));

        public readonly static IUriNode PropertyPrepRule = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "prepRule"));

        public readonly static IUriNode PropertyRule = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "rule"));

        public readonly static IUriNode PropertyShortLabel = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "shortLabel"));

        public readonly static IUriNode PropertySource = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "source"));

        public readonly static IUriNode PropertySourceClass = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "sourceClass"));

        public readonly static IUriNode PropertySourcePredicate1 = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + SOURCE_PREDICATE + "1"));

        public readonly static IUriNode PropertySourcePredicate2 = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + SOURCE_PREDICATE + "2"));

        public readonly static IUriNode PropertySourcePredicate3 = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + SOURCE_PREDICATE + "3"));

        public readonly static IUriNode PropertySourceVariable = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "_source"));

        public readonly static IUriNode PropertySuggestion_0_1 = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "suggestion-0-1"));

        public readonly static IUriNode PropertySuggestion_1_1 = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "suggestion-1-1"));

        public readonly static IUriNode PropertySuggestionScore = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "suggestionScore"));

        public readonly static IUriNode PropertyTarget = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "target"));

        public readonly static IUriNode PropertyTargetClass = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "targetClass"));

        public readonly static IUriNode PropertyTargetPredicate1 = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + TARGET_PREDICATE + "1"));

        public readonly static IUriNode PropertyTargetPredicate2 = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + TARGET_PREDICATE + "2"));

        public readonly static IUriNode PropertyTargetResource = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "targetResource"));

        public readonly static IUriNode PropertyTemplate = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "template"));

        public readonly static IUriNode PropertyType = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "type"));

        public readonly static IUriNode PropertyValue = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "value"));

        public readonly static IUriNode PropertyValue1 = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "value1"));

        public readonly static IUriNode PropertyValue2 = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "value2"));


/*
        public static bool exists(Model model)
        {
            return model.Contains(model.Object(SPINMAP.BASE_URI), RDF.type, OWL.Ontology);
        }


        public static String getURI()
        {
            return NS_URI;
        }
*/
    }
}
