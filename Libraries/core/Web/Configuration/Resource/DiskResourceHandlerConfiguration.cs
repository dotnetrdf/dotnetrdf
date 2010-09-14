/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

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

#if !NO_WEB && !NO_ASP

using System;
using System.Configuration;
using System.IO;
using System.Web;

namespace VDS.RDF.Web.Configuration.Resource
{
    /// <summary>
    /// Loads and Stores the Configuration Information for a <see cref="DiskResourceHandler">DiskResourceHandler</see>
    /// </summary>
    [Obsolete("This class is obselete", true)]
    class DiskResourceHandlerConfiguration : BaseResourceHandlerConfiguration
    {
        private String _cacheKey = String.Empty;
        private String _baseUri = String.Empty;
        private String _folder = String.Empty;
        private String _ext = ".rdf";
        private String _mimeType = "application/rdf+xml";
        private String _defaultFile = String.Empty;

        private bool _allowFormatTranslation = false;

        public DiskResourceHandlerConfiguration(HttpContext context, String cacheKey, String configPrefix)
        {
            this._cacheKey = cacheKey;

            try
            {
                //Data Configuration
                if (ConfigurationManager.AppSettings[configPrefix + "BaseURI"] != null)
                {
                    this._baseUri = ConfigurationManager.AppSettings[configPrefix + "BaseURI"];
                }
                else
                {
                    throw new RdfException("Required Base URI setting for the Disk Resource Handler not found");
                }
                if (ConfigurationManager.AppSettings[configPrefix + "DataFolder"] != null)
                {
                    this._folder = context.Server.MapPath(ConfigurationManager.AppSettings[configPrefix + "DataFolder"]);
                    if (!Directory.Exists(this._folder))
                    {
                        throw new DirectoryNotFoundException("The Data Folder for the Disk Resource Handler does not exist");
                    }

                    //Ensure Folder ends with a Backslash
                    if (!this._folder.EndsWith("\\"))
                    {
                        this._folder += "\\";
                    }
                }
                else
                {
                    throw new RdfException("Required Data Folder setting for the Disk Resource Handler not found");
                }
                if (ConfigurationManager.AppSettings[configPrefix + "DataExtension"] != null)
                {
                    this._ext = ConfigurationManager.AppSettings[configPrefix + "DataExtension"];
                    if (ConfigurationManager.AppSettings[configPrefix + "DataMIMEType"] != null)
                    {
                        //Use User Specified MIME Type
                        this._mimeType = ConfigurationManager.AppSettings[configPrefix + "DataMIMEType"];
                    }
                    else
                    {
                        //Detect MIME Type from Extension
                        this._mimeType = MimeTypesHelper.GetMimeType(this._ext);
                    }

                    //Ensure extension starts with a Dot
                    if (!this._ext.StartsWith("."))
                    {
                        this._ext = "." + this._ext;
                    }
                }
                if (ConfigurationManager.AppSettings[configPrefix + "DefaultFile"] != null)
                {
                    this._defaultFile = ConfigurationManager.AppSettings[configPrefix + "DefaultFile"];
                }

                //Handler Configuration
                bool allowTranslate;
                if (Boolean.TryParse(ConfigurationManager.AppSettings[configPrefix + "AllowFormatTranslation"], out allowTranslate))
                {
                    this._allowFormatTranslation = allowTranslate;
                }
            }
            catch (DirectoryNotFoundException)
            {
                throw;
            }
            catch
            {
                throw new RdfException("Disk Resource Handler Configuration could not be found/was invalid");
            }
        }

        public String CacheKey
        {
            get
            {
                return this._cacheKey;
            }
        }

        public String BaseURI
        {
            get
            {
                return this._baseUri;
            }
        }

        public String DataFolder
        {
            get
            {
                return this._folder;
            }
        }

        public String DataExtension
        {
            get
            {
                return this._ext;
            }
        }

        public String DataMIMEType
        {
            get
            {
                return this._mimeType;
            }
        }

        public String DefaultFile
        {
            get
            {
                return this._defaultFile;
            }
        }

        public bool AllowFormatTranslation
        {
            get
            {
                return this._allowFormatTranslation;
            }
        }
    }
}

#endif