namespace Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF;

    partial class DynamicNode : IDictionary<object, object>
    {
        public object this[object key]
        {
            get
            {
                switch (key)
                {
                    case string stringKey:
                        return this[stringKey];

                    case Uri uriKey:
                        return this[uriKey];

                    case INode nodeKey:
                        return this[nodeKey];

                    default:
                        throw new ArgumentException("unknown key type", nameof(key));
                }
            }
            set
            {
                switch (key)
                {
                    case string stringKey:
                        this[stringKey] = value;
                        break;

                    case Uri uriKey:
                        this[uriKey] = value;
                        break;

                    case INode nodeKey:
                        this[nodeKey] = value;
                        break;

                    default:
                        throw new ArgumentException("unknown key type", nameof(key));
                }
            }
        }

        ICollection<object> IDictionary<object, object>.Keys
        {
            get
            {
                return this.Graph.GetTriplesWithSubject(this).Select(t => t.Predicate).Distinct().Select(p => DynamicHelper.ConvertToName(p as IUriNode, this.BaseUri)).ToArray();
            }
        }

        public void Add(object key, object value)
        {
            switch (key)
            {
                case string stringKey:
                    this.Add(stringKey, value);
                    break;

                case Uri uriKey:
                    this.Add(uriKey, value);
                    break;

                case INode nodeKey:
                    this.Add(nodeKey, value);
                    break;

                default:
                    throw new ArgumentException("unknown key type", nameof(key));
            }
        }

        public void Add(KeyValuePair<object, object> item)
        {
            this.Add(item.Key, item.Value);
        }

        public bool Contains(object key, object value)
        {
            switch (key)
            {
                case string stringKey:
                    return this.Contains(stringKey, value);

                case Uri uriKey:
                    return this.Contains(uriKey, value);

                case INode nodeKey:
                    return this.Contains(nodeKey, value);

                default:
                    throw new ArgumentException("unknown key type", nameof(key));
            }
        }

        public bool Contains(KeyValuePair<object, object> item)
        {
            return this.Contains(item.Key, item.Value);
        }

        public bool ContainsKey(object key)
        {
            switch (key)
            {
                case string stringKey:
                    return this.ContainsKey(stringKey);

                case Uri uriKey:
                    return this.ContainsKey(uriKey);

                case INode nodeKey:
                    return this.ContainsKey(nodeKey);

                default:
                    throw new ArgumentException("unknown key type", nameof(key));
            }
        }

        public void CopyTo(KeyValuePair<object, object>[] array, int arrayIndex)
        {
            (this as IEnumerable<KeyValuePair<object, object>>).ToArray().CopyTo(array, arrayIndex);
        }

        IEnumerator<KeyValuePair<object, object>> IEnumerable<KeyValuePair<object, object>>.GetEnumerator()
        {
            return this.Graph.GetTriplesWithSubject(this).Select(t => t.Predicate.ToString()).Distinct().ToDictionary(p => p as object, p => this[p]).GetEnumerator();
        }

        public bool Remove(object key)
        {
            switch (key)
            {
                case string stringKey:
                    return this.Remove(stringKey);

                case Uri uriKey:
                    return this.Remove(uriKey);

                case INode nodeKey:
                    return this.Remove(nodeKey);

                default:
                    throw new ArgumentException("unknown key type", nameof(key));
            }
        }

        public bool Remove(object key, object value)
        {
            switch (key)
            {
                case string stringKey:
                    return this.Remove(stringKey, value);

                case Uri uriKey:
                    return this.Remove(uriKey, value);

                case INode nodeKey:
                    return this.Remove(nodeKey, value);

                default:
                    throw new ArgumentException("unknown key type", nameof(key));
            }
        }

        public bool Remove(KeyValuePair<object, object> item)
        {
            return this.Remove(item.Key, item.Value);
        }

        public bool TryGetValue(object key, out object value)
        {
            switch (key)
            {
                case string stringKey:
                    return this.TryGetValue(stringKey, out value);

                case Uri uriKey:
                    return this.TryGetValue(uriKey, out value);

                case INode nodeKey:
                    return this.TryGetValue(nodeKey, out value);

                default:
                    throw new ArgumentException("unknown key type", nameof(key));
            }
        }
    }
}
