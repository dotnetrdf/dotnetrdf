using FluentAssertions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;
using StringWriter = VDS.RDF.Writing.StringWriter;

namespace VDS.RDF.JsonLd;

public class JsonLdExpandTests
{
    [Fact]
    public void JsonLdExpand_Expand_WithUnknownType()
    {
        var input = """
            {
              "@context": "https://www.w3.org/ns/credentials/v2",
              "id": "urn:uuid:f16eb9aa-ac44-4979-8d62-da3536baf649",
              "type":
              [
                "VerifiableCredential",
                "InvalidType"
              ],
              "credentialSubject":
              {
                "id": "urn:uuid:1a0e4ef5-091f-4060-842e-18e519ab9440",
                "invalidTerm": "invalidTerm"
              },
              "issuer": "https://localhost:40443/issuers/zDnaeipTBN8tgRmkjZWaQSBFj4Ub3ywWP6vAsgGET922nkvZz"
            }
            """;
        var ts = new TripleStore();
        var parser = new JsonLdParser(new JsonLdProcessorOptions{SafeMode = true});
        var warnings = new List<string>();
        parser.Warning += m => warnings.Add(m);
        ts.LoadFromString(input, parser);
        var quadCount = ts.Quads.Count();
        Assert.NotEqual(0, quadCount);
        Assert.NotEmpty(warnings);
    }
}
