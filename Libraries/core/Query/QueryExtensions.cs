/*

Copyright Robert Vesse 2009-11
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
using System.Linq;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query
{
    /// <summary>
    /// Static Helper class containing extension methods related to queries
    /// </summary>
    static class QueryExtensions
    {
        /// <summary>
        /// Determines whether an Expresion uses the Default Dataset
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <returns></returns>
        /// <remarks>
        /// Almost all Expressions use the Default Dataset.  The only ones that does are EXISTS/NOT EXISTS expressions where the graph pattern does not use the default dataset
        /// </remarks>
        internal static bool UsesDefaultDataset(this ISparqlExpression expr)
        {
            switch (expr.Type)
            {
                case SparqlExpressionType.Aggregate:
                case SparqlExpressionType.BinaryOperator:
                case SparqlExpressionType.Function:
                case SparqlExpressionType.GraphOperator:
                case SparqlExpressionType.SetOperator:
                case SparqlExpressionType.UnaryOperator:
                    return expr.Arguments.All(arg => arg.UsesDefaultDataset());
                case SparqlExpressionType.Primary:
                    if (expr is GraphPatternExpressionTerm)
                    {
                        return ((GraphPatternExpressionTerm)expr).Pattern.UsesDefaultDataset;
                    }
                    else
                    {
                        return true;
                    }
                default:
                    return true;
            }
        }

        internal static void Apply(this ISparqlResultsHandler handler, SparqlEvaluationContext context)
        {
            try
            {
                handler.StartResults();

                SparqlQuery q = context.Query;
                SparqlQueryType type;
                if (q == null)
                {
                    type = (context.OutputMultiset.Variables.Any() || context.OutputMultiset.Sets.Any() ? SparqlQueryType.Select : SparqlQueryType.Ask);
                }
                else
                {
                    type = q.QueryType;
                }

                if (type == SparqlQueryType.Ask)
                {
                    //ASK Query so get the handler to handle an appropriate boolean result
                    if (context.OutputMultiset is IdentityMultiset)
                    {
                        handler.HandleBooleanResult(true);
                    }
                    else if (context.OutputMultiset is NullMultiset)
                    {
                        handler.HandleBooleanResult(false);
                    }
                    else
                    {
                        handler.HandleBooleanResult(!context.OutputMultiset.IsEmpty);
                    }
                }
                else
                {
                    //SELECT Query so get the handler to handle variables and then handle results
                    foreach (String var in context.OutputMultiset.Variables)
                    {
                        if (!handler.HandleVariable(var)) ParserHelper.Stop();
                    }
                    foreach (ISet s in context.OutputMultiset.Sets)
                    {
                        if (!handler.HandleResult(new SparqlResult(s))) ParserHelper.Stop();
                    }
                }

                handler.EndResults(true);
            }
            catch (RdfParsingTerminatedException)
            {
                handler.EndResults(true);
            }
            catch
            {
                handler.EndResults(false);
                throw;
            }
        }
    }
}
