using System.Collections.Generic;
using System.Linq;
using Model.Helpers.Changes;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Sudoku.Solver.StrategiesUtility.Graphs;

namespace Model.Sudoku.Solver.Strategies;

public class XYChainsStrategy : AbstractStrategy
{
    public const string OfficialName = "XY-Chains";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public XYChainsStrategy() : base(OfficialName, StrategyDifficulty.Hard, DefaultBehavior) {}

    public override void Apply(IStrategyUser strategyUser)
    {
        strategyUser.PreComputer.Graphs.ConstructSimple(ConstructRule.XYChainSpecific,
            ConstructRule.CellStrongLink);
        var graph = strategyUser.PreComputer.Graphs.SimpleLinkGraph;
        var route = new List<CellPossibility>();
        var visited = new HashSet<CellPossibility>();

        foreach (var start in graph)
        {
            if (Search(strategyUser, graph, start, route, visited)) return;
            visited.Clear();
        }
    }

    private bool Search(IStrategyUser strategyUser, ILinkGraph<CellPossibility> graph, CellPossibility current,
        List<CellPossibility> route, HashSet<CellPossibility> visited)
    {
        CellPossibility friend = graph.Neighbors(current, LinkStrength.Strong).First();

        route.Add(current);
        route.Add(friend);
        visited.Add(current);
        visited.Add(friend);
        
        if(friend.Possibility == route[0].Possibility && Process(strategyUser, route)) return true;

        foreach (var next in graph.Neighbors(friend, LinkStrength.Weak))
        {
            if (!visited.Contains(next))
            {
                if (Search(strategyUser, graph, next, route, visited)) return true;
            }
        }

        route.RemoveAt(route.Count - 1);
        route.RemoveAt(route.Count - 1);

        return false;
    }

    private bool Process(IStrategyUser strategyUser, List<CellPossibility> visited)
    {
        foreach (var coord in visited[0].SharedSeenCells(visited[^1]))
        {
            strategyUser.ChangeBuffer.ProposePossibilityRemoval(visited[0].Possibility, coord.Row, coord.Column);
        }
        
        return strategyUser.ChangeBuffer.Commit( new XYChainReportBuilder(visited))
            && OnCommitBehavior == OnCommitBehavior.Return;
    }
}

public class XYChainReportBuilder : IChangeReportBuilder
{
    private readonly CellPossibility[] _visited;

    public XYChainReportBuilder(List<CellPossibility> visited)
    {
        _visited = visited.ToArray();
    }

    public ChangeReport Build(IReadOnlyList<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport( "", lighter =>
        {
            for (int i = 0; i < _visited.Length; i++)
            {
                lighter.HighlightPossibility(_visited[i].Possibility, _visited[i].Row, _visited[i].Column, i % 2 == 0 ?
                    ChangeColoration.CauseOnOne: ChangeColoration.CauseOffTwo);
                if (i > _visited.Length - 2) continue;
                lighter.CreateLink(_visited[i], _visited[i + 1], i % 2 == 0 ? LinkStrength.Weak : LinkStrength.Strong);
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}