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

namespace VDS.RDF.Query.Inference
{
    /// <summary>
    /// Wrapper around an <see cref="IOwlReasoner">IOwlReasoner</see> to make it appear like a forward-chaining reasoner
    /// </summary>
    /// <remarks>
    /// Essentially all this class does is extract all triples which the underlying reasoner can infer.  Currently the input graph and any graph passed to the <see cref="IInferenceEngine.Initialise">Initialise()</see> method have no effect on the output of the reasoner
    /// </remarks>
    public class StaticOwlReasonerWrapper : IInferenceEngine
    {
        private IOwlReasoner _reasoner;

        /// <summary>
        /// Creates a new OWL Reasoner Wrapper around the given OWL Reasoner
        /// </summary>
        /// <param name="reasoner">OWL Reasoner</param>
        public StaticOwlReasonerWrapper(IOwlReasoner reasoner)
        {
            this._reasoner = reasoner;
        }

        /// <summary>
        /// Applies the reasoner to the given Graph outputting inferences into the same Graph
        /// </summary>
        /// <param name="g">Graph</param>
        public virtual void Apply(IGraph g)
        {
            this.Apply(g, g);
        }

        /// <summary>
        /// Applies the reasoner to the given input Graph outputting inferences into the output Graph
        /// </summary>
        /// <param name="input">Input Graph</param>
        /// <param name="output">Output Graph</param>
        public virtual void Apply(IGraph input, IGraph output)
        {
            output.Assert(this._reasoner.Extract(OwlHelper.OwlExtractMode.AllStatements));
        }

        /// <summary>
        /// Initialises the reasoner
        /// </summary>
        /// <param name="g">Graph to initialise with</param>
        public void Initialise(IGraph g)
        {
            this._reasoner.Add(g);
        }
    }

    /// <summary>
    /// Wrapper around an <see cref="IOwlReasoner">IOwlReasoner</see> to make it appear like a forward-chaining reasoner
    /// </summary>
    /// <remarks>
    /// Effectively equivalent to <see cref="StaticOwlReasonerWrapper">StaticOwlReasonerWrapper</see> except that every Graph reasoning is applied to is added to the reasoners knowledge base (unless the reasoner uses a fixed knowledge base)
    /// </remarks>
    public class OwlReasonerWrapper : StaticOwlReasonerWrapper
    {
        /// <summary>
        /// Creates a new OWL Reasoner Wrapper around the given OWL Reasoner
        /// </summary>
        /// <param name="reasoner">OWL Reasoner</param>
        public OwlReasonerWrapper(IOwlReasoner reasoner)
            : base(reasoner) { }

        /// <summary>
        /// Applies the reasoner to the given Graph outputting inferences into the same Graph
        /// </summary>
        /// <param name="g">Graph</param>
        public override void Apply(IGraph g)
        {
            this.Apply(g, g);
        }

        /// <summary>
        /// Applies the reasoner to the given input Graph outputting inferences into the output Graph
        /// </summary>
        /// <param name="input">Input Graph</param>
        /// <param name="output">Output Graph</param>
        public override void Apply(IGraph input, IGraph output)
        {
            this.Initialise(input);
            base.Apply(input, output);
        }
    }
}
