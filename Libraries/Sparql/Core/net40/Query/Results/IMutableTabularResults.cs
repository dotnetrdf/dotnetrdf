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

namespace VDS.RDF.Query.Results
{
    /// <summary>
    /// Represents tabular results that are mutable i.e. may be freely modified by the user
    /// </summary>
    public interface IMutableTabularResults
        : ITabularResults, IList<IMutableResultRow>, IEquatable<IMutableTabularResults>
    {
        /// <summary>
        /// Adds a variable to the results, the variable is added only to the <see cref="ITabularResults.Variables"/> enumeration and not to individual result rows, use the overload <see cref="AddVariable(string, INode)"/> if you wish to add the variable to individual rows
        /// </summary>
        /// <param name="var">Variable</param>
        void AddVariable(String var);

        /// <summary>
        /// Adds a variable to the results adding it to both the <see cref="ITabularResults.Variables"/> enumeration and the individual rows assigned them the given initial value
        /// </summary>
        /// <param name="var">Variable</param>
        /// <param name="initialValue">Initial value to assign to each row</param>
        void AddVariable(String var, INode initialValue);
    }
}