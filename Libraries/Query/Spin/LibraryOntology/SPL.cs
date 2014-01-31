/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/

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
    public class SPL
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