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
using System.Linq;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.PropertyFunctions
{
    /// <summary>
    /// Helper Class containing functions useful in working with property functions
    /// </summary>
    public static class PropertyFunctionHelper
    {
        /// <summary>
        /// Used to extract the patterns that make up property functions
        /// </summary>
        /// <param name="patterns">Triple Patterns</param>
        /// <returns></returns>
        public static List<IPropertyFunctionPattern> ExtractPatterns(IEnumerable<ITriplePattern> patterns)
        {
            //Do a first pass which simply looks to find any 'magic' properties
            Dictionary<PatternItem, PropertyFunctionInfo> funcInfo = new Dictionary<PatternItem, PropertyFunctionInfo>();
            List<IMatchTriplePattern> ps = patterns.OfType<IMatchTriplePattern>().ToList();
            if (ps.Count == 0) return new List<IPropertyFunctionPattern>();
            foreach (IMatchTriplePattern tp in ps)
            {
                NodeMatchPattern predItem = tp.Predicate as NodeMatchPattern;
                if (predItem == null) continue;
                IUriNode predNode = predItem.Node as IUriNode;
                if (predNode == null) continue;
                if (PropertyFunctionFactory.IsPropertyFunction(predNode.Uri))
                {
                    PropertyFunctionInfo info = new PropertyFunctionInfo(predNode.Uri);
                    info.Patterns.Add(tp);
                    funcInfo.Add(tp.Subject, info);
                }
            }
            //Remove any Patterns we found from the original patterns
            foreach (PropertyFunctionInfo info in funcInfo.Values)
            {
                info.Patterns.ForEach(tp => ps.Remove(tp));
            }

            if (funcInfo.Count == 0) return new List<IPropertyFunctionPattern>();

            //Now for each 'magic' property we found do a further search to see if we are using
            //the collection forms to provide extended arguments
            foreach (PatternItem key in funcInfo.Keys)
            {
                if (key.VariableName != null && key.VariableName.StartsWith("_:"))
                {
                    //If LHS is a blank node may be collection form
                    int count = funcInfo[key].Patterns.Count;
                    ExtractRelatedPatterns(key, key, ps, funcInfo);
                    if (funcInfo[key].Patterns.Count == count)
                    {
                        //If no further patterns found just single LHS argument
                        funcInfo[key].LhsArgs.Add(key);
                    }
                }
                else
                {
                    //Otherwise key is the only LHS argument
                    funcInfo[key].LhsArgs.Add(key);
                }
                PatternItem searchKey = funcInfo[key].Patterns.First().Object;
                if (searchKey.VariableName != null && searchKey.VariableName.StartsWith("_:"))
                {
                    //If RHS is a blank node may be collection form
                    int count = funcInfo[key].Patterns.Count;
                    ExtractRelatedPatterns(key, searchKey, ps, funcInfo);
                    if (funcInfo[key].Patterns.Count == count)
                    {
                        //If no further patterns found just single RHS argument
                        funcInfo[key].RhsArgs.Add(searchKey);
                    }
                }
                else
                {
                    //Otherwise single RHS argument
                    funcInfo[key].RhsArgs.Add(searchKey);
                }
            }

            //Now try to create actual property functions
            List<IPropertyFunctionPattern> propFunctions = new List<IPropertyFunctionPattern>();
            foreach (PatternItem key in funcInfo.Keys)
            {
                IPropertyFunctionPattern propFunc;
                if (PropertyFunctionFactory.TryCreatePropertyFunction(funcInfo[key], out propFunc))
                {
                    propFunctions.Add(propFunc);
                }
            }
            return propFunctions;
        }

        /// <summary>
        /// Used to help extract the patterns that make up a property function pattern
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="subj">Subject</param>
        /// <param name="ps">Patterns</param>
        /// <param name="funcPatterns">Discovered Full Text Patterns</param>
        static void ExtractRelatedPatterns(PatternItem key, PatternItem subj, List<IMatchTriplePattern> ps, Dictionary<PatternItem, PropertyFunctionInfo> funcInfo)
        {
            bool recurse = true;
            PatternItem nextSubj = null;
            while (recurse)
            {
                foreach (IMatchTriplePattern tp in ps)
                {
                    if (tp.Subject.VariableName == subj.VariableName)
                    {
                        NodeMatchPattern predItem = tp.Predicate as NodeMatchPattern;
                        if (predItem == null) continue;
                        IUriNode predNode = predItem.Node as IUriNode;
                        if (predNode == null) continue;
                        if (predNode.Uri.AbsoluteUri.Equals(RdfSpecsHelper.RdfListFirst))
                        {
                            funcInfo[key].Patterns.Add(tp);
                            ps.Remove(tp);
                        }
                        else if (predNode.Uri.AbsoluteUri.Equals(RdfSpecsHelper.RdfListRest))
                        {
                            funcInfo[key].Patterns.Add(tp);
                            ps.Remove(tp);
                            recurse = tp.Object.VariableName != null;
                            nextSubj = tp.Object;
                        }
                    }
                }

                if (nextSubj == null) throw new RdfQueryException("Failed to find expected rdf:rest property");
            }
        }
    }
}
