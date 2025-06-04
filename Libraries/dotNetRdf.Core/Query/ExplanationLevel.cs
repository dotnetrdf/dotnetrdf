/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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

namespace VDS.RDF.Query;

/// <summary>
/// Represents the level of Query Explanation that is desired.
/// </summary>
[Flags]
public enum ExplanationLevel
{
    /// <summary>
    /// Specifies No Explanations
    /// </summary>
    None = 0,

    /// <summary>
    /// Specifies Explanations are output to Debug
    /// </summary>
    OutputToDebug = 1,

    /// <summary>
    /// Specifies Explanations are output to Trace
    /// </summary>
    OutputToTrace = 2,

    /// <summary>
    /// Specifies Explanations are output to Console Standard Output
    /// </summary>
    OutputToConsoleStdOut = 4,

    /// <summary>
    /// Specifies Explanations are output to Console Standard Error
    /// </summary>
    OutputToConsoleStdErr = 8,

    /// <summary>
    /// Specifies Explanations are output to Debug and Console Standard Output
    /// </summary>
    OutputDefault = OutputToDebug | OutputToConsoleStdOut,

    /// <summary>
    /// Specifies Explanations are output to all
    /// </summary>
    OutputAll = OutputToDebug | OutputToTrace | OutputToConsoleStdOut | OutputToConsoleStdErr,

    /// <summary>
    /// Show the Thread ID of the Thread evaluating the query (useful in multi-threaded environments)
    /// </summary>
    ShowThreadID = 16,

    /// <summary>
    /// Show the Depth of the Algebra Operator
    /// </summary>
    ShowDepth = 32,

    /// <summary>
    /// Show the Type of the Algebra Operator
    /// </summary>
    ShowOperator = 64,

    /// <summary>
    /// Show the Action being performed (makes it clear whether the explanation marks the start/end of an operation)
    /// </summary>
    ShowAction = 128,

    /// <summary>
    /// Shows Timings for the Query
    /// </summary>
    ShowTimings = 256,

    /// <summary>
    /// Show Intermediate Result Counts at each stage of evaluation
    /// </summary>
    ShowIntermediateResultCount = 512,

    /// <summary>
    /// Shows Basic Information (Depth, Operator and Action)
    /// </summary>
    ShowBasic = ShowDepth | ShowOperator | ShowAction,

    /// <summary>
    /// Shows Default Information (Thread ID, Depth, Operator and Action)
    /// </summary>
    ShowDefault = ShowThreadID | ShowDepth | ShowOperator | ShowAction,

    /// <summary>
    /// Shows All Information
    /// </summary>
    ShowAll = ShowThreadID | ShowDepth | ShowOperator | ShowAction | ShowTimings | ShowIntermediateResultCount,

    /// <summary>
    /// Shows an analysis of BGPs prior to evaluating them
    /// </summary>
    /// <remarks>
    /// This lets you see how many joins, cross products, filters, assignments etc must be applied in each BGP
    /// </remarks>
    AnalyseBgps = 1024,

    /// <summary>
    /// Shows an analysis of Joins prior to evaluating them
    /// </summary>
    /// <remarks>
    /// This lets you see whether the join is a join/cross product and in the case of a Minus whether the RHS can be ignored completely
    /// </remarks>
    AnalyseJoins = 2048,

    /// <summary>
    /// Shows an analysis of Named Graphs used by a Graph clause prior to evaluating them
    /// </summary>
    /// <remarks>
    /// This lets you see how many graphs a given Graph clause will operate over.  As the Graph clause in SPARQL is defined as the union of evaluating the inner operator over each named graph in the dataset graph clauses applied to datasets with many named graphs can be expensive.
    /// </remarks>
    AnalyseNamedGraphs = 4096,

    /// <summary>
    /// Sets whether Evaluation should be simulated (means timings will not be accurate but allows you to explain queries without needing actual data to evaluate them against)
    /// </summary>
    Simulate = 8092,

    /// <summary>
    /// Shows all analysis information
    /// </summary>
    AnalyseAll = AnalyseBgps | AnalyseJoins | AnalyseNamedGraphs,

    /// <summary>
    /// Basic Explanation Level (Console Standard Output and Basic Information)
    /// </summary>
    Basic = OutputToConsoleStdOut | ShowBasic,

    /// <summary>
    /// Default Explanation Level (Default Outputs and Default Information)
    /// </summary>
    Default = OutputDefault | ShowDefault,

    /// <summary>
    /// Detailed Explanation Level (Default Outputs and All Information)
    /// </summary>
    Detailed = OutputDefault | ShowAll,

    /// <summary>
    /// Full Explanation Level (All Outputs, All Information and All Analysis)
    /// </summary>
    Full = OutputAll | ShowAll | AnalyseAll,

    /// <summary>
    /// Basic Explanation Level with Query Evaluation simulated
    /// </summary>
    BasicSimulation = Basic | Simulate,

    /// <summary>
    /// Default Explanation Level with Query Evaluation simulated
    /// </summary>
    DefaultSimulation = Default | Simulate,

    /// <summary>
    /// Detailed Explanation Level with Query Evaluation simulated
    /// </summary>
    DetailedSimulation = Detailed | Simulate,

    /// <summary>
    /// Full Explanation Level with Query Evaluation simulated
    /// </summary>
    FullSimulation = Full | Simulate,
}