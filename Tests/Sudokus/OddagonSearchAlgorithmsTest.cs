﻿using Model.Core.Graphs;
using Model.Sudokus;
using Model.Sudokus.Solver;
using Model.Sudokus.Solver.Utility.Graphs.ConstructRules;
using Model.Sudokus.Solver.Utility.Oddagons;
using Model.Sudokus.Solver.Utility.Oddagons.Algorithms;
using Model.Utility;
using Tests.Utility;

namespace Tests.Sudokus;

public class OddagonSearchAlgorithmsTest
{
    /*
     TODO : expanding DFS algorithm ? like v1 but when max length -> remove first element from current chain and continue
     #1 OddagonSearchAlgorithmV1
        Total: 121,9006 ms
        Average: 40,63353333333333 ms
        Minimum: 37,3837 ms on try #3
        Maximum: 44,6252 ms on try #2
        Ignored: 1
    */
    [Test]
    public void Test()
    {
        var solver = new SudokuSolver();
        solver.SetState(SudokuTranslator.TranslateBase32Format("h00ht021s003l00509j009u005s0t0l00h0303l005h0090h8121l005h0030hs0s02109l0k0810h0911210503k00921l00305k0l0810hn0l0608103090hh0050h03h041210509h081810509h00hh0034121",
            DefaultBase32Alphabet.Instance));
        solver.PreComputer.SimpleGraph.Construct(CellStrongLinkConstructionRule.Instance,
            CellWeakLinkConstructionRule.Instance, UnitWeakLinkConstructionRule.Instance,
            UnitStrongLinkConstructionRule.Instance);
        var graph = solver.PreComputer.SimpleGraph.Graph;
        var write = new bool[2];
        
        ImplementationSpeedComparator.Compare<IOddagonSearchAlgorithm>(algo =>
            {
                algo.MaxLength = 7;
                algo.MaxGuardians = 3;
                var result = algo.Search(solver, graph);

                var index = algo switch
                {
                    OddagonSearchAlgorithmV1 => 0,
                    OddagonSearchAlgorithmV3 => 1,
                    _ => throw new Exception()
                };
                if (!write[index])
                {
                    Console.WriteLine("Count : " + result.Count);
                    var hs = new HashSet<AlmostOddagon>(result);
                    Console.WriteLine("Unique count : " + hs.Count);

                    foreach (var o in result)
                    {
                        Console.WriteLine(o);
                    }
                    
                    write[index] = true;
                }
                
                Assert.That(result, Has.Count.GreaterThan(0));
            }, 3, new OddagonSearchAlgorithmV1()/*, new OddagonSearchAlgorithmV3()*/);
    }

    [Test]
    public void EqualsAndHashCodeTest()
    {
        var ao1 = new AlmostOddagon(new Loop<CellPossibility, LinkStrength>(new[]
        {
            new CellPossibility(0, 0, 1), new CellPossibility(7, 2, 3),
            new CellPossibility(5, 8, 4)
        }, new LinkStrength[3]), new[]
        {
            new CellPossibility(1, 2, 3), new CellPossibility(5, 7, 2)
        });
        
        var ao2 = new AlmostOddagon(new Loop<CellPossibility, LinkStrength>(new[]
        {
            new CellPossibility(5, 8, 4), new CellPossibility(0, 0, 1), 
            new CellPossibility(7, 2, 3)
        }, new LinkStrength[3]), new[]
        {
            new CellPossibility(5, 7, 2), new CellPossibility(1, 2, 3)
        });

        Assert.That(ao1.GetHashCode(), Is.EqualTo(ao2.GetHashCode()));
        Assert.That(ao1, Is.EqualTo(ao2));
    }
}