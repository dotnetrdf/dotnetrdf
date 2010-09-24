/*

Copyright Robert Vesse 2009-10
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
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents a BGP which is a set of Triple Patterns
    /// </summary>
    public class Bgp : ISparqlAlgebra
    {
        protected List<ITriplePattern> _triplePatterns = new List<ITriplePattern>();

        /// <summary>
        /// Creates a new empty BGP
        /// </summary>
        public Bgp()
        {

        }

        /// <summary>
        /// Creates a BGP containing a single Triple Pattern
        /// </summary>
        /// <param name="p">Triple Pattern</param>
        public Bgp(ITriplePattern p)
        {
            this._triplePatterns.Add(p);
        }

        /// <summary>
        /// Creates a BGP containing a set of Triple Patterns
        /// </summary>
        /// <param name="ps">Triple Patterns</param>
        public Bgp(IEnumerable<ITriplePattern> ps)
        {
            this._triplePatterns.AddRange(ps);
        }

        /// <summary>
        /// Creates a BGP containing a set of Triple Patterns
        /// </summary>
        /// <param name="ps">Triple Patterns</param>
        public Bgp(IEnumerable<TriplePattern> ps)
        {
            this._triplePatterns.AddRange(ps.Select(p => (ITriplePattern)p));
        }

        /// <summary>
        /// Creates a BGP containing a set of Let Patterns
        /// </summary>
        /// <param name="ps">Let Patterns</param>
        public Bgp(IEnumerable<LetPattern> ps)
        {
            this._triplePatterns.AddRange(ps.Select(p => (ITriplePattern)p));
        }

        /// <summary>
        /// Creates a BGP containing a set of Filter Patterns
        /// </summary>
        /// <param name="ps">Filter Patterns</param>
        public Bgp(IEnumerable<FilterPattern> ps)
        {
            this._triplePatterns.AddRange(ps.Select(p => (ITriplePattern)p));
        }

        /// <summary>
        /// Creates a BGP containing a set of Sub-query Patterns
        /// </summary>
        /// <param name="ps">Sub-query Patterns</param>
        public Bgp(IEnumerable<SubQueryPattern> ps)
        {
            this._triplePatterns.AddRange(ps.Select(p => (ITriplePattern)p));
        }

        /// <summary>
        /// Gets the number of Triple Patterns in the BGP
        /// </summary>
        public int PatternCount
        {
            get
            {
                return this._triplePatterns.Count;
            }
        }

        /// <summary>
        /// Evaluates the BGP against the Evaluation Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public virtual BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            if (this._triplePatterns.Count > 0)
            {
                for (int i = 0; i < this._triplePatterns.Count; i++)
                {
                    if (i == 0)
                    {
                        //If the 1st thing in a BGP is a LET/FILTER the Input becomes the Identity Multiset
                        if (this._triplePatterns[i] is FilterPattern/* || this._triplePatterns[i] is LetPattern*/)
                        {
                            context.InputMultiset = new IdentityMultiset();
                        }
                    }

                    //Create a new Output Multiset
                    context.OutputMultiset = new Multiset();

                    this._triplePatterns[i].Evaluate(context);

                    //If at any point we've got an Empty Multiset as our Output then we terminate BGP execution
                    if (context.OutputMultiset.IsEmpty) break;

                    //Check for Timeout before attempting the Join
                    context.CheckTimeout();

                    //If this isn't the first Pattern we do Join/Product the Output to the Input
                    if (i > 0)
                    {
                        if (context.InputMultiset.IsDisjointWith(context.OutputMultiset))
                        {
                            //Disjoint so do a Product
                            context.OutputMultiset = context.InputMultiset.Product(context.OutputMultiset);
                        }
                        else
                        {
                            //Normal Join
                            context.OutputMultiset = context.InputMultiset.Join(context.OutputMultiset);
                        }
                    }
                    //Then the Input for the next Pattern is the Output from the previous Pattern
                    context.InputMultiset = context.OutputMultiset;
                }
                //Trim the Multiset - this eliminates any temporary variables
                context.OutputMultiset.Trim();
            }
            else
            {
                //For an Empty BGP we just return the Identity Multiset
                context.OutputMultiset = new IdentityMultiset();
            }

            //If we've ended with an Empty Multiset (last thing was a FILTER/LET) then we turn it into the Null Multiset
            if (context.OutputMultiset is Multiset && context.OutputMultiset.IsEmpty) context.OutputMultiset = new NullMultiset();

            //Return the Output Multiset
            return context.OutputMultiset;
        }

        /// <summary>
        /// Gets the Variables used in the Algebra
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                return (from tp in this._triplePatterns
                        from v in tp.Variables
                        select v).Distinct();
            }
        }

        /// <summary>
        /// Gets whether the BGP is the emtpy BGP
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return (this._triplePatterns.Count == 0);
            }
        }

        /// <summary>
        /// Returns the String representation of the BGP
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "BGP()";
        }
    }

    /// <summary>
    /// Represents a BGP which is a set of Triple Patterns
    /// </summary>
    /// <remarks>
    /// <para>
    /// A Streamed BGP differs from a BGP in that rather than evaluating each Triple Pattern in turn it evaluates across all Triple Patterns.  This is useful for ASK queries where we are only concerned with whether a BGP matches and not in the specific solutions
    /// </para>
    /// <para>
    /// A Streamed BGP can only contain concrete Triple Patterns and not any of the specialised Triple Pattern classes
    /// </para>
    /// </remarks>
    public class StreamedBgp : Bgp
    {
        /// <summary>
        /// Creates a Streamed BGP containing a single Triple Pattern
        /// </summary>
        /// <param name="p">Triple Pattern</param>
        public StreamedBgp(TriplePattern p)
        {
            this._triplePatterns.Add((ITriplePattern)p);
        }

        /// <summary>
        /// Creates a Streamed BGP containing a set of Triple Patterns
        /// </summary>
        /// <param name="ps">Triple Patterns</param>
        public StreamedBgp(IEnumerable<TriplePattern> ps)
        {
            this._triplePatterns.AddRange(ps.Select(p => (ITriplePattern)p));
        }

        /// <summary>
        /// Evaluates the BGP against the Evaluation Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public virtual BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            throw new NotImplementedException("The StreamedBgp is an experimental class which is currently unimplemented");
        }
    }
}
