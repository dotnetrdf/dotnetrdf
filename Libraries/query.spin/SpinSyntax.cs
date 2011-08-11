using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Aggregates;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Grouping;
using VDS.RDF.Query.Ordering;
using VDS.RDF.Query.Paths;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Spin
{
    /// <summary>
    /// Static class containing Extension Methods used to serialize SPARQL Queries into the SPIN RDF Syntax
    /// </summary>
    public static class SpinSyntax
    {
        public const String SpinNamespace = "http://spinrdf.org/spin#";
        public const String SpinSparqlSyntaxNamespace = "http://spinrdf.org/sp#";

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
                            SpinClassExists = SpinSparqlSyntaxNamespace + "Exists",
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

        internal static INode ToSpinRdf(this SparqlQuery query, IGraph g)
        {
            INode root = g.CreateBlankNode();
            INode rdfType = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            SpinVariableTable varTable = new SpinVariableTable(g);

            //Ensure the Query is optimised so that all the elements are placed in Graph Patterns
            //so we can serialized them OK
            query.Optimise();

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
                    throw new SpinException("DESCRIBE queries cannot be represented in SPIN RDF Syntax");

                case SparqlQueryType.Select:
                case SparqlQueryType.SelectAll:
                case SparqlQueryType.SelectAllDistinct:
                case SparqlQueryType.SelectAllReduced:
                case SparqlQueryType.SelectDistinct:
                case SparqlQueryType.SelectReduced:
                    g.Assert(root, rdfType, g.CreateUriNode(new Uri(SpinClassSelect)));
                    break;
                case SparqlQueryType.Unknown:
                    throw new SpinException("Unknown query types cannot be represented in SPIN RDF Syntax");
            }

            //Process the WHERE clause
            g.Assert(root, g.CreateUriNode(new Uri(SpinPropertyWhere)), query.RootGraphPattern.ToSpinRdf(g, varTable));

            //Add Variables for a SELECT query
            if (SparqlSpecsHelper.IsSelectQuery(query.QueryType))
            {
                switch (query.QueryType)
                {
                    case SparqlQueryType.Select:
                    case SparqlQueryType.SelectDistinct:
                    case SparqlQueryType.SelectReduced:
                        //Only Add Variables for SELECTs with explicit variable lists
                        INode vars = g.CreateBlankNode();
                        g.Assert(root, g.CreateUriNode(new Uri(SpinPropertyResultVariables)), vars);

                        //Get the Variables and generate the Nodes we'll use to help represent them
                        List<SparqlVariable> vs = query.Variables.Where(v => v.IsResultVariable).ToList();
                        INode rdfFirst = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfListFirst));
                        INode rdfRest = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfListRest));
                        INode rdfNil = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfListNil));
                        INode varClass = g.CreateUriNode(new Uri(SpinClassVariable));
                        INode varName = g.CreateUriNode(new Uri(SpinPropertyVariableName));
                        INode varAs = g.CreateUriNode(new Uri(SpinPropertyAs));
                        INode expr = g.CreateUriNode(new Uri(SpinPropertyExpression));

                        for (int i = 0; i < vs.Count; i++)
                        {
                            SparqlVariable v = vs[i];
                            INode var = varTable[v.Name];
                            g.Assert(vars, rdfFirst, var);
                            if (i < vs.Count - 1)
                            {
                                INode temp = g.CreateBlankNode();
                                g.Assert(vars, rdfRest, temp);
                                vars = temp;
                            }
                            //g.Assert(var, rdfType, varClass);

                            if (v.IsAggregate)
                            {
                                g.Assert(var, varAs, g.CreateLiteralNode(v.Name, new Uri(XmlSpecsHelper.XmlSchemaDataTypeString)));
                                g.Assert(var, expr, v.Aggregate.ToSpinRdf(g, varTable));
                            }
                            else if (v.IsProjection)
                            {
                                g.Assert(var, varAs, g.CreateLiteralNode(v.Name, new Uri(XmlSpecsHelper.XmlSchemaDataTypeString)));
                                g.Assert(var, expr, v.Projection.ToSpinRdf(g, varTable));
                            }
                            else
                            {
                                g.Assert(var, varName, g.CreateLiteralNode(v.Name, new Uri(XmlSpecsHelper.XmlSchemaDataTypeString)));
                            }
                        }
                        g.Assert(vars, rdfRest, rdfNil);

                        break;
                }
            }

            //Add DISTINCT/REDUCED modifiers if appropriate
            if (query.HasDistinctModifier)
            {
                switch (query.QueryType)
                {
                    case SparqlQueryType.SelectAllDistinct:
                    case SparqlQueryType.SelectDistinct:
                        g.Assert(root, g.CreateUriNode(new Uri(SpinPropertyDistinct)), (true).ToLiteral(g));
                        break;
                    case SparqlQueryType.SelectAllReduced:
                    case SparqlQueryType.SelectReduced:
                        g.Assert(root, g.CreateUriNode(new Uri(SpinPropertyReduced)), (true).ToLiteral(g));
                        break;
                }
            }

            //Add LIMIT and/or OFFSET if appropriate
            if (query.Limit > -1)
            {
                g.Assert(root, g.CreateUriNode(new Uri(SpinPropertyLimit)), query.Limit.ToLiteral(g));
            }
            if (query.Offset > 0)
            {
                g.Assert(root, g.CreateUriNode(new Uri(SpinPropertyOffset)), query.Offset.ToLiteral(g));
            }

            //Add ORDER BY if appropriate
            if (query.OrderBy != null)
            {
                g.Assert(root, g.CreateUriNode(new Uri(SpinPropertyOrderBy)), query.OrderBy.ToSpinRdf(g, varTable));
            }

            //Add GROUP BY and HAVING
            if (query.GroupBy != null)
            {
                throw new SpinException("GROUP BY clauses are not yet representable in SPIN RDF Syntax");
            }
            if (query.Having != null)
            {
                throw new SpinException("HAVING clauses are not yet representable in SPIN RDF Syntax");
            }

            return root;
        }

        internal static INode ToSpinRdf(this GraphPattern pattern, IGraph g, SpinVariableTable varTable)
        {
            INode p = g.CreateBlankNode();
            INode ps = p;

            if (pattern.IsExists)
            {
                g.Assert(new Triple(p, g.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)), g.CreateUriNode(new Uri(SpinClassExists))));
                ps = g.CreateBlankNode();
                g.Assert(new Triple(p, g.CreateUriNode(new Uri(SpinPropertyElements)), ps));
            }
            else if (pattern.IsGraph)
            {
                g.Assert(new Triple(p, g.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)), g.CreateUriNode(new Uri(SpinClassNamedGraph))));
                g.Assert(new Triple(p, g.CreateUriNode(new Uri(SpinPropertyGraphNameNode)), pattern.GraphSpecifier.ToSpinRdf(g, varTable)));
                ps = g.CreateBlankNode();
                g.Assert(new Triple(p, g.CreateUriNode(new Uri(SpinPropertyElements)), ps));
            }
            else if (pattern.IsMinus)
            {
                g.Assert(new Triple(p, g.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)), g.CreateUriNode(new Uri(SpinClassMinus))));
                ps = g.CreateBlankNode();
                g.Assert(new Triple(p, g.CreateUriNode(new Uri(SpinPropertyElements)), ps));
            }
            else if (pattern.IsNotExists)
            {
                g.Assert(new Triple(p, g.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)), g.CreateUriNode(new Uri(SpinClassNotExists))));
                ps = g.CreateBlankNode();
                g.Assert(new Triple(p, g.CreateUriNode(new Uri(SpinPropertyElements)), ps));
            }
            else if (pattern.IsOptional)
            {
                g.Assert(new Triple(p, g.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)), g.CreateUriNode(new Uri(SpinClassOptional))));
                ps = g.CreateBlankNode();
                g.Assert(new Triple(p, g.CreateUriNode(new Uri(SpinPropertyElements)), ps));
            }
            else if (pattern.IsService)
            {
                g.Assert(new Triple(p, g.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)), g.CreateUriNode(new Uri(SpinClassService))));
                g.Assert(new Triple(p, g.CreateUriNode(new Uri(SpinPropertyServiceUri)), pattern.GraphSpecifier.ToSpinRdf(g, varTable)));
                ps = g.CreateBlankNode();
                g.Assert(new Triple(p, g.CreateUriNode(new Uri(SpinPropertyElements)), ps));
            }
            else if (pattern.IsSubQuery)
            {
                g.Assert(new Triple(p, g.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)), g.CreateUriNode(new Uri(SpinClassSubQuery))));
                ps = g.CreateBlankNode();
                g.Assert(new Triple(p, g.CreateUriNode(new Uri(SpinPropertyQuery)), ps));
            }
            else if (pattern.IsUnion)
            {
                g.Assert(new Triple(p, g.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)), g.CreateUriNode(new Uri(SpinClassUnion))));
                ps = g.CreateBlankNode();
                g.Assert(new Triple(p, g.CreateUriNode(new Uri(SpinPropertyElements)), ps));
            }

            INode rdfFirst = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfListFirst));
            INode rdfRest = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfListRest));
            INode rdfNil = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfListNil));

            if (!pattern.IsEmpty)
            {
                //First output Triple Patterns
                for (int i = 0; i < pattern.TriplePatterns.Count; i++)
                {
                    INode current = pattern.TriplePatterns[i].ToSpinRdf(g, varTable);
                    if (i == 0)
                    {
                        g.Assert(ps, rdfFirst, current);
                    }
                    else
                    {
                        INode temp = g.CreateBlankNode();
                        g.Assert(ps, rdfRest, temp);
                        g.Assert(temp, rdfFirst, current);
                        ps = temp;
                    }
                }

                if (!pattern.HasChildGraphPatterns)
                {
                    g.Assert(ps, rdfRest, rdfNil);
                }
            }

            //Then output Graph Patterns
            if (pattern.HasChildGraphPatterns)
            {
                for (int i = 0; i < pattern.ChildGraphPatterns.Count; i++)
                {
                    INode current = pattern.ChildGraphPatterns[i].ToSpinRdf(g, varTable);
                    if (pattern.TriplePatterns.Count == 0 && i == 0)
                    {
                        g.Assert(ps, rdfFirst, current);
                    }
                    else
                    {
                        INode temp = g.CreateBlankNode();
                        g.Assert(ps, rdfRest, temp);
                        g.Assert(temp, rdfFirst, current);
                        ps = temp;
                    }
                }
                g.Assert(ps, rdfRest, rdfNil);
            }

            return p;
        }

        internal static INode ToSpinRdf(this ITriplePattern pattern, IGraph g, SpinVariableTable varTable)
        {
            INode p = g.CreateBlankNode();
            INode rdfType = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));

            if (pattern is TriplePattern)
            {
                TriplePattern tp = (TriplePattern)pattern;
                //g.Assert(p, rdfType, g.CreateUriNode(new Uri(SpinClassTriplePattern)));
                g.Assert(p, g.CreateUriNode(new Uri(SpinPropertySubject)), tp.Subject.ToSpinRdf(g, varTable));
                g.Assert(p, g.CreateUriNode(new Uri(SpinPropertyPredicate)), tp.Predicate.ToSpinRdf(g, varTable));
                g.Assert(p, g.CreateUriNode(new Uri(SpinPropertyObject)), tp.Object.ToSpinRdf(g, varTable));
            }
            else if (pattern is SubQueryPattern)
            {
                g.Assert(p, rdfType, g.CreateUriNode(new Uri(SpinClassSubQuery)));
                g.Assert(p, g.CreateUriNode(new Uri(SpinPropertyQuery)), ((SubQueryPattern)pattern).SubQuery.ToSpinRdf(g));
            }
            else if (pattern is FilterPattern)
            {
                g.Assert(p, rdfType, g.CreateUriNode(new Uri(SpinClassFilter)));
                g.Assert(p, g.CreateUriNode(new Uri(SpinPropertyExpression)), ((FilterPattern)pattern).Filter.Expression.ToSpinRdf(g, varTable));
            }
            else if (pattern is PropertyPathPattern)
            {
                PropertyPathPattern pp = (PropertyPathPattern)pattern;
                g.Assert(p, rdfType, g.CreateUriNode(new Uri(SpinClassTriplePath)));
                g.Assert(p, g.CreateUriNode(new Uri(SpinPropertySubject)), pp.Subject.ToSpinRdf(g, varTable));
                g.Assert(p, g.CreateUriNode(new Uri(SpinPropertyPath)), pp.Path.ToSpinRdf(g, varTable));
                g.Assert(p, g.CreateUriNode(new Uri(SpinPropertyObject)), pp.Object.ToSpinRdf(g, varTable));
            }
            else if (pattern is LetPattern)
            {
                g.Assert(p, rdfType, g.CreateUriNode(new Uri(SpinClassLet)));
                INode var = g.CreateBlankNode();
                g.Assert(p, g.CreateUriNode(new Uri(SpinPropertyVariable)), var);
                g.Assert(var, g.CreateUriNode(new Uri(SpinPropertyVariableName)), g.CreateLiteralNode(((LetPattern)pattern).VariableName, new Uri(XmlSpecsHelper.XmlSchemaDataTypeString)));
                g.Assert(p, g.CreateUriNode(new Uri(SpinPropertyExpression)), ((LetPattern)pattern).AssignExpression.ToSpinRdf(g, varTable));
            }
            else if (pattern is BindPattern)
            {
                throw new SpinException("SPARQL 1.1 BINDs are not representable in SPIN RDF Syntax");
            }

            return p;
        }

        internal static INode ToSpinRdf(this PatternItem item, IGraph g, SpinVariableTable varTable)
        {
            INode i;
            IUriNode varName = g.CreateUriNode(new Uri(SpinPropertyVariableName));

            if (item is NodeMatchPattern)
            {
                i = ((NodeMatchPattern)item).Node;
            }
            else if (item is VariablePattern)
            {
                //i = g.CreateUriNode(new Uri(SpinNamespace + "_" + item.VariableName));
                i = varTable[item.VariableName];
                g.Assert(i, varName, g.CreateLiteralNode(item.VariableName, new Uri(XmlSpecsHelper.XmlSchemaDataTypeString)));
            }
            else if (item is BlankNodePattern)
            {
                throw new NotImplementedException();
            }
            else if (item is FixedBlankNodePattern)
            {
                throw new SpinException("Skolem Blank Node syntax extension is not representable in SPIN RDF Syntax");
            }
            else
            {
                throw new SpinException("Unknown Pattern Items are not representable in SPIN RDF Syntax");
            }

            return i.CopyNode(g);
        }

        internal static INode ToSpinRdf(this ISparqlAggregate aggregate, IGraph g, SpinVariableTable varTable)
        {
            INode a = g.CreateBlankNode();
            INode rdfType = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));

            if (aggregate is AverageAggregate)
            {
                g.Assert(a, rdfType, g.CreateUriNode(new Uri(SpinClassAvg)));
            }
            else if (aggregate is CountAggregate)
            {
                g.Assert(a, rdfType, g.CreateUriNode(new Uri(SpinClassCount)));
            }
            else if (aggregate is MaxAggregate)
            {
                g.Assert(a, rdfType, g.CreateUriNode(new Uri(SpinClassMax)));
            }
            else if (aggregate is MinAggregate)
            {
                g.Assert(a, rdfType, g.CreateUriNode(new Uri(SpinClassMin)));
            }
            else if (aggregate is SumAggregate)
            {
                g.Assert(a, rdfType, g.CreateUriNode(new Uri(SpinClassSum)));
            }
            else if (aggregate is GroupConcatAggregate)
            {
                throw new SpinException("GROUP_CONCAT aggregates are not yet representable in SPIN RDF Syntax");
            }
            else if (aggregate is SampleAggregate)
            {
                throw new SpinException("SAMPLE aggregates are not yet representable in SPIN RDF Syntax");
            }

            g.Assert(a, g.CreateUriNode(new Uri(SpinPropertyExpression)), aggregate.Expression.ToSpinRdf(g, varTable));

            return a;
        }

        internal static INode ToSpinRdf(this ISparqlExpression expr, IGraph g, SpinVariableTable varTable)
        {
            INode e = g.CreateBlankNode();

            return e;
        }

        internal static INode ToSpinRdf(this ISparqlOrderBy ordering, IGraph g, SpinVariableTable varTable)
        {
            INode o = g.CreateBlankNode();

            INode rdfFirst = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfListFirst));
            INode rdfRest = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfListRest));
            INode rdfNil = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfListNil));
            INode expr = g.CreateUriNode(new Uri(SpinPropertyExpression));
            INode item = null;

            do
            {
                if (item == null)
                {
                    item = g.CreateBlankNode();
                }
                else
                {
                    INode temp = g.CreateBlankNode();
                    g.Assert(o, rdfRest, temp);
                    item = temp;
                    ordering = ordering.Child;
                }
                g.Assert(o, rdfFirst, item);
                //TODO: Convert Expression
                //g.Assert(o, expr, ordering.E
            } while (ordering.Child != null);
            
            return o;
        }

        internal static INode ToSpinRdf(this ISparqlPath path, IGraph g, SpinVariableTable varTable)
        {
            INode p = g.CreateBlankNode();

            return p;
        }

        internal static INode ToSpinRdf(this IToken t, IGraph g, SpinVariableTable varTable)
        {
            switch (t.TokenType)
            {
                case Token.QNAME:
                case Token.URI:
                    return ParserHelper.TryResolveUri(g, t);
                case Token.VARIABLE:
                    return varTable[t.Value];
                default:
                    throw new SpinException("Unable to convert a Graph/Service Specifier which is not a QName/URI/Variable to SPIN RDF Syntax");
            }
        }
    }

    internal class SpinVariableTable
    {
        private Dictionary<String, INode> _vars = new Dictionary<string, INode>();
        private IGraph _g;

        public SpinVariableTable(IGraph g)
        {
            this._g = g;
        }

        public INode this[String var]
        {
            get 
            {
                if (!this._vars.ContainsKey(var))
                {
                    this._vars.Add(var, this._g.CreateBlankNode());
                }
                return this._vars[var];
            }
        }
    }
}
