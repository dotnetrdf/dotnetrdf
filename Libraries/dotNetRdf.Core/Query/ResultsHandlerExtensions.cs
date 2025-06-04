/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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

using System.Linq;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query;

/// <summary>
/// Extension methods for <see cref="ISparqlResultsHandler"/> instances.
/// </summary>
public static class ResultsHandlerExtensions
{
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
                // ASK Query so get the handler to handle an appropriate boolean result
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
                // SELECT Query so get the handler to handle variables and then handle results
                foreach (var var in context.OutputMultiset.Variables)
                {
                    if (!handler.HandleVariable(var)) ParserHelper.Stop();
                }
                foreach (ISet s in context.OutputMultiset.Sets)
                {
                    if (!handler.HandleResult(context.Options.SparqlResultFactory.MakeResult(s))) ParserHelper.Stop();
                }

                // The VirtualCount property on SparqlQuery has been marked obsolete
                // as it doesn't make sense to open it up as a public property and
                // in any case the count of results feels like it is more properly 
                // recorded in the results.
                // q.VirtualCount = context.OutputMultiset.VirtualCount;

                // TODO: Create an extended ISparqlResultsHandler that can receive the virtual count. e.g.
                // handler.SetVirtualCount(context.OutputMultiset.VirtualCount);

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
