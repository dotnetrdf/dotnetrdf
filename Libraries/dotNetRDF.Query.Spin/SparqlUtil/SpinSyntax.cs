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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Aggregates;
using VDS.RDF.Query.Aggregates.Sparql;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Grouping;
using VDS.RDF.Query.Ordering;
using VDS.RDF.Query.Paths;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF.Query.Spin.Util;
using VDS.RDF.Update;
using VDS.RDF.Update.Commands;

namespace VDS.RDF.Query.Spin
{

    // TODO rename this class to SPINFactory
    /// <summary>
    /// Static class containing Extension Methods used to serialize SPARQL Queries into the SPIN RDF Syntax
    /// </summary>
    internal static class SpinSyntax
    {

        public static IGraph ToSpinRdf(this SparqlQuery query)
        {
            Graph g = new Graph();
            g.NamespaceMap.AddNamespace("spin", UriFactory.Create(SPIN.NS_URI));
            g.NamespaceMap.AddNamespace("sp", UriFactory.Create(SP.NS_URI));
            query.ToSpinRdf(g);
            return g;
        }

        internal static INode ToSpinRdf(this SparqlQuery query, IGraph g)
        {
            INode root = g.CreateBlankNode();
            SpinVariableTable varTable = new SpinVariableTable(g);

            //Ensure the Query is optimised so that all the elements are placed in Graph Patterns
            //so we can serialized them OK
            query.Optimise();

            switch (query.QueryType)
            {
                case SparqlQueryType.Ask:
                    g.Assert(root, RDF.PropertyType, SP.ClassAsk);
                    break;

                case SparqlQueryType.Construct:
                    g.Assert(root, RDF.PropertyType, SP.ClassConstruct);
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
                    g.Assert(root, RDF.PropertyType, SP.ClassSelect);
                    break;
                case SparqlQueryType.Unknown:
                    throw new SpinException("Unknown query types cannot be represented in SPIN RDF Syntax");
            }

            //Process the WHERE clause
            g.Assert(root, SP.PropertyWhere, query.RootGraphPattern.ToSpinRdf(g, varTable));

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
                        g.Assert(root, SP.PropertyResultVariables, vars);

                        //Get the Variables and generate the Nodes we'll use to help represent them
                        List<SparqlVariable> vs = query.Variables.Where(v => v.IsResultVariable).ToList();

                        for (int i = 0; i < vs.Count; i++)
                        {
                            SparqlVariable v = vs[i];
                            INode var = varTable[v.Name];
                            g.Assert(vars, RDF.PropertyFirst, var);
                            if (i < vs.Count - 1)
                            {
                                INode temp = g.CreateBlankNode();
                                g.Assert(vars, RDF.PropertyRest, temp);
                                vars = temp;
                            }
                            // TODO check that was commented before modifications
                            //g.Assert(var, RDF.type, SP.Variable);

                            if (v.IsAggregate)
                            {
                                g.Assert(var, SP.PropertyAs, g.CreateLiteralNode(v.Name, XSD.string_.Uri));
                                g.Assert(var, SP.PropertyExpression, v.Aggregate.ToSpinRdf(g, varTable));
                            }
                            else if (v.IsProjection)
                            {
                                g.Assert(var, SP.PropertyAs, g.CreateLiteralNode(v.Name, XSD.string_.Uri));
                                //TODO check for this
                                //g.Assert(var, SP.expression, v.Projection.ToSpinRdf(query.RootGraphPattern, g, varTable));
                            }
                            else
                            {
                                g.Assert(var, SP.PropertyVarName, g.CreateLiteralNode(v.Name, XSD.string_.Uri));
                            }
                        }
                        g.Assert(vars, RDF.PropertyRest, RDF.Nil);

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
                        g.Assert(root, SP.PropertyDistinct, RDFUtil.TRUE);
                        break;
                    case SparqlQueryType.SelectAllReduced:
                    case SparqlQueryType.SelectReduced:
                        g.Assert(root, SP.PropertyReduced, RDFUtil.TRUE);
                        break;
                }
            }

            //Add LIMIT and/or OFFSET if appropriate
            if (query.Limit > -1)
            {
                g.Assert(root, SP.PropertyLimit, query.Limit.ToLiteral(g));
            }
            if (query.Offset > 0)
            {
                g.Assert(root, SP.PropertyOffset, query.Offset.ToLiteral(g));
            }

            //Add ORDER BY if appropriate
            if (query.OrderBy != null)
            {
                g.Assert(root, SP.PropertyOrderBy, query.OrderBy.ToSpinRdf(g, varTable));
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


        public static IGraph ToSpinRdf(this SparqlUpdateCommand query)
        {
            Graph g = new Graph();
            g.NamespaceMap.AddNamespace("spin", UriFactory.Create(SPIN.NS_URI));
            g.NamespaceMap.AddNamespace("sp", UriFactory.Create(SP.NS_URI));
            query.ToSpinRdf(g);
            return g;
        }

        // TODO handle the defaultGraph case 
        internal static INode ToSpinRdf(this SparqlUpdateCommand query, IGraph g)
        {
            INode root = g.CreateBlankNode();
            SpinVariableTable varTable = new SpinVariableTable(g);

            switch (query.CommandType)
            {
                case SparqlUpdateCommandType.Add:
                    g.Assert(root, RDF.PropertyType, SP.ClassAdd);
                    AddCommand add = (AddCommand)query;
                    if (add.SourceUri == null)
                    {
                        g.Assert(root, SP.PropertyGraphIRI, SP.PropertyDefault);
                    }
                    else
                    {
                        g.Assert(root, SP.PropertyGraphIRI, RDFUtil.CreateUriNode(add.SourceUri));
                    }
                    if (add.DestinationUri == null)
                    {
                        g.Assert(root, SP.PropertyInto, SP.PropertyDefault);
                    }
                    else
                    {
                        g.Assert(root, SP.PropertyInto, RDFUtil.CreateUriNode(add.DestinationUri));
                    }
                    break;
                case SparqlUpdateCommandType.Clear:
                    g.Assert(root, RDF.PropertyType, SP.ClassClear);
                    if (((ClearCommand)query).TargetUri == null)
                    {
                        g.Assert(root, SP.PropertyGraphIRI, SP.PropertyDefault);
                    }
                    else
                    {
                        g.Assert(root, SP.PropertyGraphIRI, RDFUtil.CreateUriNode(((ClearCommand)query).TargetUri));
                    }
                    break;
                case SparqlUpdateCommandType.Copy:
                    g.Assert(root, RDF.PropertyType, SP.ClassCopy);
                    CopyCommand copy = (CopyCommand)query;
                    if (copy.SourceUri == null)
                    {
                        g.Assert(root, SP.PropertyGraphIRI, SP.PropertyDefault);
                    }
                    else
                    {
                        g.Assert(root, SP.PropertyGraphIRI, RDFUtil.CreateUriNode(copy.SourceUri));
                    }
                    if (copy.DestinationUri == null)
                    {
                        g.Assert(root, SP.PropertyInto, SP.PropertyDefault);
                    }
                    else
                    {
                        g.Assert(root, SP.PropertyInto, RDFUtil.CreateUriNode(copy.DestinationUri));
                    }                
                    break;
                case SparqlUpdateCommandType.Create:
                    g.Assert(root, RDF.PropertyType, SP.ClassCreate);
                    CreateCommand create = (CreateCommand)query;
                    if (create.TargetUri== null)
                    {
                        g.Assert(root, SP.PropertyGraphIRI, SP.PropertyDefault);
                    }
                    else
                    {
                        g.Assert(root, SP.PropertyGraphIRI, RDFUtil.CreateUriNode(create.TargetUri));
                    }
                    break;
                case SparqlUpdateCommandType.Delete:
                    g.Assert(root, RDF.PropertyType, SP.ClassModify);
                    DeleteCommand delete = (DeleteCommand)query;
                    if (delete.GraphUri != null)
                    {
                        g.Assert(root, SP.PropertyWith, RDFUtil.CreateUriNode(delete.GraphUri));
                    }
                    // TODO handle the usings
                    g.Assert(root, SP.PropertyDeletePattern, delete.DeletePattern.ToSpinRdf(g, varTable));
                    g.Assert(root, SP.PropertyWhere, delete.WherePattern.ToSpinRdf(g, varTable));
                    break;
                case SparqlUpdateCommandType.DeleteData:
                    g.Assert(root, RDF.PropertyType, SP.ClassDeleteData);
                    g.Assert(root, SP.PropertyData, ((DeleteDataCommand)query).DataPattern.ToSpinRdf(g, varTable));
                    break;
                case SparqlUpdateCommandType.Drop:
                    g.Assert(root, RDF.PropertyType, SP.ClassDrop);
                    DropCommand drop = (DropCommand)query;
                    if (drop.TargetUri == null)
                    {
                        g.Assert(root, SP.PropertyGraphIRI, SP.PropertyDefault);
                    }
                    else
                    {
                        g.Assert(root, SP.PropertyGraphIRI, RDFUtil.CreateUriNode(drop.TargetUri));
                    }
                    g.Assert(root, SP.PropertyGraphIRI, RDFUtil.CreateUriNode(((DropCommand)query).TargetUri));
                    break;
                case SparqlUpdateCommandType.Insert:
                    g.Assert(root, RDF.PropertyType, SP.ClassModify);
                    InsertCommand insert = (InsertCommand)query;
                    if (insert.GraphUri != null)
                    {
                        g.Assert(root, SP.PropertyWith, RDFUtil.CreateUriNode(insert.GraphUri));
                    }
                    g.Assert(root, SP.PropertyInsertPattern, insert.InsertPattern.ToSpinRdf(g, varTable));
                    g.Assert(root, SP.PropertyWhere, insert.WherePattern.ToSpinRdf(g, varTable));
                    break;
                case SparqlUpdateCommandType.InsertData:
                    g.Assert(root, RDF.PropertyType, SP.ClassInsertData);
                    g.Assert(root, SP.PropertyData, ((InsertDataCommand)query).DataPattern.ToSpinRdf(g, varTable));
                    break;
                case SparqlUpdateCommandType.Load:
                    g.Assert(root, RDF.PropertyType, SP.ClassLoad);
                    LoadCommand load = (LoadCommand)query;
                    if (load.SourceUri == null)
                    {
                        g.Assert(root, SP.PropertyGraphIRI, SP.PropertyDefault);
                    }
                    else
                    {
                        g.Assert(root, SP.PropertyGraphIRI, RDFUtil.CreateUriNode(load.SourceUri));
                    }
                    if (load.TargetUri == null)
                    {
                        g.Assert(root, SP.PropertyInto, SP.PropertyDefault);
                    }
                    else
                    {
                        g.Assert(root, SP.PropertyInto, RDFUtil.CreateUriNode(load.TargetUri));
                    }                
                    break;
                case SparqlUpdateCommandType.Modify:
                    g.Assert(root, RDF.PropertyType, SP.ClassModify);
                    ModifyCommand modify = (ModifyCommand)query;
                    if (modify.GraphUri != null)
                    {
                        g.Assert(root, SP.PropertyWith, RDFUtil.CreateUriNode(modify.GraphUri));
                    }
                    if (modify.DeletePattern != null)
                    {
                        g.Assert(root, SP.PropertyDeletePattern, modify.DeletePattern.ToSpinRdf(g, varTable));
                    }
                    if (modify.InsertPattern != null)
                    {
                        g.Assert(root, SP.PropertyInsertPattern, modify.InsertPattern.ToSpinRdf(g, varTable));
                    }
                    g.Assert(root, SP.PropertyWhere, modify.WherePattern.ToSpinRdf(g, varTable));
                    break;
                case SparqlUpdateCommandType.Move:
                    g.Assert(root, RDF.PropertyType, SP.ClassMove);
                    MoveCommand move = (MoveCommand)query;
                    if (move.SourceUri == null)
                    {
                        g.Assert(root, SP.PropertyGraphIRI, SP.PropertyDefault);
                    }
                    else
                    {
                        g.Assert(root, SP.PropertyGraphIRI, RDFUtil.CreateUriNode(move.SourceUri));
                    }
                    if (move.DestinationUri == null)
                    {
                        g.Assert(root, SP.PropertyInto, SP.PropertyDefault);
                    }
                    else
                    {
                        g.Assert(root, SP.PropertyInto, RDFUtil.CreateUriNode(move.DestinationUri));
                    }                
                    break;
                case SparqlUpdateCommandType.Unknown:
                    throw new NotSupportedException("Unkown SPARQL update query encountered " + query.ToString());
            }
            return root;
        }

        internal static INode ToSpinRdf(this GraphPattern pattern, IGraph g, SpinVariableTable varTable)
        {
            INode p = g.CreateBlankNode();
            INode ps = p;

            if (pattern.IsExists)
            {
                g.Assert(p, RDF.PropertyType, SP.ClassExists);
                ps = g.CreateBlankNode();
                g.Assert(p, SP.PropertyElements, ps);
            }
            else if (pattern.IsGraph)
            {
                g.Assert(p, RDF.PropertyType, SP.ClassNamedGraph);
                INode gSpec = pattern.GraphSpecifier.ToSpinRdf(g, varTable);
                //g.Assert(p, SP.named, gSpec); // TODO check which is right
                g.Assert(p, SP.PropertyGraphNameNode, gSpec); // TODO check which is right
                if (gSpec is IBlankNode)
                {
                    g.Assert(gSpec, SP.PropertyVarName, pattern.GraphSpecifier.Value.Substring(1).ToLiteral(g));
                }
                ps = g.CreateBlankNode();
                g.Assert(p, SP.PropertyElements, ps);
            }
            else if (pattern.IsMinus)
            {
                g.Assert(p, RDF.PropertyType, SP.ClassMinus);
                ps = g.CreateBlankNode();
                g.Assert(p, SP.PropertyElements, ps);
            }
            else if (pattern.IsNotExists)
            {
                g.Assert(p, RDF.PropertyType, SP.ClassNotExists);
                ps = g.CreateBlankNode();
                g.Assert(p, SP.PropertyElements, ps);
            }
            else if (pattern.IsOptional)
            {
                g.Assert(p, RDF.PropertyType, SP.ClassOptional);
                ps = g.CreateBlankNode();
                g.Assert(p, SP.PropertyElements, ps);
            }
            else if (pattern.IsService)
            {
                g.Assert(p, RDF.PropertyType, SP.ClassService);
                g.Assert(p, SP.PropertyServiceURI, pattern.GraphSpecifier.ToSpinRdf(g, varTable));
                ps = g.CreateBlankNode();
                g.Assert(p, SP.PropertyElements, ps);
            }
            else if (pattern.IsSubQuery)
            {
                g.Assert(p, RDF.PropertyType, SP.ClassSubQuery);
                ps = g.CreateBlankNode();
                g.Assert(p, SP.PropertyQuery, ps);
            }
            else if (pattern.IsUnion)
            {
                g.Assert(p, RDF.PropertyType, SP.ClassUnion);
                ps = g.CreateBlankNode();
                g.Assert(p, SP.PropertyElements, ps);
            }

            if (!pattern.IsEmpty)
            {
                //First output Triple Patterns
                for (int i = 0; i < pattern.TriplePatterns.Count; i++)
                {
                    INode current = pattern.TriplePatterns[i].ToSpinRdf(g, varTable);
                    if (i == 0)
                    {
                        g.Assert(ps, RDF.PropertyFirst, current);
                    }
                    else
                    {
                        INode temp = g.CreateBlankNode();
                        g.Assert(ps, RDF.PropertyRest, temp);
                        g.Assert(temp, RDF.PropertyFirst, current);
                        ps = temp;
                    }
                }

                if (!pattern.HasChildGraphPatterns)
                {
                    g.Assert(ps, RDF.PropertyRest, RDF.Nil);
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
                        g.Assert(ps, RDF.PropertyFirst, current);
                    }
                    else
                    {
                        INode temp = g.CreateBlankNode();
                        g.Assert(ps, RDF.PropertyRest, temp);
                        g.Assert(temp, RDF.PropertyFirst, current);
                        ps = temp;
                    }
                }
                g.Assert(ps, RDF.PropertyRest, RDF.Nil);
            }

            return p;
        }

        internal static INode ToSpinRdf(this ITriplePattern pattern, IGraph g, SpinVariableTable varTable)
        {
            INode p = g.CreateBlankNode();

            if (pattern is TriplePattern)
            {
                TriplePattern tp = (TriplePattern)pattern;
                g.Assert(p, RDF.PropertyType, SP.ClassTriplePattern);
                g.Assert(p, SP.PropertySubject, tp.Subject.ToSpinRdf(g, varTable));
                g.Assert(p, SP.PropertyPredicate, tp.Predicate.ToSpinRdf(g, varTable));
                g.Assert(p, SP.PropertyObject, tp.Object.ToSpinRdf(g, varTable));
            }
            else if (pattern is SubQueryPattern)
            {
                g.Assert(p, RDF.PropertyType, SP.ClassSubQuery);
                g.Assert(p, SP.PropertyQuery, ((SubQueryPattern)pattern).SubQuery.ToSpinRdf(g));
            }
            else if (pattern is FilterPattern)
            {
                g.Assert(p, RDF.PropertyType, SP.ClassFilter);
                g.Assert(p, SP.PropertyExpression, ((FilterPattern)pattern).Filter.Expression.ToSpinRdf(g, varTable));
            }
            else if (pattern is PropertyPathPattern)
            {
                PropertyPathPattern pp = (PropertyPathPattern)pattern;
                g.Assert(p, RDF.PropertyType, SP.ClassTriplePath);
                g.Assert(p, SP.PropertySubject, pp.Subject.ToSpinRdf(g, varTable));
                g.Assert(p, SP.PropertyPath, pp.Path.ToSpinRdf(g, varTable));
                g.Assert(p, SP.PropertyObject, pp.Object.ToSpinRdf(g, varTable));
            }
            else if (pattern is LetPattern)
            {
                g.Assert(p, RDF.PropertyType, SP.ClassLet);
                INode var = g.CreateBlankNode();
                g.Assert(p, SP.PropertyVariable, var);
                g.Assert(var, SP.PropertyVarName, g.CreateLiteralNode(((LetPattern)pattern).VariableName, XSD.string_.Uri));
                g.Assert(p, SP.PropertyExpression, ((LetPattern)pattern).AssignExpression.ToSpinRdf(g, varTable));
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

            if (item is NodeMatchPattern)
            {
                i = ((NodeMatchPattern)item).Node;
            }
            else if (item is VariablePattern)
            {
                //i = g.CreateUriNode(new Uri(SPIN.NS_URI + "_" + item.VariableName));
                i = varTable[item.VariableName];
                g.Assert(i, SP.PropertyVarName, g.CreateLiteralNode(item.VariableName, XSD.string_.Uri));
            }
            else if (item is BlankNodePattern)
            {
                throw new NotImplementedException();
            }
            else if (item is FixedBlankNodePattern)
            {
                throw new SpinException("Skolem Blank INode syntax extension is not representable in SPIN RDF Syntax");
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

            if (aggregate is AverageAggregate)
            {
                g.Assert(a, RDF.PropertyType, SP.ClassAvg);
            }
            else if (aggregate is CountAggregate)
            {
                g.Assert(a, RDF.PropertyType, SP.ClassCount);
            }
            else if (aggregate is MaxAggregate)
            {
                g.Assert(a, RDF.PropertyType, SP.ClassMax);
            }
            else if (aggregate is MinAggregate)
            {
                g.Assert(a, RDF.PropertyType, SP.ClassMin);
            }
            else if (aggregate is SumAggregate)
            {
                g.Assert(a, RDF.PropertyType, SP.ClassSum);
            }
            else if (aggregate is GroupConcatAggregate)
            {
                throw new SpinException("GROUP_CONCAT aggregates are not yet representable in SPIN RDF Syntax");
            }
            else if (aggregate is SampleAggregate)
            {
                throw new SpinException("SAMPLE aggregates are not yet representable in SPIN RDF Syntax");
            }

            g.Assert(a, SP.PropertyExpression, aggregate.Expression.ToSpinRdf(g, varTable));

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
                    g.Assert(o, RDF.PropertyRest, temp);
                    item = temp;
                    ordering = ordering.Child;
                }
                g.Assert(o, RDF.PropertyFirst, item);
                //TODO: Convert Expression
                //g.Assert(o, SP.expression, ordering.E
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
}
