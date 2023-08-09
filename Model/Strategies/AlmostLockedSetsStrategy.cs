using System;
using System.Collections.Generic;
using Model.Possibilities;
using Model.StrategiesUtil;

namespace Model.Strategies;

public class AlmostLockedSetsStrategy : IStrategy //TODO IMPROVE !!!!
{
    public string Name => "Almost locked sets";
    public StrategyLevel Difficulty => StrategyLevel.Extreme;
    public int Score { get; set; }

    public void ApplyOnce(IStrategyManager strategyManager)
    {
        var allAls = AllAls(strategyManager);

        for (int i = 0; i < allAls.Count; i++)
        {
            for (int j = i + 1; j < allAls.Count; j++)
            {
                AlmostLockedSet one = allAls[i];
                AlmostLockedSet two = allAls[j];

                if (one.HasAtLeastOneCoordinateInCommon(two)) continue;

                var restrictedCommons = RestrictedCommons(strategyManager, one, two);
                if (restrictedCommons.Count == 0) continue;

                foreach (var restrictedCommon in restrictedCommons)
                {
                    foreach (var possibility in one.Possibilities)
                    {
                        if (!two.Possibilities.Peek(possibility) || possibility == restrictedCommon) continue;

                        List<Coordinate> coords = new();
                        foreach (var oneCoord in one.Coordinates)
                        {
                            if (strategyManager.Possibilities[oneCoord.Row, oneCoord.Col].Peek(possibility))
                                coords.Add(oneCoord);
                        }

                        foreach (var twoCoord in two.Coordinates)
                        {
                            if (strategyManager.Possibilities[twoCoord.Row, twoCoord.Col].Peek(possibility))
                                coords.Add(twoCoord);
                        }

                        if (coords.Count < 2)
                        {
                            throw new Exception("Wtf");
                        }
                        ProcessOneRestrictedCommon(strategyManager, coords, possibility);
                    }
                }

                if (restrictedCommons.Count == 2)
                {
                    //TODO
                }

                strategyManager.GetChangeBuffer().Push(this, new AlmostLockedSetsReportBuilder(one, two));
                return;
            }
        }
    }

    private void ProcessOneRestrictedCommon(IStrategyManager strategyManager, List<Coordinate> coords, int possibility)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                Coordinate current = new Coordinate(row, col);

                bool ok = true;
                foreach (var coord in coords)
                {
                    if (coord.Equals(current) || !coord.ShareAUnit(current))
                    {
                        ok = false;
                        break;
                    }
                }

                if (ok) strategyManager.GetChangeBuffer().AddPossibilityToRemove(possibility, row, col);
            }
        }
    }

    private List<AlmostLockedSet> AllAls(IStrategyManager strategyManager) //TODO improve !!!!
    {
        var result = new List<AlmostLockedSet>();

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (strategyManager.Sudoku[row, col] != 0) continue;

                if (strategyManager.Possibilities[row, col].Count == 2)
                    result.Add(new AlmostLockedSet(new Coordinate(row, col), strategyManager.Possibilities[row, col]));
                SearchRow(strategyManager, row, col + 1, strategyManager.Possibilities[row, col], 
                    new List<Coordinate> {new (row, col)}, result);
            }
        }
        
        for (int col = 0; col < 9; col++)
        {
            for (int row = 0; row < 9; row++)
            {
                if (strategyManager.Sudoku[row, col] != 0) continue;
                
                SearchColumn(strategyManager, col, row + 1, strategyManager.Possibilities[row, col], 
                    new List<Coordinate> {new (row, col)}, result);
            }
        }

        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                for (int n = 0; n < 9; n++)
                {
                    int row = miniRow * 3 + n / 3;
                    int col = miniCol * 3 + n % 3;
                    if (strategyManager.Sudoku[row, col] != 0) continue;
                    
                    SearchMiniGrid(strategyManager, miniRow, miniCol, n + 1, strategyManager.Possibilities[row, col],
                        new List<Coordinate> {new (row, col)}, result);
                }
            }
        }

        return result;
    }

    private void SearchRow(IStrategyManager strategyManager, int row, int start, IPossibilities current,
        List<Coordinate> visited, List<AlmostLockedSet> result)
    {
        for (int col = start; col < 9; col++)
        {
            if (strategyManager.Sudoku[row, col] != 0) continue;

            IPossibilities mashed = current.Mash(strategyManager.Possibilities[row, col]);
            if (mashed.Count == current.Count + strategyManager.Possibilities[row, col].Count) continue;

            visited.Add(new Coordinate(row, col));

            if (mashed.Count == visited.Count + 1)
            {
                result.Add(new AlmostLockedSet(visited.ToArray(), mashed));
            }

            SearchRow(strategyManager, row, col + 1, mashed, new List<Coordinate>(visited), result);
        }
    }

    private void SearchColumn(IStrategyManager strategyManager, int col, int start, IPossibilities current,
        List<Coordinate> visited, List<AlmostLockedSet> result)
    {
        for (int row = start; row < 9; row++)
        {
            if (strategyManager.Sudoku[row, col] != 0) continue;

            IPossibilities mashed = current.Mash(strategyManager.Possibilities[row, col]);
            if (mashed.Count == current.Count + strategyManager.Possibilities[row, col].Count) continue;

            visited.Add(new Coordinate(row, col));

            if (mashed.Count == visited.Count + 1)
            {
                result.Add(new AlmostLockedSet(visited.ToArray(), mashed));
            }

            SearchColumn(strategyManager, col, row + 1, mashed, new List<Coordinate>(visited), result);
        }
    }

    private void SearchMiniGrid(IStrategyManager strategyManager, int miniRow, int miniCol, int start,
        IPossibilities current, List<Coordinate> visited, List<AlmostLockedSet> result)
    {
        for (int n = start; n < 9; n++)
        {
            int row = miniRow * 3 + n / 3;
            int col = miniCol * 3 + n % 3;
                
            if (strategyManager.Sudoku[row, col] != 0) continue;

            IPossibilities mashed = current.Mash(strategyManager.Possibilities[row, col]);
            if (mashed.Count == current.Count + strategyManager.Possibilities[row, col].Count) continue;

            visited.Add(new Coordinate(row, col));

            if (mashed.Count == visited.Count + 1 && NotInSameRowOrColumn(visited))
            {
                result.Add(new AlmostLockedSet(visited.ToArray(), mashed));
            }

            SearchMiniGrid(strategyManager, miniRow, miniCol, n + 1, mashed,
                new List<Coordinate>(visited), result);
        }
    }

    private bool NotInSameRowOrColumn(List<Coordinate> coord)
    {
        int row = coord[0].Row;
        int col = coord[0].Col;

        bool rowOk = false;
        bool colOk = false;

        for (int i = 1; i < coord.Count; i++)
        {
            if (!rowOk && coord[i].Row != row) rowOk = true;
            if (!colOk && coord[i].Col != col) colOk = true;

            if (rowOk && colOk) return true;
        }

        return false;
    }

    private IPossibilities RestrictedCommons(IStrategyManager strategyManager, AlmostLockedSet one, AlmostLockedSet two)
    {
        IPossibilities result = IPossibilities.NewEmpty();

        foreach (var possibility in one.Possibilities)
        {
            if (!two.Possibilities.Peek(possibility)) continue;

            if (IsRestricted(strategyManager, one, two, possibility)) result.Add(possibility);
        }

        return result;
    }

    //Both als have to share the possibility for this function to work
    private bool IsRestricted(IStrategyManager strategyManager, AlmostLockedSet one, AlmostLockedSet two,
        int possibility)
    {
        foreach (var oneCoord in one.Coordinates)
        {
            if (!strategyManager.Possibilities[oneCoord.Row, oneCoord.Col].Peek(possibility)) continue;

            foreach (var twoCoord in two.Coordinates)
            {
                if (!strategyManager.Possibilities[twoCoord.Row, twoCoord.Col].Peek(possibility)) continue;

                if (!oneCoord.ShareAUnit(twoCoord)) return false;
            }
        }

        return true;
    }
}

public class AlmostLockedSetsReportBuilder : IChangeReportBuilder
{
    private readonly AlmostLockedSet _one;
    private readonly AlmostLockedSet _two;

    public AlmostLockedSetsReportBuilder(AlmostLockedSet one, AlmostLockedSet two)
    {
        _one = one;
        _two = two;
    }

    public ChangeReport Build(List<SolverChange> changes, IChangeManager manager)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), lighter =>
        {
            foreach (var coord in _one.Coordinates)
            {
                lighter.HighlightCell(coord.Row, coord.Col, ChangeColoration.CauseOffOne);
            }

            foreach (var coord in _two.Coordinates)
            {
                lighter.HighlightCell(coord.Row, coord.Col, ChangeColoration.CauseOffTwo);
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        }, "");
    }
}