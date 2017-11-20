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

namespace VDS.RDF.Query.Spin.Progress
{
    /**
     * A simple implementation of ProgressMonitor that prints messages
     * to System.out.
     *
     * @author Holger Knublauch
     */
    internal class SimpleProgressMonitor : IProgressMonitor
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