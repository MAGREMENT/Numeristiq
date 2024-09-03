using Model.Core.Graphs;
using Model.Core.Graphs.Implementations;
using Model.Sudokus;
using Model.Sudokus.Solver;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Sudokus.Solver.Utility.Graphs.ConstructRules;
using Model.Utility;
using Tests.Utility;

namespace Tests;

public class GraphTests
{
    [Test]
    public void LinkGraphConstructionTest()
    {
        var solver = new SudokuSolver();
        var sudoku = SudokuTranslator.TranslateBase32Format(
            "0hj0a6t009t21474n04eh8062146li815cloecj8a4tgc4tg1s7c032e2o0mc811ea0cg1c80e81h0ko42kq215e582a41h005a2qa1o9a9oa02gag03g105411818110309c0e0e0g10h05g10541180h1803a0a0",
            DefaultBase32Alphabet.Instance);
        solver.SetState(sudoku);
        
        var graphs = new ConstructedGraph<ISudokuSolverData, IGraph<ISudokuElement, LinkStrength>>[]
        {
            new(new HDictionaryLinkGraph<ISudokuElement>(), solver),
            new(new ULDictionaryLinkGraph<ISudokuElement>(), solver),
            new(new HDoubleDictionaryLinkGraph<ISudokuElement>(), solver),
            new(new ULDoubleDictionaryLinkGraph<ISudokuElement>(), solver)
        };
        
        ImplementationSpeedComparator.Compare(graph =>
        {
            graph.Construct(CellStrongLinkConstructionRule.Instance, CellWeakLinkConstructionRule.Instance,
                UnitStrongLinkConstructionRule.Instance, UnitWeakLinkConstructionRule.Instance,
                PointingPossibilitiesConstructionRule.Instance, AlmostNakedSetConstructionRule.Instance);
        }, 300000, graphs);
    }
    
    [Test]
    public void LinkGraphEnumerationTest()
    {
        var solver = new SudokuSolver();
        var sudoku = SudokuTranslator.TranslateBase32Format(
            "0hj0a6t009t21474n04eh8062146li815cloecj8a4tgc4tg1s7c032e2o0mc811ea0cg1c80e81h0ko42kq215e582a41h005a2qa1o9a9oa02gag03g105411818110309c0e0e0g10h05g10541180h1803a0a0",
            DefaultBase32Alphabet.Instance);
        solver.SetState(sudoku);

        var graphs = new ConstructedGraph<ISudokuSolverData, IGraph<ISudokuElement, LinkStrength>>[]
        {
            new(new HDictionaryLinkGraph<ISudokuElement>(), solver),
            new(new ULDictionaryLinkGraph<ISudokuElement>(), solver),
            new(new HDoubleDictionaryLinkGraph<ISudokuElement>(), solver),
            new(new ULDoubleDictionaryLinkGraph<ISudokuElement>(), solver)
        };

        foreach (var graph in graphs)
        {
            graph.Construct(CellStrongLinkConstructionRule.Instance, CellWeakLinkConstructionRule.Instance,
                UnitStrongLinkConstructionRule.Instance, UnitWeakLinkConstructionRule.Instance,
                PointingPossibilitiesConstructionRule.Instance, AlmostNakedSetConstructionRule.Instance);
        }

        foreach (var start in graphs[0].Graph)
        {
            for (int i = 1; i < graphs.Length; i++)
            {
                var otherGraph = graphs[i];
                
                foreach (var friend in graphs[0].Graph.Neighbors(start, LinkStrength.Strong))
                {
                    Assert.True(otherGraph.Graph.AreNeighbors(start, friend, LinkStrength.Strong));
                }
            
                foreach (var friend in graphs[0].Graph.Neighbors(start, LinkStrength.Weak))
                {
                    Assert.True(otherGraph.Graph.AreNeighbors(start, friend, LinkStrength.Weak));
                }
            }
        }
        
        ImplementationSpeedComparator.Compare(graph =>
        {
            foreach (var start in graph.Graph)
            {
                foreach (var friend in graph.Graph.Neighbors(start))
                {
                    var a = friend.EveryCell();
                }
            }
        }, 300, graphs);
    }
}