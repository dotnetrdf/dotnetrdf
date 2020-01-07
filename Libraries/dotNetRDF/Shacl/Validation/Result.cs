/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2020 dotNetRDF Project (http://dotnetrdf.org/)
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

namespace VDS.RDF.Shacl.Validation
{
    using System.Diagnostics;
    using System.Linq;
    using VDS.RDF;

    /// <summary>
    /// Represents a SHACL validation result.
    /// </summary>
    public class Result : WrapperNode
    {
        [DebuggerStepThrough]
        private Result(INode node)
            : base(node)
        {
        }

        /// <summary>
        /// Gets or sets the severity of the result.
        /// </summary>
        public INode Severity
        {
            get
            {
                return Vocabulary.ResultSeverity.ObjectsOf(this).SingleOrDefault();
            }

            set
            {
                foreach (var severity in Vocabulary.ResultSeverity.ObjectsOf(this).ToList())
                {
                    Graph.Retract(this, Vocabulary.ResultSeverity, severity);
                }

                if (value is null)
                {
                    return;
                }

                Graph.Assert(this, Vocabulary.ResultSeverity, value);
            }
        }

        /// <summary>
        /// Gets or sets the focus node that has caused thes result.
        /// </summary>
        public INode FocusNode
        {
            get
            {
                return Vocabulary.FocusNode.ObjectsOf(this).SingleOrDefault();
            }

            set
            {
                foreach (var focusNode in Vocabulary.FocusNode.ObjectsOf(this).ToList())
                {
                    Graph.Retract(this, Vocabulary.FocusNode, focusNode);
                }

                if (value is null)
                {
                    return;
                }

                Graph.Assert(this, Vocabulary.FocusNode, value);
            }
        }

        /// <summary>
        /// Gets or sets the value that has caused the result.
        /// </summary>
        public INode ResultValue
        {
            get
            {
                return Vocabulary.Value.ObjectsOf(this).SingleOrDefault();
            }

            set
            {
                foreach (var valueNode in Vocabulary.Value.ObjectsOf(this).ToList())
                {
                    Graph.Retract(this, Vocabulary.Value, valueNode);
                }

                if (value is null)
                {
                    return;
                }

                Graph.Assert(this, Vocabulary.Value, value);
            }
        }

        /// <summary>
        /// Gets or sets the shape that the given focus node was validated against.
        /// </summary>
        public INode SourceShape
        {
            get
            {
                return Vocabulary.SourceShape.ObjectsOf(this).SingleOrDefault();
            }

            set
            {
                foreach (var sourceShape in Vocabulary.SourceShape.ObjectsOf(this).ToList())
                {
                    Graph.Retract(this, Vocabulary.SourceShape, sourceShape);
                }

                if (value is null)
                {
                    return;
                }

                Graph.Assert(this, Vocabulary.SourceShape, value);
            }
        }

        // TODO: Spec says this is a collection
        /// <summary>
        /// Gets or sets additional textual details about the result.
        /// </summary>
        public ILiteralNode Message
        {
            get
            {
                return (ILiteralNode)Vocabulary.ResultMessage.ObjectsOf(this).SingleOrDefault();
            }

            set
            {
                foreach (var sourceShape in Vocabulary.ResultMessage.ObjectsOf(this).ToList())
                {
                    Graph.Retract(this, Vocabulary.ResultMessage, sourceShape);
                }

                if (value is null)
                {
                    return;
                }

                Graph.Assert(this, Vocabulary.ResultMessage, value);
            }
        }

        /// <summary>
        /// Gets or sets the IRI of the constraint component that has caused the result.
        /// </summary>
        public INode SourceConstraintComponent
        {
            get
            {
                return Vocabulary.SourceConstraintComponent.ObjectsOf(this).SingleOrDefault();
            }

            set
            {
                foreach (var sourceConstraintComponent in Vocabulary.SourceConstraintComponent.ObjectsOf(this).ToList())
                {
                    Graph.Retract(this, Vocabulary.SourceConstraintComponent, sourceConstraintComponent);
                }

                if (value is null)
                {
                    return;
                }

                Graph.Assert(this, Vocabulary.SourceConstraintComponent, value);
            }
        }

        /// <summary>
        /// Gets or sets the optional path of the property shape that has caused the result.
        /// </summary>
        public Path ResultPath
        {
            get
            {
                return Vocabulary.ResultPath.ObjectsOf(this).Select(Path.Parse).SingleOrDefault();
            }

            set
            {
                foreach (var sourceConstraintComponent in Vocabulary.ResultPath.ObjectsOf(this).ToList())
                {
                    Graph.Retract(this, Vocabulary.ResultPath, sourceConstraintComponent);
                }

                if (value is null)
                {
                    return;
                }

                Graph.Assert(this, Vocabulary.ResultPath, value);
            }
        }

        /// <summary>
        /// Gets or sets the optional SPARQL-based constraint the has caused the result.
        /// </summary>
        public INode SourceConstraint
        {
            get
            {
                return Vocabulary.SourceConstraint.ObjectsOf(this).SingleOrDefault();
            }

            set
            {
                foreach (var sourceConstraint in Vocabulary.SourceConstraint.ObjectsOf(this).ToList())
                {
                    Graph.Retract(this, Vocabulary.SourceConstraint, sourceConstraint);
                }

                if (value is null)
                {
                    return;
                }

                Graph.Assert(this, Vocabulary.SourceConstraint, value);
            }
        }

        private INode Type
        {
            get
            {
                return Vocabulary.RdfType.ObjectsOf(this).SingleOrDefault();
            }

            set
            {
                foreach (var type in Vocabulary.RdfType.ObjectsOf(this).ToList())
                {
                    Graph.Retract(this, Vocabulary.RdfType, type);
                }

                if (value is null)
                {
                    return;
                }

                Graph.Assert(this, Vocabulary.RdfType, value);
            }
        }

        internal static Result Create(IGraph g)
        {
            var report = new Result(g.CreateBlankNode())
            {
                Type = Vocabulary.ValidationResult,
            };

            return report;
        }

        internal static Result Parse(INode node)
        {
            return new Result(node);
        }
    }
}