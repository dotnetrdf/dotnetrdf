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

namespace VDS.RDF.Shacl
{
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF.Nodes;
    using VDS.RDF.Parsing;

    public class ShaclValidationReport : WrapperNode
    {
        private ShaclValidationReport(INode node)
            : base(node)
        {
            Graph.TripleAsserted += TripleAsserted;
            Graph.TripleRetracted += TripleRetracted;
        }

        private INode rdf_type => Graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));

        internal INode Type
        {
            get
            {
                return rdf_type.ObjectsOf(this).SingleOrDefault();
            }

            set
            {
                foreach (var type in rdf_type.ObjectsOf(this).ToList())
                {
                    Graph.Retract(this, rdf_type, type);
                }

                Graph.Assert(this, rdf_type, value);
            }
        }

        internal bool Conforms
        {
            get
            {
                var conforms = Shacl.Conforms.ObjectsOf(this).SingleOrDefault();
                if (conforms is null)
                {
                    return true;
                }

                return conforms.AsValuedNode().AsBoolean();
            }

            set
            {
                foreach (var conforms in Shacl.Conforms.ObjectsOf(this).ToList())
                {
                    Graph.Retract(this, Shacl.Conforms, conforms);
                }

                Graph.Assert(this, Shacl.Conforms, value.ToLiteral(Graph));
            }
        }

        internal ICollection<ShaclValidationResult> Results => new ShaclValidationResultCollection(this);

        internal static ShaclValidationReport Create(IGraph g)
        {
            var report = new ShaclValidationReport(g.CreateBlankNode());
            report.Type = Shacl.ValidationReport;
            report.Conforms = true;

            return report;
        }

        private void TripleRetracted(object sender, TripleEventArgs args)
        {
            if (args.Triple.Predicate.Equals(Shacl.Result))
            {
                Conforms = !Results.Any();
            }
        }

        private void TripleAsserted(object sender, TripleEventArgs args)
        {
            if (args.Triple.Predicate.Equals(Shacl.Result))
            {
                Conforms = false;
            }
        }
    }
}
