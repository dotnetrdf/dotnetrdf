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
using System.Text;
using VDS.RDF.Parsing.Validation;
using VDS.RDF.Update;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Inference
{
    /// <summary>
    /// An Inference Engine that supports simple N3 rules
    /// </summary>
    /// <remarks>
    /// <para>
    /// This reasoner should be initialised with a Graph that contains simple N3 rules such as the following:
    /// </para>
    /// <code>
    /// { ?x a ?type } => { ?type a rdfs:Class }.
    /// </code>
    /// <para>
    /// When initialised the reasoner takes account of variables declared with <em>@forAll</em> and <em>@forSome</em> directives though no guarantees that scoping will be correct if you've got multiple <em>@forAll</em> and <em>@forSome</em> directives.
    /// </para>
    /// <para>
    /// When the reasoner is applied to a Graph rules are implemented by generating a SPARQL Update INSERT command like the following and executing it on the given Graph
    /// </para>
    /// <code>
    /// INSERT
    /// {
    ///   ?type a rdfs:Class .
    /// }
    /// WHERE
    /// {
    ///   ?x a ?type .
    /// }
    /// </code>
    /// </remarks>
    public class SimpleN3RulesReasoner : IInferenceEngine
    {
        private List<String[]> _rules = new List<String[]>();
        private SparqlUpdateValidator _validator = new SparqlUpdateValidator();
        private SparqlFormatter _formatter = new SparqlFormatter();

        /// <summary>
        /// Applies reasoning to the given Graph materialising the generated Triples in the same Graph
        /// </summary>
        /// <param name="g">Graph</param>
        public void Apply(IGraph g)
        {
            Apply(g, g);
        }

        /// <summary>
        /// Applies reasoning on the Input Graph materialising the generated Triples in the Output Graph
        /// </summary>
        /// <param name="input">Input Graph</param>
        /// <param name="output">Output Graph</param>
        public void Apply(IGraph input, IGraph output)
        {
            TripleStore store = new TripleStore();
            store.Add(input);
            if (!ReferenceEquals(input, output))
            {
                store.Add(output, true);
            }

            // Apply each rule in turn
            foreach (String[] rule in _rules)
            {
                // Build the final version of the rule text for the given input and output
                StringBuilder ruleText = new StringBuilder();

                // If there's a Base URI on the Output Graph need a WITH clause
                if (output.BaseUri != null)
                {
                    ruleText.AppendLine("WITH <" + _formatter.FormatUri(output.BaseUri) + ">");
                }
                ruleText.AppendLine(rule[0]);
                // If there's a Base URI on the Input Graph need a USING clause
                if (input.BaseUri != null)
                {
                    ruleText.AppendLine("USING <" + _formatter.FormatUri(input.BaseUri) + ">");
                }
                ruleText.AppendLine(rule[1]);

                ISyntaxValidationResults results = _validator.Validate(ruleText.ToString());
                if (results.IsValid)
                {
                    store.ExecuteUpdate((SparqlUpdateCommandSet)results.Result);
                }
            }
        }

        /// <summary>
        /// Initialises the Reasoner
        /// </summary>
        /// <param name="g">Rules Graph</param>
        public void Initialise(IGraph g)
        {
            INode implies = g.CreateUriNode(UriFactory.Create("http://www.w3.org/2000/10/swap/log#implies"));

            foreach (Triple t in g.GetTriplesWithPredicate(implies))
            {
                if (t.Subject.NodeType == NodeType.GraphLiteral && t.Object.NodeType == NodeType.GraphLiteral)
                {
                    TryCreateRule(t);
                }
            }
        }

        /// <summary>
        /// Tries to create a Rule
        /// </summary>
        /// <param name="t">Triple</param>
        private void TryCreateRule(Triple t)
        {
            String[] rule = new String[2];
            Dictionary<INode, INode> variableMap = new Dictionary<INode, INode>();
            int nextVarID = 1;
            VariableContext vars = null;
            if (t.Context != null && t.Context is VariableContext) vars = (VariableContext)t.Context;

            StringBuilder output = new StringBuilder();

            // Generate the INSERT part of the Command
            output.AppendLine("INSERT");
            output.AppendLine("{");
            foreach (Triple x in ((IGraphLiteralNode)t.Object).SubGraph.Triples)
            {
                if (vars == null)
                {
                    output.AppendLine(_formatter.Format(x));
                }
                else
                {
                    if (vars.IsVariable(x.Subject))
                    {
                        if (!variableMap.ContainsKey(x.Subject))
                        {
                            variableMap.Add(x.Subject, new VariableNode(null, "autoRuleVar" + nextVarID));
                            nextVarID++;
                        }
                        output.Append(_formatter.Format(variableMap[x.Subject]));
                    } 
                    else
                    {
                        output.Append(_formatter.Format(x.Subject));
                    }
                    output.Append(' ');
                    if (vars.IsVariable(x.Predicate))
                    {
                        if (!variableMap.ContainsKey(x.Predicate))
                        {
                            variableMap.Add(x.Predicate, new VariableNode(null, "autoRuleVar" + nextVarID));
                            nextVarID++;
                        }
                        output.Append(_formatter.Format(variableMap[x.Predicate]));
                    }
                    else
                    {
                        output.Append(_formatter.Format(x.Predicate));
                    }
                    output.Append(' ');
                    if (vars.IsVariable(x.Object))
                    {
                        if (!variableMap.ContainsKey(x.Object))
                        {
                            variableMap.Add(x.Object, new VariableNode(null, "autoRuleVar" + nextVarID));
                            nextVarID++;
                        }
                        output.Append(_formatter.Format(variableMap[x.Object]));
                    }
                    else
                    {
                        output.Append(_formatter.Format(x.Object));
                    }
                    output.AppendLine(" .");
                }
            }
            output.AppendLine("}");
            rule[0] = output.ToString();

            // Generate the WHERE part of the Command
            output = new StringBuilder();
            output.AppendLine("WHERE");
            output.AppendLine("{");
            foreach (Triple x in ((IGraphLiteralNode)t.Subject).SubGraph.Triples)
            {
                if (vars == null)
                {
                    output.AppendLine(_formatter.Format(x));
                }
                else
                {
                    if (vars.IsVariable(x.Subject))
                    {
                        if (!variableMap.ContainsKey(x.Subject))
                        {
                            variableMap.Add(x.Subject, new VariableNode(null, "autoRuleVar" + nextVarID));
                            nextVarID++;
                        }
                        output.Append(_formatter.Format(variableMap[x.Subject]));
                    } 
                    else
                    {
                        output.Append(_formatter.Format(x.Subject));
                    }
                    output.Append(' ');
                    if (vars.IsVariable(x.Predicate))
                    {
                        if (!variableMap.ContainsKey(x.Predicate))
                        {
                            variableMap.Add(x.Predicate, new VariableNode(null, "autoRuleVar" + nextVarID));
                            nextVarID++;
                        }
                        output.Append(_formatter.Format(variableMap[x.Predicate]));
                    }
                    else
                    {
                        output.Append(_formatter.Format(x.Predicate));
                    }
                    output.Append(' ');
                    if (vars.IsVariable(x.Object))
                    {
                        if (!variableMap.ContainsKey(x.Object))
                        {
                            variableMap.Add(x.Object, new VariableNode(null, "autoRuleVar" + nextVarID));
                            nextVarID++;
                        }
                        output.Append(_formatter.Format(variableMap[x.Object]));
                    }
                    else
                    {
                        output.Append(_formatter.Format(x.Object));
                    }
                    output.AppendLine(" .");
                }
            }
            output.AppendLine("}");
            rule[1] = output.ToString();
            
            ISyntaxValidationResults results = _validator.Validate(rule[0] + rule[1]);
            if (results.IsValid)
            {
                _rules.Add(rule);
            }
        }
    }
}
