using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.LinkedData
{

    class UriToExpand
    {
        private Uri _uri;
        private int _depth;

        public UriToExpand(Uri u, int depth)
        {
            this._uri = u;
            this._depth = depth;
        }

        public Uri Uri
        {
            get
            {
                return this._uri;
            }
        }

        public int Depth
        {
            get
            {
                return this._depth;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is UriToExpand)
            {
                UriToExpand other = (UriToExpand)obj;
                return this._uri.ToString().Equals(other.Uri.ToString(), StringComparison.Ordinal);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return this._uri.GetEnhancedHashCode();
        }
    }
}
