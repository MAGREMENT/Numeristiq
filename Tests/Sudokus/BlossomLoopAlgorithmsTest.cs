using Model.Core.Graphs;
using Model.Sudokus;
using Model.Sudokus.Solver;
using Model.Sudokus.Solver.Strategies.BlossomLoops;
using Model.Sudokus.Solver.Strategies.BlossomLoops.BranchFinder;
using Model.Sudokus.Solver.Strategies.BlossomLoops.LoopFinders;
using Model.Sudokus.Solver.Strategies.BlossomLoops.Types;
using Model.Sudokus.Solver.Utility.Graphs.ConstructRules;
using Model.Utility;
using Tests.Utility;

namespace Tests.Sudokus;

public class BlossomLoopAlgorithmsTest
{
    [Test]
    public void Test()
    {
        var solver = new SudokuSolver();
        solver.SetState(SudokuTranslator.TranslateBase32Format("21g80i410m0681h0h8o0418o11gg210305g8h4h416g209810h2141hop0588a840e21kio28403210h41g1110984gooc4c8a2111k4kio6410h8805110ag8o2211c34g1aa820h4cd21603b41cq8o0410c900h",
            DefaultBase32Alphabet.Instance));
        solver.PreComputer.ComplexGraph.Construct(CellStrongLinkConstructionRule.Instance, CellWeakLinkConstructionRule.Instance,
            UnitStrongLinkConstructionRule.Instance, UnitWeakLinkConstructionRule.Instance,
            PointingPossibilitiesConstructionRule.Instance);
        var graph = solver.PreComputer.ComplexGraph.Graph;
        var branchFinder = new BLBranchFinderV1();
        var wrote = false;

        var types = new IBlossomLoopType[]
        {
            new CellType(),
            new UnitType()
        };

        (IBlossomLoopLoopFinder, string)[] impl =
        [
            (new BLLoopFinderV1(12), "V1")
        ];
        
        ImplementationSpeedComparator.Compare(impl, 10, i =>
        {
            var total = 0;
            
            foreach (var t in types)
            {
                foreach (var c in t.Candidates(solver))
                {
                    foreach (var loop in i.Find(c, graph))
                    {
                        var result = branchFinder.FindShortestBranches(graph, c, loop);
                        if (result != null) total++;
                    }
                }
            }

            if (!wrote)
            {
                Console.WriteLine(total);
                wrote = true;
            }
        });
    }
}