/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query;
using VDS.RDF.Query.Aggregates;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions;
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
    public class SparqlFormatter : TurtleFormatter, IQueryFormatter, IResultFormatter
    {
        private Uri _tempBaseUri;

        /// <summary>
        /// Creates a new SPARQL Formatter
        /// </summary>
        public SparqlFormatter() 
            : base("SPARQL", new QNameOutputMapper())
        {
            this._validEscapes = new char[] { '"', 'n', 't', 'u', 'U', 'r', '\\', '0', 'f', 'b', '\'' };
        }

        /// <summary>
        /// Creates a new SPARQL Formatter using the given Graph
        /// </summary>
        /// <param name="g">Graph</param>
        public SparqlFormatter(IGraph g)
            : base("SPARQL", new QNameOutputMapper(g.NamespaceMap))
        {
            this._validEscapes = new char[] { '"', 'n', 't', 'u', 'U', 'r', '\\', '0', 'f', 'b', '\'' };
        }

        /// <summary>
        /// Creates a new SPARQL Formatter using the given Namespace Map
        /// </summary>
        /// <param name="nsmap">Namespace Map</param>
        public SparqlFormatter(INamespaceMapper nsmap)
            : base("SPARQL", new QNameOutputMapper(nsmap)) 
        {
            this._validEscapes = new char[] { '"', 'n', 't', 'u', 'U', 'r', '\\', '0', 'f', 'b', '\'' };
        }

        /// <summary>
        /// Determines whether a QName is valid
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns></returns>
        protected override bool IsValidQName(string value)
        {
            return SparqlSpecsHelper.IsValidQName(value);
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
            return "PREFIX " + prefix + ": <" + this.FormatUri(namespaceUri) + ">";
        }

        /// <summary>
        /// Formats a Base URI Declaration
        /// </summary>
        /// <param name="u">Base URI</param>
        /// <returns></returns>
        public override string FormatBaseUri(Uri u)
        {
            return "BASE <" + this.FormatUri(u) + ">";
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
                this._tempBaseUri = query.BaseUri;
                StringBuilder output = new StringBuilder();

                //Base and Prefix Declarations if not a sub-query
                if (!query.IsSubQuery)
                {
                    if (query.BaseUri != null)
                    {
                        output.AppendLine("BASE <" + this.FormatUri(query.BaseUri.ToString()) + ">");
                    }
                    foreach (String prefix in this._qnameMapper.Prefixes)
                    {
                        output.AppendLine("PREFIX " + prefix + ": <" + this.FormatUri(this._qnameMapper.GetNamespaceUri(prefix).ToString()) + ">");
                    }

                    //Use a Blank Line to separate Prologue from Query where necessary
                    if (query.BaseUri != null || this._qnameMapper.Prefixes.Any())
                    {
                        output.AppendLine();
                    }
                }

                //Next up is the Query Verb
                switch (query.QueryType)
                {
                    case SparqlQueryType.Ask:
                        output.Append("ASK ");
                        break;

                    case SparqlQueryType.Construct:
                        output.AppendLine("CONSTRUCT");
                        //Add in the Construct Pattern
                        output.AppendLine(this.Format(query.ConstructTemplate));
                        break;

                    case SparqlQueryType.Describe:
                        output.Append("DESCRIBE ");
                        output.AppendLine(this.FormatDescribeVariablesList(query));
                        break;

                    case SparqlQueryType.DescribeAll:
                        output.Append("DESCRIBE * ");
                        break;

                    case SparqlQueryType.Select:
                        output.Append("SELECT ");
                        output.AppendLine(this.FormatVariablesList(query.Variables));
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
                        output.AppendLine(this.FormatVariablesList(query.Variables));
                        break;

                    case SparqlQueryType.SelectReduced:
                        output.Append("SELECT REDUCED ");
                        output.AppendLine(this.FormatVariablesList(query.Variables));
                        break;

                    default:
                        throw new RdfOutputException("Cannot Format an Unknown Query Type");
                }

                //Then add in FROM and FROM NAMED if not a sub-query
                if (!query.IsSubQuery)
                {
                    foreach (Uri u in query.DefaultGraphs)
                    {
                        output.AppendLine("FROM <" + this.FormatUri(u) + ">");
                    }
                    foreach (Uri u in query.NamedGraphs)
                    {
                        output.AppendLine("FROM NAMED <" + this.FormatUri(u) + ">");
                    }
                }

                //Then the WHERE clause (unless there isn't one)
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
                        output.AppendLine(this.Format(query.RootGraphPattern));
                    }
                }

                //Then a GROUP BY
                if (query.GroupBy != null)
                {
                    output.Append("GROUP BY ");
                    output.AppendLine(this.FormatGroupBy(query.GroupBy));
                }

                //Then a HAVING
                if (query.Having != null)
                {
                    output.Append("HAVING ");
                    output.Append('(');
                    output.Append(this.FormatExpression(query.Having.Expression));
                    output.AppendLine(")");
                }

                //Then ORDER BY
                if (query.OrderBy != null)
                {
                    output.Append("ORDER BY ");
                    output.AppendLine(this.FormatOrderBy(query.OrderBy));
                }

                //Then LIMIT and OFFSET
                if (query.Limit >= 0) output.AppendLine("LIMIT " + query.Limit);
                if (query.Offset > 0) output.AppendLine("OFFSET " + query.Offset);

                //Finally BINDINGS
                if (query.Bindings != null)
                {
                    output.Append("BINDINGS ");
                    foreach (String var in query.Bindings.Variables)
                    {
                        output.Append("?" + var);
                        output.Append(' ');
                    }
                    if (query.Bindings.Variables.Any()) output.AppendLine();
                    output.Append('{');
                    bool multipleTuples = query.Bindings.Tuples.Count() > 1;
                    if (multipleTuples) output.AppendLine();
                    foreach (BindingTuple tuple in query.Bindings.Tuples)
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
                        foreach (String var in query.Bindings.Variables)
                        {
                            output.Append(' ');
                            if (tuple[var] == null)
                            {
                                output.AppendLine(SparqlSpecsHelper.SparqlKeywordUndef);
                            }
                            else
                            {
                                output.Append(this.Format(tuple[var], null));
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
                }

                return output.ToString();
            }
            finally
            {
                this._tempBaseUri = null;
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
                    output.Append(this.Format(gp.ChildGraphPatterns[i]));
                    if (i < gp.ChildGraphPatterns.Count - 1)
                    {
                        output.AppendLine();
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
                            String uri = Tools.ResolveQName(gp.GraphSpecifier.Value, this._qnameMapper, this._tempBaseUri);
                            //If the QName resolves OK in the context of the Namespace Map we're using to format this then we
                            //can print the QName as-is
                            output.Append(gp.GraphSpecifier.Value);
                        }
                        catch
                        {
                            //If the QName fails to resolve then can't format in the context
                            throw new RdfOutputException("Cannot format the Graph/Service Specifier QName " + gp.GraphSpecifier.Value + " as the Namespace Mapper in use for this Formatter cannot resolve the QName");
                        }
                        break;

                    case Token.URI:
                        output.Append('<');
                        output.Append(this.FormatUri(gp.GraphSpecifier.Value));
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
                output.AppendLineIndented(this.Format(((SubQueryPattern)gp.TriplePatterns[0]).SubQuery), 2);
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

            if (gp.TriplePatterns.Count > 1 || gp.HasChildGraphPatterns || (gp.TriplePatterns.Count <= 1 && gp.Filter != null) || gp.UnplacedAssignments.Count() > 0 || gp.UnplacedFilters.Count() > 0)
            {
                output.AppendLine("{");
                foreach (ITriplePattern tp in gp.TriplePatterns)
                {
                    output.AppendLineIndented(this.Format(tp), 2);
                }
                foreach (IAssignmentPattern ap in gp.UnplacedAssignments)
                {
                    output.AppendLineIndented(this.Format(ap), 2);
                }
                foreach (GraphPattern child in gp.ChildGraphPatterns)
                {
                    output.AppendLineIndented(this.Format(child), 2);
                }
                foreach (ISparqlFilter fp in gp.UnplacedFilters)
                {
                    output.AppendIndented("FILTER(", 2);
                    output.Append(this.FormatExpression(fp.Expression));
                    output.AppendLine(")");
                }
                output.Append("}");
            }
            else if (gp.TriplePatterns.Count == 0)
            {
                if (gp.Filter != null)
                {
                    output.AppendIndented("{ FILTER(", 2);
                    output.Append(this.FormatExpression(gp.Filter.Expression));
                    output.AppendLine(") }");
                }
                else
                {
                    output.Append("{ }");
                }
            }
            else
            {
                output.Append("{ ");
                output.Append(this.Format(gp.TriplePatterns[0]));
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
            if (tp is TriplePattern)
            {
                TriplePattern match = (TriplePattern)tp;
                output.Append(this.Format(match.Subject, TripleSegment.Subject));
                output.Append(' ');
                output.Append(this.Format(match.Predicate, TripleSegment.Predicate));
                output.Append(' ');
                output.Append(this.Format(match.Object, TripleSegment.Object));
                output.Append(" .");
            }
            else if (tp is FilterPattern)
            {
                FilterPattern filter = (FilterPattern)tp;
                output.Append("FILTER(");
                output.Append(this.FormatExpression(filter.Filter.Expression));
                output.Append(")");
            }
            else if (tp is SubQueryPattern)
            {
                SubQueryPattern subquery = (SubQueryPattern)tp;
                output.AppendLine("{");
                output.AppendLineIndented(this.Format(subquery.SubQuery), 2);
                output.AppendLine("}");
            }
            else if (tp is PropertyPathPattern)
            {
                PropertyPathPattern path = (PropertyPathPattern)tp;
                output.Append(this.Format(path.Subject, TripleSegment.Subject));
                output.Append(' ');
                output.Append(this.FormatPath(path.Path));
                output.Append(' ');
                output.Append(this.Format(path.Object, TripleSegment.Object));
                output.Append(" .");
            }
            else if (tp is LetPattern)
            {
                LetPattern let = (LetPattern)tp;
                output.Append("LET(?");
                output.Append(let.VariableName);
                output.Append(" := ");
                output.Append(this.FormatExpression(let.AssignExpression));
                output.Append(")");
            }
            else if (tp is BindPattern)
            {
                BindPattern bind = (BindPattern)tp;
                output.Append("BIND (");
                output.Append(this.FormatExpression(bind.AssignExpression));
                output.Append(" AS ?");
                output.Append(bind.VariableName);
                output.Append(")");
            }
            else
            {
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
                return this.Format(match.Node, segment);
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
                    output.Append(this.FormatAggregate(v.Aggregate));
                    output.Append(" AS ?");
                    output.Append(v.Name);
                    output.Append(')');
                }
                else if (v.IsProjection)
                {
                    onLine += 3;
                    output.Append('(');
                    output.Append(this.FormatExpression(v.Projection));
                    output.Append(" AS ?");
                    output.Append(v.Name);
                    output.Append(')');
                }
                else
                {
                    onLine += 1;
                    output.Append(v.ToString());
                }

                //Maximum of 6 things per line (aggregates worth 2 and expression worth 3)
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
                        output.Append(this.FormatUri(t.Value));
                        output.Append('>');
                        onLine += 3;
                        break;
                    case Token.QNAME:
                        //If the QName has the same Namespace URI in this Formatter as in the Query then format
                        //as a QName otherwise expand to a full URI
                        String prefix = t.Value.Substring(0, t.Value.IndexOf(':'));
                        if (this._qnameMapper.HasNamespace(prefix) && q.NamespaceMap.GetNamespaceUri(prefix).ToString().Equals(this._qnameMapper.GetNamespaceUri(prefix).ToString()))
                        {
                            output.AppendLine(t.Value);
                            onLine += 2;
                        }
                        else if (q.NamespaceMap.HasNamespace(prefix))
                        {
                            output.Append('<');
                            output.Append(this.FormatUri(Tools.ResolveQName(t.Value, q.NamespaceMap, q.BaseUri)));
                            output.Append('>');
                            onLine += 3;
                        }
                        else
                        {
                            throw new RdfOutputException("Unable to Format the DESCRIBE variables list since one of the Variables is the QName '" + t.Value + "' which cannot be resolved using the Namespace Map of the Query");
                        }

                        break;
                }

                //Maximum of 6 things per line (URIs worth 3 and QNames worth 2)
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
                        if (expr is AggregateExpressionTerm)
                        {
                            AggregateExpressionTerm agg = (AggregateExpressionTerm)expr;
                            output.Append(this.FormatAggregate(agg.Aggregate));
                        }
                        else if (expr is NonNumericAggregateExpressionTerm)
                        {
                            NonNumericAggregateExpressionTerm nonNumAgg = (NonNumericAggregateExpressionTerm)expr;
                            output.Append(this.FormatAggregate(nonNumAgg.Aggregate));
                        }
                        else
                        {
                            output.Append(expr.ToString());
                        }
                        break;

                    case SparqlExpressionType.BinaryOperator:
                        ISparqlExpression lhs = expr.Arguments.First();
                        ISparqlExpression rhs = expr.Arguments.Skip(1).First();

                        //Format the Expression wrapping the LHS and/or RHS in brackets if required
                        //to ensure that ordering of operators is preserved
                        if (lhs.Type == SparqlExpressionType.BinaryOperator)
                        {
                            output.Append('(');
                            output.Append(this.FormatExpression(lhs));
                            output.Append(')');
                        }
                        else
                        {
                            output.Append(this.FormatExpression(lhs));
                        }
                        output.Append(' ');
                        output.Append(expr.Functor);
                        output.Append(' ');
                        if (rhs.Type == SparqlExpressionType.BinaryOperator)
                        {
                            output.Append('(');
                            output.Append(this.FormatExpression(rhs));
                            output.Append(')');
                        }
                        else
                        {
                            output.Append(this.FormatExpression(rhs));
                        }
                        break;

                    case SparqlExpressionType.Function:
                        //Show either a Keyword/URI/QName as appropriate
                        if (SparqlSpecsHelper.IsFunctionKeyword(expr.Functor))
                        {
                            output.Append(expr.Functor);
                        }
                        else
                        {
                            String funcQname;
                            if (this._qnameMapper.ReduceToQName(expr.Functor, out funcQname))
                            {
                                output.Append(funcQname);
                            }
                            else
                            {
                                output.Append('<');
                                output.Append(this.FormatUri(expr.Functor));
                                output.Append('>');
                            }
                        }

                        //Add Arguments list
                        output.Append('(');
                        List<ISparqlExpression> args = expr.Arguments.ToList();
                        for (int i = 0; i < args.Count; i++)
                        {
                            output.Append(this.FormatExpression(args[i]));
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
                            output.Append(this.FormatExpression(gArgs[i]));
                            if (i < gArgs.Count - 1)
                            {
                                output.Append(", ");
                            }
                        }
                        break;

                    case SparqlExpressionType.Primary:
                        //If Node/Numeric Term then use Node Formatting otherwise use ToString() on the expression
                        if (expr is NodeExpressionTerm)
                        {
                            NodeExpressionTerm nodeTerm = (NodeExpressionTerm)expr;
                            output.Append(this.Format(nodeTerm.Value(null, 0)));
                        }
                        else if (expr is NumericExpressionTerm)
                        {
                            NumericExpressionTerm numTerm = (NumericExpressionTerm)expr;
                            output.Append(this.Format(numTerm.Value(null, 0)));
                        }
                        else if (expr is GraphPatternExpressionTerm)
                        {
                            GraphPatternExpressionTerm gp = (GraphPatternExpressionTerm)expr;
                            output.Append(this.Format(gp.Pattern));
                        }
                        else
                        {
                            output.Append(expr.ToString());
                        }
                        break;

                    case SparqlExpressionType.SetOperator:
                        //Add First Argument and Set Operator
                        output.Append(this.FormatExpression(expr.Arguments.First()));
                        output.Append(' ');
                        output.Append(expr.Functor);

                        //Add Set
                        output.Append(" (");
                        List<ISparqlExpression> set = expr.Arguments.Skip(1).ToList();
                        for (int i = 0; i < set.Count; i++)
                        {
                            output.Append(this.FormatExpression(set[i]));
                            if (i < set.Count - 1)
                            {
                                output.Append(", ");
                            }
                        }
                        output.Append(')');
                        break;

                    case SparqlExpressionType.UnaryOperator:
                        //Just Functor then Expression
                        output.Append(expr.Functor);
                        output.Append(this.FormatExpression(expr.Arguments.First()));
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
                if (this._qnameMapper.ReduceToQName(agg.Functor, out aggQName))
                {
                    output.Append(aggQName);
                }
                else
                {
                    output.Append('<');
                    output.Append(this.FormatUri(agg.Functor));
                    output.Append('>');
                }
            }

            output.Append('(');
            List<ISparqlExpression> args = agg.Arguments.ToList();
            for (int i = 0; i < args.Count; i++)
            {
                output.Append(this.FormatExpression(args[i]));
                if (i < args.Count - 1 && !(args[i] is DistinctModifierExpression))
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
                output.Append(this.FormatPath(alt.LhsPath));
                output.Append(" | ");
                output.Append(this.FormatPath(alt.RhsPath));
                output.Append(')');
            }
            else if (path is FixedCardinality)
            {
                FixedCardinality card = (FixedCardinality)path;
                if (card.Path is BaseBinaryPath) output.Append('(');
                output.Append(this.FormatPath(card.Path));
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
                output.Append(this.FormatPath(inv.Path));
                if (inv.Path is BaseBinaryPath) output.Append(')');
            }
            else if (path is NOrMore)
            {
                NOrMore nOrMore = (NOrMore)path;
                if (nOrMore.Path is BaseBinaryPath) output.Append('(');
                output.Append(this.FormatPath(nOrMore.Path));
                if (nOrMore.Path is BaseBinaryPath) output.Append(')');
                output.Append('{');
                output.Append(nOrMore.MinCardinality);
                output.Append(",}");
            }
            else if (path is NToM)
            {
                NToM nToM = (NToM)path;
                if (nToM.Path is BaseBinaryPath) output.Append('(');
                output.Append(this.FormatPath(nToM.Path));
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
                output.Append(this.FormatPath(oneOrMore.Path));
                if (oneOrMore.Path is BaseBinaryPath) output.Append(')');
                output.Append('+');
            }
            else if (path is Property)
            {
                Property prop = (Property)path;
                output.Append(this.Format(prop.Predicate, TripleSegment.Predicate));
            }
            else if (path is SequencePath)
            {
                SequencePath seq = (SequencePath)path;
                output.Append(this.FormatPath(seq.LhsPath));
                output.Append(" / ");
                output.Append(this.FormatPath(seq.RhsPath));
            }
            else if (path is ZeroOrMore)
            {
                ZeroOrMore zeroOrMore = (ZeroOrMore)path;
                if (zeroOrMore.Path is BaseBinaryPath) output.Append('(');
                output.Append(this.FormatPath(zeroOrMore.Path));
                if (zeroOrMore.Path is BaseBinaryPath) output.Append(')');
                output.Append('*');
            }
            else if (path is ZeroOrOne)
            {
                ZeroOrOne zeroOrOne = (ZeroOrOne)path;
                if (zeroOrOne.Path is BaseBinaryPath) output.Append('(');
                output.Append(this.FormatPath(zeroOrOne.Path));
                if (zeroOrOne.Path is BaseBinaryPath) output.Append(')');
                output.Append('?');
            }
            else if (path is ZeroToN)
            {
                ZeroToN zeroToN = (ZeroToN)path;
                if (zeroToN.Path is BaseBinaryPath) output.Append('(');
                output.Append(this.FormatPath(zeroToN.Path));
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
                    output.Append(this.FormatPath(p));
                    output.Append(" | ");
                }
                foreach (Property p in negSet.InverseProperties)
                {
                    output.Append(this.FormatPath(p));
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

            if (groupBy.AssignVariable != null)
            {
                output.Append('(');
            }
            output.Append(this.FormatExpression(groupBy.Expression));
            if (groupBy.AssignVariable != null)
            {
                output.Append(" AS ?");
                output.Append(groupBy.AssignVariable);
                output.Append(')');
            }

            if (groupBy.Child != null)
            {
                output.Append(' ');
                output.Append(this.FormatGroupBy(groupBy.Child));
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
                output.Append(this.FormatExpression(orderBy.Expression));
                output.Append(')');
            }
            else 
            {
                output.Append("ASC(");
                output.Append(this.FormatExpression(orderBy.Expression));
                output.Append(')');
            }

            if (orderBy.Child != null)
            {
                output.Append(' ');
                output.Append(this.FormatOrderBy(orderBy.Child));
            }

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
