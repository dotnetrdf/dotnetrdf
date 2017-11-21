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
using VDS.RDF.Nodes;
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.Util;

namespace VDS.RDF.Query.Spin.Model
{
    internal class ValuesImpl : ElementImpl, IValues
    {

        public ValuesImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {

        }


        public List<Dictionary<String, IResource>> getBindings()
        {
            List<String> varNames = getVarNames();
            List<Dictionary<String, IResource>> bindings = new List<Dictionary<String, IResource>>();
            List<IResource> outerList = getResource(SP.PropertyBindings).AsList();
            if (outerList != null)
            {
                foreach (IResource innerList in outerList)
                {
                    Dictionary<String, IResource> binding = new Dictionary<String, IResource>();
                    bindings.Add(binding);
                    IEnumerator<String> vars = varNames.GetEnumerator();
                    IEnumerator<IResource> values = innerList.AsList().GetEnumerator();
                    while (vars.MoveNext())
                    {
                        String varName = vars.Current;
                        IResource value = values.Current;
                        if (!RDFUtil.sameTerm(SP.ClassPropertyUndef, value))
                        {
                            binding.Add(varName, value);
                        }
                    }
                }
            }
            return bindings;
        }


        public List<String> getVarNames()
        {
            List<String> results = new List<String>();
            List<IResource> list = getResource(SP.PropertyVarNames).AsList();
            foreach (IResource member in list)
            {
                results.Add(((IValuedNode)member.getSource()).AsString());
            }
            return results;
        }


        override public void Print(ISparqlPrinter p)
        {
            p.printKeyword("VALUES");
            p.print(" ");
            List<String> varNames = getVarNames();
            if (varNames.Count == 1)
            {
                p.printVariable(varNames[0]);
            }
            else
            {
                p.print("(");
                IEnumerator<String> vit = varNames.GetEnumerator();
                while (vit.MoveNext())
                {
                    p.printVariable(vit.Current);
                    if (vit.MoveNext())
                    {
                        p.print(" ");
                    }
                }
                p.print(")");
            }
            p.print(" {");
            p.println();
            foreach (Dictionary<String, IResource> binding in getBindings())
            {
                p.printIndentation(p.getIndentation() + 1);
                if (varNames.Count != 1)
                {
                    p.print("(");
                }
                IEnumerator<String> vit = varNames.GetEnumerator();
                while (vit.MoveNext())
                {
                    String varName = vit.Current;
                    IResource value = binding[varName];
                    if (value == null)
                    {
                        p.printKeyword("UNDEF");
                    }
                    else if (value.isUri())
                    {
                        p.printURIResource(value);
                    }
                    else
                    {
                        TupleImpl.print(getModel(), Resource.Get(value, getModel()), p);
                    }
                    if (vit.MoveNext())
                    {
                        p.print(" ");
                    }
                }
                if (varNames.Count != 1)
                {
                    p.print(")");
                }
                p.println();
            }
            p.printIndentation(p.getIndentation());
            p.print("}");
        }


        //override public void visit(IElementVisitor visitor)
        //{
        //    visitor.visit(this);
        //}
    }
}