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

using System.Collections.Generic;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query.Spin.Model
{
    internal class ConstructImpl : QueryImpl, IConstruct
    {

        public ConstructImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {
        }


        public List<ITripleTemplate> getTemplates()
        {
            List<ITripleTemplate> results = new List<ITripleTemplate>();
            foreach (IResource next in getList(SP.PropertyTemplates))
            {
                if (next != null && !(next.isLiteral()))
                {
                    results.Add((ITripleTemplate)next.As(typeof(TripleTemplateImpl)));
                }
            }
            return results;
        }

        override public void Print(ISparqlPrinter p)
        {
            // TODO Auto-generated method stub
        }

        override public void printSPINRDF(ISparqlPrinter context)
        {
            printComment(context);
            printPrefixes(context);
            context.printIndentation(context.getIndentation());
            context.printKeyword("CONSTRUCT");
            context.print(" {");
            context.println();
            foreach (ITripleTemplate template in getTemplates())
            {
                context.printIndentation(context.getIndentation() + 1);
                template.Print(context);
                context.print(" .");
                context.println();
            }
            context.printIndentation(context.getIndentation());
            context.print("}");
            printStringFrom(context);
            context.println();
            printWhere(context);
            printSolutionModifiers(context);
            printValues(context);
        }
    }
}