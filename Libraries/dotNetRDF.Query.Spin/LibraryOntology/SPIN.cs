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
using System.Runtime.CompilerServices;
using VDS.RDF;
using VDS.RDF.Query.Spin.Util;

namespace VDS.RDF.Query.Spin.LibraryOntology
{

    /**
     * Vocabulary of the SPIN Modeling Vocabulary.
     * 
     * @author Holger Knublauch
     */
    internal class SPIN
    {

        public const String BASE_URI = "http://spinrdf.org/spin";

        public const String NS_URI = BASE_URI + "#";

        public const String PREFIX = "spin";


        public const String THIS_VAR_NAME = "this";


        public readonly static IUriNode ClassAskTemplate = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "AskTemplate"));

        public readonly static IUriNode ClassColumn = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Column"));

        public readonly static IUriNode ClassConstraintViolation = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "ConstraintViolation"));

        public readonly static IUriNode ClassConstructTemplate = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "ConstructTemplate"));

        public readonly static IUriNode PropertyEval = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "eval"));

        public readonly static IUriNode ClassFunction = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Function"));

        public readonly static IUriNode ClassFunctions = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Functions"));

        public readonly static IUriNode ClassLibraryOntology = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "LibraryOntology"));

        public readonly static IUriNode ClassMagicProperties = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "MagicProperties"));

        public readonly static IUriNode ClassMagicProperty = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "MagicProperty"));

        public readonly static IUriNode ClassModule = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Module"));

        public readonly static IUriNode ClassModules = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Modules"));

        public readonly static IUriNode ClassRule = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Rule"));

        public readonly static IUriNode ClassRuleProperty = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "RuleProperty"));

        public readonly static IUriNode ClassSelectTemplate = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "SelectTemplate"));

        public readonly static IUriNode ClassTableDataProvider = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "TableDataProvider"));

        public readonly static IUriNode ClassTemplate = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Template"));

        public readonly static IUriNode ClassTemplates = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Templates"));

        public readonly static IUriNode ClassUpdateTemplate = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "UpdateTemplate"));


        public readonly static IUriNode PropertyAbstract = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "abstract"));

        public readonly static IUriNode PropertyBody = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "body"));

        public readonly static IUriNode PropertyColumn = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "column"));

        public readonly static IUriNode PropertyColumnIndex = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "columnIndex"));

        public readonly static IUriNode PropertyColumnWidth = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "columnWidth"));

        public readonly static IUriNode PropertyColumnType = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "columnType"));

        public readonly static IUriNode PropertyCommand = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "command"));

        public readonly static IUriNode PropertyConstraint = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "constraint"));

        public readonly static IUriNode PropertyConstructor = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "constructor"));

        public readonly static IUriNode PropertyFix = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "fix"));

        public readonly static IUriNode PropertyImports = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "imports"));

        public readonly static IUriNode PropertyLabelTemplate = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "labelTemplate"));

        public readonly static IUriNode PropertyNextRuleProperty = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "nextRuleProperty"));

        public readonly static IUriNode PropertyPrivate = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "private"));

        public readonly static IUriNode PropertyQuery = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "query"));

        public readonly static IUriNode PropertyReturnType = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "returnType"));

        public readonly static IUriNode PropertyRule = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "rule"));

        public readonly static IUriNode PropertyRulePropertyMaxIterationCount = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "rulePropertyMaxIterationCount"));

        public readonly static IUriNode PropertySymbol = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "symbol"));

        public readonly static IUriNode PropertyThisUnbound = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "thisUnbound"));

        public readonly static IUriNode PropertyViolationPath = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "violationPath"));

        public readonly static IUriNode PropertyViolationRoot = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "violationRoot"));

        public readonly static IUriNode PropertyViolationSource = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "violationSource"));


        public readonly static IUriNode Property_arg1 = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "_arg1"));

        public readonly static IUriNode Property_arg2 = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "_arg2"));

        public readonly static IUriNode Property_arg3 = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "_arg3"));

        public readonly static IUriNode Property_arg4 = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "_arg4"));

        public readonly static IUriNode Property_arg5 = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "_arg5"));

        public readonly static IUriNode Property_this = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "_this"));


        static SPIN() {
            // Force initialization
            //GetModel();
        }


        private static Graph model;
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static Graph GetModel() {
            if(model == null) {
                model = new Graph();
                model.BaseUri = UriFactory.Create(BASE_URI);
                model.LoadFromUri(UriFactory.Create(BASE_URI), new VDS.RDF.Parsing.RdfXmlParser());
            }
            return model;
        }
    }
}