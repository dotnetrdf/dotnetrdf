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
using VDS.RDF.Query;
using VDS.RDF.Query.Aggregates;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Grouping;
using VDS.RDF.Query.Ordering;
using VDS.RDF.Query.Paths;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Writing.Formatting
{
    /// <summary>
    /// Formatter for formatting Nodes for use in SPARQL and for formatting SPARQL Queries
    /// </summary>
    public class SparqlFormatter 
        : TurtleFormatter, IQueryFormatter, IResultFormatter
    {
        private Uri _tempBaseUri;

        /// <summary>
        /// Creates a new SPARQL Formatter
        /// </summary>
        public SparqlFormatter()
            : base("SPARQL", new QNameOutputMapper()) { }

        /// <summary>
        /// Creates a new SPARQL Formatter using the given Graph
        /// </summary>
        /// <param name="g">Graph</param>
        public SparqlFormatter(IGraph g)
            : base("SPARQL", new QNameOutputMapper(g.NamespaceMap)) { }

        /// <summary>
        /// Creates a new SPARQL Formatter using the given Namespace Map
        /// </summary>
        /// <param name="nsmap">Namespace Map</param>
        public SparqlFormatter(INamespaceMapper nsmap)
            : base("SPARQL", new QNameOutputMapper(nsmap)) { }

        /// <summary>
        /// Determines whether a QName is valid
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns></returns>
        protected override bool IsValidQName(string value)
        {
            return SparqlSpecsHelper.IsValidQName(value, SparqlQuerySyntax.Sparql_1_0);
        }

        /// <summary>
        /// Formats a Variable Node in SPARQL Syntax
        /// </summary>
        /// <param name="v">Variable Node</param>
        /// <param name="segment">Triple Segment</param>
        /// <returns></returns>
        protected override string FormatVariableNode(IVariableNode v, TripleSegment? segment)
        {
            return v.ToString();
        }

        /// <summary>
        /// Formats a Namespace Declaration
        /// </summary>
        /// <param name="prefix">Namespace Prefix</param>
        /// <param name="namespaceUri">Namespace URI</param>
        /// <returns></returns>
        public override string FormatNamespace(string prefix, Uri namespaceUri)
        {
            return "PREFIX " + prefix + ": <" + FormatUri(namespaceUri) + ">";
        }

        /// <summary>
        /// Formats a Base URI Declaration
        /// </summary>
        /// <param name="u">Base URI</param>
        /// <returns></returns>
        public override string FormatBaseUri(Uri u)
        {
            return "BASE <" + FormatUri(u) + ">";
        }

        #region Query Formatting

        /// <summary>
        /// Formats a Query in nicely formatted SPARQL syntax
        /// </summary>
        /// <param name="query">SPARQL Query</param>
        /// <returns></returns>
        public virtual String Format(SparqlQuery query)
        {
            if (query == null) throw new ArgumentNullException("Cannot format a null SPARQL Query as a String");

            try
            {
                _tempBaseUri = query.BaseUri;
                StringBuilder output = new StringBuilder();

                // Base and Prefix Declarations if not a sub-query
                if (!query.IsSubQuery)
                {
                    if (query.BaseUri != null)
                    {
                        output.AppendLine("BASE <" + FormatUri(query.BaseUri.AbsoluteUri) + ">");
                    }
                    foreach (String prefix in _qnameMapper.Prefixes)
                    {
                        output.AppendLine("PREFIX " + prefix + ": <" + FormatUri(_qnameMapper.GetNamespaceUri(prefix).AbsoluteUri) + ">");
                    }

                    // Use a Blank Line to separate Prologue from Query where necessary
                    if (query.BaseUri != null || _qnameMapper.Prefixes.Any())
                    {
                        output.AppendLine();
                    }
                }

                // Next up is the Query Verb
                switch (query.QueryType)
                {
                    case SparqlQueryType.Ask:
                        output.Append("ASK ");
                        break;

                    case SparqlQueryType.Construct:
                        output.AppendLine("CONSTRUCT");
                        // Add in the Construct Pattern
                        output.AppendLine(Format(query.ConstructTemplate));
                        break;

                    case SparqlQueryType.Describe:
                        output.Append("DESCRIBE ");
                        output.AppendLine(FormatDescribeVariablesList(query));
                        break;

                    case SparqlQueryType.DescribeAll:
                        output.Append("DESCRIBE * ");
                        break;

                    case SparqlQueryType.Select:
                        output.Append("SELECT ");
                        output.AppendLine(FormatVariablesList(query.Variables));
                        break;

                    case SparqlQueryType.SelectAll:
                        output.AppendLine("SELECT *");
                        break;

                    case SparqlQueryType.SelectAllDistinct:
                        output.AppendLine("SELECT DISTINCT *");
                        break;

                    case SparqlQueryType.SelectAllReduced:
                        output.AppendLine("SELECT REDUCED *");
                        break;

                    case SparqlQueryType.SelectDistinct:
                        output.Append("SELECT DISTINCT ");
                        output.AppendLine(FormatVariablesList(query.Variables));
                        break;

                    case SparqlQueryType.SelectReduced:
                        output.Append("SELECT REDUCED ");
                        output.AppendLine(FormatVariablesList(query.Variables));
                        break;

                    default:
                        throw new RdfOutputException("Cannot Format an Unknown Query Type");
                }

                // Then add in FROM and FROM NAMED if not a sub-query
                if (!query.IsSubQuery)
                {
                    foreach (Uri u in query.DefaultGraphs)
                    {
                        output.AppendLine("FROM <" + FormatUri(u) + ">");
                    }
                    foreach (Uri u in query.NamedGraphs)
                    {
                        output.AppendLine("FROM NAMED <" + FormatUri(u) + ">");
                    }
                }

                // Then the WHERE clause (unless there isn't one)
                if (query.RootGraphPattern == null)
                {
                    if (query.QueryType != SparqlQueryType.Describe) throw new RdfOutputException("Cannot Format a SPARQL Query as it has no Graph Pattern for the WHERE clause and is not a DESCRIBE query");
                }
                else
                {
                    if (query.RootGraphPattern.IsEmpty)
                    {
                        output.AppendLine("WHERE { }");
                    }
                    else
                    {
                        output.AppendLine("WHERE");
                        if (query.RootGraphPattern.HasModifier)
                        {
                            output.AppendLine("{");
                        }
                        output.AppendLine(Format(query.RootGraphPattern));
                        if (query.RootGraphPattern.HasModifier)
                        {
                            output.AppendLine("}");
                        }
                    }
                }

                // Then a GROUP BY
                if (query.GroupBy != null)
                {
                    output.Append("GROUP BY ");
                    output.AppendLine(FormatGroupBy(query.GroupBy));
                }

                // Then a HAVING
                if (query.Having != null)
                {
                    output.Append("HAVING ");
                    output.Append('(');
                    output.Append(FormatExpression(query.Having.Expression));
                    output.AppendLine(")");
                }

                // Then ORDER BY
                if (query.OrderBy != null)
                {
                    output.Append("ORDER BY ");
                    output.AppendLine(FormatOrderBy(query.OrderBy));
                }

                // Then LIMIT and OFFSET
                if (query.Limit >= 0) output.AppendLine("LIMIT " + query.Limit);
                if (query.Offset > 0) output.AppendLine("OFFSET " + query.Offset);

                // Finally BINDINGS
                if (query.Bindings != null)
                {
                    output.AppendLine(FormatInlineData(query.Bindings));
                }

                return output.ToString();
            }
            finally
            {
                _tempBaseUri = null;
            }
        }

        /// <summary>
        /// Formats a Graph Pattern in nicely formatted SPARQL syntax
        /// </summary>
        /// <param name="gp">Graph Pattern</param>
        /// <returns></returns>
        public virtual String Format(GraphPattern gp)
        {
            if (gp == null) throw new RdfOutputException("Cannot format a null Graph Pattern as a String");

            StringBuilder output = new StringBuilder();

            if (gp.IsUnion)
            {
                for (int i = 0; i < gp.ChildGraphPatterns.Count; i++)
                {
                    GraphPattern cgp = gp.ChildGraphPatterns[i];
                    if (cgp.HasModifier)
                    {
                        String formatted = Format(cgp);
                        formatted = formatted.TrimEnd(new char[] { '\n', '\r' });
                        if (formatted.Contains("\n"))
                        {
                            output.AppendLine("{");
                            output.AppendLineIndented(formatted, 2);
                            output.AppendLine("}");
                        }
                        else
                        {
                            output.AppendLine("{ " + formatted + "}");
                        }
                    }
                    else
                    {
                        output.AppendLine(Format(cgp));
                    }
                    if (i < gp.ChildGraphPatterns.Count - 1)
                    {
                        output.AppendLine("UNION");
                    }
                }
                return output.ToString();
            }
            else if (gp.IsGraph || gp.IsService)
            {
                if (gp.IsGraph)
                {
                    output.Append("GRAPH ");
                }
                else
                {
                    output.Append("SERVICE ");
                    if (gp.IsSilent) output.Append("SILENT ");
                }

                switch (gp.GraphSpecifier.TokenType)
                {
                    case Token.QNAME:
                        try
                        {
                            String uri = Tools.ResolveQName(gp.GraphSpecifier.Value, _qnameMapper, _tempBaseUri);
                            // If the QName resolves OK in the context of the Namespace Map we're using to format this then we
                            // can print the QName as-is
                            output.Append(gp.GraphSpecifier.Value);
                        }
                        catch
                        {
                            // If the QName fails to resolve then can't format in the context
                            throw new RdfOutputException("Cannot format the Graph/Service Specifier QName " + gp.GraphSpecifier.Value + " as the Namespace Mapper in use for this Formatter cannot resolve the QName");
                        }
                        break;

                    case Token.URI:
                        output.Append('<');
                        output.Append(FormatUri(gp.GraphSpecifier.Value));
                        output.Append('>');
                        break;

                    case Token.VARIABLE:
                    default:
                        output.Append(gp.GraphSpecifier.Value);
                        break;
                }
                output.Append(' ');
            }
            else if (gp.IsSubQuery)
            {
                output.AppendLine("{");
                output.AppendLineIndented(Format(((ISubQueryPattern)gp.TriplePatterns[0]).SubQuery), 2);
                output.AppendLine("}");
                return output.ToString();
            }
            else if (gp.IsOptional)
            {
                output.Append("OPTIONAL ");
            }
            else if (gp.IsExists)
            {
                output.Append("EXISTS ");
            }
            else if (gp.IsNotExists)
            {
                output.Append("NOT EXISTS ");
            }
            else if (gp.IsMinus)
            {
                output.Append("MINUS ");
            }

            if (gp.TriplePatterns.Count > 1 || gp.HasChildGraphPatterns || (gp.TriplePatterns.Count <= 1 && gp.Filter != null) || gp.UnplacedAssignments.Count() > 0 || gp.UnplacedFilters.Count() > 0 || gp.HasInlineData)
            {
                output.AppendLine("{");
                foreach (ITriplePattern tp in gp.TriplePatterns)
                {
                    output.AppendLineIndented(Format(tp), 2);
                }
                foreach (IAssignmentPattern ap in gp.UnplacedAssignments)
                {
                    output.AppendLineIndented(Format(ap), 2);
                }
                if (gp.HasInlineData)
                {
                    output.AppendLineIndented(FormatInlineData(gp.InlineData), 2);
                }
                foreach (GraphPattern child in gp.ChildGraphPatterns)
                {
                    output.AppendLineIndented(Format(child), 2);
                }
                foreach (ISparqlFilter fp in gp.UnplacedFilters)
                {
                    output.AppendIndented("FILTER(", 2);
                    output.Append(FormatExpression(fp.Expression));
                    output.AppendLine(")");
                }
                output.Append("}");
            }
            else if (gp.TriplePatterns.Count == 0)
            {
                if (gp.Filter != null)
                {
                    if (gp.HasInlineData)
                    {
                        output.AppendLineIndented("{", 2);
                        output.AppendLineIndented(FormatInlineData(gp.InlineData), 4);
                        output.AppendLineIndented("FILTER (" + FormatExpression(gp.Filter.Expression) + ")", 4);
                        output.AppendLineIndented("}", 2);
                    }
                    else
                    {
                        output.AppendIndented("{ FILTER(", 2);
                        output.Append(FormatExpression(gp.Filter.Expression));
                        output.AppendLine(") }");
                    }
                }
                else if (gp.HasInlineData)
                {
                    output.AppendLineIndented("{", 2);
                    output.AppendLineIndented(FormatInlineData(gp.InlineData), 4);
                    output.AppendLineIndented("}", 2);
                }
                else
                {
                    output.Append("{ }");
                }
            }
            else if (gp.HasInlineData)
            {
                output.AppendLineIndented("{", 2);
                output.AppendLineIndented(Format(gp.TriplePatterns[0]), 4);
                output.AppendLineIndented(FormatInlineData(gp.InlineData), 4);
                output.AppendLineIndented("}", 2);
            }
            else
            {
                output.Append("{ ");
                output.Append(Format(gp.TriplePatterns[0]));
                output.Append(" }");
            }

            return output.ToString();
        }

        /// <summary>
        /// Formats a Triple Pattern in nicely formatted SPARQL syntax
        /// </summary>
        /// <param name="tp">Triple Pattern</param>
        /// <returns></returns>
        public virtual String Format(ITriplePattern tp)
        {
            StringBuilder output = new StringBuilder();
            switch (tp.PatternType)
            {
                case TriplePatternType.Match:
                    IMatchTriplePattern match = (IMatchTriplePattern)tp;
                    output.Append(Format(match.Subject, TripleSegment.Subject));
                    output.Append(' ');
                    output.Append(Format(match.Predicate, TripleSegment.Predicate));
                    output.Append(' ');
                    output.Append(Format(match.Object, TripleSegment.Object));
                    output.Append(" .");
                    break;
                case TriplePatternType.Filter:
                    IFilterPattern filter = (IFilterPattern)tp;
                    output.Append("FILTER(");
                    output.Append(FormatExpression(filter.Filter.Expression));
                    output.Append(")");
                    break;
                case TriplePatternType.SubQuery:
                    ISubQueryPattern subquery = (ISubQueryPattern)tp;
                    output.AppendLine("{");
                    output.AppendLineIndented(Format(subquery.SubQuery), 2);
                    output.AppendLine("}");
                    break;
                case TriplePatternType.Path:
                    IPropertyPathPattern path = (IPropertyPathPattern)tp;
                    output.Append(Format(path.Subject, TripleSegment.Subject));
                    output.Append(' ');
                    output.Append(FormatPath(path.Path));
                    output.Append(' ');
                    output.Append(Format(path.Object, TripleSegment.Object));
                    output.Append(" .");
                    break;
                case TriplePatternType.LetAssignment:
                    IAssignmentPattern let = (IAssignmentPattern)tp;
                    output.Append("LET(?");
                    output.Append(let.VariableName);
                    output.Append(" := ");
                    output.Append(FormatExpression(let.AssignExpression));
                    output.Append(")");
                    break;
                case TriplePatternType.BindAssignment:
                    IAssignmentPattern bind = (IAssignmentPattern)tp;
                    output.Append("BIND (");
                    output.Append(FormatExpression(bind.AssignExpression));
                    output.Append(" AS ?");
                    output.Append(bind.VariableName);
                    output.Append(")");
                    break;
                case TriplePatternType.PropertyFunction:
                    IPropertyFunctionPattern propFunc = (IPropertyFunctionPattern)tp;
                    if (propFunc.SubjectArgs.Count() > 1)
                    {
                        output.Append("( ");
                        foreach (PatternItem arg in propFunc.SubjectArgs)
                        {
                            output.Append(Format(arg, TripleSegment.Subject));
                            output.Append(' ');
                        }
                        output.Append(')');
                    }
                    else
                    {
                        output.Append(Format(propFunc.SubjectArgs.First(), TripleSegment.Subject));
                    }
                    output.Append(" <");
                    output.Append(FormatUri(propFunc.PropertyFunction.FunctionUri));
                    output.Append("> ");
                    if (propFunc.ObjectArgs.Count() > 1)
                    {
                        output.Append("( ");
                        foreach (PatternItem arg in propFunc.ObjectArgs)
                        {
                            output.Append(Format(arg, TripleSegment.Object));
                            output.Append(' ');
                        }
                        output.Append(')');
                    }
                    else
                    {
                        output.Append(Format(propFunc.ObjectArgs.First(), TripleSegment.Object));
                    }
                    output.Append(" .");
                    break;
                default:
                    throw new RdfOutputException("Unable to Format an unknown ITriplePattern implementation as a String");
            }

            return output.ToString();
        }

        /// <summary>
        /// Formats a Pattern Item in nicely formatted SPARQL syntax
        /// </summary>
        /// <param name="item">Pattern Item</param>
        /// <param name="segment">Triple Pattern Segment</param>
        /// <returns></returns>
        public virtual String Format(PatternItem item, TripleSegment? segment)
        {
            if (item is VariablePattern)
            {
                return item.ToString();
            }
            else if (item is NodeMatchPattern)
            {
                NodeMatchPattern match = (NodeMatchPattern)item;
                return Format(match.Node, segment);
            }
            else if (item is FixedBlankNodePattern)
            {
                if (segment != null)
                {
                    if (segment == TripleSegment.Predicate) throw new RdfOutputException("Cannot format a Fixed Blank Node Pattern Item as the Predicate of a Triple Pattern as Blank Nodes are not permitted as Predicates");
                }

                return item.ToString();
            }
            else if (item is BlankNodePattern)
            {
                return item.ToString();
            }
            else
            {
                throw new RdfOutputException("Unable to Format an unknown PatternItem implementation as a String");
            }
        }

        #region Protected Helper functions which can be overridden to change specific parts of the formatting

        /// <summary>
        /// Formats the Variable List for a SPARQL Query
        /// </summary>
        /// <param name="vars">Variables</param>
        /// <returns></returns>
        protected virtual String FormatVariablesList(IEnumerable<SparqlVariable> vars)
        {
            StringBuilder output = new StringBuilder();

            List<SparqlVariable> varList = vars.Where(v => v.IsResultVariable).ToList();

            int onLine = 0;
            for (int i = 0; i < varList.Count; i++)
            {
                SparqlVariable v = varList[i];
                if (v.IsAggregate)
                {
                    onLine += 2;
                    output.Append('(');
                    output.Append(FormatAggregate(v.Aggregate));
                    output.Append(" AS ?");
                    output.Append(v.Name);
                    output.Append(')');
                }
                else if (v.IsProjection)
                {
                    onLine += 3;
                    output.Append('(');
                    output.Append(FormatExpression(v.Projection));
                    output.Append(" AS ?");
                    output.Append(v.Name);
                    output.Append(')');
                }
                else
                {
                    onLine += 1;
                    output.Append(v.ToString());
                }

                // Maximum of 6 things per line (aggregates worth 2 and expression worth 3)
                if (onLine >= 6 && i < varList.Count - 1)
                {
                    output.AppendLine();
                }
                else if (i < varList.Count - 1)
                {
                    output.Append(' ');
                }
            }

            return output.ToString();
        }

        /// <summary>
        /// Formats the Variable/QName/URI for a SPARQL DESCRIBE Query
        /// </summary>
        /// <param name="q">SPARQL Query</param>
        /// <returns></returns>
        protected virtual String FormatDescribeVariablesList(SparqlQuery q)
        {
            StringBuilder output = new StringBuilder();

            List<IToken> tokenList = q.DescribeVariables.ToList();

            int onLine = 0;
            for (int i = 0; i < tokenList.Count; i++)
            {
                IToken t = tokenList[i];

                switch (t.TokenType)
                {
                    case Token.VARIABLE:
                        output.Append(t.Value);
                        onLine++;
                        break;
                    case Token.URI:
                        output.Append('<');
                        output.Append(FormatUri(t.Value));
                        output.Append('>');
                        onLine += 3;
                        break;
                    case Token.QNAME:
                        // If the QName has the same Namespace URI in this Formatter as in the Query then format
                        // as a QName otherwise expand to a full URI
                        String prefix = t.Value.Substring(0, t.Value.IndexOf(':'));
                        if (_qnameMapper.HasNamespace(prefix) && q.NamespaceMap.GetNamespaceUri(prefix).AbsoluteUri.Equals(_qnameMapper.GetNamespaceUri(prefix).AbsoluteUri))
                        {
                            output.AppendLine(t.Value);
                            onLine += 2;
                        }
                        else if (q.NamespaceMap.HasNamespace(prefix))
                        {
                            output.Append('<');
                            output.Append(FormatUri(Tools.ResolveQName(t.Value, q.NamespaceMap, q.BaseUri)));
                            output.Append('>');
                            onLine += 3;
                        }
                        else
                        {
                            throw new RdfOutputException("Unable to Format the DESCRIBE variables list since one of the Variables is the QName '" + t.Value + "' which cannot be resolved using the Namespace Map of the Query");
                        }

                        break;
                }

                // Maximum of 6 things per line (URIs worth 3 and QNames worth 2)
                if (onLine >= 6 && i < tokenList.Count - 1)
                {
                    output.AppendLine();
                }
                else if (i < tokenList.Count - 1)
                {
                    output.Append(' ');
                }
            }

            return output.ToString();
        }

        /// <summary>
        /// Formats a SPARQL Expression
        /// </summary>
        /// <param name="expr">SPARQL Expression</param>
        /// <returns></returns>
        protected virtual String FormatExpression(ISparqlExpression expr)
        {
            StringBuilder output = new StringBuilder();

            try
            {
                switch (expr.Type)
                {
                    case SparqlExpressionType.Aggregate:
                        if (expr is AggregateTerm)
                        {
                            AggregateTerm agg = (AggregateTerm)expr;
                            output.Append(FormatAggregate(agg.Aggregate));
                        }
                        else
                        {
                            output.Append(expr.ToString());
                        }
                        break;

                    case SparqlExpressionType.BinaryOperator:
                        ISparqlExpression lhs = expr.Arguments.First();
                        ISparqlExpression rhs = expr.Arguments.Skip(1).First();

                        // Format the Expression wrapping the LHS and/or RHS in brackets if required
                        // to ensure that ordering of operators is preserved
                        if (lhs.Type == SparqlExpressionType.BinaryOperator)
                        {
                            output.Append('(');
                            output.Append(FormatExpression(lhs));
                            output.Append(')');
                        }
                        else
                        {
                            output.Append(FormatExpression(lhs));
                        }
                        output.Append(' ');
                        output.Append(expr.Functor);
                        output.Append(' ');
                        if (rhs.Type == SparqlExpressionType.BinaryOperator)
                        {
                            output.Append('(');
                            output.Append(FormatExpression(rhs));
                            output.Append(')');
                        }
                        else
                        {
                            output.Append(FormatExpression(rhs));
                        }
                        break;

                    case SparqlExpressionType.Function:
                        // Show either a Keyword/URI/QName as appropriate
                        if (SparqlSpecsHelper.IsFunctionKeyword(expr.Functor))
                        {
                            output.Append(expr.Functor);
                        }
                        else
                        {
                            String funcQname;
                            if (_qnameMapper.ReduceToQName(expr.Functor, out funcQname))
                            {
                                output.Append(funcQname);
                            }
                            else
                            {
                                output.Append('<');
                                output.Append(FormatUri(expr.Functor));
                                output.Append('>');
                            }
                        }

                        // Add Arguments list
                        output.Append('(');
                        List<ISparqlExpression> args = expr.Arguments.ToList();
                        for (int i = 0; i < args.Count; i++)
                        {
                            output.Append(FormatExpression(args[i]));
                            if (i < args.Count - 1)
                            {
                                output.Append(", ");
                            }
                        }
                        output.Append(')');
                        break;

                    case SparqlExpressionType.GraphOperator:
                        output.Append(expr.Functor);
                        output.Append(' ');

                        List<ISparqlExpression> gArgs = expr.Arguments.ToList();
                        if (gArgs.Count > 1) throw new RdfOutputException("Error Formatting SPARQL Expression - Expressions of type GraphOperator are only allowed a single argument");
                        for (int i = 0; i < gArgs.Count; i++)
                        {
                            output.Append(FormatExpression(gArgs[i]));
                            if (i < gArgs.Count - 1)
                            {
                                output.Append(", ");
                            }
                        }
                        break;

                    case SparqlExpressionType.Primary:
                        // If Node/Numeric Term then use Node Formatting otherwise use ToString() on the expression
                        if (expr is ConstantTerm)
                        {
                            ConstantTerm nodeTerm = (ConstantTerm)expr;
                            output.Append(Format(nodeTerm.Evaluate(null, 0)));
                        }
                        else if (expr is GraphPatternTerm)
                        {
                            GraphPatternTerm gp = (GraphPatternTerm)expr;
                            output.Append(Format(gp.Pattern));
                        }
                        else
                        {
                            output.Append(expr.ToString());
                        }
                        break;

                    case SparqlExpressionType.SetOperator:
                        // Add First Argument and Set Operator
                        output.Append(FormatExpression(expr.Arguments.First()));
                        output.Append(' ');
                        output.Append(expr.Functor);

                        // Add Set
                        output.Append(" (");
                        List<ISparqlExpression> set = expr.Arguments.Skip(1).ToList();
                        for (int i = 0; i < set.Count; i++)
                        {
                            output.Append(FormatExpression(set[i]));
                            if (i < set.Count - 1)
                            {
                                output.Append(", ");
                            }
                        }
                        output.Append(')');
                        break;

                    case SparqlExpressionType.UnaryOperator:
                        // Just Functor then Expression
                        output.Append(expr.Functor);
                        output.Append(FormatExpression(expr.Arguments.First()));
                        break;
                }
            }
            catch (RdfOutputException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new RdfOutputException("Error formatting a SPARQL Expression - the Expression may have the wrong number of arguments for the reported expression type", ex);
            }

            return output.ToString();
        }

        /// <summary>
        /// Formats a SPARQL Aggregate
        /// </summary>
        /// <param name="agg">SPARQL Aggregate</param>
        /// <returns></returns>
        protected virtual String FormatAggregate(ISparqlAggregate agg)
        {
            StringBuilder output = new StringBuilder();
            if (SparqlSpecsHelper.IsAggregateFunctionKeyword(agg.Functor))
            {
                output.Append(agg.Functor);
            }
            else
            {
                String aggQName;
                if (_qnameMapper.ReduceToQName(agg.Functor, out aggQName))
                {
                    output.Append(aggQName);
                }
                else
                {
                    output.Append('<');
                    output.Append(FormatUri(agg.Functor));
                    output.Append('>');
                }
            }

            output.Append('(');
            List<ISparqlExpression> args = agg.Arguments.ToList();
            for (int i = 0; i < args.Count; i++)
            {
                output.Append(FormatExpression(args[i]));
                if (i < args.Count - 1 && !(args[i] is DistinctModifier))
                {
                    output.Append(", ");
                }
            }
            output.Append(')');

            return output.ToString();
        }

        /// <summary>
        /// Formats a SPARQL Property Path
        /// </summary>
        /// <param name="path">SPARQL Property Path</param>
        /// <returns></returns>
        protected virtual String FormatPath(ISparqlPath path)
        {
            StringBuilder output = new StringBuilder();

            if (path is AlternativePath)
            {
                AlternativePath alt = (AlternativePath)path;
                output.Append('(');
                output.Append(FormatPath(alt.LhsPath));
                output.Append(" | ");
                output.Append(FormatPath(alt.RhsPath));
                output.Append(')');
            }
            else if (path is FixedCardinality)
            {
                FixedCardinality card = (FixedCardinality)path;
                if (card.Path is BaseBinaryPath) output.Append('(');
                output.Append(FormatPath(card.Path));
                if (card.Path is BaseBinaryPath) output.Append(')');
                output.Append('{');
                output.Append(card.MaxCardinality);
                output.Append('}');
            }
            else if (path is InversePath)
            {
                InversePath inv = (InversePath)path;
                output.Append('^');
                if (inv.Path is BaseBinaryPath) output.Append('(');
                output.Append(FormatPath(inv.Path));
                if (inv.Path is BaseBinaryPath) output.Append(')');
            }
            else if (path is NOrMore)
            {
                NOrMore nOrMore = (NOrMore)path;
                if (nOrMore.Path is BaseBinaryPath) output.Append('(');
                output.Append(FormatPath(nOrMore.Path));
                if (nOrMore.Path is BaseBinaryPath) output.Append(')');
                output.Append('{');
                output.Append(nOrMore.MinCardinality);
                output.Append(",}");
            }
            else if (path is NToM)
            {
                NToM nToM = (NToM)path;
                if (nToM.Path is BaseBinaryPath) output.Append('(');
                output.Append(FormatPath(nToM.Path));
                if (nToM.Path is BaseBinaryPath) output.Append(')');
                output.Append('{');
                output.Append(nToM.MinCardinality);
                output.Append(',');
                output.Append(nToM.MaxCardinality);
                output.Append('}');
            }
            else if (path is OneOrMore)
            {
                OneOrMore oneOrMore = (OneOrMore)path;
                if (oneOrMore.Path is BaseBinaryPath) output.Append('(');
                output.Append(FormatPath(oneOrMore.Path));
                if (oneOrMore.Path is BaseBinaryPath) output.Append(')');
                output.Append('+');
            }
            else if (path is Property)
            {
                Property prop = (Property)path;
                output.Append(Format(prop.Predicate, TripleSegment.Predicate));
            }
            else if (path is SequencePath)
            {
                SequencePath seq = (SequencePath)path;
                output.Append(FormatPath(seq.LhsPath));
                output.Append(" / ");
                output.Append(FormatPath(seq.RhsPath));
            }
            else if (path is ZeroOrMore)
            {
                ZeroOrMore zeroOrMore = (ZeroOrMore)path;
                if (zeroOrMore.Path is BaseBinaryPath) output.Append('(');
                output.Append(FormatPath(zeroOrMore.Path));
                if (zeroOrMore.Path is BaseBinaryPath) output.Append(')');
                output.Append('*');
            }
            else if (path is ZeroOrOne)
            {
                ZeroOrOne zeroOrOne = (ZeroOrOne)path;
                if (zeroOrOne.Path is BaseBinaryPath) output.Append('(');
                output.Append(FormatPath(zeroOrOne.Path));
                if (zeroOrOne.Path is BaseBinaryPath) output.Append(')');
                output.Append('?');
            }
            else if (path is ZeroToN)
            {
                ZeroToN zeroToN = (ZeroToN)path;
                if (zeroToN.Path is BaseBinaryPath) output.Append('(');
                output.Append(FormatPath(zeroToN.Path));
                if (zeroToN.Path is BaseBinaryPath) output.Append(')');
                output.Append("{,");
                output.Append(zeroToN.MaxCardinality);
                output.Append('}');
            }
            else if (path is NegatedSet)
            {
                NegatedSet negSet = (NegatedSet)path;
                output.Append('!');
                if (negSet.Properties.Count() + negSet.InverseProperties.Count() > 1) output.Append('(');
                foreach (Property p in negSet.Properties)
                {
                    output.Append(FormatPath(p));
                    output.Append(" | ");
                }
                foreach (Property p in negSet.InverseProperties)
                {
                    output.Append(FormatPath(p));
                    output.Append(" | ");
                }
                output.Remove(output.Length - 3, 3);
                if (negSet.Properties.Count() + negSet.InverseProperties.Count() > 1) output.Append(')');
            }
            else
            {
                throw new RdfOutputException("Unable to Format an unknown ISparqlPath implementations as a String");
            }

            return output.ToString();
        }

        /// <summary>
        /// Formats a SPARQL GROUP BY Clause
        /// </summary>
        /// <param name="groupBy">GROUP BY Clause</param>
        /// <returns></returns>
        protected virtual String FormatGroupBy(ISparqlGroupBy groupBy)
        {
            StringBuilder output = new StringBuilder();

            bool isAssignment = groupBy.AssignVariable != null && (groupBy.Expression.Variables.Count() != 1 || !groupBy.AssignVariable.Equals(groupBy.Expression.Variables.FirstOrDefault()));

            if (isAssignment)
            {
                output.Append('(');
            }
            output.Append(FormatExpression(groupBy.Expression));
            if (isAssignment)
            {
                output.Append(" AS ?");
                output.Append(groupBy.AssignVariable);
                output.Append(')');
            }

            if (groupBy.Child != null)
            {
                output.Append(' ');
                output.Append(FormatGroupBy(groupBy.Child));
            }

            return output.ToString();
        }

        /// <summary>
        /// Formats a SPARQL ORDER BY Clause
        /// </summary>
        /// <param name="orderBy">ORDER BY Clause</param>
        /// <returns></returns>
        protected virtual String FormatOrderBy(ISparqlOrderBy orderBy)
        {
            StringBuilder output = new StringBuilder();

            if (orderBy.Descending)
            {
                output.Append("DESC(");
                output.Append(FormatExpression(orderBy.Expression));
                output.Append(')');
            }
            else 
            {
                output.Append("ASC(");
                output.Append(FormatExpression(orderBy.Expression));
                output.Append(')');
            }

            if (orderBy.Child != null)
            {
                output.Append(' ');
                output.Append(FormatOrderBy(orderBy.Child));
            }

            return output.ToString();
        }

        /// <summary>
        /// Formats the Inline Data portion of a Query
        /// </summary>
        /// <param name="data">Inline Data</param>
        /// <returns></returns>
        protected virtual String FormatInlineData(BindingsPattern data)
        {
            StringBuilder output = new StringBuilder();
            output.Append("VALUES ( ");
            foreach (String var in data.Variables)
            {
                output.Append("?" + var);
                output.Append(' ');
            }
            output.Append(')');
            if (data.Variables.Any()) output.AppendLine();
            output.Append('{');
            bool multipleTuples = data.Tuples.Count() > 1;
            if (multipleTuples) output.AppendLine();
            foreach (BindingTuple tuple in data.Tuples)
            {
                if (tuple.IsEmpty)
                {
                    if (multipleTuples)
                    {
                        output.AppendLineIndented("()", 2);
                    }
                    else
                    {
                        output.Append(" () ");
                    }
                    continue;
                }

                if (multipleTuples)
                {
                    output.AppendIndented("(", 2);
                }
                else
                {
                    output.Append("(");
                }
                foreach (String var in data.Variables)
                {
                    output.Append(' ');
                    if (tuple[var] == null)
                    {
                        output.AppendLine(SparqlSpecsHelper.SparqlKeywordUndef);
                    }
                    else
                    {
                        output.Append(Format(tuple[var], null));
                    }
                }
                if (multipleTuples)
                {
                    output.AppendLine(")");
                }
                else
                {
                    output.Append(')');
                }
            }
            output.AppendLine("}");
            return output.ToString();
        }

        #endregion

        #endregion

        #region Result Formatting

        /// <summary>
        /// Formats a SPARQL Result using this Formatter to format the Node values for each Variable
        /// </summary>
        /// <param name="result">SPARQL Result</param>
        /// <returns></returns>
        public override String Format(SparqlResult result)
        {
            return result.ToString(this);
        }

        /// <summary>
        /// Formats a Boolean Result
        /// </summary>
        /// <param name="result">Boolean Result</param>
        /// <returns></returns>
        public override String FormatBooleanResult(bool result)
        {
            return result.ToString().ToLower();
        }

        #endregion
    }
}
