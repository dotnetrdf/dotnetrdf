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
    public static class ConstraintsExtensions
    {

        /// <summary>
        /// Checks all spin:constraints for a given Resource set.
        /// </summary>
        /// <param name="dataset">the dataset containing the resource</param>
        /// <param name="resource">the instance to run constraint checks on</param>
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
        /// <param name="resource">the instance to run constraint checks on</param>
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
