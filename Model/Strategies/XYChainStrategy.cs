using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Model.StrategiesUtil;

namespace Model.Strategies;

public class XYChainStrategy : IStrategy
{
    public string Name => "XYChain";
    
    public StrategyLevel Difficulty => StrategyLevel.Hard;
    public int Score { get; set; }

    private readonly HashSet<PossibilityCoordinate> _used = new ();
    
    public void ApplyOnce(IStrategyManager strategyManager)
    
    {
        var map = new BiValueMap(strategyManager);
        _used.Clear();

        foreach (var start in map)
        {
            if (_used.Contains(start)) continue;
            Search(strategyManager, map, new List<PossibilityCoordinate>(), start);
        }
    }

    private void Search(IStrategyManager strategyManager, BiValueMap map, List<PossibilityCoordinate> visited,
         PossibilityCoordinate current)
    {
        PossibilityCoordinate friend = map.AssociatedCoordinate(current);
        visited.Add(current);
        visited.Add(friend);
        
        if(friend.Possibility == visited[0].Possibility) Process(strategyManager, visited);

        foreach (var shared in map.AssociatedCoordinates(friend.Possibility))
        {
            if (!visited.Contains(shared) && shared.ShareAUnit(current))
            {
                Search(strategyManager, map, new List<PossibilityCoordinate>(visited), shared);
            }
        }
    }

    private void Process(IStrategyManager strategyManager, List<PossibilityCoordinate> visited)
    {
        var changeBuffer = strategyManager.GetChangeBuffer();
        foreach (var coord in visited[0].SharedSeenCells(visited[^1]))
        {
            changeBuffer.AddPossibilityToRemove(visited[0].Possibility, coord.Row, coord.Col);
        }

        _used.Add(visited[^1]); //TODO improve this
        changeBuffer.Push(this, new XYChainReportBuilder(visited));
    }
}

public class BiValueMap : IEnumerable<PossibilityCoordinate>
{
    private readonly Dictionary<PossibilityCoordinate, PossibilityCoordinate> _cells = new();
    private readonly Dictionary<int, HashSet<PossibilityCoordinate>> _map = new();

    public BiValueMap(IStrategyManager strategyManager)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (strategyManager.Possibilities[row, col].Count == 2)
                {
                    int[] possibilities = strategyManager.Possibilities[row, col].ToArray();
                    
                    PossibilityCoordinate first = new PossibilityCoordinate(row, col, possibilities[0]);
                    PossibilityCoordinate second = new PossibilityCoordinate(row, col, possibilities[1]);

                    _cells.Add(first, second);
                    _cells.Add(second, first);
                    
                    if (!_map.TryAdd(possibilities[0], new HashSet<PossibilityCoordinate> { first }))
                        _map[possibilities[0]].Add(first);
                    if (!_map.TryAdd(possibilities[1], new HashSet<PossibilityCoordinate> { second }))
                        _map[possibilities[1]].Add(second);
                }
            }
        }
    }

    public IEnumerator<PossibilityCoordinate> GetEnumerator()
    {
        return _cells.Keys.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public HashSet<PossibilityCoordinate> AssociatedCoordinates(int possibility)
    {
        return _map.TryGetValue(possibility, out var result) ?
            result : new HashSet<PossibilityCoordinate>();
    }

    public PossibilityCoordinate AssociatedCoordinate(PossibilityCoordinate coord)
    {
        return _cells[coord];
    }
}

public class XYChainReportBuilder : IChangeReportBuilder
{
    private readonly List<PossibilityCoordinate> _visited;

    public XYChainReportBuilder(List<PossibilityCoordinate> visited)
    {
        _visited = visited;
    }

    public ChangeReport Build(List<SolverChange> changes, IChangeManager manager)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), lighter =>
        {
            for (int i = 0; i < _visited.Count; i++)
            {
                lighter.HighlightPossibility(_visited[i].Possibility, _visited[i].Row, _visited[i].Col, i % 2 == 0 ?
                    ChangeColoration.CauseOnOne: ChangeColoration.CauseOffTwo);
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        }, "");
    }
}