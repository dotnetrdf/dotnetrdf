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

namespace VDS.RDF.Update.Commands
{
    /// <summary>
    /// Represents the SPARQL Update ADD Command
    /// </summary>
    public class AddCommand 
        : BaseTransferCommand
    {
        /// <summary>
        /// Creates a Command which merges the data from the Source Graph into the Destination Graph
        /// </summary>
        /// <param name="sourceUri">Source Graph URI</param>
        /// <param name="destUri">Destination Graph URI</param>
        /// <param name="silent">Whether errors should be suppressed</param>
        public AddCommand(Uri sourceUri, Uri destUri, bool silent)
            : base(SparqlUpdateCommandType.Add, sourceUri, destUri, silent) { }

        /// <summary>
        /// Creates a Command which merges the data from the Source Graph into the Destination Graph
        /// </summary>
        /// <param name="sourceUri">Source Graph URI</param>
        /// <param name="destUri">Destination Graph URI</param>
        public AddCommand(Uri sourceUri, Uri destUri)
            : base(SparqlUpdateCommandType.Add, sourceUri, destUri) { }

        /// <summary>
        /// Evaluates the Command in the given Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        public override void Evaluate(SparqlUpdateEvaluationContext context)
        {
            try
            {
                if (context.Data.HasGraph(_sourceUri))
                {
                    // Get the Source Graph
                    IGraph source = context.Data.GetModifiableGraph(_sourceUri);

                    // Get the Destination Graph
                    IGraph dest;
                    if (!context.Data.HasGraph(_destUri))
                    {
                        dest = new Graph();
                        dest.BaseUri = _destUri;
                        context.Data.AddGraph(dest);
                    }
                    dest = context.Data.GetModifiableGraph(_destUri);

                    // Move data from the Source into the Destination
                    dest.Merge(source);
                }
                else
                {
                    // Only show error if not Silent
                    if (!_silent)
                    {
                        if (_sourceUri != null)
                        {
                            throw new SparqlUpdateException("Cannot ADD from Graph <" + _sourceUri.AbsoluteUri + "> as it does not exist");
                        }
                        else
                        {
                            // This would imply a more fundamental issue with the Dataset not understanding that null means default graph
                            throw new SparqlUpdateException("Cannot ADD from the Default Graph as it does not exist");
                        }
                    }
                }
            }
            catch
            {
                // If not silent throw the exception upwards
                if (!_silent) throw;
            }
        }

        /// <summary>
        /// Processes the Command using the given Update Processor
        /// </summary>
        /// <param name="processor">SPARQL Update Processor</param>
        public override void Process(ISparqlUpdateProcessor processor)
        {
            processor.ProcessAddCommand(this);
        }
    }
}
