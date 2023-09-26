using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Model.Solver.Helpers;
using Model.Solver.Helpers.Changes;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.Strategies;

public class XYChainStrategy : IStrategy
{
    public const string OfficialName = "XY-Chain";
    
    public string Name => OfficialName;
    public StrategyDifficulty Difficulty => StrategyDifficulty.Hard;
    public StatisticsTracker Tracker { get; } = new();

    public void ApplyOnce(IStrategyManager strategyManager)
    
    {
        var map = new BiValueMap(strategyManager);

        foreach (var start in map)
        {
            Search(strategyManager, map, start, new List<CellPossibility>(), new HashSet<CellPossibility>());
        }
    }

    private void Search(IStrategyManager strategyManager, BiValueMap map, CellPossibility current,
        List<CellPossibility> route, HashSet<CellPossibility> visited)

    {
        CellPossibility friend = map.AssociatedCoordinate(current);

        route.Add(current);
        route.Add(friend);
        visited.Add(current);
        visited.Add(friend);
        
        if(friend.Possibility == route[0].Possibility) Process(strategyManager, route);

        foreach (var shared in map.AssociatedCoordinates(friend.Possibility))
        {
            if (!visited.Contains(shared) && shared.ShareAUnit(current))
            {
                Search(strategyManager, map, shared, new List<CellPossibility>(route), visited);
            }
        }
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

public class BiValueMap : IEnumerable<CellPossibility>
{
    private readonly Dictionary<CellPossibility, CellPossibility> _cells = new();
    private readonly Dictionary<int, HashSet<CellPossibility>> _map = new();

    public BiValueMap(IStrategyManager strategyManager)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (strategyManager.PossibilitiesAt(row, col).Count == 2)
                {
                    int[] possibilities = strategyManager.PossibilitiesAt(row, col).ToArray();
                    
                    CellPossibility first = new CellPossibility(row, col, possibilities[0]);
                    CellPossibility second = new CellPossibility(row, col, possibilities[1]);

                    _cells.Add(first, second);
                    _cells.Add(second, first);
                    
                    if (!_map.TryAdd(possibilities[0], new HashSet<CellPossibility> { first }))
                        _map[possibilities[0]].Add(first);
                    if (!_map.TryAdd(possibilities[1], new HashSet<CellPossibility> { second }))
                        _map[possibilities[1]].Add(second);
                }
            }
        }
    }

    public IEnumerator<CellPossibility> GetEnumerator()
    {
        return _cells.Keys.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public HashSet<CellPossibility> AssociatedCoordinates(int possibility)
    {
        return _map.TryGetValue(possibility, out var result) ?
            result : new HashSet<CellPossibility>();
    }

    public CellPossibility AssociatedCoordinate(CellPossibility coord)
    {
        return _cells[coord];
    }
}

public class XYChainReportBuilder : IChangeReportBuilder
{
    private readonly List<CellPossibility> _visited;

    public XYChainReportBuilder(List<CellPossibility> visited)
    {
        _visited = visited;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            for (int i = 0; i < _visited.Count; i++)
            {
                lighter.HighlightPossibility(_visited[i].Possibility, _visited[i].Row, _visited[i].Col, i % 2 == 0 ?
                    ChangeColoration.CauseOnOne: ChangeColoration.CauseOffTwo);
                if (i > _visited.Count - 2) continue;
                lighter.CreateLink(_visited[i], _visited[i + 1], i % 2 == 0 ? LinkStrength.Weak : LinkStrength.Strong);
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}