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
            _reasoner = reasoner;
        }

        /// <summary>
        /// Applies the reasoner to the given Graph outputting inferences into the same Graph
        /// </summary>
        /// <param name="g">Graph</param>
        public virtual void Apply(IGraph g)
        {
            Apply(g, g);
        }

        /// <summary>
        /// Applies the reasoner to the given input Graph outputting inferences into the output Graph
        /// </summary>
        /// <param name="input">Input Graph</param>
        /// <param name="output">Output Graph</param>
        public virtual void Apply(IGraph input, IGraph output)
        {
            output.Assert(_reasoner.Extract(OwlHelper.OwlExtractMode.AllStatements));
        }

        /// <summary>
        /// Initialises the reasoner
        /// </summary>
        /// <param name="g">Graph to initialise with</param>
        public void Initialise(IGraph g)
        {
            _reasoner.Add(g);
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
            Apply(g, g);
        }

        /// <summary>
        /// Applies the reasoner to the given input Graph outputting inferences into the output Graph
        /// </summary>
        /// <param name="input">Input Graph</param>
        /// <param name="output">Output Graph</param>
        public override void Apply(IGraph input, IGraph output)
        {
            Initialise(input);
            base.Apply(input, output);
        }
    }
}
