using Model.Core.Graphs;
using Model.Core.Graphs.Implementations;
using Model.Sudokus;
using Model.Sudokus.Solver;
using Model.Sudokus.Solver.Utility.Graphs.ConstructRules;
using Model.Utility;
using Tests.Utility;

namespace Tests;

public class GraphTests
{
    [Test]
    public void SpeedSudokuLinkGraphTest()
    {
        var solver = new SudokuSolver();
        var sudoku = SudokuTranslator.TranslateBase32Format(
            "0hj0a6t009t21474n04eh8062146li815cloecj8a4tgc4tg1s7c032e2o0mc811ea0cg1c80e81h0ko42kq215e582a41h005a2qa1o9a9oa02gag03g105411818110309c0e0e0g10h05g10541180h1803a0a0",
            DefaultBase32Alphabet.Instance);
        solver.SetState(sudoku);
        
        var graphs = new (ConstructedGraph<ISudokuSolverData, IGraph<ISudokuElement, LinkStrength>>, string)[]
        {
            (new(GraphCreator.CreateForLink<ISudokuElement>(GraphCollectionType.HashSet, false), solver), "Single HashSet"),
            (new(GraphCreator.CreateForLink<ISudokuElement>(GraphCollectionType.UniqueList, false), solver), "Single UniqueList"),
            (new(GraphCreator.CreateForLink<ISudokuElement>(GraphCollectionType.HashSet, true), solver), "Divided HashSet"),
            (new(GraphCreator.CreateForLink<ISudokuElement>(GraphCollectionType.UniqueList, true), solver), "Divided UniqueList"),
            (new(new SudokuElementArrayLinkGraph(), solver), "Element Array")
        };

        Console.WriteLine("\n");
        ImplementationSpeedComparator.Compare(graphs, 300, graph =>
        {
            graph.Construct(CellStrongLinkConstructionRule.Instance, CellWeakLinkConstructionRule.Instance,
                UnitStrongLinkConstructionRule.Instance, UnitWeakLinkConstructionRule.Instance,
                PointingPossibilitiesConstructionRule.Instance, AlmostNakedSetConstructionRule.Instance);
        }, graph =>
        {
            graph.Clear();
        });
        
        foreach (var graph in graphs)
        {
            graph.Item1.Construct(CellStrongLinkConstructionRule.Instance, CellWeakLinkConstructionRule.Instance,
                UnitStrongLinkConstructionRule.Instance, UnitWeakLinkConstructionRule.Instance,
                PointingPossibilitiesConstructionRule.Instance, AlmostNakedSetConstructionRule.Instance);
        }
        
        foreach (var start in graphs[0].Item1.Graph)
        {
            for (int i = 1; i < graphs.Length; i++)
            {
                var otherGraph = graphs[i];
                
                foreach (var friend in graphs[0].Item1.Graph.GetNeighbors(start, LinkStrength.Strong))
                {
                    Assert.True(otherGraph.Item1.Graph.AreNeighbors(start, friend, LinkStrength.Strong));
                }
            
                foreach (var friend in graphs[0].Item1.Graph.GetNeighbors(start, LinkStrength.Weak))
                {
                    Assert.True(otherGraph.Item1.Graph.AreNeighbors(start, friend, LinkStrength.Weak));
                }
            }
        }
        
        ImplementationSpeedComparator.Compare(graphs, 300, graph =>
        {
            foreach (var start in graph.Graph)
            {
                foreach (var friend in graph.Graph.GetNeighbors(start))
                {
                    var a = friend.EveryCell();
                }
            }
        });
    }
}