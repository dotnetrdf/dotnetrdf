/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2023 dotNetRDF Project (http://dotnetrdf.org/)
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
using Resta.UriTemplates;
using VDS.RDF.LinkedPatternFragments.Hydra;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.LinkedPatternFragments
{
    internal class Parameters
    {
        private readonly IriTemplate template;
        private readonly INode subject;
        private readonly INode predicate;
        private readonly INode @object;

        internal Parameters(IriTemplate template, INode subject = null, INode predicate = null, INode @object = null)
        {
            if (template is null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            if (subject is not null && subject.NodeType != NodeType.Uri)
            {
                throw new ArgumentException("Subject can only be IRI", nameof(subject));
            }

            if (predicate is not null && predicate.NodeType != NodeType.Uri)
            {
                throw new ArgumentException("Predicate can only be IRI", nameof(predicate));
            }

            if (@object is not null && @object.NodeType != NodeType.Uri && @object.NodeType != NodeType.Literal)
            {
                throw new ArgumentException("Object can only be IRI or literal", nameof(@object));
            }

            this.template = template;
            this.subject = subject;
            this.predicate = predicate;
            this.@object = @object;
        }

        private Uri Uri
        {
            get
            {
                var uriTemplate = new UriTemplate(this.template.Template);
                var variables = new Dictionary<string, object>();
                var formatter = (INodeFormatter)new ExplicitRepresentationFormatter();

                if (this.subject is not null)
                {
                    variables.Add("subject", formatter.Format(this.subject));
                }

                if (this.predicate is not null)
                {
                    variables.Add("predicate", formatter.Format(this.predicate));
                }

                if (this.@object is not null)
                {
                    variables.Add("object", formatter.Format(this.@object));
                }

                return uriTemplate.ResolveUri(variables);
            }
        }

        public static implicit operator Uri(Parameters parameters)
        {
            return parameters.Uri;
        }
    }
}
