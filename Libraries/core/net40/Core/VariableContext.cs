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
using System.Linq;
using System.Text;

namespace VDS.RDF
{
    /// <summary>
    /// Possible Variable Context Types
    /// </summary>
    public enum VariableContextType
    {
        /// <summary>
        /// There is currently no variable context
        /// </summary>
        None,
        /// <summary>
        /// Existential Variable Context
        /// </summary>
        Existential,
        /// <summary>
        /// Universal Variable Context
        /// </summary>
        Universal
    }

    /// <summary>
    /// Represents the Variable Context for Triples
    /// </summary>
    public class VariableContext : BasicTripleContext
    {
        private VariableContextType _type;
        private HashSet<INode> _vars = new HashSet<INode>();
        private VariableContext _innerContext;

        /// <summary>
        /// Creates a new Variable Context
        /// </summary>
        /// <param name="type">Context Type</param>
        public VariableContext(VariableContextType type)
        {
            this._type = type;
        }

        /// <summary>
        /// Gets the Context Type
        /// </summary>
        public VariableContextType Type
        {
            get
            {
                return this._type;
            }
        }

        /// <summary>
        /// Gets the Variables in this Context
        /// </summary>
        public IEnumerable<INode> Variables
        {
            get
            {
                return this._vars;
            }
        }

        /// <summary>
        /// Adds a Variable to this Context
        /// </summary>
        /// <param name="var">Variable</param>
        public void AddVariable(INode var)
        {
            if (var == null) return;

            if (this._innerContext == null)
            {
                this._vars.Add(var);
            }
            else
            {
                this._innerContext.AddVariable(var);
            }
        }

        /// <summary>
        /// Gets whether a given Variable exists in this Context
        /// </summary>
        /// <param name="var">Variable Node</param>
        /// <returns></returns>
        public bool IsVariable(INode var)
        {
            if (this.InnerContext == null)
            {
                return this._vars.Contains(var);
            }
            else
            {
                return this._vars.Contains(var) || this.InnerContext.IsVariable(var);
            }
        }

        /// <summary>
        /// Gets/Sets the Inner Context
        /// </summary>
        /// <remarks>
        /// When you set the Inner Context this sets the Inner Context of the most nested inner context, you can remove all nested contexts by setting this to null
        /// </remarks>
        public VariableContext InnerContext
        {
            get
            {
                return this._innerContext;
            }
            set
            {
                if (value == null || this._innerContext == null)
                {
                    this._innerContext = value;
                }
                else 
                {
                    this._innerContext.InnerContext = value;
                }
            }
        }
    }
}
