using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// A Results Handler which extracts URIs from one/more variables in a Result Set
    /// </summary>
    public class ListUrisHandler 
        : BaseResultsHandler
    {
        private List<Uri> _uris;
        private HashSet<String> _vars = new HashSet<String>();

        public ListUrisHandler(String var)
        {
            this._vars.Add(var);
        }

        public ListUrisHandler(IEnumerable<String> vars)
        {
            foreach (String var in vars)
            {
                this._vars.Add(var);
            }
        }

        public IEnumerable<Uri> Uris
        {
            get
            {
                return this._uris;
            }
        }

        protected override void StartResultsInternal()
        {
            this._uris = new List<Uri>();
        }

        protected override void HandleBooleanResultInternal(bool result)
        {
            //Nothing to do
        }

        protected override bool HandleVariableInternal(string var)
        {
            //Nothing to do
            return true;
        }

        protected override bool HandleResultInternal(Query.SparqlResult result)
        {
            foreach (String var in result.Variables)
            {
                if (this._vars.Contains(var) && result.HasValue(var))
                {
                    INode value = result[var];
                    if (value.NodeType == NodeType.Uri)
                    {
                        this._uris.Add(((IUriNode)value).Uri);
                    }
                }
            }
            return true;
        }
    }

    /// <summary>
    /// A Results Handler which extracts Literals from one/more variables in a Result Set
    /// </summary>
    public class ListStringsHandler
        : BaseResultsHandler
    {
        private List<String> _values;
        private HashSet<String> _vars = new HashSet<String>();

        public ListStringsHandler(String var)
        {
            this._vars.Add(var);
        }

        public ListStringsHandler(IEnumerable<String> vars)
        {
            foreach (String var in vars)
            {
                this._vars.Add(var);
            }
        }

        public IEnumerable<String> Strings
        {
            get
            {
                return this._values;
            }
        }

        protected override void StartResultsInternal()
        {
            this._values = new List<string>();
        }

        protected override void HandleBooleanResultInternal(bool result)
        {
            //Nothing to do
        }

        protected override bool HandleVariableInternal(string var)
        {
            //Nothing to do
            return true;
        }

        protected override bool HandleResultInternal(Query.SparqlResult result)
        {
            foreach (String var in result.Variables)
            {
                if (this._vars.Contains(var) && result.HasValue(var))
                {
                    INode value = result[var];
                    if (value.NodeType == NodeType.Literal)
                    {
                        this._values.Add(((ILiteralNode)value).Value);
                    }
                }
            }
            return true;
        }
    }
}
