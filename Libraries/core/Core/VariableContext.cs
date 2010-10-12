using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF
{
    public enum VariableContextType
    {
        Existential,
        Universal
    }

    public class VariableContext : BasicTripleContext
    {
        private VariableContextType _type;
        private HashSet<INode> _vars = new HashSet<INode>();
        private VariableContext _innerContext;

        public VariableContext(VariableContextType type)
        {
            this._type = type;
        }

        public VariableContextType Type
        {
            get
            {
                return this._type;
            }
        }

        public IEnumerable<INode> Variables
        {
            get
            {
                return this._vars;
            }
        }

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
