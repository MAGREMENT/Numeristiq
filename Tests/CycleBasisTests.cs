using Model.Core.Graphs;
using Model.Core.Graphs.Implementations;

namespace Tests;

public class CycleBasisTests
{
    [Test]
    public void BasicTest()
    {
        var graph = new HDictionaryLinkGraph<int>();
        graph.Add(1, 2, LinkStrength.Strong);
        graph.Add(2, 3, LinkStrength.Weak);
        graph.Add(3, 4, LinkStrength.Strong);
        graph.Add(4, 1, LinkStrength.Weak);

        var loops = CycleBasis.Find(graph, CycleBasis.DefaultConstructLoop);
        foreach (var loop in loops)
        {
            Console.WriteLine(loop);
        }
        Assert.That(loops, Has.Count.EqualTo(1));

        graph.Add(3, 5, LinkStrength.Strong);
        graph.Add(5, 4, LinkStrength.Weak);
        
        loops = CycleBasis.Find(graph, CycleBasis.DefaultConstructLoop);
        Console.WriteLine();
        foreach (var loop in loops)
        {
            Console.WriteLine(loop);
        }
        Assert.That(loops, Has.Count.EqualTo(2));

        loops = new List<Loop<int, LinkStrength>>(CycleBasis.Multiply(loops, CycleBasis.DefaultCombineLoops, 1));
        Console.WriteLine();
        foreach (var loop in loops)
        {
            Console.WriteLine(loop);
        }
        Assert.That(loops, Has.Count.EqualTo(3));
    }

    [Test]
    public void CombineLoopsTest() //TODO test more situations + links
    {
        var l1 = new Loop<int, LinkStrength>(new[] { 1, 2, 3, 4, 5 }, new LinkStrength[5]);
        var l2 = new Loop<int, LinkStrength>(new[] { 5, 1, 6 }, new LinkStrength[3]);
        var expected = new Loop<int, LinkStrength>(new[] { 1, 2, 3, 4, 5, 6 }, new LinkStrength[6]);

        var result = CycleBasis.DefaultCombineLoops(l1, l2);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(expected));

        l1 = new Loop<int, LinkStrength>(new[] { 3, 2, 1, 4 }, new LinkStrength[4]);
        l2 = new Loop<int, LinkStrength>(new[] { 5, 4, 1, 2, 3 }, new LinkStrength[5]);
        expected = new Loop<int, LinkStrength>(new[] { 3, 5, 4 }, new LinkStrength[3]);
        
        result = CycleBasis.DefaultCombineLoops(l1, l2);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(expected));
    }
}