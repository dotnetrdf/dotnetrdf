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
using System.Linq;
using System.Text;
using VDS.RDF.Query.Spin.Progress;
using VDS.RDF.Query.Spin.Statistics;

namespace VDS.RDF.Query.Spin.Constraints
{
    /// <summary>
    /// Provides extensions to the SpinWrappedDataset class to check SPIN constraints
    /// </summary>
    internal static class ConstraintsExtensions
    {

        /// <summary>
        /// Checks all spin:constraints for a given Resource set.
        /// </summary>
        /// <param name="dataset">the dataset containing the resource</param>
        /// <param name="resources">the instances to run constraint checks on</param>
        /// <param name="monitor">an (optional) progress monitor (currently ignored)</param>
        /// <returns>a List of ConstraintViolations (empty if all is OK)</returns>
        public static List<ConstraintViolation> CheckConstraints(this SpinWrappedDataset dataset, IEnumerable<INode> resources, IProgressMonitor monitor)
        {
            return CheckConstraints(dataset, resources, new List<SPINStatistics>(), monitor);
        }

        /// <summary>
        /// Checks all spin:constraints for a given Resource set.
        /// </summary>
        /// <param name="dataset">the model containing the resource</param>
        /// <param name="resources">the instances to run constraint checks on</param>
        /// <param name="stats">an (optional) List to add statistics to</param>
        /// <param name="monitor">an (optional) progress monitor (currently ignored)</param>
        /// <returns>a List of ConstraintViolations (empty if all is OK)</returns>
        public static List<ConstraintViolation> CheckConstraints(this SpinWrappedDataset dataset, IEnumerable<INode> resources, List<SPINStatistics> stats, IProgressMonitor monitor)
        {
            List<ConstraintViolation> results = new List<ConstraintViolation>();
            //SPINConstraints.addConstraintViolations(this, results, dataset, resource, SPIN.constraint, false, stats, monitor);
            return results;
        }

        /// <summary>
        /// Checks all instances in a given Model against all spin:constraints and returns a List of constraint violations. 
        /// A IProgressMonitor can be provided to enable the user to get intermediate status reports and to cancel the operation.
        /// </summary>
        /// <param name="dataset">the dataset to run constraint checks on</param>
        /// <param name="monitor">an (optional) progress monitor (currently ignored)</param>
        /// <returns>a List of ConstraintViolations (empty if all is OK)</returns>
        public static List<ConstraintViolation> CheckConstraints(this SpinWrappedDataset dataset, IProgressMonitor monitor)
        {
            return CheckConstraints(dataset, (List<SPINStatistics>)null, monitor);
        }


        /// <summary>
        /// Checks all instances in a given Model against all spin:constraints and returns a List of constraint violations. 
        /// A IProgressMonitor can be provided to enable the user to get intermediate status reports and to cancel the operation.
        /// </summary>
        /// <param name="dataset">the dataset to run constraint checks on</param>
        /// <param name="stats">an (optional) List to add statistics to</param>
        /// <param name="monitor">an (optional) progress monitor (currently ignored)</param>
        /// <returns>a List of ConstraintViolations (empty if all is OK)</returns>
        public static List<ConstraintViolation> CheckConstraints(this SpinWrappedDataset dataset, List<SPINStatistics> stats, IProgressMonitor monitor)
        {
            List<ConstraintViolation> results = new List<ConstraintViolation>();
            //SPINConstraints.run(this, dataset, results, stats, monitor);
            return results;
        }

    }
}
