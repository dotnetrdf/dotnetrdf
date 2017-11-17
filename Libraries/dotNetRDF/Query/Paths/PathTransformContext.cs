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

using System.Collections.Generic;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Paths
{
    /// <summary>
    /// Transform Context class that is used in the Path to Algebra Transformation process
    /// </summary>
    public class PathTransformContext
    {
        private List<ITriplePattern> _patterns = new List<ITriplePattern>();
        private int _nextID = 0;
        private PatternItem _currSubj, _currObj, _start, _end;
        private bool _top = true;

        /// <summary>
        /// Creates a new Path Transform Context
        /// </summary>
        /// <param name="start">Subject that is the start of the Path</param>
        /// <param name="end">Object that is the end of the Path</param>
        public PathTransformContext(PatternItem start, PatternItem end)
        {
            _start = start;
            _currSubj = start;
            _end = end;
            _currObj = end;
        }

        /// <summary>
        /// Creates a new Path Transform Context from an existing context
        /// </summary>
        /// <param name="context">Context</param>
        public PathTransformContext(PathTransformContext context)
        {
            _start = context._start;
            _end = context._end;
            _currSubj = context._currSubj;
            _currObj = context._currObj;
            _nextID = context._nextID;
        }

        /// <summary>
        /// Returns the BGP that the Path Transform produces
        /// </summary>
        /// <returns></returns>
        public ISparqlAlgebra ToAlgebra()
        {
            if (_patterns.Count > 0)
            {
                return new Bgp(_patterns);
            }
            else
            {
                throw new RdfQueryException("Unexpected Error: Path Transform returned no Patterns");
            }
        }

        /// <summary>
        /// Gets the next available temporary variable
        /// </summary>
        /// <returns></returns>
        public BlankNodePattern GetNextTemporaryVariable()
        {
            _nextID++;
            return new BlankNodePattern("sparql-path-autos-" + _nextID);
        }

        /// <summary>
        /// Adds a Triple Pattern to the Path Transform
        /// </summary>
        /// <param name="p">Triple Pattern</param>
        public void AddTriplePattern(ITriplePattern p)
        {
            _patterns.Add(p);
        }

        /// <summary>
        /// Gets the Next ID to be used
        /// </summary>
        public int NextID
        {
            get
            {
                return _nextID;
            }
            set
            {
                _nextID = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Subject of the Triple Pattern at this point in the Path Transformation
        /// </summary>
        public PatternItem Subject
        {
            get
            {
                return _currSubj;
            }
            set
            {
                _currSubj = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Object of the Triple Pattern at this point in the Path Transformation
        /// </summary>
        public PatternItem Object
        {
            get
            {
                return _currObj;
            }
            set
            {
                _currObj = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Object at the end of the Pattern
        /// </summary>
        public PatternItem End
        {
            get
            {
                return _end;
            }
            set
            {
                _end = value;
            }
        }

        /// <summary>
        /// Resets the current Object to be the end Object of the Path
        /// </summary>
        public void ResetObject()
        {
            _currObj = _end;
        }

        /// <summary>
        /// Gets/Sets whether this is the Top Level Pattern
        /// </summary>
        public bool Top
        {
            get
            {
                return _top;
            }
            set
            {
                _top = value;
            }
        }

        /// <summary>
        /// Creates a Triple Pattern
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="path">Property Path</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public ITriplePattern GetTriplePattern(PatternItem subj, ISparqlPath path, PatternItem obj)
        {
            if (path is Property)
            {
                NodeMatchPattern nodeMatch = new NodeMatchPattern(((Property)path).Predicate, true);
                return new TriplePattern(subj, nodeMatch, obj);
            }
            else
            {
                return new PropertyPathPattern(subj, path, obj);
            }
        }
    }
}
