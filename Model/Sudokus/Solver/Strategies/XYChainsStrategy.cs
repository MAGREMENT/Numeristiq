using System.Collections.Generic;
using System.Linq;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;

namespace Model.Sudokus.Solver.Strategies;

public class XYChainsStrategy : SudokuStrategy
{
    public const string OfficialName = "XY-Chains";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    public XYChainsStrategy() : base(OfficialName, StepDifficulty.Hard, DefaultInstanceHandling) {}

    public override void Apply(ISudokuSolverData solverData)
    {
        solverData.PreComputer.Graphs.ConstructSimple(SudokuConstructRuleBank.XYChainSpecific,
            SudokuConstructRuleBank.CellStrongLink);
        var graph = solverData.PreComputer.Graphs.SimpleLinkGraph;
        var route = new List<CellPossibility>();
        var visited = new HashSet<CellPossibility>();

        foreach (var start in graph)
        {
            if (Search(solverData, graph, start, route, visited)) return;
            visited.Clear();
        }
    }

    private bool Search(ISudokuSolverData solverData, ILinkGraph<CellPossibility> graph, CellPossibility current,
        List<CellPossibility> route, HashSet<CellPossibility> visited)
    {
        var friend = graph.Neighbors(current, LinkStrength.Strong).First();

        route.Add(current);
        route.Add(friend);
        visited.Add(current);
        visited.Add(friend);
        
        if(friend.Possibility == route[0].Possibility && Process(solverData, route)) return true;

        foreach (var next in graph.Neighbors(friend, LinkStrength.Weak))
        {
            if (!visited.Contains(next))
            {
                if (Search(solverData, graph, next, route, visited)) return true;
            }
        }

        route.RemoveAt(route.Count - 1);
        route.RemoveAt(route.Count - 1);

        return false;
    }

    private bool Process(ISudokuSolverData solverData, List<CellPossibility> visited)
    {
        foreach (var coord in visited[0].SharedSeenCells(visited[^1]))
        {
            solverData.ChangeBuffer.ProposePossibilityRemoval(visited[0].Possibility, coord.Row, coord.Column);
        }
        
        return solverData.ChangeBuffer.Commit( new XYChainReportBuilder(visited))
            && StopOnFirstPush;
    }
}

public class XYChainReportBuilder : IChangeReportBuilder<IUpdatableSudokuSolvingState, ISudokuHighlighter>
{
    private readonly CellPossibility[] _visited;

    public XYChainReportBuilder(List<CellPossibility> visited)
    {
        _visited = visited.ToArray();
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>( "", lighter =>
        {
            for (int i = 0; i < _visited.Length; i++)
            {
                lighter.HighlightPossibility(_visited[i].Possibility, _visited[i].Row, _visited[i].Column, i % 2 == 0 ?
                    ChangeColoration.CauseOnOne: ChangeColoration.CauseOffTwo);
                if (i > _visited.Length - 2) continue;
                lighter.CreateLink(_visited[i], _visited[i + 1], i % 2 == 0 ? LinkStrength.Weak : LinkStrength.Strong);
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}