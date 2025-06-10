/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.IO;
using System.Linq;
using Xunit;
using VDS.RDF.Query;

namespace VDS.RDF.Parsing;


public class SparqlJsonTests
{
    private readonly SparqlJsonParser _parser = new SparqlJsonParser();

    [Fact]
    public void ParsingSparqlJsonDates1()
    {
        const string data = @"{
 ""head"" : { ""vars"" : [ ""date"" ] } ,
 ""results"" : {
  ""bindings"" : [
    { ""date"" : { ""type"" : ""literal"" , ""value"" : ""2012-12-03T11:41:00-08:00"" } }
  ]
 }
}";

        var results = new SparqlResultSet();
        _parser.Load(results, new StringReader(data));

        Assert.Equal(1, results.Count);
    }

    [Fact]
    public void ParsingSparqlJsonNumerics1()
    {
        const string data = @"{
 ""head"" : { ""vars"" : [ ""num"" ] } ,
 ""results"" : {
  ""bindings"" : [
    { ""num"" : { ""type"" : ""literal"" , ""value"" : ""1234"" } }
  ]
 }
}";

        var results = new SparqlResultSet();
        _parser.Load(results, new StringReader(data));

        Assert.Equal(1, results.Count);
    }

    [Fact]
    public void ParsingSparqlJsonNumerics2()
    {
        const string data = @"{
 ""head"" : { ""vars"" : [ ""num"" ] } ,
 ""results"" : {
  ""bindings"" : [
    { ""num"" : { ""type"" : ""literal"" , ""value"" : 1234 } }
  ]
 }
}";

        var results = new SparqlResultSet();
        Assert.Throws<RdfParseException>(() => _parser.Load(results, new StringReader(data)));
    }

    [Fact]
    public void ParsingSparqlJsonBoolean1()
    {
        const string data = @"{
 ""head"" : { ""vars"" : [ ""bool"" ] } ,
 ""results"" : {
  ""bindings"" : [
    { ""bool"" : { ""type"" : ""literal"" , ""value"" : ""true"" } }
  ]
 }
}";

        var results = new SparqlResultSet();
        _parser.Load(results, new StringReader(data));

        Assert.Equal(1, results.Count);
    }

    [Fact]
    public void ParsingSparqlJsonBoolean2()
    {
        const string data = @"{
 ""head"" : { ""vars"" : [ ""bool"" ] } ,
 ""results"" : {
  ""bindings"" : [
    { ""bool"" : { ""type"" : ""literal"" , ""value"" : true } }
  ]
 }
}";

        var results = new SparqlResultSet();
        Assert.Throws<RdfParseException>(() => _parser.Load(results, new StringReader(data)));
    }

    [Fact]
    public void ParsingSparqlJsonGuid1()
    {
        var data = @"{
 ""head"" : { ""vars"" : [ ""guid"" ] } ,
 ""results"" : {
  ""bindings"" : [
    { ""guid"" : { ""type"" : ""literal"" , ""value"" : """ + Guid.NewGuid().ToString() + @""" } }
  ]
 }
}";

        var results = new SparqlResultSet();
        _parser.Load(results, new StringReader(data));

        Assert.Equal(1, results.Count);
    }

    [Fact]
    public void ParsingSparqlJsonCore419_01()
    {
        const String data = @"{
  ""head"" : { ""link"" : [], ""vars"" : [ ""g"" ] },
  ""results"" : {
   ""bindings"" : [ 
    { ""g"" : { ""type"" : ""uri"",  ""value"" : ""urn:a:test"" } }
   ],
   ""distinct"" : false,
   ""ordered"" : true
 }
}";
        var results = new SparqlResultSet();
        _parser.Load(results, new StringReader(data));

        Assert.Equal(1, results.Count);
    }

    [Fact]
    public void ParsingSparqlJsonCore419_02()
    {
        const String data = @"{
  ""head"" : { ""link"" : [], ""vars"" : [ ""g"" ] },
  ""results"" : {
   ""distinct"" : false,
   ""bindings"" : [ 
    { ""g"" : { ""type"" : ""uri"",  ""value"" : ""urn:a:test"" } }
   ],
   ""ordered"" : true
 }
}";
        var results = new SparqlResultSet();
        _parser.Load(results, new StringReader(data));

        Assert.Equal(1, results.Count);
    }

    [Fact]
    public void ParsingSparqlJsonCore419_03()
    {
        const String data = @"{
  ""head"" : { ""link"" : [], ""vars"" : [ ""g"" ] },
  ""results"" : {
   ""ordered"" : true,
   ""distinct"" : false,
   ""bindings"" : [ 
    { ""g"" : { ""type"" : ""uri"",  ""value"" : ""urn:a:test"" } }
   ]
 }
}";
        var results = new SparqlResultSet();
        _parser.Load(results, new StringReader(data));

        Assert.Equal(1, results.Count);
    }

    [Fact]
    public void ParsingSparqlJsonCore419_04()
    {
        const String data = @"{
  ""head"" : { ""link"" : [], ""vars"" : [ ""g"" ] },
  ""results"" : {
   ""ordered"" : true,
   ""distinct"" : false,
   ""bindings"" : [ 
    { ""g"" : { ""type"" : ""uri"",  ""value"" : ""urn:a:test"" } }
   ],
   ""extra"" : ""ignored""
 }
}";
        var results = new SparqlResultSet();
        _parser.Load(results, new StringReader(data));

        Assert.Equal(1, results.Count);
    }

    [Fact]
    public void ParsingSparqlJsonCore419_05()
    {
        const String data = @"{
  ""head"" : { ""link"" : [], ""vars"" : [ ""g"" ] },
  ""results"" : {
   ""ordered"" : true,
   ""distinct"" : false,
   ""bindings"" : [ 
    { ""g"" : { ""type"" : ""uri"",  ""value"" : ""urn:a:test"" } }
   ],
   ""extra"" : [ ""ignored"", ""junk"" ]
 }
}";
        var results = new SparqlResultSet();
        _parser.Load(results, new StringReader(data));

        Assert.Equal(1, results.Count);
    }

    [Fact]
    public void ParsingSparqlJsonCore419_06()
    {
        const String data = @"{
  ""head"" : { ""link"" : [], ""vars"" : [ ""g"" ] },
  ""results"" : {
   ""ordered"" : true,
   ""distinct"" : false,
   ""bindings"" : [ 
    { ""g"" : { ""type"" : ""uri"",  ""value"" : ""urn:a:test"" } }
   ],
   ""extra"" : { ""ignored"" : true }
 }
}";
        var results = new SparqlResultSet();
        _parser.Load(results, new StringReader(data));

        Assert.Equal(1, results.Count);
    }

    [Fact]
    public void ParsingSparqlJsonCore419_07()
    {
        const String data = @"{
  ""head"" : { ""link"" : [], ""vars"" : [ ""g"" ] },
  ""results"" : {
   ""ordered"" : true,
   ""distinct"" : false,
   ""bindings"" : [ 
    { ""g"" : { ""type"" : ""uri"",  ""value"" : ""urn:a:test"" } }
   ],
   ""extra"" : { ""ignored"" : { ""foo"" : ""bar"" } }
 }
}";
        var results = new SparqlResultSet();
        _parser.Load(results, new StringReader(data));

        Assert.Equal(1, results.Count);
    }

    [Fact]
    public void ParsingSparqlJsonCore419_08()
    {
        const String data = @"{
  ""head"" : { ""link"" : [], ""vars"" : [ ""g"" ] },
  ""results"" : {
   ""ordered"" : true,
   ""distinct"" : false,
   ""bindings"" : [ 
    { ""g"" : { ""type"" : ""uri"",  ""value"" : ""urn:a:test"" } }
   ],
   ""extra"" : { ""ignored"" : { ""foo"" : [ ""bar"", ""faz"", { ""object"" : ""value"" } ] } }
 }
}";
        var results = new SparqlResultSet();
        _parser.Load(results, new StringReader(data));

        Assert.Equal(1, results.Count);
    }

    [Fact]
    public void ParsingSparqlJsonCore419_09()
    {
        const String data = @"{
  ""head"" : { ""link"" : [], ""vars"" : [ ""g"" ] },
  ""results"" : {
   ""ordered"" : true,
   ""distinct"" : false,
   ""bindings"" : [ 
    { ""g"" : { ""type"" : ""uri"",  ""value"" : ""urn:a:test"" } }
   ],
   ""extra"" : 
 }
}";
        var results = new SparqlResultSet();

        Assert.Throws<RdfParseException>(() => _parser.Load(results, new StringReader(data)));
    }

    [Fact]
    public void ParsingSparqlJsonMalformed()
    {
        const String data = @"{ ""junk"": ]";

        var results = new SparqlResultSet();

        Assert.Throws<RdfParseException>(() => _parser.Load(results, new StringReader(data)));
    }

    [Fact]
    public void ParsingSparqlJsonCore423_01()
    {
        const String data = @"{""boolean"": false, ""head"": {""link"": []}}";
        var results = new SparqlResultSet();
        _parser.Load(results, new StringReader(data));

        Assert.Equal(SparqlResultsType.Boolean, results.ResultsType);
        Assert.False(results.Result);
    }

    [Fact]
    public void ParsingSparqlJsonCore423_02()
    {
        const String data = @"{""boolean"": true, ""head"": {""link"": []}}";
        var results = new SparqlResultSet();
        _parser.Load(results, new StringReader(data));

        Assert.Equal(SparqlResultsType.Boolean, results.ResultsType);
        Assert.True(results.Result);
    }

    [Fact]
    public void ParsingSparqlJsonCore423_03()
    {
        const String data = @"{ 
  ""results"" : {
   ""bindings"" : [ 
    { ""x"" : { ""type"" : ""uri"",  ""value"" : ""urn:a:test"" } }
   ],
  },
  ""head"": { ""vars"": [ ""x"" ] }";
        var results = new SparqlResultSet();
        _parser.Load(results, new StringReader(data));

        Assert.Equal(SparqlResultsType.VariableBindings, results.ResultsType);
        Assert.Equal(1, results.Count);
        Assert.Single(results.Variables);
    }

    [Fact]
    public void ParsingSparqlJsonCore423_04()
    {
        const String data = @"{ 
  ""results"" : {
   ""bindings"" : [ 
    { ""x"" : { ""type"" : ""uri"",  ""value"" : ""urn:a:test"" } }
   ],
  },
  ""head"": { ""vars"": [ ""x"", ""y"" ] }";
        var results = new SparqlResultSet();
        _parser.Load(results, new StringReader(data));

        Assert.Equal(SparqlResultsType.VariableBindings, results.ResultsType);
        Assert.Equal(1, results.Count);
        Assert.Equal(2, results.Variables.Count());
    }

    [Fact]
    public void ParsingSparqlJsonCore423_05()
    {
        const String data = @"{ 
  ""results"" : {
   ""bindings"" : [ 
    { ""y"" : { ""type"" : ""uri"",  ""value"" : ""urn:a:test"" } }
   ],
  },
  ""head"": { ""vars"": [ ""x"" ] }";
        var results = new SparqlResultSet();

        Assert.Throws<RdfParseException>(() => _parser.Load(results, new StringReader(data)));
    }

    [Fact]
    public void ParsingSparqlJsonCore432_01()
    {
        // Test case based off of CORE-432 - relative URI in JSON
        const String data = @"{ 
  ""head"": { ""vars"": [ ""x"", ""y"" ] },
  ""results"" : {
   ""bindings"" : [ 
    { ""x"" : { ""type"" : ""uri"",  ""value"" : ""relative"" } }
   ],
  }
}";
        var results = new SparqlResultSet();

        Assert.Throws<RdfParseException>(() => _parser.Load(results, new StringReader(data)));
    }

    [Fact]
    public void ParsingSparqlJsonCore432_02()
    {
        // Test case based off of CORE-432 - relative URI in JSON
        const String data = @"{ 
  ""head"": { ""vars"": [ ""x"", ""y"" ] },
  ""results"" : {
   ""bindings"" : [ 
    { ""x"" : { ""type"" : ""literal"",  ""value"" : ""Literal with relative datatype"", ""datatype"" : ""relative"" } }
   ],
  }
}";
        var results = new SparqlResultSet();

        Assert.Throws<RdfParseException>(() => _parser.Load(results, new StringReader(data)));
    }

    [Fact]
    public void ParsingSparqlJsonCore432_03()
    {
        // Test case based off of CORE-432 - invalid URI in JSON
        const String data = @"{ 
  ""head"": { ""vars"": [ ""x"", ""y"" ] },
  ""results"" : {
   ""bindings"" : [ 
    { ""x"" : { ""type"" : ""uri"",  ""value"" : ""http://an invalid uri"" } }
   ],
  }
}";
        var results = new SparqlResultSet();

        Assert.Throws<RdfParseException>(() => _parser.Load(results, new StringReader(data)));
    }

    [Fact]
    public void ParsingSparqlJsonCore432_04()
    {
        // Test case based off of CORE-432 - invalid URI in JSON
        const String data = @"{ 
  ""head"": { ""vars"": [ ""x"", ""y"" ] },
  ""results"" : {
   ""bindings"" : [ 
    { ""x"" : { ""type"" : ""literal"",  ""value"" : ""Literal with invalid datatype"", ""datatype"" : ""http://an invalid uri"" } }
   ],
  }
}";
        var results = new SparqlResultSet();

        Assert.Throws<RdfParseException>(() => _parser.Load(results, new StringReader(data)));
    }
}
