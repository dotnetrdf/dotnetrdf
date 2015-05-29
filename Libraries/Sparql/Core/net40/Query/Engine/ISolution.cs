/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

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
    /// Interface which represents a possible solution during SPARQL evaluation
    /// </summary>
    public interface ISolution 
        : IEquatable<ISolution>
    {
        /// <summary>
        /// Adds a Value for a Variable to the solution
        /// </summary>
        /// <param name="variable">Variable</param>
        /// <param name="value">Value</param>
        void Add(string variable, INode value);

        /// <summary>
        /// Checks whether the solution contains a given Variable
        /// </summary>
        /// <param name="variable">Variable</param>
        /// <returns></returns>
        bool ContainsVariable(string variable);

        /// <summary>
        /// Gets whether the solution is compatible with a given solution based on the given variables
        /// </summary>
        /// <param name="s">Solution</param>
        /// <param name="vars">Variables</param>
        /// <returns></returns>
        bool IsCompatibleWith(ISolution s, IEnumerable<String> vars);

        /// <summary>
        /// Gets whether the Solution is minus compatible with a given solution based on the given variables
        /// </summary>
        /// <param name="s">Solution</param>
        /// <param name="vars">Variables</param>
        /// <returns></returns>
        bool IsMinusCompatibleWith(ISolution s, IEnumerable<String> vars);

        /// <summary>
        /// Removes a Value for a Variable from the solution
        /// </summary>
        /// <param name="variable">Variable</param>
        void Remove(string variable);

        /// <summary>
        /// Retrieves the Value in this solution for the given Variable
        /// </summary>
        /// <param name="variable">Variable</param>
        /// <returns>Either a Node or a null</returns>
        INode this[string variable] 
        { 
            get; 
        }

        /// <summary>
        /// Gets the Values in the solution
        /// </summary>
        IEnumerable<INode> Values 
        { 
            get;
        }

        /// <summary>
        /// Gets the Variables in the solution
        /// </summary>
        IEnumerable<string> Variables 
        { 
            get;
        }

        /// <summary>
        /// Gets whether the solution is empty
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Joins the solution to another solution
        /// </summary>
        /// <param name="other">Other solution</param>
        /// <returns></returns>
        ISolution Join(ISolution other);

        /// <summary>
        /// Copies the solution
        /// </summary>
        /// <returns></returns>
        ISolution Copy();

        /// <summary>
        /// Copies the solution only including the specified variables
        /// </summary>
        /// <returns></returns>
        ISolution Project(IEnumerable<String> vars);
    }
}
