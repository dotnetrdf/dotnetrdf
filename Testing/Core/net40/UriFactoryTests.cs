/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

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
using NUnit.Framework;
using VDS.RDF.Graphs;
using VDS.RDF.Namespaces;

namespace VDS.RDF
{
    [TestFixture]
    public class UriFactoryTests
    {
        private static void TestUriResolution(String uri, Uri baseUri, String expected, bool expectAbsolute)
        {
            Uri u = UriFactory.ResolveUri(uri, baseUri);
            if (ReferenceEquals(expected, null)) Assert.Fail("Expected URI resolution to fail");
            Assert.AreEqual(expected, expectAbsolute ? u.AbsoluteUri : u.ToString());
        }

        private static void TestPrefixedNameResolution(string prefixedName, INamespaceMapper nsmap, Uri baseUri, string expected)
        {
            TestPrefixedNameResolution(prefixedName, nsmap, baseUri, false, expected);
        }

        private static void TestPrefixedNameResolution(string prefixedName, INamespaceMapper nsmap, Uri baseUri, bool allowDefaultPrefixFallback, string expected)
        {
            Uri u = UriFactory.ResolvePrefixedName(prefixedName, nsmap, baseUri, allowDefaultPrefixFallback);
            if (ReferenceEquals(expected, null)) Assert.Fail("Expected URI resolution to fail");
            Assert.AreEqual(expected, u.AbsoluteUri);
        }

        [Test]
        public void UriResolution1()
        {
            TestUriResolution("file.ext", new Uri("http://example.org"), "http://example.org/file.ext", true);
        }

        [Test]
        public void UriResolution2()
        {
            TestUriResolution("file.ext", new Uri("http://example.org"), "http://example.org/file.ext", true);
        }

        [Test]
        public void UriResolution3()
        {
            TestUriResolution("file.ext", null, "file.ext", false);
        }

        [Test]
        public void UriResolution4()
        {
            TestUriResolution("file.ext", new Uri("/path", UriKind.Relative), "file.ext", false);
        }

        [Test, ExpectedException(typeof(RdfException))]
        public void UriResolutionBad1()
        {
            TestUriResolution("file:/file.ext", new Uri("http://example.org"), null, false);
        }

        [Test, ExpectedException(typeof(RdfException))]
        public void UriResolutionBad2()
        {
            TestUriResolution(null, new Uri("http://example.org"), null, false);
        }

        [Test]
        public void PrefixedNameResolution1()
        {
            INamespaceMapper nsmap = new NamespaceMapper();
            nsmap.AddNamespace("ex", new Uri("http://example.org"));

            // Namespace resolution
            TestPrefixedNameResolution("ex:test", nsmap, null, "http://example.org/test");
        }

        [Test]
        public void PrefixedNameResolution2()
        {
            INamespaceMapper nsmap = new NamespaceMapper();
            nsmap.AddNamespace(String.Empty, new Uri("http://example.org"));

            // Default namespace resolution
            TestPrefixedNameResolution(":test", nsmap, null, "http://example.org/test");
        }

        [Test]
        public void PrefixedNameResolution3()
        {
            INamespaceMapper nsmap = new NamespaceMapper();

            // Namespace not in scope and default namespace fallback possible
            TestPrefixedNameResolution(":test", nsmap, new Uri("http://example.org"), true, "http://example.org/#test");
        }

        [Test]
        public void PrefixedNameResolution4()
        {
            INamespaceMapper nsmap = new NamespaceMapper();

            // Namespace not in scope and default namespace fallback possible
            TestPrefixedNameResolution(":test", nsmap, new Uri("http://example.org/ns#"), true, "http://example.org/ns#test");
        }

        [Test, ExpectedException(typeof(RdfException))]
        public void PrefixedNameResolutionBad1()
        {
            INamespaceMapper nsmap = new NamespaceMapper();

            // Namespace not in scope
            TestPrefixedNameResolution("ex:test", nsmap, null, null);
        }

        [Test, ExpectedException(typeof(RdfException))]
        public void PrefixedNameResolutionBad2()
        {
            INamespaceMapper nsmap = new NamespaceMapper();

            // Namespace not in scope and default namespace fallback not applicable
            TestPrefixedNameResolution("ex:test", nsmap, new Uri("http://example.org"), null);
        }

        [Test, ExpectedException(typeof(RdfException))]
        public void PrefixedNameResolutionBad3()
        {
            INamespaceMapper nsmap = new NamespaceMapper();

            // Namespace not in scope and default namespace fallback not allowed
            TestPrefixedNameResolution(":test", nsmap, new Uri("http://example.org"), null);
        }

        [Test, ExpectedException(typeof(RdfException))]
        public void PrefixedNameResolutionBad4()
        {
            INamespaceMapper nsmap = new NamespaceMapper();

            // Namespace not in scope and default namespace fallback not possible due to relative base URI
            TestPrefixedNameResolution(":test", nsmap, new Uri("file.ext", UriKind.Relative), true, null);
        }

        [Test, ExpectedException(typeof(RdfException))]
        public void PrefixedNameResolutionBad5()
        {
            INamespaceMapper nsmap = new NamespaceMapper();

            // Namespace not in scope and default namespace fallback not possible due to null base URI
            TestPrefixedNameResolution(":test", nsmap, null, true, null);
        }

        [Test]
        public void InternUri1()
        {
            try
            {
                Uri u1 = new Uri("http://example.org");
                Uri u2 = new Uri("http://example.org/test");

                UriFactory.Intern(u2);
                Assert.IsTrue(UriFactory.IsInterned(u2));
                Assert.IsFalse(UriFactory.IsInterned(u1));

                UriFactory.Intern(u1);
                Assert.IsTrue(UriFactory.IsInterned(u1));
            }
            finally
            {
                UriFactory.Clear();
            }
        }

        [Test]
        public void InternUri2()
        {
            try
            {
                Uri u1 = new Uri("http://example.org");
                Uri u2 = new Uri("http://example.org:8080");

                UriFactory.Intern(u2);
                Assert.IsTrue(UriFactory.IsInterned(u2));
                Assert.IsFalse(UriFactory.IsInterned(u1));

                UriFactory.Intern(u1);
                Assert.IsTrue(UriFactory.IsInterned(u1));
            }
            finally
            {
                UriFactory.Clear();
            }
        }

        [Test]
        public void InternUri3()
        {
            try
            {
                Uri u1 = new Uri("http://example.org/#test");
                Uri u2 = new Uri("http://example.org/?test");

                UriFactory.Intern(u2);
                Assert.IsTrue(UriFactory.IsInterned(u2));
                Assert.IsFalse(UriFactory.IsInterned(u1));

                UriFactory.Intern(u1);
                Assert.IsTrue(UriFactory.IsInterned(u1));
            }
            finally
            {
                UriFactory.Clear();
            }
        }

        [Test]
        public void UriResolution5()
        {
            String[] baseUris = { "http://www.bbc.co.uk",
                                  "http://www.bbc.co.uk/",
                                  "http://www.bbc.co.uk/test.txt",
                                  "http://www.bbc.co.uk/test",
                                  "http://www.bbc.co.uk/test/",
                                  "http://www.bbc.co.uk/test/subdir",
                                  "http://www.bbc.co.uk/really/really/long/path",
                                  "http://www.bbc.co.uk#fragment",
                                  "http://www.bbc.co.uk/test.txt#fragment"//,
                                };
            String[] uriRefs = { "test2.txt",
                                 "test2",
                                 "/test2",
                                 "test2/subdir",
                                 "/test2/subdir",
                                 "../test2",
                                 "#fragment2"
                                };
            String[][] expected = { new String[] {"http://www.bbc.co.uk/test2.txt","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/test2/subdir","http://www.bbc.co.uk/test2/subdir","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/#fragment2"},
                                    new String[] {"http://www.bbc.co.uk/test2.txt","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/test2/subdir","http://www.bbc.co.uk/test2/subdir","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/#fragment2"},
                                    new String[] {"http://www.bbc.co.uk/test2.txt","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/test2/subdir","http://www.bbc.co.uk/test2/subdir","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/test.txt#fragment2"},
                                    new String[] {"http://www.bbc.co.uk/test2.txt","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/test2/subdir","http://www.bbc.co.uk/test2/subdir","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/test#fragment2"},
                                    new String[] {"http://www.bbc.co.uk/test/test2.txt","http://www.bbc.co.uk/test/test2","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/test/test2/subdir","http://www.bbc.co.uk/test2/subdir","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/test/#fragment2"},
                                    new String[] {"http://www.bbc.co.uk/test/test2.txt","http://www.bbc.co.uk/test/test2","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/test/test2/subdir","http://www.bbc.co.uk/test2/subdir","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/test/subdir#fragment2"},
                                    new String[] {"http://www.bbc.co.uk/really/really/long/test2.txt","http://www.bbc.co.uk/really/really/long/test2","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/really/really/long/test2/subdir","http://www.bbc.co.uk/test2/subdir","http://www.bbc.co.uk/really/really/test2","http://www.bbc.co.uk/really/really/long/path#fragment2"},
                                    new String[] {"http://www.bbc.co.uk/test2.txt","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/test2/subdir","http://www.bbc.co.uk/test2/subdir","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/#fragment2"},
                                    new String[] {"http://www.bbc.co.uk/test2.txt","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/test2/subdir","http://www.bbc.co.uk/test2/subdir","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/test.txt#fragment2"}
                                  };

            for (int i = 0; i < baseUris.Length; i++)
            {
                Console.WriteLine("Resolving against Base URI " + baseUris[i]);

                Uri baseUri = new Uri(baseUris[i]);

                for (int j = 0; j < uriRefs.Length; j++)
                {
                    Console.WriteLine("Resolving " + uriRefs[j]);

                    Uri result = UriFactory.ResolveUri(uriRefs[j], baseUri);
                    string expectedResult = expected[i][j];

                    Console.WriteLine("Expected: " + expectedResult);
                    Console.WriteLine("Actual: " + result);

                    Assert.AreEqual(expectedResult, result.AbsoluteUri);
                }

                Console.WriteLine();
            }
        }

        [Test, ExpectedException(typeof(RdfException))]
        public void UriResolutionUriProvidedToQNameMethod()
        {
                IGraph g = new Graph();
                g.CreateUriNode("http://example.org");
        }

        [Test]
        public void UriHashCodes()
        {
            //Quick Test to see if how the Uri classes Hash Codes behave
            Uri test1 = new Uri("http://example.org/test#one");
            Uri test2 = new Uri("http://example.org/test#two");
            Uri test3 = new Uri("http://example.org/test#three");

            Console.WriteLine("Three identical URIs with different Fragment IDs, .Net ignores the Fragments in creating Hash Codes");
            Console.WriteLine("URI 1 has Hash Code " + test1.GetHashCode());
            Console.WriteLine("URI 2 has Hash Code " + test2.GetHashCode());
            Console.WriteLine("URI 3 has Hash Code " + test3.GetHashCode());

            Assert.AreEqual(test1.GetHashCode(), test2.GetHashCode());
            Assert.AreEqual(test2.GetHashCode(), test3.GetHashCode());
            Assert.AreEqual(test1.GetHashCode(), test3.GetHashCode());
        }

#if PORTABLE
        [TestCase("The following String needs URL Encoding <node>Test</node> 100% not a percent encode", "The%20following%20String%20needs%20URL%20Encoding%20%3Cnode%3ETest%3C%2Fnode%3E%20100%25%20not%20a%20percent%20encode")]
        [TestCase("This string contains UTF-8 納豆 characters", "This%20string%20contains%20UTF-8%20%E7%B4%8D%E8%B1%86%20characters")]
        [TestCase("This string contains UTF-8 ç´è± characters", "This%20string%20contains%20UTF-8%20%C3%A7%C2%B4%C2%8D%C3%A8%C2%B1%C2%86%20characters")]
        [TestCase("This string has safe characters -._~", "This%20string%20has%20safe%20characters%20-._~")]
        public void UriEncoding(string test, string expectedEncoded)
        {
            string encoded = HttpUtility.UrlEncode(test);
            string encodedTwice = HttpUtility.UrlEncode(encoded); 
            string decoded = HttpUtility.UrlDecode(encoded);
            string decodedTwice = HttpUtility.UrlDecode(decoded);

            Console.WriteLine("Encoded once:  {0}", encoded);
            Console.WriteLine("Encoded twice: {0}", encodedTwice);
            Console.WriteLine("Decoded once:  {0}", decoded);
            Console.WriteLine("Decoded twice: {0}", decodedTwice);

            Assert.That(encoded, Is.EqualTo(expectedEncoded));
            Assert.That(encodedTwice, Is.EqualTo(expectedEncoded));
            Assert.That(decoded, Is.EqualTo(test));
            Assert.That(decodedTwice, Is.EqualTo(test));
        }
#endif

        [Test]
        public void UriPathAndQuery()
        {
            Uri u = new Uri("http://example.org/some/path/with?query=some&param=values");

            String pathAndQuery = u.PathAndQuery;
            String absPathPlusQuery = u.AbsolutePath + u.Query;

            Console.WriteLine("PathAndQuery - " + pathAndQuery);
            Console.WriteLine("AbsolutePath + Query - " + absPathPlusQuery);

            Assert.AreEqual(pathAndQuery, absPathPlusQuery);
        }

        [Test]
        public void UriQuery()
        {
            Uri withQuery = new Uri("http://example.org/with?some=query");
            Uri withoutQuery = new Uri("http://example.org/without");

            Assert.AreNotEqual(String.Empty, withQuery.Query);
            Assert.AreEqual(String.Empty, withoutQuery.Query);
        }

        //[Test]
        //public void UriTrailingDot()
        //{
        //    Uri u = new Uri("http://example.org/path.");
        //    Console.WriteLine(u.ToString());
        //    Console.WriteLine("Is IRI? " + IriSpecsHelper.IsIri("http://example.org/path.").ToString());

        //    foreach (PropertyInfo info in u.GetType().GetProperties())
        //    {
        //        Console.WriteLine(info.Name + " = " + info.GetValue(u, null));
        //    }
        //    Console.WriteLine();

        //    MethodInfo getSyntax = typeof(UriParser).GetMethod("GetSyntax", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        //    FieldInfo flagsField = typeof(UriParser).GetField("m_Flags", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        //    if (getSyntax != null && flagsField != null)
        //    {
        //        foreach (string scheme in new[] { "http", "https" })
        //        {
        //            UriParser parser = (UriParser)getSyntax.Invoke(null, new object[] { scheme });
        //            if (parser != null)
        //            {
        //                int flagsValue = (int)flagsField.GetValue(parser);
        //                // Clear the CanonicalizeAsFilePath attribute
        //                if ((flagsValue & 0x1000000) != 0)
        //                    flagsField.SetValue(parser, flagsValue & ~0x1000000);
        //            }
        //        }
        //    }
        //    Uri v = new Uri("http://example.org/path.");
        //    Console.WriteLine(v.ToString());

        //    foreach (PropertyInfo info in v.GetType().GetProperties())
        //    {
        //        Console.WriteLine(info.Name + " = " + info.GetValue(v, null));
        //    }
        //}
    }
}
