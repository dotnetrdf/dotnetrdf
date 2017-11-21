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
using System.Linq;
using System.Text;

namespace VDS.RDF.Update.Commands
{
    /// <summary>
    /// Represents a SPARQL Update DROP command
    /// </summary>
    public class DropCommand : SparqlUpdateCommand
    {
        private Uri _graphUri;
        private ClearMode _mode = ClearMode.Graph;
        private bool _silent = false;

        /// <summary>
        /// Creates a new DROP command
        /// </summary>
        /// <param name="graphUri">URI ofthe Graph to DROP</param>
        /// <param name="mode">DROP Mode to use</param>
        /// <param name="silent">Whether the DROP should be done silently</param>
        public DropCommand(Uri graphUri, ClearMode mode, bool silent)
            : base(SparqlUpdateCommandType.Drop)
        {
            _graphUri = graphUri;
            _mode = mode;
            if (_graphUri == null && _mode == ClearMode.Graph) _mode = ClearMode.Default;
            if (_mode == ClearMode.Default) _graphUri = null;
            _silent = silent;
        }

        /// <summary>
        /// Creates a new DROP command
        /// </summary>
        /// <param name="graphUri">URI of the Graph to DROP</param>
        /// <param name="mode">DROP Mode to use</param>
        public DropCommand(Uri graphUri, ClearMode mode)
            : this(graphUri, mode, false) { }

        /// <summary>
        /// Creates a new DROP command
        /// </summary>
        /// <param name="graphUri">URI of the Graph to DROP</param>
        public DropCommand(Uri graphUri)
            : this(graphUri, ClearMode.Graph, false) { }

        /// <summary>
        /// Creates a new DROP command which drops the Default Graph
        /// </summary>
        public DropCommand()
            : this(null, ClearMode.Default) { }

        /// <summary>
        /// Creates a new DROP command which performs a specific clear mode drop operation
        /// </summary>
        /// <param name="mode">Clear Mode</param>
        public DropCommand(ClearMode mode)
            : this(mode, false) { }

        /// <summary>
        /// Creates a new DROP command which performs a specific clear mode drop operation
        /// </summary>
        /// <param name="mode">Clear Mode</param>
        /// <param name="silent">Whether errors should be suppressed</param>
        public DropCommand(ClearMode mode, bool silent)
            : this(null, mode, silent) { }

        /// <summary>
        /// Gets whether the Command affects a single Graph
        /// </summary>
        public override bool AffectsSingleGraph
        {
            get
            {
                return _mode == ClearMode.Graph || _mode == ClearMode.Default;
            }
        }

        /// <summary>
        /// Gets whether the Command affects a given Graph
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        public override bool AffectsGraph(Uri graphUri)
        {
            switch (_mode)
            {
                case ClearMode.All:
                    return true;
                case ClearMode.Default:
                    return graphUri == null;
                case ClearMode.Named:
                    return graphUri != null;
                case ClearMode.Graph:
                    if (_graphUri == null)
                    {
                        return true;
                    }
                    else
                    {
                        return _graphUri.AbsoluteUri.Equals(graphUri.ToSafeString());
                    }
                default:
                    // No Other Clear Modes but have to keep the compiler happy
                    return true;
            }
        }

        /// <summary>
        /// Gets the URI of the Graph to be dropped
        /// </summary>
        public Uri TargetUri
        {
            get
            {
                return _graphUri;
            }
        }

        /// <summary>
        /// Gets whether the Drop should be done silently
        /// </summary>
        public bool Silent
        {
            get
            {
                return _silent;
            }
        }

        /// <summary>
        /// Gets the type of DROP operation to perform
        /// </summary>
        public ClearMode Mode
        {
            get
            {
                return _mode;
            }
        }

        /// <summary>
        /// Evaluates the Command in the given Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        public override void Evaluate(SparqlUpdateEvaluationContext context)
        {
            try
            {
                switch (_mode)
                {
                    case ClearMode.Default:
                    case ClearMode.Graph:
                        if (!context.Data.HasGraph(_graphUri))
                        {
                            if (!_silent) throw new SparqlUpdateException("Cannot remove a Named Graph with URI '" + _graphUri.AbsoluteUri + "' since a Graph with this URI does not exist in the Store");
                        }
                        else
                        {
                            if (_mode == ClearMode.Graph)
                            {
                                context.Data.RemoveGraph(_graphUri);
                            }
                            else
                            {
                                // DROPing the DEFAULT graph only results in clearing it
                                // This is because removing the default graph may cause errors in later commands/queries
                                // which rely on it existing
                                context.Data.GetModifiableGraph(_graphUri).Clear();
                            }
                        }
                        break;

                    case ClearMode.Named:
                        foreach (Uri u in context.Data.GraphUris.ToList())
                        {
                            if (u != null)
                            {
                                context.Data.RemoveGraph(u);
                            }
                        }
                        break;
                    case ClearMode.All:
                        foreach (Uri u in context.Data.GraphUris.ToList())
                        {
                            if (u != null)
                            {
                                context.Data.RemoveGraph(u);
                            }
                            else
                            {
                                // DROPing the DEFAULT graph only results in clearing it
                                // This is because removing the default graph may cause errors in later commands/queries
                                // which rely on it existing
                                context.Data.GetModifiableGraph(u).Clear();
                            }
                        }
                        break;
                }
            }
            catch
            {
                if (!_silent) throw;
            }
        }

        /// <summary>
        /// Processes the Command using the given Update Processor
        /// </summary>
        /// <param name="processor">SPARQL Update Processor</param>
        public override void Process(ISparqlUpdateProcessor processor)
        {
            processor.ProcessDropCommand(this);
        }

        /// <summary>
        /// Gets the String representation of the command
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append("DROP ");
            if (_silent) output.Append("SILENT ");
            switch (_mode)
            {
                case ClearMode.All:
                    output.Append("ALL");
                    break;
                case ClearMode.Default:
                    output.Append("DEFAULT");
                    break;
                case ClearMode.Named:
                    output.Append("NAMED");
                    break;
                case ClearMode.Graph:
                    output.Append("GRAPH <");
                    output.Append(_graphUri.AbsoluteUri.Replace(">", "\\>"));
                    output.Append('>');
                    break;
            }
            return output.ToString();
        }

    }
}
