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
    /// Represents a SPARQL Update MOVE Command
    /// </summary>
    public class MoveCommand : BaseTransferCommand
    {
        /// <summary>
        /// Creates a Command which Moves data from one Graph to another overwriting the destination Graph and deleting the source Graph
        /// </summary>
        /// <param name="sourceUri">Source Graph URI</param>
        /// <param name="destUri">Destination Graph URI</param>
        /// <param name="silent">Whether errors should be suppressed</param>
        public MoveCommand(Uri sourceUri, Uri destUri, bool silent)
            : base(SparqlUpdateCommandType.Move, sourceUri, destUri, silent) { }

        /// <summary>
        /// Creates a Command which Moves data from one Graph to another overwriting the destination Graph and deleting the source Graph
        /// </summary>
        /// <param name="sourceUri">Source Graph URI</param>
        /// <param name="destUri">Destination Graph URI</param>
        public MoveCommand(Uri sourceUri, Uri destUri)
            : base(SparqlUpdateCommandType.Move, sourceUri, destUri) { }

        /// <summary>
        /// Evaluates the Command in the given Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        public override void Evaluate(SparqlUpdateEvaluationContext context)
        {
            try
            {
                // If Source and Destination are same this is a no-op
                if (EqualityHelper.AreUrisEqual(_sourceUri, _destUri)) return;

                if (context.Data.HasGraph(_sourceUri))
                {
                    // Get the Source Graph
                    IGraph source = context.Data.GetModifiableGraph(_sourceUri);

                    // Create/Delete/Clear the Destination Graph
                    IGraph dest;
                    if (context.Data.HasGraph(_destUri))
                    {
                        if (_destUri == null)
                        {
                            dest = context.Data.GetModifiableGraph(_destUri);
                            dest.Clear();
                        }
                        else
                        {
                            context.Data.RemoveGraph(_destUri);
                            dest = new Graph();
                            dest.BaseUri = _destUri;
                            context.Data.AddGraph(dest);
                            dest = context.Data.GetModifiableGraph(_destUri);
                        }
                    }
                    else
                    {
                        dest = new Graph();
                        dest.BaseUri = _destUri;
                        context.Data.AddGraph(dest);
                    }

                    // Move data from the Source into the Destination
                    dest.Merge(source);

                    // Delete/Clear the Source Graph
                    if (_sourceUri == null)
                    {
                        source.Clear();
                    }
                    else
                    {
                        context.Data.RemoveGraph(_sourceUri);
                    }
                }
                else
                {
                    // Only show error if not Silent
                    if (!_silent)
                    {
                        if (_sourceUri != null)
                        {
                            throw new SparqlUpdateException("Cannot MOVE from Graph <" + _sourceUri.AbsoluteUri + "> as it does not exist");
                        }
                        else
                        {
                            // This would imply a more fundamental issue with the Dataset not understanding that null means default graph
                            throw new SparqlUpdateException("Cannot MOVE from the Default Graph as it does not exist");
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
            processor.ProcessMoveCommand(this);
        }
    }
}
