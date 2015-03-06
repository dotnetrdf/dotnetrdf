/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System;

namespace VDS.RDF.Query.Spin.Progress
{
    /**
     * A simple implementation of ProgressMonitor that prints messages
     * to System.out.
     *
     * @author Holger Knublauch
     */
    public class SimpleProgressMonitor : IProgressMonitor
    {

        private String name;

        private int currentWork;

        private int totalWork;


        public SimpleProgressMonitor(String name)
        {
            this.name = name;
        }


        public void beginTask(String label, int totalWork)
        {
            println("Beginning task " + label + " (" + totalWork + ")");
            this.totalWork = totalWork;
            this.currentWork = 0;
        }


        public void done()
        {
            println("Done");
        }


        public bool isCanceled()
        {
            return false;
        }


        protected void println(String text)
        {
            Console.WriteLine(name + ": " + text);
        }


        public void setCanceled(bool value)
        {
        }


        public void setTaskName(String value)
        {
            println("Task name: " + value);
        }


        public void subTask(String label)
        {
            println("Subtask: " + label);
        }


        public void worked(int amount)
        {
            currentWork += amount;
            println("Worked " + currentWork + " / " + totalWork);
        }
    }
}