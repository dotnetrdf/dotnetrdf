/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved.
 *******************************************************************************/
/*

dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using System.Linq;
using System.Runtime.CompilerServices;
using VDS.RDF.Query.Spin.Utility;

namespace VDS.RDF.Query.Spin.OntologyHelpers
{
    /**
     * Vocabulary of the SPIN SPARQL Syntax schema.
     *
     * @author Holger Knublauch
     */

    public class SP
    {
        public const String BASE_URI = "http://spinrdf.org/sp";

        public const String NS_URI = BASE_URI + "#";

        public const String PREFIX = "sp";

        public const String VAR_NS = "http://spinrdf.org/var#";

        public const String VAR_PREFIX = "var";

        public readonly static IUriNode ClassAdd = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Add"));

        public readonly static IUriNode ClassAggregation = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Aggregation"));

        public readonly static IUriNode ClassAltPath = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "AltPath"));

        public readonly static IUriNode ClassAsc = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Asc"));

        public readonly static IUriNode ClassAsk = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Ask"));

        public readonly static IUriNode ClassAvg = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Avg"));

        public readonly static IUriNode ClassBind = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Bind"));

        public readonly static IUriNode ClassClear = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Clear"));

        public readonly static IUriNode ClassCommand = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Command"));

        public readonly static IUriNode ClassConstruct = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Construct"));

        public readonly static IUriNode ClassCopy = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Copy"));

        public readonly static IUriNode ClassCount = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Count"));

        public readonly static IUriNode ClassCreate = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Create"));

        [Obsolete("Use SP.ClassModify instead")]
        public readonly static IUriNode ClassDelete = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Delete"));

        public readonly static IUriNode ClassDeleteData = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "DeleteData"));

        public readonly static IUriNode ClassDeleteWhere = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "DeleteWhere"));

        public readonly static IUriNode ClassDesc = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Desc"));

        public readonly static IUriNode ClassDescribe = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Describe"));

        public readonly static IUriNode ClassDrop = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Drop"));

        public readonly static IUriNode PropertyExists = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "exists"));

        public readonly static IUriNode ClassExists = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Exists"));

        public readonly static IUriNode ClassExpression = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Expression"));

        public readonly static IUriNode ClassFilter = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Filter"));

        /// <summary>
        /// Warning : This is not yet fully defined by the http://www.w3.org/Submission/spin-sparql/ specification
        /// </summary>
        public readonly static IUriNode ClassGroupConcat = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "GroupConcat"));

        [Obsolete("Use SP.ClassModify instead")]
        public readonly static IUriNode ClassInsert = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Insert"));

        public readonly static IUriNode ClassInsertData = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "InsertData"));

        [Obsolete("Use SP.ClassBind instead")]
        public readonly static IUriNode ClassLet = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Let"));

        public readonly static IUriNode ClassLoad = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Load"));

        public readonly static IUriNode ClassMax = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Max"));

        public readonly static IUriNode ClassMin = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Min"));

        public readonly static IUriNode ClassModify = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Modify"));

        public readonly static IUriNode ClassModPath = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "ModPath"));

        public readonly static IUriNode ClassMove = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Move"));

        public readonly static IUriNode ClassMinus = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Minus"));

        public readonly static IUriNode ClassNamedGraph = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "NamedGraph"));

        public readonly static IUriNode PropertyNotExists = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "notExists"));

        public readonly static IUriNode ClassNotExists = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "NotExists"));

        public readonly static IUriNode ClassOptional = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Optional"));

        public readonly static IUriNode ClassQuery = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Query"));

        public readonly static IUriNode ClassReverseLinkPath = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "ReverseLinkPath"));

        public readonly static IUriNode ClassReversePath = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "ReversePath"));

        /// <summary>
        /// Warning : This is not yet fully defined by the http://www.w3.org/Submission/spin-sparql/ specification
        /// </summary>
        public readonly static IUriNode ClassSample = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Sample"));

        public readonly static IUriNode ClassSelect = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Select"));

        public readonly static IUriNode ClassService = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Service"));

        public readonly static IUriNode ClassSeqPath = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "SeqPath"));

        public readonly static IUriNode ClassSubQuery = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "SubQuery"));

        public readonly static IUriNode ClassSum = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Sum"));

        public readonly static IUriNode ClassTriple = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Triple"));

        public readonly static IUriNode ClassTriplePath = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "TriplePath"));

        public readonly static IUriNode ClassTriplePattern = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "TriplePattern"));

        public readonly static IUriNode ClassTripleTemplate = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "TripleTemplate"));

        public readonly static IUriNode ClassPropertyUndef = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "undef"));

        public readonly static IUriNode ClassUnion = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Union"));

        public readonly static IUriNode ClassUpdate = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Update"));

        public readonly static IUriNode ClassValues = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Values"));

        public readonly static IUriNode ClassVariable = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Variable"));

        public readonly static IUriNode PropertyAll = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "all"));

        public readonly static IUriNode PropertyArg = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "arg"));

        public readonly static IUriNode PropertyArg1 = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "arg1"));

        public readonly static IUriNode PropertyArg2 = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "arg2"));

        public readonly static IUriNode PropertyArg3 = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "arg3"));

        public readonly static IUriNode PropertyArg4 = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "arg4"));

        public readonly static IUriNode PropertyArg5 = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "arg5"));

        public readonly static IUriNode PropertyAs = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "as"));

        public readonly static IUriNode PropertyBindings = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "bindings"));

        public readonly static IUriNode PropertyData = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "data"));

        public readonly static IUriNode PropertyDefault = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "default"));

        public readonly static IUriNode PropertyDeletePattern = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "deletePattern"));

        public readonly static IUriNode PropertyDistinct = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "distinct"));

        public readonly static IUriNode PropertyDocument = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "document"));

        public readonly static IUriNode PropertyElements = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "elements"));

        public readonly static IUriNode PropertyExpression = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "expression"));

        public readonly static IUriNode PropertyFrom = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "from"));

        public readonly static IUriNode PropertyFromNamed = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "fromNamed"));

        public readonly static IUriNode PropertyGraphIRI = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "graphIRI"));

        public readonly static IUriNode PropertyGraphNameNode = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "graphNameNode"));

        public readonly static IUriNode PropertyGroupBy = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "groupBy"));

        public readonly static IUriNode PropertyHaving = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "having"));

        public readonly static IUriNode PropertyInsertPattern = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "insertPattern"));

        public readonly static IUriNode PropertyInto = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "into"));

        public readonly static IUriNode PropertyLimit = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "limit"));

        public readonly static IUriNode PropertyModMax = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "modMax"));

        public readonly static IUriNode PropertyModMin = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "modMin"));

        public readonly static IUriNode PropertyNamed = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "named"));

        public readonly static IUriNode PropertyNode = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "node"));

        public readonly static IUriNode PropertyObject = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "object"));

        public readonly static IUriNode PropertyOffset = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "offset"));

        public readonly static IUriNode PropertyOrderBy = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "orderBy"));

        public readonly static IUriNode PropertyPath = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "path"));

        public readonly static IUriNode PropertyPath1 = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "path1"));

        public readonly static IUriNode PropertyPath2 = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "path2"));

        public readonly static IUriNode PropertyPredicate = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "predicate"));

        public readonly static IUriNode PropertyQuery = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "query"));

        public readonly static IUriNode PropertyReduced = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "reduced"));

        public readonly static IUriNode PropertyResultNodes = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "resultNodes"));

        public readonly static IUriNode PropertyResultVariables = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "resultVariables"));

        public readonly static IUriNode PropertySeparator = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "separator"));

        public readonly static IUriNode PropertyServiceURI = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "serviceURI"));

        public readonly static IUriNode PropertySilent = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "silent"));

        public readonly static IUriNode PropertySubject = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "subject"));

        public readonly static IUriNode PropertySubPath = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "subPath"));

        public readonly static IUriNode PropertyTemplates = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "templates"));

        public readonly static IUriNode PropertyText = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "text"));

        public readonly static IUriNode PropertyUsing = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "using"));

        public readonly static IUriNode PropertyUsingNamed = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "usingNamed"));

        public readonly static IUriNode PropertyValues = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "values"));

        public readonly static IUriNode PropertyVariable = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "variable"));

        public readonly static IUriNode PropertyVarName = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "varName"));

        public readonly static IUriNode PropertyVarNames = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "varNames"));

        public readonly static IUriNode PropertyWhere = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "where"));

        public readonly static IUriNode PropertyWith = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "with"));

        public readonly static IUriNode PropertyBound = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "bound"));

        public readonly static IUriNode PropertyEq = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "eq"));

        public readonly static IUriNode PropertyNeq = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "neq"));

        public readonly static IUriNode PropertyNot = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "not"));

        public readonly static IUriNode PropertyRegex = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "regex"));

        public readonly static IUriNode PropertySub = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "sub"));

        public readonly static IUriNode PropertyUnaryMinus = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "unaryMinus"));

        public readonly static IUriNode PropertyOr = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "or"));

        public readonly static IUriNode PropertyAnd = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "and"));

        public readonly static IUriNode PropertyLt = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "lt"));

        public readonly static IUriNode PropertyLeq = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "leq"));

        public readonly static IUriNode PropertyGt = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "gt"));

        public readonly static IUriNode PropertyGeq = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "geq"));

        private static Graph model;

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static Graph GetModel()
        {
            if (model == null)
            {
                model = new Graph();
                model.BaseUri = UriFactory.Create(BASE_URI);
                //TODO trouver comment utiliser le fichier local sans avoir besoin de recompiler (si possible ?)
                model.LoadFromUri(UriFactory.Create(BASE_URI), new VDS.RDF.Parsing.RdfXmlParser());
            }
            return model;
        }

        static SP()
        {
            //GetModel();
            //SP.init(BuiltinPersonalities.model);
        }

        /**
         * Checks whether the SP ontology is used in a given Model.
         * This is true if the model defines the SP namespace prefix
         * and also has sp:Query defined with an rdf:type.
         * The goal of this call is to be very fast when SP is not
         * imported, i.e. it checks the namespace first and can then
         * omit the type query.
         * @param model  the Model to check
         * @return true if SP exists in model
         */

        public static bool existsModel(IGraph model)
        {
            return model != null &&
                model.NamespaceMap.GetPrefix(UriFactory.Create(SP.NS_URI)) != null &&
                model.GetTriplesWithSubjectPredicate(SP.ClassQuery, RDF.PropertyType).Any();
        }

        public static int? getArgPropertyIndex(String varName)
        {
            if (varName.StartsWith("arg"))
            {
                String subString = varName.Substring(3);
                try
                {
                    return Int32.Parse(subString) - 1;
                }
                catch (Exception t)
                {
                }
            }
            return null;
        }

#if UNDEFINED
    static SP() {
		SP.init(BuiltinPersonalities.model);
    }

    //@SuppressWarnings("deprecation")
	private static void init(Personality<INode> p) {
    	p.Add(Aggregation, new SimpleImplementation(SPL.Argument, AggregationImpl));
    	p.Add(Argument, new SimpleImplementation(SPL.Argument, ArgumentImpl));
    	p.Add(Attribute, new SimpleImplementation(SPL.Attribute, AttributeImpl));
    	p.Add(Ask, new SimpleImplementation(Ask, AskImpl));
    	p.Add(Bind, new SimpleImplementation2(Bind, Let, BindImpl));
    	p.Add(Clear, new SimpleImplementation(Clear, ClearImpl));
    	p.Add(Construct, new SimpleImplementation(Construct, ConstructImpl));
    	p.Add(Create, new SimpleImplementation(Create, CreateImpl));
    	p.Add(VDS.RDF.Query.Spin.Model.Delete, new SimpleImplementation(Delete, VDS.RDF.Query.Spin.Model.DeleteImpl));
    	p.Add(DeleteData, new SimpleImplementation(DeleteData, DeleteDataImpl));
    	p.Add(DeleteWhere, new SimpleImplementation(DeleteWhere, DeleteWhereImpl));
    	p.Add(Describe, new SimpleImplementation(Describe, DescribeImpl));
    	p.Add(Drop, new SimpleImplementation(Drop, DropImpl));
    	p.Add(ElementList, new SimpleImplementation(RDF.List, ElementListImpl));
    	p.Add(Exists, new SimpleImplementation(Exists, ExistsImpl));
    	p.Add(Function, new SimpleImplementation(SPIN.Function, FunctionImpl));
    	p.Add(FunctionCall, new SimpleImplementation(SPIN.Function, FunctionCallImpl));
    	p.Add(Filter, new SimpleImplementation(Filter, FilterImpl));
    	p.Add(VDS.RDF.Query.Spin.Model.Insert, new SimpleImplementation(Insert, VDS.RDF.Query.Spin.Model.InsertImpl));
    	p.Add(InsertData, new SimpleImplementation(InsertData, InsertDataImpl));
    	p.Add(Load, new SimpleImplementation(Load, LoadImpl));
    	p.Add(Minus, new SimpleImplementation(Minus, MinusImpl));
    	p.Add(Modify, new SimpleImplementation(Modify, ModifyImpl));
    	p.Add(Module, new SimpleImplementation(SPIN.Module, ModuleImpl));
    	p.Add(NamedGraph, new SimpleImplementation(NamedGraph, NamedGraphImpl));
    	p.Add(NotExists, new SimpleImplementation(NotExists, NotExistsImpl));
    	p.Add(Optional, new SimpleImplementation(Optional, OptionalImpl));
    	p.Add(Service, new SimpleImplementation(Service, ServiceImpl));
    	p.Add(Select, new SimpleImplementation(Select, SelectImpl));
    	p.Add(SubQuery, new SimpleImplementation(SubQuery, SubQueryImpl));
    	p.Add(SPINInstance, new SimpleImplementation(RDFS.Resource, SPINInstanceImpl));
    	p.Add(Template, new SimpleImplementation(SPIN.Template, TemplateImpl));
    	p.Add(TemplateCall, new SimpleImplementation(RDFS.Resource, TemplateCallImpl));
    	p.Add(TriplePath, new SimpleImplementation(TriplePath, TriplePathImpl));
    	p.Add(TriplePattern, new SimpleImplementation(TriplePattern, TriplePatternImpl));
    	p.Add(TripleTemplate, new SimpleImplementation(TripleTemplate, TripleTemplateImpl));
    	p.Add(Union, new SimpleImplementation(Union, UnionImpl));
    	p.Add(Values, new SimpleImplementation(Values, ValuesImpl));
    	p.Add(Variable, new SimpleImplementation(Variable, VariableImpl));
    }

    public static INode getArgProperty(int index) {
    	return RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "arg" + index);
    }

    public static INode getArgProperty(String varName) {
    	return RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + varName);
    }

	public static String getURI() {
        return NS_URI;
    }

	public static void toStringElementList(StringBuilder buffer, INode resource) {
		RDFList list = resource.As(RDFList);
		for(IEnumerator<INode> it = list.GetEnumerator(); it.MoveNext(); ) {
			INode item = (INode) it.Current;
			Element e = SPINFactory.asElement(item);
			buffer.append(e.ToString());
			if(it.MoveNext()) {
				buffer.append(" .\n"));
			}
		}
	}
#endif
    }
}