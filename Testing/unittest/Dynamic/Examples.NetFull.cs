/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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

namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections.Generic;
    using Xunit;

    public partial class Examples
    {
        [Fact]
        public void Example3()
        {
            var expected = new Graph();
            expected.LoadFromFile(@"resources\rvesse.ttl");
            var actual = new Graph();
            var d = actual.AsDynamic(UriFactory.Create("http://www.dotnetrdf.org/people#"));

            var rvesse = new FoafPerson(d.rvesse);
            rvesse.Names.Add("Rob Vesse");
            rvesse.FirstNames.Add("Rob");
            rvesse.LastNames.Add("Vesse");
            rvesse.Homepages.Add(UriFactory.Create("http://www.dotnetrdf.org/"));
            rvesse.Emails.Add(UriFactory.Create("mailto:rav08r@ecs.soton.ac.uk"));
            rvesse.Emails.Add(UriFactory.Create("mailto:rvesse@apache.org"));
            rvesse.Emails.Add(UriFactory.Create("mailto:rvesse@cray.com"));
            rvesse.Emails.Add(UriFactory.Create("mailto:rvesse@dotnetrdf.org"));
            rvesse.Emails.Add(UriFactory.Create("mailto:rvesse@vdesign-studios.com"));
            rvesse.Emails.Add(UriFactory.Create("mailto:rvesse@yarcdata.com"));

            var account = new FoafAccount(d.CreateBlankNode());
            account.Names.Add("RobVesse");
            account.Profiles.Add(UriFactory.Create("http://twitter.com/RobVesse"));
            account.Services.Add(UriFactory.Create("http://twitter.com/"));
            rvesse.Accounts.Add(account);

            var jena = new DoapProject(d.CreateBlankNode());
            jena.Names.Add("Apache Jena");
            jena.Homepages.Add(UriFactory.Create("http://incubator.apache.org/jena"));
            rvesse.Projects.Add(jena);

            var dnr = new DoapProject(d.CreateBlankNode());
            dnr.Names.Add("dotNetRDF");
            dnr.Homepages.Add(UriFactory.Create("http://www.dotnetrdf.org/"));
            rvesse.Projects.Add(dnr);

            var key = new WotKey(d.CreateBlankNode());
            key.Ids.Add("6E2497EB");
            key.Fingerprints.Add("7C4C 2916 BF74 E242 53BC  C5B7 764D FB13 6E24 97EB");
            rvesse.Keys.Add(key);

            Assert.Equal(expected, actual);
        }

        private class FoafPerson : DynamicNode
        {
            public FoafPerson(INode node)
                : base(node, UriFactory.Create("http://xmlns.com/foaf/0.1/"))
                => this.Add("rdf:type", UriFactory.Create("http://xmlns.com/foaf/0.1/Person"));

            public ICollection<string> Names => new DynamicObjectCollection<string>(this, "name");

            public ICollection<string> FirstNames => new DynamicObjectCollection<string>(this, "givenname");

            public ICollection<string> LastNames => new DynamicObjectCollection<string>(this, "family_name");

            public ICollection<Uri> Homepages => new DynamicObjectCollection<Uri>(this, "homepage");

            public ICollection<Uri> Emails => new DynamicObjectCollection<Uri>(this, "mbox");

            public ICollection<FoafAccount> Accounts => new DynamicObjectCollection<FoafAccount>(this, "OnlineAccount");

            public ICollection<DoapProject> Projects => new DynamicObjectCollection<DoapProject>(this, "currentProject");

            public ICollection<WotKey> Keys => new DynamicObjectCollection<WotKey>(this, "http://xmlns.com/wot/0.1/hasKey");
        }

        private class FoafAccount : DynamicNode
        {
            public FoafAccount(INode node)
                : base(node, UriFactory.Create("http://xmlns.com/foaf/0.1/")) { }

            public ICollection<string> Names => new DynamicObjectCollection<string>(this, "accountName");

            public ICollection<Uri> Profiles => new DynamicObjectCollection<Uri>(this, "accountProfilePage");

            public ICollection<Uri> Services => new DynamicObjectCollection<Uri>(this, "accountServiceHomepage");
        }

        private class DoapProject : DynamicNode
        {
            public DoapProject(INode node)
                : base(node, UriFactory.Create("http://www.web-semantics.org/ns/pm#"))
                => this.Add("rdf:type", UriFactory.Create("http://usefulinc.com/ns/doap#Project"));

            public ICollection<string> Names => new DynamicObjectCollection<string>(this, "name");

            public ICollection<Uri> Homepages => new DynamicObjectCollection<Uri>(this, "homepage");
        }

        private class WotKey : DynamicNode
        {
            public WotKey(INode node)
                : base(node, UriFactory.Create("http://xmlns.com/wot/0.1/"))
                => this.Add("rdf:type", UriFactory.Create("http://xmlns.com/wot/0.1/PubKey"));

            public ICollection<string> Ids => new DynamicObjectCollection<string>(this, "hex_id");

            public ICollection<string> Fingerprints => new DynamicObjectCollection<string>(this, "fingerprint");
        }
    }
}
