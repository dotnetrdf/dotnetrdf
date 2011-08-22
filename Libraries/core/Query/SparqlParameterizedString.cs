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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query
{
    /// <summary>
    /// A SPARQL Parameterized String is a String that can contain parameters in the same fashion as a SQL command string
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is intended for use in applications which may want to dynamically build SPARQL queries/updates where user input may comprise individual values in the triples patterns and the applications want to avoid SPARQL injection attacks which change the meaning of the query/update
    /// </para>
    /// <para>
    /// It works broadly in the same way as a SqlCommand would in that you specify a string with paramters specified in the form <strong>@name</strong> and then use various set methods to set the actual values that should be used.  The values are only substituted for parameters when you actually call the <see cref="SparqlParameterizedString.ToString">ToString()</see> method to get the final string representation of the command. E.g.
    /// </para>
    /// <code>
    /// SparqlParameterizedString queryString = @"SELECT * WHERE
    /// {
    ///     ?s a @type .
    /// }";
    /// queryString.SetUri("type", new Uri("http://example.org/myType"));
    /// Console.WriteLine(queryString.ToString());
    /// </code>
    /// <para>
    /// Would result in the following being printed to the Console:
    /// </para>
    /// <code>
    /// SELECT * WHERE
    /// {
    ///     ?s a &lt;http://example.org/myType&gt;
    /// }
    /// </code>
    /// <para>
    /// Calling a Set method to set a parameter that has already been set changes that value and the new value will be used next time you call <see cref="SparqlParameterizedString.ToString">ToString()</see> - this may be useful if you plan to execute a series of queries/updates using a series of values since you need not instantiate a completely new parameterized string each time
    /// </para>
    /// <para>
    /// This class was added to a library based on a suggestion by Alexander Sidorov and ideas from slides from <a href="http://www.slideshare.net/Morelab/sparqlrdqlsparul-injection">Slideshare</a> by Almedia et al
    /// </para>
    /// </remarks>
    public class SparqlParameterizedString
    {
        private String _command = String.Empty;
        private INamespaceMapper _nsmap = new NamespaceMapper(true);
        private Dictionary<String, INode> _parameters = new Dictionary<string, INode>();
        private Dictionary<String, INode> _variables = new Dictionary<string, INode>();
        private SparqlFormatter _formatter = new SparqlFormatter();
        private IGraph _g = new NonIndexedGraph();

        private const String _validParameterNamePattern = "^@?[\\w\\-_]+$";
        private const String _validVariableNamePattern = "^[?$]?[\\w\\-_]+$";

        /// <summary>
        /// Creates a new empty parameterized String
        /// </summary>
        public SparqlParameterizedString()
        {
        }

        /// <summary>
        /// Creates a new parameterized String
        /// </summary>
        /// <param name="command">Command Text</param>
        public SparqlParameterizedString(String command)
            : this()
        {
            this._command = command;
        }

        /// <summary>
        /// Gets/Sets the Namespace Map that is used to prepend PREFIX declarations to the Query/Update
        /// </summary>
        public INamespaceMapper Namespaces
        {
            get
            {
                return this._nsmap;
            }
            set
            {
                if (value != null) this._nsmap = value;
            }
        }

        /// <summary>
        /// Gets/Sets the parameterized Command Text
        /// </summary>
        public String CommandText
        {
            get
            {
                return this._command;
            }
            set
            {
                this._command = value;
            }
        }

        /// <summary>
        /// Gets/Sets the parameterized Query Text
        /// </summary>
        [Obsolete("Deprecated - Use the more descriptive synonym CommandText instead",true)]
        public String QueryText
        {
            get
            {
                return this._command;
            }
            set
            {
                this._command = value;
            }
        }

        /// <summary>
        /// Clears all set Parameters and Variables
        /// </summary>
        public void Clear()
        {
            this.ClearParameters();
            this.ClearVariables();
        }

        /// <summary>
        /// Clears all set Parameters
        /// </summary>
        public void ClearParameters()
        {
            this._parameters.Clear();
        }

        /// <summary>
        /// Clears all set Variables
        /// </summary>
        public void ClearVariables()
        {
            this._variables.Clear();
        }

        /// <summary>
        /// Gets an enumeration of the Variables for which Values have been set
        /// </summary>
        public IEnumerable<KeyValuePair<String, INode>> Variables
        {
            get
            {
                return this._variables;
            }
        }

        /// <summary>
        /// Gets an enumeration of the Parameters for which Values have been set
        /// </summary>
        public IEnumerable<KeyValuePair<String, INode>> Parameters
        {
            get
            {
                return this._parameters;
            }
        }

        /// <summary>
        /// Sets the Value of a Parameter 
        /// </summary>
        /// <param name="name">Parameter Name</param>
        /// <param name="value">Value</param>
        /// <remarks>
        /// Can be used in derived classes to set the value of parameters if the derived class defines additional methods for adding values for parameters
        /// </remarks>
        public void SetParameter(String name, INode value)
        {
            //Only allow the setting of valid parameter names
            if (!Regex.IsMatch(name, _validParameterNamePattern)) throw new FormatException("The parameter name '" + name + "' is not a valid parameter name, parameter names must consist only of alphanumeric characters and hypens/underscores");

            //OPT: Could ensure that the parameter name actually appears in the command?
            name = (name.StartsWith("@")) ? name.Substring(1) : name;

            //Finally can set/update parameter value
            if (this._parameters.ContainsKey(name))
            {
                this._parameters[name] = value;
            }
            else
            {
                this._parameters.Add(name, value);
            }
        }

        /// <summary>
        /// Removes a previously set value for a Parameter
        /// </summary>
        /// <param name="name">Parameter Name</param>
        /// <remarks>
        /// There is generally no reason to do this since you can just set a parameters value to change it
        /// </remarks>
        public void UnsetParameter(String name)
        {
            name = (name.StartsWith("@")) ? name.Substring(1) : name;
            this._parameters.Remove(name);
        }

        /// <summary>
        /// Removes a previously set value for a Variable
        /// </summary>
        /// <param name="name">Variable Name</param>
        /// <remarks>
        /// May be useful if you have a skeleton query/update into which you sometimes substitute values for variables but don't always do so
        /// </remarks>
        public void UnsetVariable(String name)
        {
            name = (name.StartsWith("@")) ? name.Substring(1) : name;
            this._variables.Remove(name);
        }

        /// <summary>
        /// Sets the Value of a Variable
        /// </summary>
        /// <param name="name">Variable Name</param>
        /// <param name="value">Value</param>
        public void SetVariable(String name, INode value)
        {
            //Only allow the setting of valid variable names
            if (!Regex.IsMatch(name, _validVariableNamePattern)) throw new FormatException("The variable name '" + name + "' is not a valid variable name, variable names must consist only of alphanumeric characters and hyphens/underscores");

            if (this._variables.ContainsKey(name))
            {
                this._variables[name] = value;
            }
            else
            {
                this._variables.Add(name, value);
            }
        }

        /// <summary>
        /// Sets the Parameter to an Integer Literal
        /// </summary>
        /// <param name="name">Parameter</param>
        /// <param name="value">Integer</param>
        public void SetLiteral(String name, int value)
        {
            this.SetParameter(name, value.ToLiteral(this._g));
        }

        /// <summary>
        /// Sets the Parameter to an Integer Literal
        /// </summary>
        /// <param name="name">Parameter</param>
        /// <param name="value">Integer</param>
        public void SetLiteral(String name, long value)
        {
            this.SetParameter(name, value.ToLiteral(this._g));
        }

        /// <summary>
        /// Sets the Parameter to an Integer Literal
        /// </summary>
        /// <param name="name">Parameter</param>
        /// <param name="value">Integer</param>
        public void SetLiteral(String name, short value)
        {
            this.SetParameter(name, value.ToLiteral(this._g));
        }

        /// <summary>
        /// Sets the Parameter to a Decimal Literal
        /// </summary>
        /// <param name="name">Parameter</param>
        /// <param name="value">Integer</param>
        public void SetLiteral(String name, decimal value)
        {
            this.SetParameter(name, value.ToLiteral(this._g));
        }

        /// <summary>
        /// Sets the Parameter to a Float Literal
        /// </summary>
        /// <param name="name">Parameter</param>
        /// <param name="value">Integer</param>
        public void SetLiteral(String name, float value)
        {
            this.SetParameter(name, value.ToLiteral(this._g));
        }

        /// <summary>
        /// Sets the Parameter to a Double Literal
        /// </summary>
        /// <param name="name">Parameter</param>
        /// <param name="value">Integer</param>
        public void SetLiteral(String name, double value)
        {
            this.SetParameter(name, value.ToLiteral(this._g));
        }

        /// <summary>
        /// Sets the Parameter to a Date Time Literal
        /// </summary>
        /// <param name="name">Parameter</param>
        /// <param name="value">Integer</param>
        public void SetLiteral(String name, DateTime value)
        {
            this.SetParameter(name, value.ToLiteral(this._g));
        }

        /// <summary>
        /// Sets the Parameter to a Duration Literal
        /// </summary>
        /// <param name="name">Parameter</param>
        /// <param name="value">Integer</param>
        public void SetLiteral(String name, TimeSpan value)
        {
            this.SetParameter(name, value.ToLiteral(this._g));
        }

        /// <summary>
        /// Sets the Parameter to a Boolean Literal
        /// </summary>
        /// <param name="name">Parameter</param>
        /// <param name="value">Integer</param>
        public void SetLiteral(String name, bool value)
        {
            this.SetParameter(name, value.ToLiteral(this._g));
        }

        /// <summary>
        /// Sets the Parameter to an Untyped Literal
        /// </summary>
        /// <param name="name">Parameter</param>
        /// <param name="value">Integer</param>
        public void SetLiteral(String name, String value)
        {
            if (value == null) throw new ArgumentNullException("value", "Cannot set a Literal to be null");
            this.SetParameter(name, new LiteralNode(this._g, value));
        }

        /// <summary>
        /// Sets the Parameter to a Typed Literal
        /// </summary>
        /// <param name="name">Parameter</param>
        /// <param name="value">Integer</param>
        /// <param name="datatype">Datatype URI</param>
        public void SetLiteral(String name, String value, Uri datatype)
        {
            if (value == null) throw new ArgumentNullException("value", "Cannot set a Literal to be null");
            if (datatype == null)
            {
                this.SetParameter(name, new LiteralNode(this._g, value));
            }
            else
            {
                this.SetParameter(name, new LiteralNode(this._g, value, datatype));
            }
        }

        /// <summary>
        /// Sets the Parameter to a Literal with a Language Specifier
        /// </summary>
        /// <param name="name">Parameter</param>
        /// <param name="value">Integer</param>
        /// <param name="lang">Language Specifier</param>
        public void SetLiteral(String name, String value, String lang)
        {
            if (value == null) throw new ArgumentNullException("value", "Cannot set a Literal to be null");
            if (lang == null) throw new ArgumentNullException("lang", "Cannot set a Literal to have a null Language");
            this.SetParameter(name, new LiteralNode(this._g, value, lang));
        }

        /// <summary>
        /// Sets the Parameter to a URI
        /// </summary>
        /// <param name="name">Parameter</param>
        /// <param name="value">URI</param>
        public void SetUri(String name, Uri value)
        {
            if (value == null) throw new ArgumentNullException("value", "Cannot set a URI to be null");
            this.SetParameter(name, new UriNode(this._g, value));
        }

        /// <summary>
        /// Sets the Parameter to be a Blank Node with the given ID
        /// </summary>
        /// <param name="name">Parameter</param>
        /// <param name="value">Node ID</param>
        /// <remarks>
        /// Only guarantees that the Blank Node ID will not clash with any other Blank Nodes added by other calls to this method or it's overload which generates anonymous Blank Nodes.  If the base query text into which you are inserting parameters contains Blank Nodes then the IDs generated here may clash with those IDs.
        /// </remarks>
        public void SetBlankNode(String name, String value)
        {
            if (value == null) throw new ArgumentNullException("value", "Cannot set a Blank Node to have a null ID");
            if (value.Equals(String.Empty)) throw new ArgumentException("value", "Cannot set a Blank Node to have an empty ID");
            this.SetParameter(name, this._g.CreateBlankNode(value));
        }

        /// <summary>
        /// Sets the Parameter to be a new anonymous Blank Node
        /// </summary>
        /// <param name="name">Parameter</param>
        /// <remarks>
        /// Only guarantees that the Blank Node ID will not clash with any other Blank Nodes added by other calls to this method or it's overload which takes an explicit Node ID.  If the base query text into which you are inserting parameters contains Blank Nodes then the IDs generated here may clash with those IDs.
        /// </remarks>
        public void SetBlankNode(String name)
        {
            this.SetParameter(name, this._g.CreateBlankNode());
        }

        /// <summary>
        /// Returns the actual Query/Update String with parameter and variable values inserted
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            String output = String.Empty;

            //First prepend any Namespace Declarations
            foreach (String prefix in this._nsmap.Prefixes)
            {
                output += "PREFIX " + prefix + ": <" + this._formatter.FormatUri(this._nsmap.GetNamespaceUri(prefix)) + ">\n";
            }
                
            //Then add the actual Command Text
            output += this._command;

            //Finally substitue in values for parameters and variables

            //Make the replacements starting with the longest parameter names first so in the event
            //of one parameter name being a prefix of another we've already replaced the longer name
            //first
            foreach (String param in this._parameters.Keys.OrderByDescending(k => k.Length))
            {
                if (this._parameters[param] != null)
                {
                    //Do a Regex based replace to avoid replacing other parameters whose names may be suffixes/prefixes of this name
                    output = Regex.Replace(output, "(@" + param + ")([^\\w]|$)", this._formatter.Format(this._parameters[param]) + "$2");
                }
            }

            //Do Variable replacements after Parameter replacements
            foreach (String var in this._variables.Keys.OrderByDescending(k => k.Length))
            {
                if (this._variables[var] != null)
                {
                    //Do a Reged based replace to avoid replacing other variables whose names may be suffixes/prefixes of this name
                    output = Regex.Replace(output, "([?$]" + var + ")([^\\w]|$)", this._formatter.Format(this._variables[var]) + "$2");
                }
            }

            return output;
        }
    }
}
