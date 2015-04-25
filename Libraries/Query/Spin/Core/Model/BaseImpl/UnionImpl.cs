/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved.
 *******************************************************************************/
/*
 
A C# port of the SPIN API (http://topbraid.org/spin/api/)
an open source Java API distributed by TopQuadrant to encourage the adoption of SPIN in the community. The SPIN API is built on the Apache Jena API and provides the following features: 
 
-----------------------------------------------------------------------------

dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System.Collections.Generic;
using VDS.RDF.Query.Spin.Model.IO;

namespace VDS.RDF.Query.Spin.Model
{
    public class UnionImpl : ElementImpl, IUnionResource
    {
        public UnionImpl(INode node, SpinModel spinModel)
            : base(node, spinModel)
        {
        }

        override public void Print(ISparqlPrinter p)
        {
            List<IElementResource> elements = getElements();
            for (IEnumerator<IElementResource> it = elements.GetEnumerator(); it.MoveNext(); )
            {
                IElementResource element = it.Current;
                p.print("{");
                p.println();
                p.setIndentation(p.getIndentation() + 1);
                element.Print(p);
                p.setIndentation(p.getIndentation() - 1);
                p.printIndentation(p.getIndentation());
                p.print("}");
                if (it.MoveNext())
                {
                    p.println();
                    p.printIndentation(p.getIndentation());
                    p.printKeyword("UNION");
                    p.println();
                    p.printIndentation(p.getIndentation());
                }
            }
        }

        //override public void visit(IElementVisitor visitor)
        //{
        //    visitor.visit(this);
        //}
    }
}