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

namespace VDS.RDF.Skos
{
    using System.Collections.Generic;

    public class SkosConcept : SkosMember
    {
        public SkosConcept(INode resource) : base(resource) { }

        public IEnumerable<SkosConceptScheme> InScheme
        {
            get
            {
                return this.GetConceptSchemes(SkosHelper.InScheme);
            }
        }

        public IEnumerable<SkosConceptScheme> TopConceptOf
        {
            get
            {
                return this.GetConceptSchemes(SkosHelper.TopConceptOf);
            }
        }

        public IEnumerable<ILiteralNode> PrefLabel
        {
            get
            {
                return this.GetLiterals(SkosHelper.PrefLabel);
            }
        }

        public IEnumerable<ILiteralNode> AltLabel
        {
            get
            {
                return this.GetLiterals(SkosHelper.AltLabel);
            }
        }

        public IEnumerable<ILiteralNode> HiddenLabel
        {
            get
            {
                return this.GetLiterals(SkosHelper.HiddenLabel);
            }
        }

        public IEnumerable<ILiteralNode> Notation
        {
            get
            {
                return this.GetLiterals(SkosHelper.Notation);
            }
        }

        public IEnumerable<INode> Note
        {
            get
            {
                return this.GetObjects(SkosHelper.Note);
            }
        }

        public IEnumerable<INode> ChangeNote
        {
            get
            {
                return this.GetObjects(SkosHelper.ChangeNote);
            }
        }

        public IEnumerable<INode> Definition
        {
            get
            {
                return this.GetObjects(SkosHelper.Definition);
            }
        }

        public IEnumerable<INode> EditorialNote
        {
            get
            {
                return this.GetObjects(SkosHelper.EditorialNote);
            }
        }

        public IEnumerable<INode> Example
        {
            get
            {
                return this.GetObjects(SkosHelper.Example);
            }
        }

        public IEnumerable<INode> HistoryNote
        {
            get
            {
                return this.GetObjects(SkosHelper.HistoryNote);
            }
        }

        public IEnumerable<INode> ScopeNote
        {
            get
            {
                return this.GetObjects(SkosHelper.ScopeNote);
            }
        }

        public IEnumerable<SkosConcept> SemanticRelation
        {
            get
            {
                return this.GetConcepts(SkosHelper.SemanticRelation);
            }
        }

        public IEnumerable<SkosConcept> Broader
        {
            get
            {
                return this.GetConcepts(SkosHelper.Broader);
            }
        }

        public IEnumerable<SkosConcept> Narrower
        {
            get
            {
                return this.GetConcepts(SkosHelper.Narrower);
            }
        }

        public IEnumerable<SkosConcept> Related
        {
            get
            {
                return this.GetConcepts(SkosHelper.Related);
            }
        }

        public IEnumerable<SkosConcept> BroaderTransitive
        {
            get
            {
                return this.GetConcepts(SkosHelper.BroaderTransitive);
            }
        }

        public IEnumerable<SkosConcept> NarrowerTransitive
        {
            get
            {
                return this.GetConcepts(SkosHelper.NarrowerTransitive);
            }
        }

        public IEnumerable<SkosConcept> MappingRelation
        {
            get
            {
                return this.GetConcepts(SkosHelper.MappingRelation);
            }
        }

        public IEnumerable<SkosConcept> CloseMatch
        {
            get
            {
                return this.GetConcepts(SkosHelper.CloseMatch);
            }
        }

        public IEnumerable<SkosConcept> ExactMatch
        {
            get
            {
                return this.GetConcepts(SkosHelper.ExactMatch);
            }
        }

        public IEnumerable<SkosConcept> BroadMatch
        {
            get
            {
                return this.GetConcepts(SkosHelper.BroadMatch);
            }
        }

        public IEnumerable<SkosConcept> NarrowMatch
        {
            get
            {
                return this.GetConcepts(SkosHelper.NarrowMatch);
            }
        }

        public IEnumerable<SkosConcept> RelatedMatch
        {
            get
            {
                return this.GetConcepts(SkosHelper.RelatedMatch);
            }
        }
    }
}
