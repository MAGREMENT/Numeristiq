using System.Collections.Generic;
using System.Linq;
using Global.Enums;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.Strategies;

public class XYChainStrategy : AbstractStrategy
{
    public const string OfficialName = "XY-Chain";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public XYChainStrategy() : base(OfficialName, StrategyDifficulty.Hard, DefaultBehavior) {}

    public override void Apply(IStrategyManager strategyManager)
    {
        strategyManager.GraphManager.ConstructSimple(ConstructRule.XYChainSpecific,
            ConstructRule.CellStrongLink);
        var graph = strategyManager.GraphManager.SimpleLinkGraph;
        var route = new List<CellPossibility>();
        var visited = new HashSet<CellPossibility>();

        foreach (var start in graph)
        {
            if (Search(strategyManager, graph, start, route, visited)) return;
            visited.Clear();
        }
    }

    private bool Search(IStrategyManager strategyManager, LinkGraph<CellPossibility> graph, CellPossibility current,
        List<CellPossibility> route, HashSet<CellPossibility> visited)
    {
        CellPossibility friend = graph.GetLinks(current, LinkStrength.Strong).First();

        route.Add(current);
        route.Add(friend);
        visited.Add(current);
        visited.Add(friend);
        
        if(friend.Possibility == route[0].Possibility && Process(strategyManager, route)) return true;

        foreach (var next in graph.GetLinks(friend, LinkStrength.Weak))
        {
            if (!visited.Contains(next))
            {
                if (Search(strategyManager, graph, next, route, visited)) return true;
            }
        }

        route.RemoveAt(route.Count - 1);
        route.RemoveAt(route.Count - 1);

        return false;
    }

    private bool Process(IStrategyManager strategyManager, List<CellPossibility> visited)
    {
        foreach (var coord in visited[0].SharedSeenCells(visited[^1]))
        {
            strategyManager.ChangeBuffer.ProposePossibilityRemoval(visited[0].Possibility, coord.Row, coord.Col);
        }
        
        return strategyManager.ChangeBuffer.Commit(this, new XYChainReportBuilder(visited))
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

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            for (int i = 0; i < _visited.Length; i++)
            {
                lighter.HighlightPossibility(_visited[i].Possibility, _visited[i].Row, _visited[i].Col, i % 2 == 0 ?
                    ChangeColoration.CauseOnOne: ChangeColoration.CauseOffTwo);
                if (i > _visited.Length - 2) continue;
                lighter.CreateLink(_visited[i], _visited[i + 1], i % 2 == 0 ? LinkStrength.Weak : LinkStrength.Strong);
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}