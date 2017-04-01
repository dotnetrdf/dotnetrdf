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

namespace VDS.RDF.Query.Inference
{
    /*public static class InferenceHelper
    {
    }*/

    /// <summary>
    /// Helper class containing constants and methods for use in implementing OWL support
    /// </summary>
    public static class OwlHelper
    {
        /// <summary>
        /// Class containing Extraction Mode constants
        /// </summary>
        public static class OwlExtractMode
        {
            /// <summary>
            /// OWL Extraction Mode constants
            /// </summary>
            public const String DefaultStatements = "DefaultStatements",
                                AllClass = "AllClass",
                                AllIndividual = "AllIndividual",
                                AllProperty = "AllProperty",
                                AllStatements = "AllStatements",
                                AllStatementsIncludingJena = "AllStatementsIncludingJena",
                                ClassAssertion = "ClassAssertion",
                                ComplementOf = "ComplementOf",
                                DataPropertyAssertion = "DataPropertyAssertion",
                                DifferentIndividuals = "DifferentIndividuals",
                                DirectClassAssertion = "DirectClassAssertion",
                                DirectSubClassOf = "DirectSubClassOf",
                                DirectSubPropertyOf = "DirectSubPropertyOf",
                                DisjointClasses = "DisjointClasses",
                                DisjointProperties = "DisjointProperties",
                                EquivalentClasses = "EquivalentClasses",
                                EquivalentProperties = "EquivalentProperties",
                                InverseProperties = "InverserProperties",
                                ObjectPropertyAssertion = "ObjectPropertyAssertion",
                                PropertyAssertion = "PropertyAssertion",
                                SameIndividual = "SameIndividual",
                                SubClassOf = "SubClassOf",
                                SubPropertyOf = "SubPropertyOf";
        }

        /// <summary>
        /// OWL Class and Property Constants
        /// </summary>
        public const String OwlNothing = "http://www.w3.org/2002/07/owl#Nothing";
    }

}
