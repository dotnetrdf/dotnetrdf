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
            if (_failed)
            {
                Assert.Fail("UnitTestConfig.properties cannot be found, please make a copy of UnitTestConfig.template and configure for your environment in order to run this test");
            }

            if (File.Exists("UnitTestConfig.properties"))
            {
                using (StreamReader reader = new StreamReader("UnitTestConfig.properties"))
                {
                    String line = reader.ReadLine();
                    if (line != null)
                    {
                        do
                        {
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

                            line = reader.ReadLine();
                        } while (line != null);
                    }
                }
            }
            else
            {
                _failed = true;
                Assert.Fail("UnitTestConfig.properties cannot be found, please make a copy of UnitTestConfig.template and configure for your environment in order to run this test");
            }

            _init = true;
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
