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
using VDS.RDF.Query.Spin.Model;
using VDS.RDF;
using VDS.RDF.Query.Spin;

namespace VDS.RDF.Query.Spin.Core
{

    /**
     * A singleton that is used to render resources into strings.
     * By default this displays qnames (if possible). 
     * Can be changed, for example, to switch to displaying rdfs:labels
     * instead of qnames etc.
     * 
     * @author Holger Knublauch
     */
    internal class SPINLabels
    {

        /**
         * Gets a "human-readable" label for a given Resource.
         * This checks for any existing rdfs:label, otherwise falls back to
         * <code>getLabel()</code>.
         * @param resource
         * @return the label (never null)
         */
        public static String getCustomizedLabel(IResource resource)
        {
            String label = resource.getString(RDFS.PropertyLabel);
            if (label != null)
            {
                return label;
            }
            return getLabel(resource);
        }


        /**
         * Gets the label for a given Resource.
         * @param resource  the Resource to get the label of
         * @return the label (never null)
         */
        public static String getLabel(INode resource)
        {
            if (resource is IUriNode)
            {
                String qname = null; // TODO chercher dans les NS prefixes ;
                if (qname != null)
                {
                    return qname;
                }
                else
                {
                    return "<" + ((IUriNode)resource).Uri.ToString() + ">";
                }
            }
            else
            {
                return resource.ToString();
            }
        }
    }
}