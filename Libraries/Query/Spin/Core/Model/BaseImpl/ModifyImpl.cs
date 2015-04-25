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

using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Spin.Model.IO;
using VDS.RDF.Query.Spin.OntologyHelpers;

namespace VDS.RDF.Query.Spin.Model
{
    public class ModifyImpl : UpdateImpl, IModifyResource
    {
        public ModifyImpl(INode node, SpinModel graph)
            : base(node, graph)
        {
        }

        public IEnumerable<Uri> getUsing()
        {
            return ListProperties(SP.PropertyUsing).Select(t => ((IUriNode)t.Object).Uri);
        }

        public IEnumerable<Uri> getUsingNamed()
        {
            return ListProperties(SP.PropertyUsingNamed).Select(t => ((IUriNode)t.Object).Uri);
        }

        public override void printSPINRDF(ISparqlPrinter p)
        {
            printComment(p);
            printPrefixes(p);

            IResource iri = GetResource(SP.PropertyGraphIRI);

            IResource with = GetResource(SP.PropertyWith);
            if (with != null)
            {
                p.printIndentation(p.getIndentation());
                p.printKeyword("WITH");
                p.print(" ");
                p.printURIResource(with);
                p.println();
            }

            // TODO add a INSERT/CONSTRUCT pattern before the delete is effective
            if (printTemplates(p, SP.PropertyDeletePattern, "DELETE", HasProperty(SP.PropertyDeletePattern), iri))
            {
                p.print("\n");
            }
            if (printTemplates(p, SP.PropertyInsertPattern, "INSERT", HasProperty(SP.PropertyInsertPattern), iri))
            {
                p.print("\n");
            }

            foreach (Uri _using in getUsing())
            {
                p.printKeyword("USING");
                p.print(" <");
                p.print(_using.ToString());
                p.print(">");
                p.println();
            }

            foreach (Uri usingNamed in getUsingNamed())
            {
                p.printKeyword("USING");
                p.print(" ");
                p.printKeyword("NAMED");
                p.print(" <");
                p.print(usingNamed.ToString());
                p.print(">");
                p.println();
            }
            printWhere(p);
        }
    }
}