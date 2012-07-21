/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VDS.RDF.Test
{
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
                            AllegroGraphRepository = "Storage.AllegroGraph.Repository";

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

        private static bool _init = false, _failed = false;
        private static Dictionary<String, String> _settings = new Dictionary<string,string>();

        private static void Init()
        {
            if (_init) return;
            if (_failed) Fail();

            if (File.Exists("UnitTestConfig.properties"))
            {
                using (StreamReader reader = new StreamReader("UnitTestConfig.properties"))
                {
                    do
                    {
                        String line = reader.ReadLine();
                        if (line == null) break;
                        if (line.TrimStart().StartsWith("#")) continue;
                        if (line.Equals("")) continue;

                        String[] parts = line.Split(new char[] { '=' }, 2);
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
            Assert.Fail("UnitTestConfig.properties cannot be found, to configure your test environment please make a copy of UnitTestConfig.template under the resources directory, add it to this project as a Content item and then edit it to match your test environment");
        }

        /// <summary>
        /// Gets a Setting by its key, if the setting doesn't exist or is null/empty the the test calling this will be marked as failing with an appropriate error message
        /// </summary>
        /// <param name="key">Setting Key</param>
        /// <returns></returns>
        public static String GetSetting(String key)
        {
            if (!_init) Init();

            if (_settings.ContainsKey(key))
            {
                String value = _settings[key];
                if (String.IsNullOrEmpty(value))
                {
                    Assert.Fail("Configuration setting '" + key + "' in your UnitTestConfig.properties file is empty/null");
                    return null;
                }
                else
                {
                    return value;
                }
            }
            else
            {
                Assert.Fail("Required configuration setting '" + key + "' not found in your UnitTestConfig.properties file");
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
            String value = GetSetting(key);
            int i;
            if (Int32.TryParse(value, out i))
            {
                return i;
            }
            else
            {
                Assert.Fail("Configuration setting '" + key + "' in your UnitTestConfig.properties file is not a valid integer");
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
            String value = GetSetting(key);
            bool b;
            if (Boolean.TryParse(value, out b))
            {
                return b;
            }
            else
            {
                Assert.Fail("Configuration setting '" + key + "' in your UnitTestConfig.properties file is not a valid boolean");
                return false;
            }
        }
    }
}
