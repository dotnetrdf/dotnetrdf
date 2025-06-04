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
using System.IO;
using Xunit;

namespace VDS.RDF;

/// <summary>
/// Manager for Unit Test Configuration which is basically key value properties used in various tests.  Primarily intended for tests that rely on per system software like triple stores
/// </summary>
public static class TestConfigManager
{

    /// <summary>
    /// Constants for Test Configuration setings related to IIS and ASP.Net tests
    /// </summary>
    public const String UseIIS = "Web.IIS",
                        LocalQueryUri = "Web.Query",
                        LocalGraphStoreUri = "Web.SparqlServer",
                        LocalGraphStoreQueryUri = "Web.SparqlServer.Query",
                        LocalGraphStoreUpdateUri = "Web.SparqlServer.Update";

    public const String UseRemoteSparql = "Sparql.Remote",
                        RemoteSparqlQuery = "Sparql.Remote.Query";

    /// <summary>
    /// Constants for Test Configuration settings related to AllegroGraph test
    /// </summary>
    public const String UseAllegroGraph = "Storage.AllegroGraph",
                        AllegroGraphServer = "Storage.AllegroGraph.Server",
                        AllegroGraphCatalog = "Storage.AllegroGraph.Catalog",
                        AllegroGraphRepository = "Storage.AllegroGraph.Repository",
                        AllegroGraphUser = "Storage.AllegroGraph.User",
                        AllegroGraphPassword = "Storage.AllegroGraph.Password";

    public const String UseDydra = "Storage.Dydra",
                        DydraAccount = "Storage.Dydra.Account",
                        DydraRepository = "Storage.Dydra.Repository",
                        DydraApiKey = "Storage.Dydra.ApiKey";

    public const String UseFourStore = "Storage.FourStore",
                        FourStoreServer = "Storage.FourStore.Server";

    public const String UseFuseki = "Storage.Fuseki",
                        FusekiServer = "Storage.Fuseki.Server";

    public const String UseSesame = "Storage.Sesame",
                        SesameServer = "Storage.Sesame.Server",
                        SesameRepository = "Storage.Sesame.Repository";

    public const String UseStardog = "Storage.Stardog",
                        StardogServer = "Storage.Stardog.Server",
                        StardogDatabase = "Storage.Stardog.DB",
                        StardogUser = "Storage.Stardog.User",
                        StardogPassword = "Storage.Stardog.Password";


    public const String UseVirtuoso = "Storage.Virtuoso",
                        VirtuosoServer = "Storage.Virtuoso.Server",
                        VirtuosoPort = "Storage.Virtuoso.Port",
                        VirtuosoDatabase = "Storage.Virtuoso.DB",
                        VirtuosoUser = "Storage.Virtuoso.User",
                        VirtuosoPassword = "Storage.Virtuoso.Password",
                        VirtuosoEndpoint = "Storage.Virtuoso.Endpoint";

    public const String UseGraphViz = "Writing.GraphViz";

    public const String UseRemoteParsing = "Parsing.Remote";

    private static bool _init = false, _failed = false;
    private static readonly Dictionary<String, String> _settings = new Dictionary<string,string>();

    private static void Init()
    {
        if (_init) return;
        if (_failed) Fail();
        var configFilePath = Path.Combine("resources", "UnitTestConfig.properties");

        if (File.Exists(configFilePath))
        {
            using (StreamReader reader = File.OpenText(configFilePath))
            {
                do
                {
                    var line = reader.ReadLine();
                    if (line == null) break;
                    if (line.TrimStart().StartsWith("#")) continue;
                    if (line.Equals("")) continue;

                    var parts = line.Split(new char[] { '=' }, 2);
                    if (parts.Length == 2)
                    {
                        if (_settings.ContainsKey(parts[0]))
                        {
                            _settings[parts[0]] = parts[1];
                        }
                        else
                        {
                            _settings.Add(parts[0], parts[1]);
                        }
                    }
                } while (true);
                reader.Close();
            }
        }
        else
        {
            _failed = true;
            Fail();
        }

        _init = true;
    }

    private static void Fail()
    {
        Assert.SkipUnless(true, "UnitTestConfig.properties cannot be found, to configure your test environment please make a copy of UnitTestConfig.template under the resources directory, add it to this project as a Content item and then edit it to match your test environment");
    }

    /// <summary>
    /// Gets a Setting by its key, if the setting doesn't exist or is null/empty the the test calling this will be marked as failing with an appropriate error message
    /// </summary>
    /// <param name="key">Setting Key</param>
    /// <returns></returns>
    public static String GetSetting(String key)
    {
        if (!_init) Init();

        if (_failed) return null;
        if (_settings.ContainsKey(key))
        {
            var value = _settings[key];
            if (String.IsNullOrEmpty(value))
            {
                Assert.Fail(
                    "Configuration setting '" + key + "' in your UnitTestConfig.properties file is empty/null");
                return null;
            }
            else
            {
                return value;
            }
        }
        else
        {
            Assert.Fail(
                "Required configuration setting '" + key +
                "' not found in your UnitTestConfig.properties file");
            return null;
        }

    }

    /// <summary>
    /// Gets a Setting by its key as an integer, if the setting doesn't exist or is an invalid integer the the test calling this will be marked as failing with an appropriate error message
    /// </summary>
    /// <param name="key">Setting Key</param>
    /// <returns></returns>
    public static int GetSettingAsInt(String key)
    {
        var value = GetSetting(key);
        if (_failed) return 0;
        int i;
        if (Int32.TryParse(value, out i))
        {
            return i;
        }
        else
        {
            Assert.Fail(
                "Configuration setting '" + key +
                "' in your UnitTestConfig.properties file is not a valid integer");
            return 0;
        }

    }

    /// <summary>
    /// Gets a Setting by its key, if the setting doesn't exist or is an invalid boolean the the test calling this will be marked as failing with an appropriate error message
    /// </summary>
    /// <param name="key">Setting Key</param>
    /// <returns></returns>
    public static bool GetSettingAsBoolean(String key)
    {
        var value = GetSetting(key);
        if (_failed) return false;
        bool b;
        if (Boolean.TryParse(value, out b))
        {
            return b;
        }
        else
        {
            Assert.Fail(
                "Configuration setting '" + key +
                "' in your UnitTestConfig.properties file is not a valid boolean");
            return false;
        }

    }
}
