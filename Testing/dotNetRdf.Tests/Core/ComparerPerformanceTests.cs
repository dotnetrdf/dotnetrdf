using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;

namespace VDS.RDF;

[Trait("Category", "performance")]
public class ComparerPerformanceTests
{

    private List<INode> GenerateIntegerNodes(int amount)
    {
        var g = new Graph();
        var ns = new List<INode>(amount);
        var rnd = new Random();
        while (ns.Count < amount)
        {
            ns.Add(rnd.Next(Int32.MaxValue).ToLiteral(g));
        }

        return ns;
    }

    private void TestSpeed(IEnumerable<INode> nodes, IComparer<INode> comparer, bool expectFaster)
    {
        var defaultSorted = new List<INode>(nodes);
        var timer = new Stopwatch();
        timer.Start();
        defaultSorted.Sort();
        timer.Stop();

        Console.WriteLine("Default Sort: " + timer.Elapsed);
        var defTime = timer.ElapsedTicks;

        defaultSorted.Clear();
        defaultSorted = null;
        GC.GetTotalMemory(true);

        var custSorted = new List<INode>(nodes);
        timer.Reset();
        timer.Start();
        custSorted.Sort(comparer);
        timer.Stop();

        custSorted.Clear();
        custSorted = null;
        GC.GetTotalMemory(true);

        Console.WriteLine(comparer.GetType().Name + " Sort: " + timer.Elapsed);
        var custTime = timer.ElapsedTicks;

        if (expectFaster)
        {
            Console.WriteLine("Speed Up: " + ((double) defTime) / ((double) custTime));
            Assert.True(custTime <= defTime, comparer.GetType().Name + " should be faster");
        }
        else
        {
            Console.WriteLine("Slow Down: " + ((double) defTime) / ((double) custTime));
            Assert.True(defTime <= custTime, comparer.GetType().Name + " should be slower");
        }
    }

    [Fact]
    public void NodeCompareSpeed1()
    {
        //Generate 10,000 node list of random integer nodes
        List<INode> ns = GenerateIntegerNodes(10000);

        TestSpeed(ns, new FastNodeComparer(), true);
    }

    [Fact]
    [Trait("Coverage", "Skip")]
    public void NodeCompareSpeed2()
    {
        //Generate 100,000 node list of random integer nodes
        List<INode> ns = GenerateIntegerNodes(100000);

        TestSpeed(ns, new FastNodeComparer(), true);
    }

    [Fact]
    [Trait("Coverage", "Skip")]
    [Trait("Category", "explicit")]
    public void NodeCompareSpeed3()
    {
        //Generate 1,000,000 node list of random integer nodes
        List<INode> ns = GenerateIntegerNodes(1000000);

        TestSpeed(ns, new FastNodeComparer(), true);
    }
}