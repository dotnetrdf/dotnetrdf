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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Patterns
{
    /// <summary>
    /// Specialised Triple Pattern implementation used as a temporary data structure by the <see cref="VDS.RDF.Query.Optimisation.FullTextOptimiser">FullTextOptimiser</see>
    /// </summary>
    public class FullTextPattern
        : BaseTriplePattern
    {
        private List<TriplePattern> _origPatterns = new List<TriplePattern>();
        private PatternItem _matchVar, _scoreVar, _searchTerm, _thresholdTerm, _limitTerm;

        /// <summary>
        /// Creates a new Full Text Pattern
        /// </summary>
        /// <param name="origPatterns">Original Patterns</param>
        public FullTextPattern(IEnumerable<TriplePattern> origPatterns)
        {
            this._origPatterns.AddRange(origPatterns.OrderBy(tp => tp.Predicate.ToString()));
            PatternItem matchVar = null;
            PatternItem searchVar = null;
            Dictionary<String, PatternItem> firsts = new Dictionary<string, PatternItem>();
            Dictionary<String, PatternItem> rests = new Dictionary<string, PatternItem>();

            foreach (TriplePattern tp in this._origPatterns)
            {
                NodeMatchPattern predItem = tp.Predicate as NodeMatchPattern;
                if (predItem == null) continue;
                IUriNode predUri = predItem.Node as IUriNode;
                if (predUri == null) continue;

                switch (predUri.Uri.ToString())
                {
                    case FullTextHelper.FullTextMatchPredicateUri:
                        //Extract the Search Term
                        if (searchVar != null) throw new RdfQueryException("More than one pf:textMatch property specified");
                        if (tp.Object.VariableName == null)
                        {
                            this._searchTerm = tp.Object;
                        }
                        else
                        {
                            searchVar = tp.Object;
                        }

                        //Extract the Match Variable
                        if (matchVar != null) throw new RdfQueryException("More than one pf:textMatch property specified");
                        if (tp.Subject.VariableName != null && !tp.Subject.VariableName.StartsWith("_:"))
                        {
                            this._matchVar = tp.Subject;
                            if (this._origPatterns.Count > 1 && searchVar == null) throw new RdfQueryException("Too many patterns provided");
                        }
                        else
                        {
                            matchVar = tp.Subject;
                        }
                        break;

                    case RdfSpecsHelper.RdfListFirst:
                        firsts.Add(tp.Subject.VariableName.ToString(), tp.Object);
                        break;

                    case RdfSpecsHelper.RdfListRest:
                        rests.Add(tp.Subject.VariableName.ToString(), tp.Object);
                        break;
                    default:
                        throw new RdfQueryException("Unexpected pattern");
                }
            }

            //Use the first and rest lists to determine Match and Score Variables if necessary
            if (this._matchVar == null)
            {
                firsts.TryGetValue(matchVar.VariableName, out this._matchVar);
                String restKey = rests[matchVar.VariableName].VariableName;
                firsts.TryGetValue(restKey, out this._scoreVar);
            }
            //Use the first and rest lists to determine search term, threshold and limit if necessary
            if (this._searchTerm == null)
            {
                firsts.TryGetValue(searchVar.VariableName, out this._searchTerm);
                String restKey = rests[searchVar.VariableName].VariableName;
                firsts.TryGetValue(restKey, out this._thresholdTerm);
                PatternItem last = rests[restKey];
                if (!last.ToString().Equals("<" + RdfSpecsHelper.RdfListNil + ">"))
                {
                    restKey = rests[restKey].VariableName;
                    firsts.TryGetValue(restKey, out this._limitTerm);
                }
                else
                {
                    //If there is only 2 arguments for the search term determine whether it should actually be a 
                    //limit rather than a threshold
                    //Essentially if it is an integer assume that it was meant as a limit
                    INode temp = ((NodeMatchPattern)this._thresholdTerm).Node;
                    if (temp is ILiteralNode)
                    {
                        ILiteralNode lit = (ILiteralNode)temp;
                        if (lit.DataType != null)
                        {
                            if (SparqlSpecsHelper.GetNumericTypeFromDataTypeUri(lit.DataType) == VDS.RDF.Query.Expressions.SparqlNumericType.Integer)
                            {
                                //Is actually a limit
                                this._limitTerm = this._thresholdTerm;
                                this._thresholdTerm = null;
                            }
                        }
                        else
                        {
                            if (SparqlSpecsHelper.IsDecimal(lit.Value) || SparqlSpecsHelper.IsDouble(lit.Value))
                            {
                                //Remains as a Threshold
                            }
                            else if (SparqlSpecsHelper.IsInteger(lit.Value))
                            {
                                //Is actually a limit
                                this._limitTerm = this._thresholdTerm;
                                this._thresholdTerm = null;
                            }
                        }
                    }
                }
            }

            if (this._matchVar == null) throw new RdfQueryException("Failed to specify match variable");
            if (this._searchTerm == null) this._searchTerm = searchVar;
            if (this._searchTerm == null) throw new RdfQueryException("Failed to specify search terms");
        }

#if UNFINISHED
        /// <summary>
        /// Creates a new Full Text Pattern
        /// </summary>
        /// <param name="matchVar">Match Variable</param>
        /// <param name="scoreVar">Score Variable</param>
        /// <param name="searchTerm">Search Term</param>
        public FullTextPattern(PatternItem matchVar, PatternItem scoreVar, PatternItem searchTerm)
        {
            this._matchVar = matchVar;
            this._scoreVar = scoreVar;
            this._searchTerm = searchTerm;

            NodeFactory factory = new NodeFactory();
            if (this._scoreVar != null)
            {
                BlankNodePattern a = new BlankNodePattern(factory.GetNextBlankNodeID());
                BlankNodePattern b = new BlankNodePattern(factory.GetNextBlankNodeID());
                this._origPatterns.Add(new TriplePattern(a, new NodeMatchPattern(factory.CreateUriNode(new Uri(FullTextHelper.FullTextMatchPredicateUri))), this._searchTerm));
                this._origPatterns.Add(new TriplePattern(a, new NodeMatchPattern(factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfListFirst))), this._matchVar));
                this._origPatterns.Add(new TriplePattern(a, new NodeMatchPattern(factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfListRest))), b));
                this._origPatterns.Add(new TriplePattern(b, new NodeMatchPattern(factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfListFirst))), this._scoreVar));
                this._origPatterns.Add(new TriplePattern(b, new NodeMatchPattern(factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfListRest))), new NodeMatchPattern(factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfListNil)))));
            }
            else
            {
                this._origPatterns.Add(new TriplePattern(this._matchVar, new NodeMatchPattern(factory.CreateUriNode(new Uri(FullTextHelper.FullTextMatchPredicateUri))), this._searchTerm));
            }
        }
#endif

        /// <summary>
        /// Gets the Original Triple Patterns
        /// </summary>
        public IEnumerable<TriplePattern> OriginalPatterns
        {
            get
            {
                return this._origPatterns;
            }
        }

        /// <summary>
        /// Gets the Match Variable
        /// </summary>
        public PatternItem MatchVariable
        {
            get
            {
                return this._matchVar;
            }
        }

        /// <summary>
        /// Gets the Score Variable
        /// </summary>
        public PatternItem ScoreVariable
        {
            get
            {
                return this._scoreVar;
            }
        }

        /// <summary>
        /// Gets the Search Terms
        /// </summary>
        public PatternItem SearchTerm
        {
            get
            {
                return this._searchTerm;
            }
        }

        /// <summary>
        /// Gets the Score Threshold
        /// </summary>
        public double ScoreThreshold
        {
            get
            {
                try
                {
                    return SparqlSpecsHelper.ToDouble((this._thresholdTerm as NodeMatchPattern).Node as ILiteralNode);
                }
                catch
                {
                    return Double.NaN;
                }
            }
        }

        /// <summary>
        /// Gets the Result Limit
        /// </summary>
        public int Limit
        {
            get
            {
                try
                {
                    return Convert.ToInt32(SparqlSpecsHelper.ToInteger((this._limitTerm as NodeMatchPattern).Node as ILiteralNode));
                }
                catch
                {
                    return -1;
                }
            }
        }
        
        /// <summary>
        /// Evaluates the Triple Pattern in the given Context
        /// </summary>
        /// <param name="context">Context</param>
        /// <remarks>
        /// <para>
        /// Since a <see cref="FullTextPattern">FullTextPattern</see> on its own does not know how to execute itself as it would need a <see cref="VDS.RDF.Query.FullText.Search.IFullTextSearchProvider">IFullTextSearchProvider</see> it simply creates a BGP from the original patterns and they are evaluated as simple triple matches
        /// </para>
        /// </remarks>
        public override void Evaluate(SparqlEvaluationContext context)
        {
            Bgp bgp = new Bgp(this._origPatterns);
            context.Evaluate(bgp);
        }

        /// <summary>
        /// Returns that the pattern does not accept all triples
        /// </summary>
        public override bool IsAcceptAll
        {
            get 
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the String representation of the Triple Pattern
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            foreach (TriplePattern tp in this._origPatterns)
            {
                output.AppendLine(tp.ToString());
            }
            return output.ToString();
        }
    }
}
