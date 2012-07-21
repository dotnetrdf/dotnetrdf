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
using System.Xml;

namespace VDS.Web.Configuration
{
    /// <summary>
    /// Class for managing MIME types which are used to determine what static content the server can serve
    /// </summary>
    public class MimeTypeManager
    {
        private Dictionary<String, MimeTypeMapping> _mappings = new Dictionary<string, MimeTypeMapping>();

        /// <summary>
        /// Creates a new MIME Type manager optionally using default mappings
        /// </summary>
        /// <param name="useDefaults">Whether to use defaults</param>
        public MimeTypeManager(bool useDefaults)
        {
            if (useDefaults) this.InitialiseWithDefaults();
        }

        /// <summary>
        /// Creates a new MIME Type manager from XML configuration
        /// </summary>
        /// <param name="node">Node representing MIME Types settings</param>
        public MimeTypeManager(XmlNode node)
        {
            if (node.Name.Equals("mimeTypes"))
            {
                //Use Defaults?
                //Do this first so user specified ones can override these settings
                String useDefaults = node.Attributes.GetSafeNamedItem("usedefault");
                if (useDefaults != null)
                {
                    if (useDefaults.ToLower().Equals("true")) this.InitialiseWithDefaults();
                }

                foreach (XmlNode mimeType in node.ChildNodes)
                {
                    if (mimeType.NodeType != XmlNodeType.Element) continue;

                    if (mimeType.Name.Equals("mimeType"))
                    {
                        String ext = mimeType.Attributes.GetSafeNamedItem("extension");
                        if (ext == null) throw new HttpServerException("Configuration File is invalid since a <mimeType> element does not have the required extension attribute");
                        String type = mimeType.Attributes.GetSafeNamedItem("type");
                        if (type == null) throw new HttpServerException("Configuration File is invalid since a <mimeTyoe> element does not have the required type attribute");
                        String bin = mimeType.Attributes.GetSafeNamedItem("binary");
                        if (bin == null) bin = "false";
                        bin = bin.ToLower();

                        this.AddMimeType(new MimeTypeMapping(ext, type, bin.Equals("true")));
                    }
                    else
                    {
                        throw new HttpServerException("Configuration File is invalid since the <mimeTypes> element contains an unexpected <" + mimeType.Name + "> element");
                    }
                }
            }
            else
            {
                throw new HttpServerException("MimeTypeManager constructor expected to receive a <mimeTypes> node but received a <" + node.Name + "> node");
            }
        }

        /// <summary>
        /// Initialises the manager with default MIME type mappings
        /// </summary>
        private void InitialiseWithDefaults()
        {
            this.AddMimeType(new MimeTypeMapping(".html", "text/html"));
            this.AddMimeType(new MimeTypeMapping(".htm", "text/html"));
            this.AddMimeType(new MimeTypeMapping(".xhtml", "application/xhtml+xml"));
            this.AddMimeType(new MimeTypeMapping(".xml", "application/xml"));
            this.AddMimeType(new MimeTypeMapping(".css", "text/css"));
            this.AddMimeType(new MimeTypeMapping(".js", "text/javascript"));
            this.AddMimeType(new MimeTypeMapping(".txt", "text/plain"));
            this.AddMimeType(new MimeTypeMapping(".csv", "text/csv"));
            this.AddMimeType(new MimeTypeMapping(".tsv", "text/tab-separated-values"));
            this.AddMimeType(new MimeTypeMapping(".jpg", "image/jpeg", true));
            this.AddMimeType(new MimeTypeMapping(".jpeg", "image/jpeg", true));
            this.AddMimeType(new MimeTypeMapping(".gif", "image/gif", true));
            this.AddMimeType(new MimeTypeMapping(".png", "image/png", true));
            this.AddMimeType(new MimeTypeMapping(".bmp", "image/bmp", true));
            this.AddMimeType(new MimeTypeMapping(".ico", "image/vnd.microsoft.icon", true));
        }

        /// <summary>
        /// Adds a MIME Type
        /// </summary>
        /// <param name="mapping">Mapping</param>
        public void AddMimeType(MimeTypeMapping mapping)
        {
            if (this._mappings.ContainsKey(mapping.Extension))
            {
                this._mappings[mapping.Extension] = mapping;
            }
            else
            {
                this._mappings.Add(mapping.Extension, mapping);
            }
        }

        /// <summary>
        /// Adds a MIME Type
        /// </summary>
        /// <param name="extension">File Extension</param>
        /// <param name="binary">MIME Type</param>
        /// <param name="mimeType">Whether the content is binary</param>
        public void AddMimeType(String extension, String mimeType, bool binary)
        {
            this.AddMimeType(new MimeTypeMapping(extension, mimeType, binary));
        }

        /// <summary>
        /// Adds a MIME Type
        /// </summary>
        /// <param name="extension">File Extension</param>
        /// <param name="mimeType">MIME Type</param>
        public void AddMimeType(String extension, String mimeType)
        {
            this.AddMimeType(extension, mimeType, false);
        }

        /// <summary>
        /// Removes a MIME Type
        /// </summary>
        /// <param name="mapping">Mapping</param>
        public void RemoveMimeType(MimeTypeMapping mapping)
        {
            this.RemoveMimeType(mapping.Extension);
        }

        /// <summary>
        /// Removes a MIME Type
        /// </summary>
        /// <param name="extension">File Extension</param>
        public void RemoveMimeType(String extension)
        {
            if (this._mappings.ContainsKey(extension))
            {
                this._mappings.Remove(extension);
            }
        }

        /// <summary>
        /// Gets a Mapping
        /// </summary>
        /// <param name="extension">File Extension</param>
        /// <returns>Mapping or null if none found</returns>
        public MimeTypeMapping GetMapping(String extension)
        {
            if (this._mappings.ContainsKey(extension))
            {
                return this._mappings[extension];
            }
            else
            {
                return null;
            }
        }
    }
}
