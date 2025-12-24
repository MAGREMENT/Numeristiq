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
    public void TestSudokuElement()
    {
        var graphs = new[]
        {
            (GraphCreator.CreateForLink<ISudokuElement>(GraphCollectionType.HashSet, false), "Single HashSet"),
            (GraphCreator.CreateForLink<ISudokuElement>(GraphCollectionType.UniqueList, false), "Single UniqueList"),
            (GraphCreator.CreateForLink<ISudokuElement>(GraphCollectionType.HashSet, true), "Divided HashSet"),
            (GraphCreator.CreateForLink<ISudokuElement>(GraphCollectionType.UniqueList, true), "Divided HashSet"),
            (new SudokuElementArrayLinkGraph(), "Element Array")
        };

        SudokuLinkGraphSpeedTest(graphs, graph => graph.Construct(CellStrongLinkConstructionRule.Instance, CellWeakLinkConstructionRule.Instance,
            UnitStrongLinkConstructionRule.Instance, UnitWeakLinkConstructionRule.Instance,
            PointingPossibilitiesConstructionRule.Instance, AlmostNakedSetConstructionRule.Instance));
    }
    
    /*
     * Construct :

        Name        |     Average    |   Minimum   |   Maximum   |     Dry     |
--------------------+----------------+-------------+-------------+-------------+
   Single HashSet   |  241,4347 mis  |  207,2 mis  |  1,8548 ms  | v 3,7035 ms |
--------------------+----------------+-------------+-------------+-------------+
  Single UniqueList | v 279,5033 mis | v 265,1 mis |   639 mis   |  892,9 mis  |
--------------------+----------------+-------------+-------------+-------------+
   Divided HashSet  |  230,1767 mis  |  206,7 mis  |  852,3 mis  |  502,8 mis  |
--------------------+----------------+-------------+-------------+-------------+
 Divided UniqueList |   274,845 mis  |  169,1 mis  | v 3,2429 ms |  328,2 mis  |
--------------------+----------------+-------------+-------------+-------------+
  Array UniqueList  |   146,684 mis  |  142,3 mis  |  503,6 mis  |  705,5 mis  |
--------------------+----------------+-------------+-------------+-------------+
    Array HashSet   | ^ 104,4123 mis |  ^ 94,2 mis | ^ 462,7 mis | ^ 197,4 mis |
--------------------+----------------+-------------+-------------+-------------+


Clear :

        Name        |         Average        |  Minimum |  Maximum  |     Dry    |
--------------------+------------------------+----------+-----------+------------+
   Single HashSet   |  124,33333333333334 ns | v 100 ns |  1,2 mis  | v 23,2 mis |
--------------------+------------------------+----------+-----------+------------+
  Single UniqueList |  131,66666666666666 ns |  100 ns  |   600 ns  |  ^ 400 ns  |
--------------------+------------------------+----------+-----------+------------+
   Divided HashSet  |         ^ 95 ns        |  ^ 0 ns  |  ^ 400 ns |   500 ns   |
--------------------+------------------------+----------+-----------+------------+
 Divided UniqueList |  101,66666666666666 ns |   0 ns   |   800 ns  |   400 ns   |
--------------------+------------------------+----------+-----------+------------+
  Array UniqueList  | v 338,3333333333333 ns |  100 ns  | v 3,7 mis |   1,5 mis  |
--------------------+------------------------+----------+-----------+------------+
    Array HashSet   |  313,3333333333333 ns  |  100 ns  |   3 mis   |   400 ns   |
--------------------+------------------------+----------+-----------+------------+


GetNeighbors :

        Name        |    Average    |   Minimum  |   Maximum   |     Dry     |
--------------------+---------------+------------+-------------+-------------+
   Single HashSet   |  52,3477 mis  |  45,5 mis  | v 540,8 mis |  725,2 mis  |
--------------------+---------------+------------+-------------+-------------+
  Single UniqueList |  v 53,843 mis | v 49,9 mis |  490,3 mis  |  350,4 mis  |
--------------------+---------------+------------+-------------+-------------+
   Divided HashSet  |  51,3877 mis  |   46 mis   |  482,2 mis  | v 746,3 mis |
--------------------+---------------+------------+-------------+-------------+
 Divided UniqueList |  50,8603 mis  |  46,5 mis  |  510,9 mis  |  ^ 81,5 mis |
--------------------+---------------+------------+-------------+-------------+
  Array UniqueList  | ^ 45,9353 mis | ^ 42,2 mis |  498,6 mis  |  687,6 mis  |
--------------------+---------------+------------+-------------+-------------+
    Array HashSet   |   47,716 mis  |  43,5 mis  | ^ 467,1 mis |    83 mis   |
--------------------+---------------+------------+-------------+-------------+


GetNeighborsWithLinks :

        Name        |    Average   |   Minimum  |   Maximum   |     Dry     |
--------------------+--------------+------------+-------------+-------------+
   Single HashSet   |  39,3833 mis |  36,6 mis  |  465,9 mis  | v 268,7 mis |
--------------------+--------------+------------+-------------+-------------+
  Single UniqueList |  39,6167 mis |  37,4 mis  |   434 mis   |   50,1 mis  |
--------------------+--------------+------------+-------------+-------------+
   Divided HashSet  |  40,248 mis  |  37,7 mis  |  465,4 mis  |  225,1 mis  |
--------------------+--------------+------------+-------------+-------------+
 Divided UniqueList | v 42,269 mis | v 38,2 mis |  517,6 mis  |   51,5 mis  |
--------------------+--------------+------------+-------------+-------------+
  Array UniqueList  |  35,9563 mis | ^ 31,8 mis | v 534,1 mis |  201,3 mis  |
--------------------+--------------+------------+-------------+-------------+
    Array HashSet   | ^ 34,785 mis |  31,8 mis  | ^ 425,4 mis |  ^ 43,2 mis |
--------------------+--------------+------------+-------------+-------------+
     */

    [Test]
    public void TestCellPossibility()
    {
        var graphs = new[]
        {
            (GraphCreator.CreateForLink<CellPossibility>(GraphCollectionType.HashSet, false), "Single HashSet"),
            (GraphCreator.CreateForLink<CellPossibility>(GraphCollectionType.UniqueList, false), "Single UniqueList"),
            (GraphCreator.CreateForLink<CellPossibility>(GraphCollectionType.HashSet, true), "Divided HashSet"),
            (GraphCreator.CreateForLink<CellPossibility>(GraphCollectionType.UniqueList, true), "Divided UniqueList"),
            (GraphCreator.CreateForCellPossibilityLink(GraphCollectionType.UniqueList, 9, 9, 9), "Array UniqueList"),
            (GraphCreator.CreateForCellPossibilityLink(GraphCollectionType.HashSet, 9, 9, 9), "Array HashSet"),
        };
        
        SudokuLinkGraphSpeedTest(graphs, graph => graph.Construct(CellStrongLinkConstructionRule.Instance, CellWeakLinkConstructionRule.Instance,
            UnitStrongLinkConstructionRule.Instance, UnitWeakLinkConstructionRule.Instance));
    }

    private static volatile ISudokuElement? _sink;
    
    private void SudokuLinkGraphSpeedTest<T>((IGraph<T, LinkStrength>, string)[] graphs, Construct<T> construct)
        where T : ISudokuElement
    {
        var solver = new SudokuSolver();
        var sudoku = SudokuTranslator.TranslateBase32Format(
            "0hj0a6t009t21474n04eh8062146li815cloecj8a4tgc4tg1s7c032e2o0mc811ea0cg1c80e81h0ko42kq215e582a41h005a2qa1o9a9oa02gag03g105411818110309c0e0e0g10h05g10541180h1803a0a0",
            DefaultBase32Alphabet.Instance);
        solver.SetState(sudoku);

        var cGraphs = new (ConstructedGraph<ISudokuSolverData, IGraph<T, LinkStrength>>, string)[graphs.Length];
        for (int i = 0; i < graphs.Length; i++)
        {
            cGraphs[i] = (new ConstructedGraph<ISudokuSolverData, IGraph<T, LinkStrength>>(graphs[i].Item1, solver), graphs[i].Item2);
        }

        Console.WriteLine("\nConstruct :\n");
        ImplementationSpeedComparator.Compare(cGraphs, 300, graph => construct(graph), graph =>
        {
            graph.Clear();
        });
        
        foreach (var graph in cGraphs)
        {
            construct(graph.Item1);
        }
        
        Console.WriteLine("\nClear :\n");
        ImplementationSpeedComparator.Compare(cGraphs, 300, graph => graph.Clear(), graph =>
        {
            construct(graph);
        });
        
        foreach (var graph in cGraphs)
        {
            construct(graph.Item1);
        }
        
        foreach (var start in cGraphs[0].Item1.Graph)
        {
            for (int i = 1; i < graphs.Length; i++)
            {
                var otherGraph = cGraphs[i];
                
                foreach (var friend in cGraphs[0].Item1.Graph.GetNeighbors(start, LinkStrength.Strong))
                {
                    Assert.True(otherGraph.Item1.Graph.AreNeighbors(start, friend, LinkStrength.Strong));
                }
            
                foreach (var friend in cGraphs[0].Item1.Graph.GetNeighbors(start, LinkStrength.Weak))
                {
                    Assert.True(otherGraph.Item1.Graph.AreNeighbors(start, friend, LinkStrength.Weak));
                }
            }
        }

        var els = cGraphs[0].Item1.Graph.ToArray();
        
        Console.WriteLine("\nGetNeighbors :\n");
        ImplementationSpeedComparator.Compare(cGraphs, 300, graph =>
        {
            foreach (var start in els)
            {
                foreach (var friend in graph.Graph.GetNeighbors(start))
                {
                    _sink = friend;
                }
            }
        });
        
        Console.WriteLine("\nGetNeighborsWithLinks :\n");
        ImplementationSpeedComparator.Compare(cGraphs, 300, graph =>
        {
            foreach (var start in els)
            {
                foreach (var friend in graph.Graph.GetNeighbors(start, LinkStrength.Strong))
                {
                    _sink = friend;
                }
                
                foreach (var friend in graph.Graph.GetNeighbors(start, LinkStrength.Weak))
                {
                    _sink = friend;
                }
            }
        });
        
        Console.WriteLine("\nEnumeration :\n");
        ImplementationSpeedComparator.Compare(cGraphs, 300, graph =>
        {
            foreach (var start in graph.Graph)
            {
                _sink = start;
            }
        });
    }

    private delegate void Construct<T>( ConstructedGraph<ISudokuSolverData,IGraph<T,LinkStrength>> cGraph) where T : notnull;
}