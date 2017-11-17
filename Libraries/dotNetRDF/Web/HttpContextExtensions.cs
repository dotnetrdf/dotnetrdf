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

namespace VDS.RDF.Web
{
    /// <summary>
    /// Utility extension methods for the <see cref="IHttpContext"/> class
    /// </summary>
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Retrieves the Accept Types to be used to determine the content format to be used in responding to requests
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <returns></returns>
        public static string[] GetAcceptTypes(this IHttpContext context)
        {
            var accept = context.Request.Headers["Accept"];
            if (accept != null && !accept.Equals(string.Empty))
            {
                // If Accept Header is not null or empty then check to see if it matches up with the AcceptTypes
                // array
                var acceptTypes = accept.Split(',');
                if (Equals(acceptTypes, context.Request.AcceptTypes))
                {
                    return context.Request.AcceptTypes;
                }
                return acceptTypes;
            }
            // Otherwise use full accept header
            return context.Request.AcceptTypes;
        }
    }
}