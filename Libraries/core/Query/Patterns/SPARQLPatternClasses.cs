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
using VDS.RDF.Query.Construct;

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
        /// <param name="context">Construct Context</param>
        /// <returns></returns>
        protected internal abstract INode Construct(ConstructContext context);

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
        /// <param name="context">Construct Context</param>
        /// <returns>The Node which is bound to this Variable in this Solution</returns>
        protected internal override INode Construct(ConstructContext context)
        {
            INode value = context.Set[this._varname];

            if (value == null) throw new RdfQueryException("Unable to construct a Value for this Variable for this solution as it is bound to a null");
            switch (value.NodeType)
            {
                case NodeType.Blank:
                    if (!context.PreserveBlankNodes && value.GraphUri != null)
                    {
                        //Rename Blank Node based on the Graph Uri Hash Code
                        int hash = value.GraphUri.GetEnhancedHashCode();
                        if (hash >= 0)
                        {
                            return new BlankNode(context.Graph, ((IBlankNode)value).InternalID + "-" + value.GraphUri.GetEnhancedHashCode());
                        }
                        else
                        {
                            return new BlankNode(context.Graph, ((IBlankNode)value).InternalID + hash);
                        }
                    }
                    else
                    {
                        return new BlankNode(context.Graph, ((IBlankNode)value).InternalID);
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
        /// <param name="context">Construct Context</param>
        protected internal override INode Construct(ConstructContext context)
        {
            return this._node.CopyNode(context.Graph);
        }

        /// <summary>
        /// Gets a String representation of the Node
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.Formatter.Format(this._node);
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
    /// Pattern which matches the Blank Node with the given Internal ID regardless of the Graph the nodes come from
    /// </summary>
    public class FixedBlankNodePattern : PatternItem
    {
        private String _id;

        /// <summary>
        /// Creates a new Fixed Blank Node Pattern
        /// </summary>
        /// <param name="id">ID</param>
        public FixedBlankNodePattern(String id)
        {
            if (id.StartsWith("_:"))
            {
                this._id = id.Substring(2);
            }
            else
            {
                this._id = id;
            }
        }

        /// <summary>
        /// Checks whether the pattern accepts the given Node
        /// </summary>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <param name="obj">Node to test</param>
        /// <returns></returns>
        protected internal override bool Accepts(SparqlEvaluationContext context, INode obj)
        {
            if (obj.NodeType == NodeType.Blank)
            {
                return ((IBlankNode)obj).InternalID.Equals(this._id);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns a Blank Node with a fixed ID scoped to whichever graph is provided
        /// </summary>
        /// <param name="context">Construct Context</param>
        protected internal override INode Construct(ConstructContext context)
        {
            return new BlankNode(context.Graph, this._id);
        }

        /// <summary>
        /// Gets the String representation of the Pattern Item
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<_:" + this._id + ">";
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
        /// <param name="context">Construct Context</param>
        /// <returns></returns>
        protected internal override INode Construct(ConstructContext context)
        {
            return context.GetBlankNode(this._name);
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