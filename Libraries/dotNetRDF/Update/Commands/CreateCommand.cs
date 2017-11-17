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
using System.Text;

namespace VDS.RDF.Update.Commands
{
    /// <summary>
    /// Represents the SPARQL Update CREATE command
    /// </summary>
    public class CreateCommand : SparqlUpdateCommand
    {
        private Uri _graphUri;
        private bool _silent = false;

        /// <summary>
        /// Creates a new CREATE command
        /// </summary>
        /// <param name="graphUri">URI of the Graph to create</param>
        /// <param name="silent">Whether the create should be done silenty</param>
        public CreateCommand(Uri graphUri, bool silent)
            : base(SparqlUpdateCommandType.Create) 
        {
            if (graphUri == null) throw new ArgumentNullException("graphUri");
            _graphUri = graphUri;
            _silent = silent;
        }

        /// <summary>
        /// Creates a new CREATE command
        /// </summary>
        /// <param name="graphUri">URI of the Graph to create</param>
        public CreateCommand(Uri graphUri)
            : this(graphUri, false) { }

        /// <summary>
        /// Gets whether the Command affects a Single Graph
        /// </summary>
        public override bool AffectsSingleGraph
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets whether the Command affects a given Graph
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        public override bool AffectsGraph(Uri graphUri)
        {
            if (_graphUri == null)
            {
                return true;
            }
            else
            {
                return _graphUri.AbsoluteUri.Equals(graphUri.ToSafeString());
            }
        }

        /// <summary>
        /// Gets the URI of the Graph to be created
        /// </summary>
        public Uri TargetUri
        {
            get
            {
                return _graphUri;
            }
        }

        /// <summary>
        /// Gets whether the Create should be done silently
        /// </summary>
        public bool Silent
        {
            get
            {
                return _silent;
            }
        }

        /// <summary>
        /// Evaluates the Command in the given Context
        /// </summary>
        /// <param name="context">Update Evaluation Context</param>
        public override void Evaluate(SparqlUpdateEvaluationContext context)
        {
            if (context.Data.HasGraph(_graphUri))
            {
                if (!_silent) throw new SparqlUpdateException("Cannot create a Named Graph with URI '" + _graphUri.AbsoluteUri + "' since a Graph with this URI already exists in the Store");
            }
            else
            {
                Graph g = new Graph();
                g.BaseUri = _graphUri;
                context.Data.AddGraph(g);
            }
        }

        /// <summary>
        /// Processes the Command using the given Update Processor
        /// </summary>
        /// <param name="processor">SPARQL Update Processor</param>
        public override void Process(ISparqlUpdateProcessor processor)
        {
            processor.ProcessCreateCommand(this);
        }

        /// <summary>
        /// Gets the String representation of the Command
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append("CREATE ");
            if (_silent) output.Append("SILENT ");
            output.Append("GRAPH <");
            output.Append(_graphUri.AbsoluteUri.Replace(">", "\\>"));
            output.Append('>');
            return output.ToString();
        }
    }
}
