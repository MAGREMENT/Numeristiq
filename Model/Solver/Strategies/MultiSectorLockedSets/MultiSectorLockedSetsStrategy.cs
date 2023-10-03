using System.Collections.Generic;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;
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

    private readonly ICoverHouseSearchAlgorithm[] _searchAlgorithms;

    public MultiSectorLockedSetsStrategy(params ICoverHouseSearchAlgorithm[] searchAlgorithms)
        : base(OfficialName, StrategyDifficulty.Extreme)
    {
        _searchAlgorithms = searchAlgorithms;
    }

    public override void ApplyOnce(IStrategyManager strategyManager)
    {
        foreach (var algorithm in _searchAlgorithms)
        {
            foreach (var result in algorithm.Search(strategyManager))
            {
                Try(strategyManager, result.Home, result.Away, result.HomeList, result.AwayList);
            }
        }
        
    }

    private void Try(IStrategyManager strategyManager, IPossibilities home, IPossibilities away,
        List<ICoverHouse> homeList, List<ICoverHouse> awayList)
    {
        var gpHome = new GridPositions();
        var gpAway = new GridPositions();

        List<DigitCover> digitCovers = new List<DigitCover>(homeList.Count + awayList.Count);
        var digitsCovered = 0;

        foreach (var coverHouse in homeList)
        {
            var dc = coverHouse.Apply(strategyManager, home, ref gpHome);
            digitCovers.Add(dc);
            digitsCovered += dc.Possibilities.Count;
        }

        foreach (var coverHouse in awayList)
        {
            var dc = coverHouse.Apply(strategyManager, away, ref gpAway);
            digitCovers.Add(dc);
            digitsCovered += dc.Possibilities.Count;
        }

        var total = gpHome.Or(gpAway);
        var common = gpHome.And(gpAway);

        if (total.Count == common.Count) return;
        
        if(common.Count + 1 == digitsCovered) 
            ProcessAlmostNakedSet(strategyManager, home, gpHome, gpAway, digitCovers);
        else if(common.Count == digitsCovered) 
            ProcessNakedSet(strategyManager, home, away, gpHome, gpAway, digitCovers);
        else if(total.Count + 1 == digitsCovered) 
            ProcessAlmostHiddenSet(strategyManager, digitCovers);
        else if (total.Count == digitsCovered)
            ProcessHiddenSet(strategyManager, home, away, gpHome, gpAway, digitCovers);
        else return;

        strategyManager.ChangeBuffer.Push(this,
            new MultiSectorLockedSetsReportBuilder(home, away));
    }

    private void ProcessAlmostNakedSet(IStrategyManager strategyManager, IPossibilities home,
        GridPositions gpHome, GridPositions gpAway, List<DigitCover> digitCovers)
    {
        foreach (var cp in DigitsCoveredMoreThanOnce(strategyManager, digitCovers))
        {
            if (home.Peek(cp.Possibility))
            {
                if (gpHome.Peek(cp.Row, cp.Col) && !gpAway.Peek(cp.Row, cp.Col))
                    strategyManager.ChangeBuffer.AddSolutionToAdd(cp);
            }
            else
            {
                if (!gpHome.Peek(cp.Row, cp.Col) && gpAway.Peek(cp.Row, cp.Col))
                    strategyManager.ChangeBuffer.AddPossibilityToRemove(cp);
            }
        }
    }
    
    private void ProcessNakedSet(IStrategyManager strategyManager, IPossibilities home, IPossibilities away,
        GridPositions gpHome, GridPositions gpAway, List<DigitCover> digitCovers)
    {
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

        foreach (var cp in DigitsCoveredMoreThanOnce(strategyManager, digitCovers))
        {
            if (gpHome.Peek(cp.Row, cp.Col) && gpAway.Peek(cp.Row, cp.Col))
                strategyManager.ChangeBuffer.AddPossibilityToRemove(cp);
        }
    }
    
    private void ProcessAlmostHiddenSet(IStrategyManager strategyManager, List<DigitCover> digitCovers)
    {
        IPossibilities possibilities = IPossibilities.New();
        Cell? cell = null;
        foreach (var cp in DigitsCoveredMoreThanOnce(strategyManager, digitCovers))
        {
            if (cell is null)
            {
                cell = new Cell(cp.Row, cp.Col);
                possibilities.Add(cp.Possibility);
            }
            else if (cp.Row != cell.Value.Row || cp.Col != cell.Value.Col) return;
            else possibilities.Add(cp.Possibility);
        }

        if (cell == null) return;

        foreach (var possibility in strategyManager.PossibilitiesAt(cell.Value.Row, cell.Value.Col))
        {
            if (possibilities.Peek(possibility)) return;
            
            strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, cell.Value.Row, cell.Value.Col);
        }
    }
    
    private void ProcessHiddenSet(IStrategyManager strategyManager, IPossibilities home, IPossibilities away,
        GridPositions gpHome, GridPositions gpAway, List<DigitCover> digitCovers)
    {
        var doubleCovers = DigitsCoveredMoreThanOnce(strategyManager, digitCovers);

        if (doubleCovers.Count == 0)
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (gpHome.Peek(row, col) && !gpAway.Peek(row, col))
                    {
                        foreach (var possibility in strategyManager.PossibilitiesAt(row, col))
                        {
                            if (home.Peek(possibility)) continue;

                            strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, row, col);
                        }
                    }
                    else if (!gpHome.Peek(row, col) && gpAway.Peek(row, col))
                    {
                        foreach (var possibility in strategyManager.PossibilitiesAt(row, col))
                        {
                            if (away.Peek(possibility)) continue;

                            strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, row, col);
                        }
                    }
                }
            }
        }
        else
        {
            IPossibilities possibilities = IPossibilities.New();
            Cell? cell = null;
            foreach (var cp in DigitsCoveredMoreThanOnce(strategyManager, digitCovers))
            {
                if (cell is null)
                {
                    cell = new Cell(cp.Row, cp.Col);
                    possibilities.Add(cp.Possibility);
                }
                else if (cp.Row != cell.Value.Row || cp.Col != cell.Value.Col) return;
                else possibilities.Add(cp.Possibility);
            }

            if (cell == null) return;

            foreach (var possibility in strategyManager.PossibilitiesAt(cell.Value.Row, cell.Value.Col))
            {
                if (possibilities.Peek(possibility)) return;
            
                strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, cell.Value.Row, cell.Value.Col);
            }
        }
    }

    private HashSet<CellPossibility> DigitsCoveredMoreThanOnce(IStrategyManager strategyManager,
        List<DigitCover> digitCovers)
    {
        GridPositions[] gps = { new(), new(), new(), new(), new(), new(), new(), new(), new() };
        HashSet<CellPossibility> result = new();

        foreach (var dc in digitCovers)
        {
            foreach (var possibility in dc.Possibilities)
            {
                switch (dc.Unit)
                {
                    case Unit.Row :
                        for (int col = 0; col < 9; col++)
                        {
                            IsCoveredMoreThanOnce(strategyManager, dc.UnitNumber, col, possibility, result, gps);
                        }
                        break;
                    case Unit.Column :
                        for (int row = 0; row < 9; row++)
                        {
                            IsCoveredMoreThanOnce(strategyManager, row, dc.UnitNumber, possibility, result, gps);
                        }
                        break;
                    case Unit.MiniGrid :
                        for (int gridRow = 0; gridRow < 3; gridRow++)
                        {
                            for (int gridCol = 0; gridCol < 3; gridCol++)
                            {
                                IsCoveredMoreThanOnce(strategyManager, dc.UnitNumber / 3 * 3 + gridRow,
                                    dc.UnitNumber % 3 * 3 + gridCol, possibility, result, gps);
                            }
                        }

                        break;
                }
            }
        }

        return result;
    }

    private void IsCoveredMoreThanOnce(IStrategyManager strategyManager, int row, int col, int possibility,
        HashSet<CellPossibility> result, GridPositions[] gps)
    {
        if (strategyManager.Sudoku[row, col] != 0) return;

        if (gps[possibility - 1].Peek(row, col))
            result.Add(new CellPossibility(possibility, row, col));
        else gps[possibility - 1].Add(row, col);
    }
}

public interface ICoverHouse
{
    public DigitCover Apply(IStrategyManager strategyManager, IPossibilities associatedSet, ref GridPositions gp);
}

public class CoverRow : ICoverHouse
{
    private readonly int _row;

    public CoverRow(int row)
    {
        _row = row;
    }

    public DigitCover Apply(IStrategyManager strategyManager, IPossibilities associatedSet, ref GridPositions gp)
    {
        IPossibilities cover = associatedSet.Copy();
        
        for (int col = 0; col < 9; col++)
        {
            var solved = strategyManager.Sudoku[_row, col];

            if (solved == 0) gp.Add(_row, col);
            else if (associatedSet.Peek(solved)) cover.Remove(solved);
        }

        return new DigitCover(cover, Unit.Row, _row);
    }
}

public class CoverColumn : ICoverHouse
{
    private readonly int _col;

    public CoverColumn(int col)
    {
        _col = col;
    }

    public DigitCover Apply(IStrategyManager strategyManager, IPossibilities associatedSet, ref GridPositions gp)
    {
        IPossibilities cover = associatedSet.Copy();
        
        for (int row = 0; row < 9; row++)
        {
            var solved = strategyManager.Sudoku[row, _col];

            if (solved == 0) gp.Add(row, _col);
            else if (associatedSet.Peek(solved)) cover.Remove(solved);
        }

        return new DigitCover(cover, Unit.Column, _col);
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

    public DigitCover Apply(IStrategyManager strategyManager, IPossibilities associatedSet, ref GridPositions gp)
    {
        IPossibilities cover = associatedSet.Copy();

        for (int gridRow = 0; gridRow < 3; gridRow++)
        {
            for (int gridCol = 0; gridCol < 3; gridCol++)
            {
                int row = _miniRow * 3 + gridRow;
                int col = _miniCol * 3 + gridCol;
                
                var solved = strategyManager.Sudoku[row, col];

                if (solved == 0) gp.Add(row, col);
                else if (associatedSet.Peek(solved)) cover.Remove(solved);
            }
        }

        return new DigitCover(cover, Unit.MiniGrid, _miniRow * 3 + _miniCol);
    }
}

public class DigitCover
{
    public DigitCover(IPossibilities possibilities, Unit unit, int unitNumber)
    {
        Possibilities = possibilities;
        Unit = unit;
        UnitNumber = unitNumber;
    }

    public IPossibilities Possibilities { get; }
    public Unit Unit { get; }
    public int UnitNumber { get; }
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

            IChangeReportBuilder.HighlightChanges(lighter, changes);
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