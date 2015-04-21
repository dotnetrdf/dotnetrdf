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
     * Vocabulary of the SPIN Modeling Vocabulary.
     *
     * @author Holger Knublauch
     */

    public class SPIN
    {
        public const String BASE_URI = "http://spinrdf.org/spin";

        public const String NS_URI = BASE_URI + "#";

        public const String PREFIX = "spin";

        public const String THIS_VAR_NAME = "this";

        public readonly static IUriNode ClassAskTemplate = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "AskTemplate"));

        public readonly static IUriNode ClassColumn = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Column"));

        public readonly static IUriNode ClassConstraintViolation = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "ConstraintViolation"));

        public readonly static IUriNode ClassConstructTemplate = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "ConstructTemplate"));

        public readonly static IUriNode PropertyEval = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "eval"));

        public readonly static IUriNode ClassFunction = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Function"));

        public readonly static IUriNode ClassFunctions = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Functions"));

        public readonly static IUriNode ClassLibraryOntology = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "LibraryOntology"));

        public readonly static IUriNode ClassMagicProperties = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "MagicProperties"));

        public readonly static IUriNode ClassMagicProperty = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "MagicProperty"));

        public readonly static IUriNode ClassModule = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Module"));

        public readonly static IUriNode ClassModules = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Modules"));

        public readonly static IUriNode ClassRule = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Rule"));

        public readonly static IUriNode ClassRuleProperty = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "RuleProperty"));

        public readonly static IUriNode ClassSelectTemplate = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "SelectTemplate"));

        public readonly static IUriNode ClassTableDataProvider = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "TableDataProvider"));

        public readonly static IUriNode ClassTemplate = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Template"));

        public readonly static IUriNode ClassTemplates = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Templates"));

        public readonly static IUriNode ClassUpdateTemplate = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "UpdateTemplate"));

        public readonly static IUriNode PropertyAbstract = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "abstract"));

        public readonly static IUriNode PropertyBody = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "body"));

        public readonly static IUriNode PropertyColumn = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "column"));

        public readonly static IUriNode PropertyColumnIndex = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "columnIndex"));

        public readonly static IUriNode PropertyColumnWidth = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "columnWidth"));

        public readonly static IUriNode PropertyColumnType = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "columnType"));

        public readonly static IUriNode PropertyCommand = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "command"));

        public readonly static IUriNode PropertyConstraint = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "constraint"));

        public readonly static IUriNode PropertyConstructor = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "constructor"));

        public readonly static IUriNode PropertyFix = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "fix"));

        public readonly static IUriNode PropertyImports = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "imports"));

        public readonly static IUriNode PropertyLabelTemplate = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "labelTemplate"));

        public readonly static IUriNode PropertyNextRuleProperty = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "nextRuleProperty"));

        public readonly static IUriNode PropertyPrivate = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "private"));

        public readonly static IUriNode PropertyQuery = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "query"));

        public readonly static IUriNode PropertyReturnType = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "returnType"));

        public readonly static IUriNode PropertyRule = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "rule"));

        public readonly static IUriNode PropertyRulePropertyMaxIterationCount = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "rulePropertyMaxIterationCount"));

        public readonly static IUriNode PropertySymbol = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "symbol"));

        public readonly static IUriNode PropertyThisUnbound = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "thisUnbound"));

        public readonly static IUriNode PropertyViolationPath = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "violationPath"));

        public readonly static IUriNode PropertyViolationRoot = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "violationRoot"));

        public readonly static IUriNode PropertyViolationSource = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "violationSource"));

        public readonly static IUriNode Property_arg1 = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "_arg1"));

        public readonly static IUriNode Property_arg2 = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "_arg2"));

        public readonly static IUriNode Property_arg3 = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "_arg3"));

        public readonly static IUriNode Property_arg4 = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "_arg4"));

        public readonly static IUriNode Property_arg5 = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "_arg5"));

        public readonly static IUriNode Property_this = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "_this"));

        static SPIN()
        {
            // TODO Force initialization through the SPINImports class
            //GetModel();
        }

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