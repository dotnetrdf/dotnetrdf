/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System;

namespace VDS.RDF.Query.Spin.Progress
{


    /**
     * An abstraction of the Eclipse IProgressMonitor for intermediate
     * status messages and the ability to cancel long-running processes.
     * 
     * @author Holger Knublauch
     */
    public interface IProgressMonitor
    {

        bool isCanceled();


        void beginTask(String label, int totalWork);


        void done();


        void setCanceled(bool value);


        void setTaskName(String value);


        void subTask(String label);


        void worked(int amount);
    }
}