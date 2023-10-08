using System.Collections.Generic;
using System.Linq;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.Strategies;

public class XYChainStrategy : AbstractStrategy
{
    public const string OfficialName = "XY-Chain";
    
    public XYChainStrategy() : base(OfficialName, StrategyDifficulty.Hard) {}

    public override void ApplyOnce(IStrategyManager strategyManager)
    {
        strategyManager.GraphManager.ConstructSimple(ConstructRule.XYChainSpecific,
            ConstructRule.CellStrongLink);
        var graph = strategyManager.GraphManager.SimpleLinkGraph;
        var route = new List<CellPossibility>();
        var visited = new HashSet<CellPossibility>();

        foreach (var start in graph)
        {
            Search(strategyManager, graph, start, route, visited);
            visited.Clear();
        }
    }

    private void Search(IStrategyManager strategyManager, LinkGraph<CellPossibility> graph, CellPossibility current,
        List<CellPossibility> route, HashSet<CellPossibility> visited)
    {
        CellPossibility friend = graph.GetLinks(current, LinkStrength.Strong).First();

        route.Add(current);
        route.Add(friend);
        visited.Add(current);
        visited.Add(friend);
        
        if(friend.Possibility == route[0].Possibility) Process(strategyManager, route);

        foreach (var next in graph.GetLinks(friend, LinkStrength.Weak))
        {
            if (!visited.Contains(next))
            {
                Search(strategyManager, graph, next, route, visited);
            }
        }

        route.RemoveAt(route.Count - 1);
        route.RemoveAt(route.Count - 1);
    }

    private void Process(IStrategyManager strategyManager, List<CellPossibility> visited)
    {
        foreach (var coord in visited[0].SharedSeenCells(visited[^1]))
        {
            strategyManager.ChangeBuffer.AddPossibilityToRemove(visited[0].Possibility, coord.Row, coord.Col);
        }
        
        strategyManager.ChangeBuffer.Push(this, new XYChainReportBuilder(visited));
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