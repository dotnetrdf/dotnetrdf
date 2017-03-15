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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace VDS.RDF
{

    public class NamespaceMapperTest : BaseTest
    {
        [Fact]
        public void NamespaceMapperEvent()
        {
            bool eventRaised = false;

            NamespaceChanged added = delegate(String prefix, Uri u) { eventRaised = true; };
            NamespaceChanged changed = delegate(String prefix, Uri u) { eventRaised = true; };
            NamespaceChanged removed = delegate(String prefix, Uri u) { eventRaised = true; };

            NamespaceMapper nsmap = new NamespaceMapper();
            nsmap.NamespaceAdded += added;
            nsmap.NamespaceModified += changed;
            nsmap.NamespaceRemoved += removed;

            Console.WriteLine("Trying to add the RDF Namespace, this should already be defined");
            nsmap.AddNamespace("rdf", new Uri(NamespaceMapper.RDF));
            Assert.Equal(false, eventRaised);
            eventRaised = false;
            Console.WriteLine();

            Console.WriteLine("Trying to add an example Namespace which isn't defined");
            nsmap.AddNamespace("ex", new Uri("http://example.org/"));
            Assert.Equal(true, eventRaised);
            eventRaised = false;
            Console.WriteLine(nsmap.GetNamespaceUri("ex").AbsoluteUri);
            Console.WriteLine();

            Console.WriteLine("Trying to modify the example Namespace");
            nsmap.AddNamespace("ex", new Uri("http://example.org/test/"));
            Assert.Equal(true, eventRaised);
            eventRaised = false;
            Console.WriteLine(nsmap.GetNamespaceUri("ex").AbsoluteUri);
            Console.WriteLine();

            Console.WriteLine("Trying to remove the example Namespace");
            nsmap.RemoveNamespace("ex");
            Assert.Equal(true, eventRaised);
            eventRaised = false;
            Console.WriteLine();

            Console.WriteLine("Trying to remove a non-existent Namespace");
            nsmap.RemoveNamespace("ex");
            Assert.Equal(false, eventRaised);
            eventRaised = false;
            Console.WriteLine();

            Console.WriteLine("Adding some example Namespace back in again for an import test");
            nsmap.AddNamespace("ex", new Uri("http://example.org/"));
            nsmap.AddNamespace("ns0", new Uri("http://example.org/clashes/"));

            Console.WriteLine("Creating another Namespace Mapper with the ex prefix mapped to a different URI");
            NamespaceMapper nsmap2 = new NamespaceMapper();
            nsmap2.AddNamespace("ex", new Uri("http://example.org/test/"));

            Console.WriteLine("Importing the new NamespaceMapper into the original");
            nsmap.Import(nsmap2);
            Console.WriteLine("NamespaceMapper now contains the following Namespaces:");
            foreach (String prefix in nsmap.Prefixes)
            {
                Console.WriteLine("\t" + prefix + " <" + nsmap.GetNamespaceUri(prefix).AbsoluteUri + ">");
            }
            Assert.Equal(nsmap.GetNamespaceUri("ex"), new Uri("http://example.org/"));
            Assert.Equal(nsmap.GetNamespaceUri("ns1"), new Uri("http://example.org/test/"));
        }
    }
}
