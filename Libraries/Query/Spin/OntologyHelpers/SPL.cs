/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved.
 *******************************************************************************/

using System;
using System.Runtime.CompilerServices;
using VDS.RDF.Query.Spin.Utility;

namespace VDS.RDF.Query.Spin.OntologyHelpers
{
    /**
     * Vocabulary of the SPIN Standard Modules Library (SPL).
     *
     * @author Holger Knublauch
     */

    public class SPL
    {
        public const String BASE_URI = "http://spinrdf.org/spl";

        public const String NS_URI = BASE_URI + "#";

        public const String PREFIX = "spl";

        public readonly static IUriNode ClassArgument = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Argument"));

        public readonly static IUriNode ClassAttribute = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Attribute"));

        public readonly static IUriNode ClassInferDefaultValue = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "InferDefaultValue"));

        public readonly static IUriNode ClassObjectCountPropertyConstraint = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "ObjectCountPropertyConstraint"));

        public readonly static IUriNode ClassRunTestCases = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "RunTestCases"));

        public readonly static IUriNode ClassSPINOverview = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "SPINOverview"));

        public readonly static IUriNode ClassTestCase = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "TestCase"));

        public readonly static IUriNode PropertyObjectCount = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "objectCount"));

        public readonly static IUriNode PropertyDefaultValue = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "defaultValue"));

        public readonly static IUriNode PropertyHasValue = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "hasValue"));

        public readonly static IUriNode PropertyMaxCount = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "maxCount"));

        public readonly static IUriNode PropertyMinCount = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "minCount"));

        public readonly static IUriNode PropertyOptional = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "optional"));

        public readonly static IUriNode PropertyPredicate = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "predicate"));

        public readonly static IUriNode PropertyValueType = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "valueType"));

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