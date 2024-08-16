using Model.Core.Graphs;
using Model.Core.Graphs.Implementations;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Sudokus.Solver.Utility.Graphs.Implementations;

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
        foreach (var loop in loops)
        {
            Console.WriteLine(loop);
        }
        Assert.That(loops, Has.Count.EqualTo(2));
    }
}