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

namespace VDS.RDF.Query.Paths
{
    /// <summary>
    /// Class representing a potential path used during the evaluation of complex property paths
    /// </summary>
    public class PotentialPath
    {
        private INode _start, _current;
        private bool _complete = false, _deadend = false, _partial = false;
        private int _length = 1;

        /// <summary>
        /// Creates a new Potential Path
        /// </summary>
        /// <param name="start">Start Point of the Path</param>
        /// <param name="current">Current Point on the Path</param>
        public PotentialPath(INode start, INode current)
        {
            _start = start;
            _current = current;
        }

        /// <summary>
        /// Creates a new Potential Path which is a copy of an existing Path
        /// </summary>
        /// <param name="p">Potentuak Path</param>
        public PotentialPath(PotentialPath p)
        {
            _start = p.Start;
            _current = p.Current;
            _complete = p.IsComplete;
            _deadend = p.IsDeadEnd;
            _partial = p.IsPartial;
            _length = p.Length;
        }

        /// <summary>
        /// Gets the Start of the Path
        /// </summary>
        public INode Start
        {
            get
            {
                return _start;
            }
        }

        /// <summary>
        /// Gets/Sets the Current Point of the Path - in the case of a complete Path this is the end of the Path
        /// </summary>
        public INode Current
        {
            get
            {
                return _current;
            }
        }

        /// <summary>
        /// Gets/Sets whether the Path is complete
        /// </summary>
        public bool IsComplete
        {
            get
            {
                return _complete;
            }
            set
            {
                if (value) _complete = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether the Path is a dead-end
        /// </summary>
        /// <remarks>
        /// <para>
        /// This may be useful information as it can help stop us uneccessarily regenerating partial paths which are dead ends
        /// </para>
        /// </remarks>
        public bool IsDeadEnd
        {
            get
            {
                return _deadend;
            }
            set
            {
                if (value) _deadend = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether the Path is Partial
        /// </summary>
        /// <remarks>
        /// While this may seem something of a misnomer what this represents is that the path is only part of the overall path so in the case of a sequence path we'll make all incomplete paths from the first part of the sequence as partial so they can't be themselves completed but they can be used to form complete paths
        /// </remarks>
        public bool IsPartial
        {
            get
            {
                return _partial;
            }
            set
            {
                _partial = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Length of the Path
        /// </summary>
        public int Length
        {
            get
            {
                return _length;
            }
            set
            {
                _length = value;
            }
        }

        /// <summary>
        /// Gets the Hash Code for the potential path
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Tools.CombineHashCodes(_start, _current);
        }

        /// <summary>
        /// Checks whether the other object is an equivalent potential path
        /// </summary>
        /// <param name="obj">Object to test</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is PotentialPath)
            {
                PotentialPath other = (PotentialPath)obj;
                return (_start.Equals(other.Start) && _current.Equals(other.Current));
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the String representation of the path
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _start.ToString() + " -> " + _current.ToString();
        }
    }
}
