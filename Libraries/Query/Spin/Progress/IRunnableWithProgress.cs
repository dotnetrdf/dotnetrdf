/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
namespace VDS.RDF.Query.Spin.Progress
{
    /**
     * A generic interface similar to Runnable, but with an additional
     * argument that allows the Runnable to display progress.
     *
     * @author Holger Knublauch
     */
    public interface IRunnableWithProgress
    {

        /**
         * Runs the runnable.
         * @param monitor  an optional ProgressMonitor
         */
        void run(IProgressMonitor monitor);
    }
}