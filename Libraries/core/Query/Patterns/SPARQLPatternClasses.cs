/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Patterns
{
    /// <summary>
    /// Class for representing Node Patterns in Sparql Queries
    /// </summary>
    public abstract class PatternItem
    {
        /// <summary>
        /// Binding Context for Pattern Item
        /// </summary>
        protected SparqlResultBinder _context = null;

        private bool _repeated = false;

        /// <summary>
        /// Checks whether the Pattern Item accepts the given Node in the given Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="obj">Node to test</param>
        /// <returns></returns>
        protected internal abstract bool Accepts(SparqlEvaluationContext context, INode obj);

        /// <summary>
        /// Constructs a Node based on this Pattern for the given Set
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="s">Set</param>
        /// <param name="preserveBNodes">Whether Blank Node IDs should be preserved</param>
        /// <returns></returns>
        protected internal abstract INode Construct(IGraph g, Set s, bool preserveBNodes);

        /// <summary>
        /// Sets the Binding Context for the Pattern Item
        /// </summary>
        public SparqlResultBinder BindingContext
        {
            set
            {
                this._context = value;
            }
        }

        /// <summary>
        /// Gets the String representation of the Pattern
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();

        /// <summary>
        /// Gets the Variable Name if this is a Variable Pattern or null otherwise
        /// </summary>
        public virtual String VariableName
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets/Sets whether the Variable is repeated in the Pattern
        /// </summary>
        public virtual bool Repeated
        {
            get
            {
                return this._repeated;
            }
            set
            {
                this._repeated = value;
            }
        }

    }

    #region Concrete Pattern Implementations

    /// <summary>
    /// Pattern which matches Variables
    /// </summary>
    public class VariablePattern : PatternItem
    {
        private String _varname;

        /// <summary>
        /// Creates a new Variable Pattern
        /// </summary>
        /// <param name="name">Variable name including the leading ?/$</param>
        public VariablePattern(String name)
        {
            this._varname = name.Substring(1);
        }

        /// <summary>
        /// Checks whether the given Node is a valid value for the Variable in the current Binding Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="obj">Node to test</param>
        /// <returns></returns>
        protected internal override bool Accepts(SparqlEvaluationContext context, INode obj)
        {
            if (context.InputMultiset.ContainsVariable(this._varname))
            {
                return context.InputMultiset.ContainsValue(this._varname, obj);
            }
            else if (this.Repeated)
            {
                return true;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Constructs a Node based on the given Set
        /// </summary>
        /// <param name="s">Set</param>
        /// <param name="preserveBNodes">Whether BNode IDs should be preserved</param>
        /// <returns>The Node which is bound to this Variable in this Solution</returns>
        protected internal override INode Construct(IGraph g, Set s, bool preserveBNodes)
        {
            INode temp = s[this._varname];
            return this.ConstructInternal(g, temp, preserveBNodes);               
        }

        /// <summary>
        /// Constructs a Node based on the given Node
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="value">Node</param>
        /// <param name="preserveBNodes">Whether BNode IDs should be preserved</param>
        /// <returns></returns>
        /// <remarks>
        /// Adjusts the ID of Blank Nodes appropriately
        /// </remarks>
        private INode ConstructInternal(IGraph g, INode value, bool preserveBNodes)
        {
            if (value == null) throw new RdfQueryException("Unable to construct a Value for this Variable for this solution as it is bound to a null");
            switch (value.NodeType)
            {
                case NodeType.Blank:
                    if (!preserveBNodes && value.GraphUri != null)
                    {
                        //Rename Blank Node based on the Graph Uri Hash Code
                        int hash = value.GraphUri.GetEnhancedHashCode();
                        if (hash >= 0)
                        {
                            return new BlankNode(null, ((BlankNode)value).InternalID + "-" + value.GraphUri.GetEnhancedHashCode());
                        }
                        else
                        {
                            return new BlankNode(null, ((BlankNode)value).InternalID + hash);
                        }
                    }
                    else
                    {
                        return new BlankNode(g, ((BlankNode)value).InternalID);
                    }

                default:
                    return value;
            }
        }

        /// <summary>
        /// Gets the String representation of this pattern
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "?" + this._varname;
        }

        /// <summary>
        /// Gets the Name of the Variable this Pattern matches
        /// </summary>
        public override string VariableName
        {
            get
            {
                return this._varname;
            }
        }
    }

    /// <summary>
    /// Pattern which matches specific Nodes
    /// </summary>
    public class NodeMatchPattern : PatternItem
    {
        private INode _node;

        /// <summary>
        /// Creates a new Node Match Pattern
        /// </summary>
        /// <param name="n">Exact Node to match</param>
        public NodeMatchPattern(INode n)
        {
            this._node = n;
        }

        /// <summary>
        /// Checks whether the given Node matches the Node this pattern was instantiated with
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="obj">Node to test</param>
        /// <returns></returns>
        protected internal override bool Accepts(SparqlEvaluationContext context, INode obj)
        {
            return this._node.Equals(obj);
        }

        /// <summary>
        /// Constructs a Node based on the given Set
        /// </summary>
        /// <param name="s">Set</param>
        /// <returns>The Node this pattern matches on</returns>
        /// <param name="preserveBNodes">Whether Blank Node IDs should be preserved</param>
        protected internal override INode Construct(IGraph g, Set s, bool preserveBNodes)
        {
            return this._node.CopyNode(g);
        }

        /// <summary>
        /// Gets a String representation of the Node
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();

            switch (this._node.NodeType) {
                case NodeType.Literal:
                    LiteralNode lit = (LiteralNode)this._node;
                    bool longlit = (lit.Value.Contains('\n') || lit.Value.Contains('\r') || lit.Value.Contains('"'));

                    if (longlit)
                    {
                        output.Append("\"\"\"");
                    }
                    else
                    {
                        output.Append("\"");
                    }

                    output.Append(lit.Value);

                    if (longlit)
                    {
                        output.Append("\"\"\"");
                    }
                    else
                    {
                        output.Append("\"");
                    }

                    if (!lit.Language.Equals(String.Empty))
                    {
                        output.Append("@");
                        output.Append(lit.Language);
                    }
                    else if (lit.DataType != null)
                    {
                        output.Append("^^<");
                        output.Append(lit.DataType.ToString());
                        output.Append(">");
                    }

                    break;

                case NodeType.Uri:
                    UriNode uri = (UriNode)this._node;
                    output.Append('<');
                    output.Append(this._node.ToString());
                    output.Append('>');
                    break;

                default:
                    output.Append(this._node.ToString());
                    break;
            }

            return output.ToString();
        }

        /// <summary>
        /// Gets the Node that this Pattern matches
        /// </summary>
        public INode Node
        {
            get
            {
                return this._node;
            }
        }
    }

    /// <summary>
    /// Pattern which matches Blank Nodes
    /// </summary>
    public class BlankNodePattern : PatternItem
    {
        private String _name;

        /// <summary>
        /// Creates a new Pattern representing a Blank Node
        /// </summary>
        /// <param name="name">Blank Node ID</param>
        public BlankNodePattern(String name)
        {
            this._name = "_:" + name;
        }

        /// <summary>
        /// Gets the Blank Node ID
        /// </summary>
        public String ID
        {
            get
            {
                return this._name;
            }
        }

        /// <summary>
        /// Checks whether the given Node is a valid value for the Temporary Variable
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="obj">Node to test</param>
        /// <returns></returns>
        protected internal override bool Accepts(SparqlEvaluationContext context, INode obj)
        {
            if (context.InputMultiset.ContainsVariable(this._name))
            {
                return context.InputMultiset.ContainsValue(this._name, obj);
            }
            else if (this.Repeated)
            {
                return true;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Constructs a Node based on the given Set
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="s">Set</param>
        /// <param name="preserveBNodes">Whether Blank Node IDs should be preserved</param>
        /// <returns>A Blank Node whose ID is based on the ID of the Blank Node in the pattern and the Set ID of the Solution</returns>
        protected internal override INode Construct(IGraph g, Set s, bool preserveBNodes)
        {
            if (!preserveBNodes)
            {
                return new BlankNode(g, this._name.Substring(2) + "-" + s.ID);
            }
            else
            {
                return new BlankNode(g, this._name.Substring(2));
            }
        }

        /// <summary>
        /// Gets the String representation of this Pattern
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this._name;
        }

        /// <summary>
        /// Gets the Temporary Variable Name of this Pattern
        /// </summary>
        public override string VariableName
        {
            get
            {
                return this._name;
            }
        }
    }

    #endregion

}