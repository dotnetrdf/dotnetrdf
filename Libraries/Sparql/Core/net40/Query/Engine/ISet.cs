/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Engine
{
    /// <summary>
    /// Interface for Sets which represents a possible solution during SPARQL evaluation
    /// </summary>
    public interface ISet 
        : IEquatable<ISet>
    {
        /// <summary>
        /// Adds a Value for a Variable to the Set
        /// </summary>
        /// <param name="variable">Variable</param>
        /// <param name="value">Value</param>
        void Add(string variable, INode value);

        /// <summary>
        /// Checks whether the Set contains a given Variable
        /// </summary>
        /// <param name="variable">Variable</param>
        /// <returns></returns>
        bool ContainsVariable(string variable);

        /// <summary>
        /// Gets whether the Set is compatible with a given set based on the given variables
        /// </summary>
        /// <param name="s">Set</param>
        /// <param name="vars">Variables</param>
        /// <returns></returns>
        bool IsCompatibleWith(ISet s, IEnumerable<String> vars);

        /// <summary>
        /// Gets whether the Set is minus compatible with a given set based on the given variables
        /// </summary>
        /// <param name="s">Set</param>
        /// <param name="vars">Variables</param>
        /// <returns></returns>
        bool IsMinusCompatibleWith(ISet s, IEnumerable<String> vars);

        /// <summary>
        /// Removes a Value for a Variable from the Set
        /// </summary>
        /// <param name="variable">Variable</param>
        void Remove(string variable);

        /// <summary>
        /// Retrieves the Value in this set for the given Variable
        /// </summary>
        /// <param name="variable">Variable</param>
        /// <returns>Either a Node or a null</returns>
        INode this[string variable] 
        { 
            get; 
        }

        /// <summary>
        /// Gets the Values in the Set
        /// </summary>
        IEnumerable<INode> Values 
        { 
            get;
        }

        /// <summary>
        /// Gets the Variables in the Set
        /// </summary>
        IEnumerable<string> Variables 
        { 
            get;
        }

        /// <summary>
        /// Gets whether the set is empty
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Joins the set to another set
        /// </summary>
        /// <param name="other">Other Set</param>
        /// <returns></returns>
        ISet Join(ISet other);

        /// <summary>
        /// Copies the Set
        /// </summary>
        /// <returns></returns>
        ISet Copy();
    }
}
