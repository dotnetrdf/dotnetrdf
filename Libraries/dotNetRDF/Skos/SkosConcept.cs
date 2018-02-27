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
    using System.Linq;

    /// <summary>
    /// Represents a SKOS concept
    /// </summary>
    public class SkosConcept : SkosMember
    {
        /// <summary>
        /// Creates a new concept for the given resource
        /// </summary>
        /// <param name="resource">Resource representing the concept</param>
        public SkosConcept(INode resource) : base(resource) { }

        /// <summary>
        /// Gets the concept schemes the concept is contained in
        /// </summary>
        public IEnumerable<SkosConceptScheme> InScheme
        {
            get
            {
                return this.GetConceptSchemes(SkosHelper.InScheme);
            }
        }

        /// <summary>
        /// Get the concept schemes the concept is the top concept of
        /// </summary>
        public IEnumerable<SkosConceptScheme> TopConceptOf
        {
            get
            {
                return this.GetConceptSchemes(SkosHelper.TopConceptOf);
            }
        }

        /// <summary>
        /// Gets the preferred labels of the concept
        /// </summary>
        public IEnumerable<ILiteralNode> PrefLabel
        {
            get
            {
                return this.GetLiterals(SkosHelper.PrefLabel);
            }
        }

        /// <summary>
        /// Gets the alternative labels of the concept
        /// </summary>
        public IEnumerable<ILiteralNode> AltLabel
        {
            get
            {
                return this.GetLiterals(SkosHelper.AltLabel);
            }
        }

        /// <summary>
        /// Gets the hidden labels of the concept
        /// </summary>
        public IEnumerable<ILiteralNode> HiddenLabel
        {
            get
            {
                return this.GetLiterals(SkosHelper.HiddenLabel);
            }
        }

        /// <summary>
        /// Gets a unique identifiers of the concept in a given concept scheme
        /// </summary>
        public IEnumerable<ILiteralNode> Notation
        {
            get
            {
                return this.GetLiterals(SkosHelper.Notation);
            }
        }

        /// <summary>
        /// Gets the general notes of the concept
        /// </summary>
        public IEnumerable<INode> Note
        {
            get
            {
                return this.GetObjects(SkosHelper.Note);
            }
        }

        /// <summary>
        /// Gets the modification notes of the concept
        /// </summary>
        public IEnumerable<INode> ChangeNote
        {
            get
            {
                return this.GetObjects(SkosHelper.ChangeNote);
            }
        }

        /// <summary>
        /// Gets the formal explanation of the concept
        /// </summary>
        public IEnumerable<INode> Definition
        {
            get
            {
                return this.GetObjects(SkosHelper.Definition);
            }
        }

        /// <summary>
        /// Gets the editorial notes the concept
        /// </summary>
        public IEnumerable<INode> EditorialNote
        {
            get
            {
                return this.GetObjects(SkosHelper.EditorialNote);
            }
        }

        /// <summary>
        /// Gets examples of the use of the concept
        /// </summary>
        public IEnumerable<INode> Example
        {
            get
            {
                return this.GetObjects(SkosHelper.Example);
            }
        }

        /// <summary>
        /// Gets notes about the past of the concept
        /// </summary>
        public IEnumerable<INode> HistoryNote
        {
            get
            {
                return this.GetObjects(SkosHelper.HistoryNote);
            }
        }

        /// <summary>
        /// Gets notes that help to clarify the meaning and/or the use of the concept
        /// </summary>
        public IEnumerable<INode> ScopeNote
        {
            get
            {
                return this.GetObjects(SkosHelper.ScopeNote);
            }
        }

        /// <summary>
        /// Gets concepts related by meaning
        /// </summary>
        public IEnumerable<SkosConcept> SemanticRelation
        {
            get
            {
                return this.GetConcepts(SkosHelper.SemanticRelation);
            }
        }

        /// <summary>
        /// Gets more general concepts 
        /// </summary>
        public IEnumerable<SkosConcept> Broader
        {
            get
            {
                return this.GetConcepts(SkosHelper.Broader);
            }
        }

        /// <summary>
        /// Gets more specific concepts
        /// </summary>
        public IEnumerable<SkosConcept> Narrower
        {
            get
            {
                return this.GetConcepts(SkosHelper.Narrower);
            }
        }

        /// <summary>
        /// Gets associated concepts
        /// </summary>
        public IEnumerable<SkosConcept> Related
        {
            get
            {
                return this.GetConcepts(SkosHelper.Related);
            }
        }

        /// <summary>
        /// Gets more general concepts (transitive)
        /// </summary>
        public IEnumerable<SkosConcept> BroaderTransitive
        {
            get
            {
                return this.GetConcepts(SkosHelper.BroaderTransitive);
            }
        }

        /// <summary>
        /// Gets more specific concepts (transitive)
        /// </summary>
        public IEnumerable<SkosConcept> NarrowerTransitive
        {
            get
            {
                return this.GetConcepts(SkosHelper.NarrowerTransitive);
            }
        }

        /// <summary>
        /// Gets concepts with comparable meaning from other concept schemes
        /// </summary>
        public IEnumerable<SkosConcept> MappingRelation
        {
            get
            {
                return this.GetConcepts(SkosHelper.MappingRelation);
            }
        }

        /// <summary>
        /// Gets confidently interchangeable concepts from other concept schemes
        /// </summary>
        public IEnumerable<SkosConcept> CloseMatch
        {
            get
            {
                return this.GetConcepts(SkosHelper.CloseMatch);
            }
        }

        /// <summary>
        /// Gets interchangeably similar concepts from other concept schemes
        /// </summary>
        public IEnumerable<SkosConcept> ExactMatch
        {
            get
            {
                return this.GetConcepts(SkosHelper.ExactMatch);
            }
        }

        /// <summary>
        /// Gets more general concepts from other concept schemes
        /// </summary>
        public IEnumerable<SkosConcept> BroadMatch
        {
            get
            {
                return this.GetConcepts(SkosHelper.BroadMatch);
            }
        }

        /// <summary>
        /// Gets more specific concepts from other concept schemes
        /// </summary>
        public IEnumerable<SkosConcept> NarrowMatch
        {
            get
            {
                return this.GetConcepts(SkosHelper.NarrowMatch);
            }
        }

        /// <summary>
        /// Gets associated concepts from other concept schemes
        /// </summary>
        public IEnumerable<SkosConcept> RelatedMatch
        {
            get
            {
                return this.GetConcepts(SkosHelper.RelatedMatch);
            }
        }

        private IEnumerable<SkosConceptScheme> GetConceptSchemes(string predicateUri)
        {
            return this
                .GetObjects(predicateUri)
                .Select(o => new SkosConceptScheme(o));
        }

        private IEnumerable<ILiteralNode> GetLiterals(string predicateUri)
        {
            return this
                .GetObjects(predicateUri)
                .Cast<ILiteralNode>();
        }
    }
}
