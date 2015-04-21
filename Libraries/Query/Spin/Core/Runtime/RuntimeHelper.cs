//
using System;
using VDS.RDF.Query.Spin.Utility;

namespace VDS.RDF.Query.Spin.Core.Runtime
{
    public static class RuntimeHelper
    {
        /// <summary>
        /// The base namespace prefix for temporary resources and vocabulary
        /// </summary>
        public const String BASE_URI = "tmp:dotnetrdf.org";

        /// <summary>
        /// The Uri prefix for temporary resources
        /// </summary>
        public const String NS_URI = BASE_URI + ":";

        public static readonly IUriNode
                ClassTemporaryGraph = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "TemporaryGraph")),
                PropertyRequiredBy = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "requiredBy")),
                PropertyLastAccess = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "lastAccess")),
                PropertyStartedAt = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "startedAt"));

        public static readonly IUriNode BLACKHOLE = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "NULL"));
    }
}