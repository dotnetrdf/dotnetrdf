// unset

using System.Collections.Generic;
using System.Linq;
using VDS.RDF;

namespace dotNetRDF.Query.Behemoth
{
    public class Bindings
    {
        private readonly IDictionary<string, INode> _dict;
        public Bindings(params KeyValuePair<string, INode>[]values)
        {
            _dict = new Dictionary<string, INode>(values.ToDictionary(x => x.Key, x => x.Value));
        }

        private Bindings(IDictionary<string, INode> bindings)
        {
            _dict = bindings;
        }

        public INode this[string key]
        {
            get => _dict[key];
        }

        public Bindings Extend(params KeyValuePair<string, INode>[] extensions)
        {
            var d = new Dictionary<string, INode>(_dict);
            foreach (KeyValuePair<string, INode> ext in extensions)
            {
                d.Add(ext.Key, ext.Value);
            }
            return new Bindings(d);
        }

        public Bindings Extend(Bindings extension)
        {
            var d = new Dictionary<string, INode>(_dict);
            foreach(KeyValuePair<string, INode> ext in extension._dict)
            {
                if (!d.ContainsKey(ext.Key))
                {
                    d.Add(ext.Key, ext.Value);
                }
            }

            return new Bindings(d);
        }

        public bool ContainsVariable(string varName)
        {
            return _dict.ContainsKey(varName);
        }

        public static Bindings Empty = new Bindings(new Dictionary<string, INode>(0));

        public override bool Equals(object obj)
        {
            if (!(obj is Bindings other))
            {
                return false;
            }

            if (ReferenceEquals(this, other)) return true;
            return _dict.Count == other._dict.Count && !_dict.Except(other._dict).Any();

        }

        public override int GetHashCode()
        {
            return _dict.GetHashCode();
        }
    }
}