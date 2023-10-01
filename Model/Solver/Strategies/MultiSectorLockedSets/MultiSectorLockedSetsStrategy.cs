using System.Collections.Generic;
using Model.Solver.Helpers.Changes;
using Model.Solver.Positions;
using Model.Solver.Possibilities;
using Model.Solver.StrategiesUtil;

namespace Model.Solver.Strategies.MultiSectorLockedSets;

public class MultiSectorLockedSetsStrategy : AbstractStrategy
{
    public const string OfficialName = "Multi-Sector Locked Sets";

    public const int CoverRowStart = 0;
    public const int CoverRowEnd = 8;
    public const int CoverColumnStart = 9;
    public const int CoverColumnEnd = 17;
    public const int CoverMiniGridStart = 18;
    public const int CoverMiniGridEnd = 26;
    public static readonly ICoverHouse[] CoverHouses =
    {
        new CoverRow(0),
        new CoverRow(1),
        new CoverRow(2),
        new CoverRow(3),
        new CoverRow(4),
        new CoverRow(5),
        new CoverRow(6),
        new CoverRow(7),
        new CoverRow(8),
        new CoverColumn(0),
        new CoverColumn(1),
        new CoverColumn(2),
        new CoverColumn(3),
        new CoverColumn(4),
        new CoverColumn(5),
        new CoverColumn(6),
        new CoverColumn(7),
        new CoverColumn(8),
        new CoverMiniGrid(0, 0),
        new CoverMiniGrid(0, 1),
        new CoverMiniGrid(0, 2),
        new CoverMiniGrid(1, 0),
        new CoverMiniGrid(1, 1),
        new CoverMiniGrid(1, 2),
        new CoverMiniGrid(2, 0),
        new CoverMiniGrid(2, 1),
        new CoverMiniGrid(2, 2),
    };

    private readonly ICoverHouseSearchAlgorithm _searchAlgorithm;

    public MultiSectorLockedSetsStrategy(ICoverHouseSearchAlgorithm searchAlgorithm)
        : base(OfficialName, StrategyDifficulty.Extreme)
    {
        _searchAlgorithm = searchAlgorithm;
    }

    public override void ApplyOnce(IStrategyManager strategyManager)
    {
        foreach (var result in _searchAlgorithm.Search(strategyManager))
        {
            Try(strategyManager, result.Home, result.Away, result.HomeList, result.AwayList);
        }
    }

    private void Try(IStrategyManager strategyManager, IPossibilities home, IPossibilities away,
        List<ICoverHouse> homeList, List<ICoverHouse> awayList)
    {
        var gpHome = new GridPositions();
        var gpAway = new GridPositions();

        int digitCovers = 0;

        foreach (var coverHouse in homeList)
        {
            digitCovers += coverHouse.Apply(strategyManager, home, ref gpHome);
        }

        foreach (var coverHouse in awayList)
        {
            digitCovers += coverHouse.Apply(strategyManager, away, ref gpAway);
        }

        var common = gpHome.And(gpAway);

        if (common.Count != digitCovers) return;
        
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (gpHome.Peek(row, col) && !gpAway.Peek(row, col))
                {
                    foreach (var possibility in home)
                    {
                        strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, row, col);
                    }
                }
                
                if (!gpHome.Peek(row, col) && gpAway.Peek(row, col))
                {
                    foreach (var possibility in away)
                    {
                        strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, row, col);
                    }
                }
            }
        }
    }
}

public interface ICoverHouse
{
    public int Apply(IStrategyManager strategyManager, IPossibilities associatedSet, ref GridPositions gp);
}

public class CoverRow : ICoverHouse
{
    private readonly int _row;

    public CoverRow(int row)
    {
        _row = row;
    }

    public int Apply(IStrategyManager strategyManager, IPossibilities associatedSet, ref GridPositions gp)
    {
        int count = associatedSet.Count;
        
        for (int col = 0; col < 9; col++)
        {
            var solved = strategyManager.Sudoku[_row, col];

            if (solved == 0) gp.Add(_row, col);
            else if (associatedSet.Peek(solved)) count--;
        }

        return count;
    }
}

public class CoverColumn : ICoverHouse
{
    private readonly int _col;

    public CoverColumn(int col)
    {
        _col = col;
    }

    public int Apply(IStrategyManager strategyManager, IPossibilities associatedSet, ref GridPositions gp)
    {
        int count = associatedSet.Count;
        
        for (int row = 0; row < 9; row++)
        {
            var solved = strategyManager.Sudoku[row, _col];

            if (solved == 0) gp.Add(row, _col);
            else if (associatedSet.Peek(solved)) count--;
        }

        return count;
    }
}

public class CoverMiniGrid : ICoverHouse
{
    private readonly int _miniRow;
    private readonly int _miniCol;

    public CoverMiniGrid(int miniRow, int miniCol)
    {
        _miniRow = miniRow;
        _miniCol = miniCol;
    }

    public int Apply(IStrategyManager strategyManager, IPossibilities associatedSet, ref GridPositions gp)
    {
        int count = associatedSet.Count;

        for (int gridRow = 0; gridRow < 3; gridRow++)
        {
            for (int gridCol = 0; gridCol < 3; gridCol++)
            {
                int row = _miniRow * 3 + gridRow;
                int col = _miniCol * 3 + gridCol;
                
                var solved = strategyManager.Sudoku[row, col];

                if (solved == 0) gp.Add(row, col);
                else if (associatedSet.Peek(solved)) count--;
            }
        }

        return count;
    }
}

public class MultiSectorLockedSetsReportBuilder : IChangeReportBuilder
{
    private readonly IPossibilities _home;
    private readonly IPossibilities _away;
    
    public MultiSectorLockedSetsReportBuilder(IPossibilities home, IPossibilities away)
    {
        _home = home;
        _away = away;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        var homeCells = new List<Cell>();
        var awayCells = new List<Cell>();

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var solved = snapshot.Sudoku[row, col];
                if (_home.Peek(solved)) homeCells.Add(new Cell(row, col));
                if (_away.Peek(solved)) awayCells.Add(new Cell(row, col));
            }
        }

        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            foreach (var cell in homeCells)
            {
                lighter.HighlightCell(cell, ChangeColoration.CauseOnOne);
            }

            foreach (var cell in awayCells)
            {
                lighter.HighlightCell(cell, ChangeColoration.CauseOffOne);
            }
        });
    }
}

public interface ICoverHouseSearchAlgorithm
{
    public IEnumerable<SearchResult> Search(IStrategyManager strategyManager);
}

public class SearchResult
{
    public SearchResult(IPossibilities home, IPossibilities away, List<ICoverHouse> homeList, List<ICoverHouse> awayList)
    {
        Home = home;
        Away = away;
        HomeList = homeList;
        AwayList = awayList;
    }

    public IPossibilities Home { get; }
    public IPossibilities Away { get; }
    public List<ICoverHouse> HomeList { get; }
    public List<ICoverHouse> AwayList { get; }
}