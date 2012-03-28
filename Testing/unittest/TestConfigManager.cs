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
    static class TestConfigManager
    {
        private static bool _init = false;
        private static Dictionary<String, String> _settings;

        private static void Init()
        {
            if (_init) return;

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

            _init = true;
        }

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
