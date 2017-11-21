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
using VDS.RDF.Query.Optimisation;

namespace VDS.RDF.Update
{
    /// <summary>
    /// Possible SPARQL Update Command Types
    /// </summary>
    public enum SparqlUpdateCommandType
    {
        /// <summary>
        /// Command inserts data
        /// </summary>
        InsertData,
        /// <summary>
        /// Command deletes data
        /// </summary>
        DeleteData,
        /// <summary>
        /// Command inserts data and may be based upon a template
        /// </summary>
        Insert,
        /// <summary>
        /// Command deletes data and may be based upon a template
        /// </summary>
        Delete,
        /// <summary>
        /// Command modifies data
        /// </summary>
        Modify,
        /// <summary>
        /// Command loads a graph into the Store
        /// </summary>
        Load,
        /// <summary>
        /// Command clears a graph in the Store
        /// </summary>
        Clear,
        /// <summary>
        /// Command creates a Graph in the Store
        /// </summary>
        Create,
        /// <summary>
        /// Command removes a Graph from the Store
        /// </summary>
        Drop,
        /// <summary>
        /// Command which merges the data from one Graph into another
        /// </summary>
        Add,
        /// <summary>
        /// Command which copies the data from one Graph into another overwriting the destination Graph
        /// </summary>
        Copy,
        /// <summary>
        /// Command which moves data from one Graph to another overwriting the destination Graph and deleting the Source Graph
        /// </summary>
        Move,
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown
    }

    /// <summary>
    /// Base Class of SPARQL Update Commands
    /// </summary>
    public abstract class SparqlUpdateCommand
    {
        private SparqlUpdateCommandType _type = SparqlUpdateCommandType.Unknown;

        /// <summary>
        /// Creates a new SPARQL Update Command
        /// </summary>
        /// <param name="type">Command Type</param>
        public SparqlUpdateCommand(SparqlUpdateCommandType type)
        {
            _type = type;
        }

        /// <summary>
        /// Gets the Type of this Command
        /// </summary>
        public SparqlUpdateCommandType CommandType
        {
            get
            {
                return _type;
            }
        }

        /// <summary>
        /// Gets whether the Command will only affect a single Graph
        /// </summary>
        public abstract bool AffectsSingleGraph
        {
            get;
        }

        /// <summary>
        /// Gets whether the Command will potentially affect the given Graph
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        /// <remarks>
        /// A return value of <strong>true</strong> does not guarantee that the Graph will be affected.  Some Commands (e.g. DROP ALL) affect all Graphs in the Dataset but the command itself doesn't know whether a Graph with the given URI is actually present in the dataset to which it is applied
        /// </remarks>
        public abstract bool AffectsGraph(Uri graphUri);

        /// <summary>
        /// Optimises the Command
        /// </summary>
        public virtual void Optimise(IQueryOptimiser optimiser)
        {
            // Does Nothing by Default
        }

        /// <summary>
        /// Evaluates the Command in the given Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        public abstract void Evaluate(SparqlUpdateEvaluationContext context);

        /// <summary>
        /// Processes the Command Set using the given Update Processor
        /// </summary>
        /// <param name="processor">Update Processor</param>
        public abstract void Process(ISparqlUpdateProcessor processor);

        /// <summary>
        /// Gets the String representation of the Command
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();
    }
}
