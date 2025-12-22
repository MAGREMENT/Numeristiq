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
    /*
        838
         Name |    Total    |    Average   |   Minimum   |   Maximum   |     Dry     
        ------+-------------+--------------+-------------+-------------+-------------
          V1  | 2,1422566 s | 214,22566 ms | 202,9517 ms | 261,2931 ms | 252,4954 ms 
        ------+-------------+--------------+-------------+-------------+-------------
     */
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
        var branchFinder = new BlossomLoopBFSBranchFinder();

        var types = new IBlossomLoopType[]
        {
            new CellType(),
            new UnitType()
        };

        (IBlossomLoopLoopFinder, string)[] impl =
        [
            (new BlossomLoopDFSLoopFinder(12), "DFS"),
            (new BlossomLoopExclusiveDFSLoopFinder(12), "Exclusive DFS")
        ];
        
        ImplementationSpeedComparator.Compare(impl, 10, i =>
        {
            var total = new HashSet<Loop<ISudokuElement, LinkStrength>>();
            
            foreach (var t in types)
            {
                foreach (var c in t.Candidates(solver))
                {
                    foreach (var loop in i.Find(c, graph))
                    {
                        var result = branchFinder.FindShortestBranches(graph, c, loop);
                        if (result != null) total.Add(loop);
                    }
                }
            }

            Console.WriteLine(total.Count);
        });
    }
}