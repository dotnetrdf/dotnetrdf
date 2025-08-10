/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Optimisation;

namespace VDS.RDF.Update;

/// <summary>
/// Represents a sequence of SPARQL Update Commands to be executed on a Dataset.
/// </summary>
public class SparqlUpdateCommandSet
{
    private List<SparqlUpdateCommand> _commands = new List<SparqlUpdateCommand>();
    private NamespaceMapper _nsmap = new NamespaceMapper(true);
    private Uri _baseUri;
    private long _timeout = 0;
    private TimeSpan? _executionTime = null;
    private IEnumerable<IAlgebraOptimiser> _optimisers = Enumerable.Empty<IAlgebraOptimiser>();

    /// <summary>
    /// Creates a new empty Command Set.
    /// </summary>
    public SparqlUpdateCommandSet()
    {

    }

    /// <summary>
    /// Creates a new Command Set containing the given Command.
    /// </summary>
    /// <param name="command">Command.</param>
    public SparqlUpdateCommandSet(SparqlUpdateCommand command)
    {
        _commands.Add(command);
    }

    /// <summary>
    /// Creates a new Command Set with the given Commands.
    /// </summary>
    /// <param name="commands">Commands.</param>
    public SparqlUpdateCommandSet(IEnumerable<SparqlUpdateCommand> commands)
    {
        _commands.AddRange(commands);
    }

    /// <summary>
    /// Adds a new Command to the end of the sequence of Commands.
    /// </summary>
    /// <param name="command">Command to add.</param>
    internal void AddCommand(SparqlUpdateCommand command)
    {
        _commands.Add(command);
    }

    /// <summary>
    /// Gets the Command at the given index.
    /// </summary>
    /// <param name="index">Index.</param>
    /// <returns></returns>
    public SparqlUpdateCommand this[int index]
    {
        get
        {
            if (index < 0 || index >= _commands.Count)
            {
                if (_commands.Count > 0)
                {
                    throw new IndexOutOfRangeException(index + " is not a valid index into the Command Set, the Set contains " + _commands.Count + " Commands so only indexes in the range 0-" + (_commands.Count - 1) + " are valid");
                }
                else
                {
                    throw new IndexOutOfRangeException(index + " is not a valid index into the Command Set since it is an empty Command Set");
                }
            }
            else
            {
                return _commands[index];
            }
        }
    }

    /// <summary>
    /// Gets the number of Commands in the set.
    /// </summary>
    public int CommandCount
    {
        get
        {
            return _commands.Count;
        }
    }

    /// <summary>
    /// Gets the enumeration of Commands in the set.
    /// </summary>
    public IEnumerable<SparqlUpdateCommand> Commands
    {
        get
        {
            return _commands;
        }
    }

    /// <summary>
    /// Gets/Sets the Base URI for the Command Set.
    /// </summary>
    public Uri BaseUri
    {
        get
        {
            return _baseUri;
        }
        internal set
        {
            _baseUri = value;
        }
    }

    /// <summary>
    /// Gets the Namespace Map for the Command Set.
    /// </summary>
    public NamespaceMapper NamespaceMap
    {
        get
        {
            return _nsmap;
        }
    }

    /// <summary>
    /// Gets/Sets the Timeout in milliseconds for the execution of the Updates.
    /// </summary>
    /// <remarks>Default is no timeout.</remarks>
    public long Timeout
    {
        get
        {
            return _timeout;
        }
        set
        {
            _timeout = Math.Max(value, 0);
        }
    }

    /// <summary>
    /// Gets/Sets the Time the updates took to execute.
    /// </summary>
    /// <remarks>Returns NULL if no execution time has been set yet (e.g. if the command set has not yet been processed).</remarks>
    public TimeSpan? UpdateExecutionTime
    {
        get => _executionTime;
        set => _executionTime = value;
    }

    /// <summary>
    /// Gets/Sets the Algebra Optimisers to be applied to portions of updates that require queries to be made.
    /// </summary>
    public IEnumerable<IAlgebraOptimiser> AlgebraOptimisers
    {
        get
        {
            return _optimisers;
        }
        set
        {
            if (value == null)
            {
                _optimisers = Enumerable.Empty<IAlgebraOptimiser>();
            }
            else
            {
                _optimisers = value;
            }
        }
    }

    /// <summary>
    /// Optimises the Commands in the Command Set.
    /// </summary>
    /// <param name="optimiser">The query optimiser to apply. If this parameter is null or omitted, the query optimiser from <see cref="SparqlOptimiser.Default"/> is used.</param>
    public void Optimise(IQueryOptimiser optimiser = null)
    {
        optimiser ??= SparqlOptimiser.Default.QueryOptimiser;
        foreach (SparqlUpdateCommand c in _commands)
        {
            c.Optimise(optimiser);
        }
    }

    /// <summary>
    /// Processes the Command Set using the given Update Processor.
    /// </summary>
    /// <param name="processor">Update Processor.</param>
    public void Process(ISparqlUpdateProcessor processor)
    {
        processor.ProcessCommandSet(this);
    }

    /// <summary>
    /// Apply algebra optimisers to the specified algebra, returning the optimised algebra.
    /// </summary>
    /// <param name="algebra">The algebra to be optimised.</param>
    /// <param name="globalOptimisers">Additional optimisers to apply after applying all of the local optimisers specified by <see cref="AlgebraOptimisers"/>.</param>
    /// <returns>The optimised algebra.</returns>
    /// <remarks>If the <paramref name="globalOptimisers"/> is null or not specified, the default algebra optimisers defined by <see cref="SparqlOptimiser.Default"/> are applied.</remarks>
    public ISparqlAlgebra ApplyAlgebraOptimisers(ISparqlAlgebra algebra, IEnumerable<IAlgebraOptimiser> globalOptimisers = null)
    {
        try
        {
            // Apply Local Optimisers
            foreach (IAlgebraOptimiser opt in _optimisers.Where(o => o.IsApplicable(this)))
            {
                try
                {
                    algebra = opt.Optimise(algebra);
                }
                catch
                {
                    // Ignore errors - if an optimiser errors then we leave the algebra unchanged
                }
            }
            // Apply Global Optimisers
            globalOptimisers ??= SparqlOptimiser.Default.AlgebraOptimisers;
            foreach (IAlgebraOptimiser opt in globalOptimisers.Where(o => o.IsApplicable(this)))
            {
                try
                {
                    algebra = opt.Optimise(algebra);
                }
                catch
                {
                    // Ignore errors - if an optimiser errors then we leave the algebra unchanged
                }
            }
            return algebra;
        }
        catch
        {
            return algebra;
        }
    }

    /// <summary>
    /// Gets the String representation of the Command Set.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var output = new StringBuilder();

        if (_baseUri != null)
        {
            output.AppendLine("BASE <" + _baseUri.AbsoluteUri.Replace(">", "\\>") + ">");
        }

        foreach (var prefix in _nsmap.Prefixes)
        {
            output.Append("PREFIX ");
            output.Append(prefix);
            output.Append(": <");
            output.Append(_nsmap.GetNamespaceUri(prefix).AbsoluteUri.Replace(">", "\\>"));
            output.AppendLine(">");
        }

        for (var i = 0; i < _commands.Count; i++)
        {
            output.Append(_commands[i]);
            if (i < _commands.Count - 1)
            {
                output.AppendLine(";");
            }
        }
        return output.ToString();
    }
}
