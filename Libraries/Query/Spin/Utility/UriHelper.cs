using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.Spin.Utility
{
    public static class UriHelper
    {
        /// <summary>
        /// Translates iis-schemed URIs into a local file-system based URI, otherwise returns the Uri unchanged
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static Uri ResolveUri(String uri)
        {
            return ResolveUri(UriFactory.Create(uri));
        }

        /// <summary>
        /// Translates iis-schemed URIs into a local file-system based URI, otherwise returns the Uri unchanged
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static Uri ResolveUri(Uri uri)
        {
            if (uri == null) return null;
            if (uri.Scheme == "iis")
            {
                uri = new Uri("file:///" + System.Web.Hosting.HostingEnvironment.MapPath(uri.AbsolutePath));
            }
            return uri;
        }
    }
}
