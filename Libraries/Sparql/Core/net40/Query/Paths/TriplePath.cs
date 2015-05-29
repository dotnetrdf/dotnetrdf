/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

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
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Paths
{
    /// <summary>
    /// Represents a triple or a path
    /// </summary>
    public class TriplePath
        : IEquatable<TriplePath>
    {

        public TriplePath(Triple t)
            : this(t.Subject, new Property(t.Predicate), t.Object) { }

        public TriplePath(INode subj, IPath path, INode obj)
        {
            if (subj == null) throw new ArgumentNullException("subj");
            if (path == null) throw new ArgumentNullException("path");
            if (obj == null) throw new ArgumentNullException("obj");
            this.Subject = subj;
            this.Path = path;
            this.Object = obj;

            List<String> vars = new List<string>();
            if (subj.NodeType == NodeType.Variable) vars.Add(subj.VariableName);
            if (!this.IsPath)
            {
                if (((Property)this.Path).Predicate.NodeType == NodeType.Variable) vars.Add(((Property)this.Path).Predicate.VariableName);
            }
            if (obj.NodeType == NodeType.Variable) vars.Add(obj.VariableName);
            this.Variables = vars.AsReadOnly();
        }

        /// <summary>
        /// Gets whether this is a simple triple
        /// </summary>
        public bool IsTriple { get { return this.Path is Property; } }

        /// <summary>
        /// Gets whether this is a complex path
        /// </summary>
        public bool IsPath { get { return !this.IsTriple; } }

        public INode Subject { get; private set; }

        public INode Object { get; private set; }

        public IPath Path { get; private set; }

        public IEnumerable<String> Variables { get; private set; }

        /// <summary>
        /// Converts into a simple triple if possible
        /// </summary>
        /// <returns>Simple triple</returns>
        /// <exception cref="RdfException">Thrown if the path is not a simple triple</exception>
        public Triple AsTriple()
        {
            if (this.IsTriple) return new Triple(this.Subject, ((Property)this.Path).Predicate, this.Object);
            throw new RdfException("A path cannot be converted to a triple");
        }

        public bool Equals(TriplePath other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;

            return this.Subject.Equals(other.Subject) && this.Object.Equals(other.Object) && this.Path.Equals(other.Path);
        }
    }
}
