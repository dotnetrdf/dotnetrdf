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
            this.Apply(g, g);
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

            //Apply each rule in turn
            foreach (String[] rule in this._rules)
            {
                //Build the final version of the rule text for the given input and output
                StringBuilder ruleText = new StringBuilder();

                //If there's a Base URI on the Output Graph need a WITH clause
                if (output.BaseUri != null)
                {
                    ruleText.AppendLine("WITH <" + this._formatter.FormatUri(output.BaseUri) + ">");
                }
                ruleText.AppendLine(rule[0]);
                //If there's a Base URI on the Input Graph need a USING clause
                if (input.BaseUri != null)
                {
                    ruleText.AppendLine("USING <" + this._formatter.FormatUri(input.BaseUri) + ">");
                }
                ruleText.AppendLine(rule[1]);

                ISyntaxValidationResults results = this._validator.Validate(ruleText.ToString());
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
            INode implies = g.CreateUriNode(new Uri("http://www.w3.org/2000/10/swap/log#implies"));

            foreach (Triple t in g.GetTriplesWithPredicate(implies))
            {
                if (t.Subject.NodeType == NodeType.GraphLiteral && t.Object.NodeType == NodeType.GraphLiteral)
                {
                    this.TryCreateRule(t);
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

            //Generate the INSERT part of the Command
            output.AppendLine("INSERT");
            output.AppendLine("{");
            foreach (Triple x in ((IGraphLiteralNode)t.Object).SubGraph.Triples)
            {
                if (vars == null)
                {
                    output.AppendLine(this._formatter.Format(x));
                }
                else
                {
                    if (vars.IsVariable(x.Subject))
                    {
                        if (!variableMap.ContainsKey(x.Subject))
                        {
                            variableMap.Add(x.Subject, new VariableNode(null, "auto-rule-var-" + nextVarID));
                            nextVarID++;
                        }
                        output.Append(this._formatter.Format(variableMap[x.Subject]));
                    } 
                    else
                    {
                        output.Append(this._formatter.Format(x.Subject));
                    }
                    output.Append(' ');
                    if (vars.IsVariable(x.Predicate))
                    {
                        if (!variableMap.ContainsKey(x.Predicate))
                        {
                            variableMap.Add(x.Predicate, new VariableNode(null, "auto-rule-var-" + nextVarID));
                            nextVarID++;
                        }
                        output.Append(this._formatter.Format(variableMap[x.Predicate]));
                    }
                    else
                    {
                        output.Append(this._formatter.Format(x.Predicate));
                    }
                    output.Append(' ');
                    if (vars.IsVariable(x.Object))
                    {
                        if (!variableMap.ContainsKey(x.Object))
                        {
                            variableMap.Add(x.Object, new VariableNode(null, "auto-rule-var-" + nextVarID));
                            nextVarID++;
                        }
                        output.Append(this._formatter.Format(variableMap[x.Object]));
                    }
                    else
                    {
                        output.Append(this._formatter.Format(x.Object));
                    }
                    output.AppendLine(" .");
                }
            }
            output.AppendLine("}");
            rule[0] = output.ToString();

            //Generate the WHERE part of the Command
            output = new StringBuilder();
            output.AppendLine("WHERE");
            output.AppendLine("{");
            foreach (Triple x in ((IGraphLiteralNode)t.Subject).SubGraph.Triples)
            {
                if (vars == null)
                {
                    output.AppendLine(this._formatter.Format(x));
                }
                else
                {
                    if (vars.IsVariable(x.Subject))
                    {
                        if (!variableMap.ContainsKey(x.Subject))
                        {
                            variableMap.Add(x.Subject, new VariableNode(null, "auto-rule-var-" + nextVarID));
                            nextVarID++;
                        }
                        output.Append(this._formatter.Format(variableMap[x.Subject]));
                    } 
                    else
                    {
                        output.Append(this._formatter.Format(x.Subject));
                    }
                    output.Append(' ');
                    if (vars.IsVariable(x.Predicate))
                    {
                        if (!variableMap.ContainsKey(x.Predicate))
                        {
                            variableMap.Add(x.Predicate, new VariableNode(null, "auto-rule-var-" + nextVarID));
                            nextVarID++;
                        }
                        output.Append(this._formatter.Format(variableMap[x.Predicate]));
                    }
                    else
                    {
                        output.Append(this._formatter.Format(x.Predicate));
                    }
                    output.Append(' ');
                    if (vars.IsVariable(x.Object))
                    {
                        if (!variableMap.ContainsKey(x.Object))
                        {
                            variableMap.Add(x.Object, new VariableNode(null, "auto-rule-var-" + nextVarID));
                            nextVarID++;
                        }
                        output.Append(this._formatter.Format(variableMap[x.Object]));
                    }
                    else
                    {
                        output.Append(this._formatter.Format(x.Object));
                    }
                    output.AppendLine(" .");
                }
            }
            output.AppendLine("}");
            rule[1] = output.ToString();
            
            ISyntaxValidationResults results = this._validator.Validate(rule[0] + rule[1]);
            if (results.IsValid)
            {
                this._rules.Add(rule);
            }
        }
    }
}
