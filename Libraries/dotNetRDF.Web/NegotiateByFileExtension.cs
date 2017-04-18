/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using VDS.RDF.Parsing;

namespace VDS.RDF.Web
{
    /// <summary>
    /// A HTTP Module that attempts to allow content negotiation by file extension wherever applicable
    /// </summary>
    public class NegotiateByFileExtension 
        : IHttpModule
    {
        /// <summary>
        /// Disposes of the Module
        /// </summary>
        public void Dispose()
        {
            // clean-up code here.
        }

        /// <summary>
        /// Intialises the Module
        /// </summary>
        /// <param name="context">HTTP Application</param>
        public void Init(HttpApplication context)
        {
            context.BeginRequest += new EventHandler(context_BeginRequest);
        }

        /// <summary>
        /// Handles the start of requests by doing conneg wherever applicable
        /// </summary>
        /// <param name="sender">Sender of the Event</param>
        /// <param name="e">Event Arguments</param>
        void context_BeginRequest(object sender, EventArgs e)
        {
            HttpApplication app = sender as HttpApplication;
            if (app == null) return;
            HttpContext context = app.Context;
            if (context == null) return;

            if (context.Request.Url.AbsolutePath.Contains("."))
            {
                String actualPath = context.Request.MapPath(context.Request.Path);
                if (!File.Exists(actualPath))
                {
                    // Get the File Extension and see if it is for an RDF format
                    String ext = context.Request.Url.AbsolutePath.Substring(context.Request.Url.AbsolutePath.LastIndexOf("."));
                    switch (ext)
                    {
                        case ".aspx":
                        case ".asmx":
                        case ".ashx":
                        case ".axd":
                            // The above file extensions are special to ASP.Net and so may not actually exist as files
                            // so we need to ignore them for the purposes of negotiating by file extension
                            return;
                    }

                    try
                    {
                        List<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(ext).ToList();
                        if (defs.Count == 0) return;

                        context.Request.Headers["Accept"] = String.Join(",", defs.Select(d => d.CanonicalMimeType).ToArray());
                        String filePath = Path.GetFileNameWithoutExtension(actualPath);
                        if (filePath == null || filePath.Equals(String.Empty))
                        {
                            if (context.Request.Url.AbsolutePath.EndsWith(ext)) filePath = context.Request.Url.AbsolutePath.Substring(0, context.Request.Url.AbsolutePath.Length - ext.Length);
                        }
                        String query = context.Request.Url.Query;
                        if (query.StartsWith("?")) query = query.Substring(1);
                        context.RewritePath(filePath, String.Empty, query, true);
                    }
                    catch (RdfParserSelectionException)
                    {
                        // If we get a RdfParserSelectionException we shouldn't do anything, this fixes bug CORE-94
                    }
                }
            }
        }
    }
}
