using System;
using System.Collections.Generic;
using Model.Changes;
using Model.Possibilities;
using Model.Solver;
using Model.StrategiesUtil;

namespace Model.Strategies;

public class AlmostLockedSetsStrategy : IStrategy
{
    public string Name => "Almost locked sets";
    public StrategyLevel Difficulty => StrategyLevel.Extreme;
    public int Score { get; set; }

    public void ApplyOnce(IStrategyManager strategyManager)
    {
        var allAls = strategyManager.AlmostLockedSets();

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
                    foreach (var possibility in one.Possibilities)
                    {
                        if (restrictedCommons.Peek(possibility) || two.Possibilities.Peek(possibility)) continue;

                        List<Coordinate> where = new();
                        foreach (var coord in one.Coordinates)
                        {
                            if (strategyManager.Possibilities[coord.Row, coord.Col].Peek(possibility)) where.Add(coord);
                        }
                        
                        ProcessTwoRestrictedCommon(strategyManager, where, possibility);
                    }
                    
                    foreach (var possibility in two.Possibilities)
                    {
                        if (restrictedCommons.Peek(possibility) || one.Possibilities.Peek(possibility)) continue;

                        List<Coordinate> where = new();
                        foreach (var coord in two.Coordinates)
                        {
                            if (strategyManager.Possibilities[coord.Row, coord.Col].Peek(possibility)) where.Add(coord);
                        }
                        
                        ProcessTwoRestrictedCommon(strategyManager, where, possibility);
                    }
                }

                if(strategyManager.ChangeBuffer.Push(this, new AlmostLockedSetsReportBuilder(one, two))) return;
            }
        }
    }

    private void ProcessTwoRestrictedCommon(IStrategyManager strategyManager, List<Coordinate> coords, int possibility)
    {
        bool shareRow = true;
        bool shareCol = true;
        bool shareMini = true;

        var first = coords[0];
        foreach (var coord in coords)
        {
            if (coord.Row != first.Row) shareRow = false;
            if (coord.Col != first.Col) shareCol = false;
            if (!(coord.Row / 3 == first.Row / 3 && coord.Col / 3 == first.Col / 3)) shareMini = false;
        }

        if (shareRow)
        {
            for (int col = 0; col < 9; col++)
            {
                Coordinate current = new Coordinate(first.Row, col);
                if(coords.Contains(current)) continue;
                strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, first.Row, col);
            }
        }
        
        if (shareCol)
        {
            for (int row = 0; row < 9; row++)
            {
                Coordinate current = new Coordinate(row, first.Col);
                if(coords.Contains(current)) continue;
                strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, row, first.Col);
            }
        }
        
        if (shareMini)
        {
            for (int gridRow = 0; gridRow < 3; gridRow++)
            {
                for (int gridCol = 0; gridCol < 3; gridCol++)
                {
                    int row = first.Row / 3 * 3 + gridRow;
                    int col = first.Col / 3 * 3 + gridCol;
                    
                    Coordinate current = new Coordinate(row, col);
                    if(coords.Contains(current)) continue;
                    strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, row, col);
                }
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
                    if (coord == current || !coord.ShareAUnit(current))
                    {
                        ok = false;
                        break;
                    }
                }

                if (ok) strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, row, col);
            }
        }
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
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
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
        });
    }
}