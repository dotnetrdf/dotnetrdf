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
using System.Linq;
using System.Runtime.CompilerServices;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Spin.Util;

namespace VDS.RDF.Query.Spin.LibraryOntology
{

    /* *
     * Vocabulary of the SPIN SPARQL Syntax schema.
     * 
     * @author Holger Knublauch
     */
    internal class SP
    {

        public const String BASE_URI = "http://spinrdf.org/sp";

        public const String NS_URI = BASE_URI + "#";

        public const String PREFIX = "sp";

        public const String VAR_NS = "http://spinrdf.org/var#";

        public const String VAR_PREFIX = "var";


        public readonly static IUriNode ClassAdd = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Add"));

        public readonly static IUriNode ClassAggregation = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Aggregation"));

        public readonly static IUriNode ClassAltPath = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "AltPath"));

        public readonly static IUriNode ClassAsc = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Asc"));

        public readonly static IUriNode ClassAsk = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Ask"));

        public readonly static IUriNode ClassAvg = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Avg"));

        public readonly static IUriNode ClassBind = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Bind"));

        public readonly static IUriNode ClassClear = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Clear"));

        public readonly static IUriNode ClassCommand = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Command"));

        public readonly static IUriNode ClassConstruct = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Construct"));

        public readonly static IUriNode ClassCopy = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Copy"));

        public readonly static IUriNode ClassCount = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Count"));

        public readonly static IUriNode ClassCreate = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Create"));

        [Obsolete()]
        public readonly static IUriNode ClassDelete = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Delete"));

        public readonly static IUriNode ClassDeleteData = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "DeleteData"));

        public readonly static IUriNode ClassDeleteWhere = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "DeleteWhere"));

        public readonly static IUriNode ClassDesc = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Desc"));

        public readonly static IUriNode ClassDescribe = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Describe"));

        public readonly static IUriNode ClassDrop = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Drop"));

        public readonly static IUriNode PropertyExists = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "exists"));

        public readonly static IUriNode ClassExists = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Exists"));

        public readonly static IUriNode ClassExpression = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Expression"));

        public readonly static IUriNode ClassFilter = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Filter"));

        [Obsolete()]
        public readonly static IUriNode ClassInsert = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Insert"));

        public readonly static IUriNode ClassInsertData = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "InsertData"));

        [Obsolete()]
        public readonly static IUriNode ClassLet = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Let"));

        public readonly static IUriNode ClassLoad = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Load"));

        public readonly static IUriNode ClassMax = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Max"));

        public readonly static IUriNode ClassMin = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Min"));

        public readonly static IUriNode ClassModify = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Modify"));

        public readonly static IUriNode ClassModPath = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "ModPath"));

        public readonly static IUriNode ClassMove = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Move"));

        public readonly static IUriNode ClassMinus = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Minus"));

        public readonly static IUriNode ClassNamedGraph = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "NamedGraph"));

        public readonly static IUriNode PropertyNotExists = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "notExists"));

        public readonly static IUriNode ClassNotExists = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "NotExists"));

        public readonly static IUriNode ClassOptional = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Optional"));

        public readonly static IUriNode ClassQuery = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Query"));

        public readonly static IUriNode ClassReverseLinkPath = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "ReverseLinkPath"));

        public readonly static IUriNode ClassReversePath = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "ReversePath"));

        public readonly static IUriNode ClassSelect = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Select"));

        public readonly static IUriNode ClassService = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Service"));

        public readonly static IUriNode ClassSeqPath = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "SeqPath"));

        public readonly static IUriNode ClassSubQuery = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "SubQuery"));

        public readonly static IUriNode ClassSum = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Sum"));

        public readonly static IUriNode ClassTriple = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Triple"));

        public readonly static IUriNode ClassTriplePath = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "TriplePath"));

        public readonly static IUriNode ClassTriplePattern = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "TriplePattern"));

        public readonly static IUriNode ClassTripleTemplate = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "TripleTemplate"));

        public readonly static IUriNode ClassPropertyUndef = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "undef"));

        public readonly static IUriNode ClassUnion = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Union"));

        public readonly static IUriNode ClassUpdate = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Update"));

        public readonly static IUriNode ClassValues = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Values"));

        public readonly static IUriNode ClassVariable = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Variable"));


        public readonly static IUriNode PropertyAll = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "all"));

        public readonly static IUriNode PropertyArg = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "arg"));

        public readonly static IUriNode PropertyArg1 = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "arg1"));

        public readonly static IUriNode PropertyArg2 = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "arg2"));

        public readonly static IUriNode PropertyArg3 = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "arg3"));

        public readonly static IUriNode PropertyArg4 = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "arg4"));

        public readonly static IUriNode PropertyArg5 = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "arg5"));

        public readonly static IUriNode PropertyAs = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "as"));

        public readonly static IUriNode PropertyBindings = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "bindings"));

        public readonly static IUriNode PropertyData = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "data"));

        public readonly static IUriNode PropertyDefault = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "default"));

        public readonly static IUriNode PropertyDeletePattern = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "deletePattern"));

        public readonly static IUriNode PropertyDistinct = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "distinct"));

        public readonly static IUriNode PropertyDocument = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "document"));

        public readonly static IUriNode PropertyElements = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "elements"));

        public readonly static IUriNode PropertyExpression = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "expression"));

        public readonly static IUriNode PropertyFrom = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "from"));

        public readonly static IUriNode PropertyFromNamed = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "fromNamed"));

        public readonly static IUriNode PropertyGraphIRI = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "graphIRI"));

        public readonly static IUriNode PropertyGraphNameNode = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "graphNameNode"));

        public readonly static IUriNode PropertyGroupBy = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "groupBy"));

        public readonly static IUriNode PropertyHaving = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "having"));

        public readonly static IUriNode PropertyInsertPattern = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "insertPattern"));

        public readonly static IUriNode PropertyInto = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "into"));

        public readonly static IUriNode PropertyLimit = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "limit"));

        public readonly static IUriNode PropertyModMax = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "modMax"));

        public readonly static IUriNode PropertyModMin = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "modMin"));

        public readonly static IUriNode PropertyNamed = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "named"));

        public readonly static IUriNode PropertyNode = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "node"));

        public readonly static IUriNode PropertyObject = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "object"));

        public readonly static IUriNode PropertyOffset = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "offset"));

        public readonly static IUriNode PropertyOrderBy = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "orderBy"));

        public readonly static IUriNode PropertyPath = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "path"));

        public readonly static IUriNode PropertyPath1 = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "path1"));

        public readonly static IUriNode PropertyPath2 = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "path2"));

        public readonly static IUriNode PropertyPredicate = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "predicate"));

        public readonly static IUriNode PropertyQuery = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "query"));

        public readonly static IUriNode PropertyReduced = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "reduced"));

        public readonly static IUriNode PropertyResultNodes = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "resultNodes"));

        public readonly static IUriNode PropertyResultVariables = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "resultVariables"));

        public readonly static IUriNode PropertySeparator = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "separator"));

        public readonly static IUriNode PropertyServiceURI = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "serviceURI"));

        public readonly static IUriNode PropertySilent = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "silent"));

        public readonly static IUriNode PropertySubject = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "subject"));

        public readonly static IUriNode PropertySubPath = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "subPath"));

        public readonly static IUriNode PropertyTemplates = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "templates"));

        public readonly static IUriNode PropertyText = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "text"));

        public readonly static IUriNode PropertyUsing = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "using"));

        public readonly static IUriNode PropertyUsingNamed = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "usingNamed"));

        public readonly static IUriNode PropertyValues = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "values"));

        public readonly static IUriNode PropertyVariable = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "variable"));

        public readonly static IUriNode PropertyVarName = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "varName"));

        public readonly static IUriNode PropertyVarNames = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "varNames"));

        public readonly static IUriNode PropertyWhere = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "where"));

        public readonly static IUriNode PropertyWith = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "with"));


        public readonly static IUriNode PropertyBound = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "bound"));

        public readonly static IUriNode PropertyEq = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "eq"));

        public readonly static IUriNode PropertyNeq = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "neq"));

        public readonly static IUriNode PropertyNot = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "not"));

        public readonly static IUriNode PropertyRegex = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "regex"));

        public readonly static IUriNode PropertySub = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "sub"));

        public readonly static IUriNode PropertyUnaryMinus = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "unaryMinus"));

        public readonly static IUriNode PropertyOr = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "or"));

        public readonly static IUriNode PropertyAnd = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "and"));

        public readonly static IUriNode PropertyLt = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "lt"));

        public readonly static IUriNode PropertyLeq = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "leq"));

        public readonly static IUriNode PropertyGt = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "gt"));

        public readonly static IUriNode PropertyGeq = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "geq"));

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


        /* *
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
                var subString = varName.Substring(3);
                try
                {
                    return int.Parse(subString);
                }
                catch (Exception)
                {
                }
            }
            return null;
        }

/*
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
*/
    }
}
