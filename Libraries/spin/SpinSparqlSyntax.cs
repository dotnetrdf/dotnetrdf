using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;

namespace VDS.RDF.Query.Spin
{
    /// <summary>
    /// Static class containing Extension Methods used to serialize SPARQL Queries into the SPIN SPARQL Syntax
    /// </summary>
    public static class SpinSparqlSyntax
    {
        public const String SpinSparqlSyntaxNamespace = "http://spinrdf.org/sp";

        public const String SpinClassAggregation = SpinSparqlSyntaxNamespace + "Aggregation",
                            SpinClassAltPath = SpinSparqlSyntaxNamespace + "AltPath",
                            SpinClassAsc = SpinSparqlSyntaxNamespace + "Asc",
                            SpinClassAsk = SpinSparqlSyntaxNamespace + "Ask",
                            SpinClassAvg = SpinSparqlSyntaxNamespace + "Avg",
                            SpinClassCommand = SpinSparqlSyntaxNamespace + "Command",
                            SpinClassConstruct = SpinSparqlSyntaxNamespace + "Construct",
                            SpinClassCount = SpinSparqlSyntaxNamespace + "Count",
                            SpinClassDelete = SpinSparqlSyntaxNamespace + "Delete",
                            SpinClassDesc = SpinSparqlSyntaxNamespace + "Desc",
                            SpinClassElement = SpinSparqlSyntaxNamespace + "Element",
                            SpinClassElementGroup = SpinClassElement + "Group",
                            SpinClassElementList = SpinClassElement + "List",
                            SpinClassFilter = SpinSparqlSyntaxNamespace + "Filter",
                            SpinClassInsert = SpinSparqlSyntaxNamespace + "Insert",
                            SpinClassLet = SpinSparqlSyntaxNamespace + "Let",
                            SpinClassMax = SpinSparqlSyntaxNamespace + "Max",
                            SpinClassMin = SpinSparqlSyntaxNamespace + "Min",
                            SpinClassMinus = SpinSparqlSyntaxNamespace + "Minus",
                            SpinClassModify = SpinSparqlSyntaxNamespace + "Modify",
                            SpinClassModPath = SpinSparqlSyntaxNamespace + "ModPath",
                            SpinClassNamedGraph = SpinSparqlSyntaxNamespace + "NamedGraph",
                            SpinClassNotExists = SpinSparqlSyntaxNamespace + "NotExists",
                            SpinClassOptional = SpinSparqlSyntaxNamespace + "Optional",
                            SpinClassOrderByCondition = SpinSparqlSyntaxNamespace + "OrderByCondition",
                            SpinClassPath = SpinSparqlSyntaxNamespace + "Path",
                            SpinClassQuery = SpinSparqlSyntaxNamespace + "Query",
                            SpinClassReversePath = SpinSparqlSyntaxNamespace + "ReversePath",
                            SpinClassSelect = SpinSparqlSyntaxNamespace + "Select",
                            SpinClassSeqPath = SpinSparqlSyntaxNamespace + "SeqPath",
                            SpinClassService = SpinSparqlSyntaxNamespace + "Service",
                            SpinClassSubQuery = SpinSparqlSyntaxNamespace + "SubQuery",
                            SpinClassSum = SpinSparqlSyntaxNamespace + "Sum",
                            SpinClassSystemClass = SpinSparqlSyntaxNamespace + "SystemClass",
                            SpinClassTriple = SpinSparqlSyntaxNamespace + "Triple",
                            SpinClassTriplePath = SpinClassTriple + "Path",
                            SpinClassTriplePattern = SpinClassTriple + "Pattern",
                            SpinClassTripleTemplate = SpinClassTriple + "Template",
                            SpinClassTuple = SpinSparqlSyntaxNamespace + "Tuple",
                            SpinClassUnion = SpinSparqlSyntaxNamespace + "Union",
                            SpinClassVariable = SpinSparqlSyntaxNamespace + "Variable";

        public const String SpinPropertyArgument = SpinSparqlSyntaxNamespace + "arg",
                            SpinPropertyArgument1 = SpinPropertyArgument + "1",
                            SpinPropertyArgument2 = SpinPropertyArgument + "2",
                            SpinPropertyArgument3 = SpinPropertyArgument + "3",
                            SpinPropertyArgument4 = SpinPropertyArgument + "4",
                            SpinPropertyAs = SpinSparqlSyntaxNamespace + "as",
                            SpinPropertyDeletePattern  = SpinSparqlSyntaxNamespace + "deletePattern",
                            SpinPropertyDistinct = SpinSparqlSyntaxNamespace + "distinct",
                            SpinPropertyElements = SpinSparqlSyntaxNamespace + "elements",
                            SpinPropertyExpression = SpinSparqlSyntaxNamespace + "expression",
                            SpinPropertyFrom = SpinSparqlSyntaxNamespace + "from",
                            SpinPropertyFromNamed = SpinSparqlSyntaxNamespace + "fromNamed",
                            SpinPropertyGraphIri = SpinSparqlSyntaxNamespace + "graphIRI",
                            SpinPropertyGraphNameNode = SpinSparqlSyntaxNamespace + "graphNameNode",
                            SpinPropertyGroupBy = SpinSparqlSyntaxNamespace + "groupBy",
                            SpinPropertyHaving = SpinSparqlSyntaxNamespace + "having",
                            SpinPropertyInsertPattern = SpinSparqlSyntaxNamespace + "insertPattern",
                            SpinPropertyLimit = SpinSparqlSyntaxNamespace + "limit",
                            SpinPropertyModMax = SpinSparqlSyntaxNamespace + "modMax",
                            SpinPropertyModMin = SpinSparqlSyntaxNamespace + "modMin",
                            SpinPropertyObject = SpinSparqlSyntaxNamespace + "object",
                            SpinPropertyOffset = SpinSparqlSyntaxNamespace + "offset",
                            SpinPropertyOrderBy = SpinSparqlSyntaxNamespace + "orderBy",
                            SpinPropertyPath = SpinSparqlSyntaxNamespace + "path",
                            SpinPropertyPath1 = SpinPropertyPath + "1",
                            SpinPropertyPath2 = SpinPropertyPath + "2",
                            SpinPropertyPredicate = SpinSparqlSyntaxNamespace + "predicate",
                            SpinPropertyQuery = SpinSparqlSyntaxNamespace + "query",
                            SpinPropertyReduced = SpinSparqlSyntaxNamespace + "reduced",
                            SpinPropertyResultNodes = SpinSparqlSyntaxNamespace + "resultNodes",
                            SpinPropertyResultVariables = SpinSparqlSyntaxNamespace + "resultVariables",
                            SpinPropertyServiceUri = SpinSparqlSyntaxNamespace + "serviceURI",
                            SpinPropertySubject = SpinSparqlSyntaxNamespace + "subject",
                            SpinPropertySubPath = SpinSparqlSyntaxNamespace + "subPath",
                            SpinPropertySystemProperty = SpinSparqlSyntaxNamespace + "systemProperty",
                            SpinPropertyTemplates = SpinSparqlSyntaxNamespace + "templates",
                            SpinPropertyText = SpinSparqlSyntaxNamespace + "text",
                            SpinPropertyVariable = SpinSparqlSyntaxNamespace + "variable",
                            SpinPropertyVariableName = SpinSparqlSyntaxNamespace + "varName",
                            SpinPropertyWhere = SpinSparqlSyntaxNamespace + "where";

        public static IGraph ToSpinRdf(this SparqlQuery query)
        {
            Graph g = new Graph();
            query.ToSpinRdf(g);
            return g;            
        }

        public static INode ToSpinRdf(this SparqlQuery query, IGraph g)
        {
            INode root = g.CreateBlankNode();
            INode rdfType = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));

            switch (query.QueryType)
            {
                case SparqlQueryType.Ask:
                    g.Assert(root, rdfType, g.CreateUriNode(new Uri(SpinClassAsk)));
                    break;

                case SparqlQueryType.Construct:
                    g.Assert(root, rdfType, g.CreateUriNode(new Uri(SpinClassConstruct)));
                    break;

                case SparqlQueryType.Describe:
                case SparqlQueryType.DescribeAll:
                    throw new SpinException("DESCRIBE queries cannot be represented in SPIN SPARQL Syntax");

                case SparqlQueryType.Select:
                case SparqlQueryType.SelectAll:
                case SparqlQueryType.SelectAllDistinct:
                case SparqlQueryType.SelectAllReduced:
                case SparqlQueryType.SelectDistinct:
                case SparqlQueryType.SelectReduced:
                    g.Assert(root, rdfType, g.CreateUriNode(new Uri(SpinClassSelect)));
                    break;
                case SparqlQueryType.Unknown:
                    throw new SpinException("Unknown query types cannot be represented in SPIN SPARQL Syntax");
            }

            return root;
        }
    }
}
