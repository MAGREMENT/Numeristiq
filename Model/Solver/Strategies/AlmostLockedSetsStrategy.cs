using System.Collections.Generic;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.Possibilities;
using Model.Solver.StrategiesUtil;

namespace Model.Solver.Strategies;

public class AlmostLockedSetsStrategy : AbstractStrategy //TODO add chains
{
    public const string OfficialName = "Almost Locked Sets";
 
    public AlmostLockedSetsStrategy() : base(OfficialName, StrategyDifficulty.Extreme)
    {
    }

    public override void ApplyOnce(IStrategyManager strategyManager)
    {
        var allAls = strategyManager.PreComputer.AlmostLockedSets();

        for (int i = 0; i < allAls.Count; i++)
        {
            for (int j = i + 1; j < allAls.Count; j++)
            {
                AlmostLockedSet one = allAls[i];
                AlmostLockedSet two = allAls[j];

                if (one.HasAtLeastOneCoordinateInCommon(two)) continue;

                var restrictedCommons = RestrictedCommons(strategyManager, one, two);
                if (restrictedCommons.Count is 0 or > 2) continue;

                foreach (var restrictedCommon in restrictedCommons)
                {
                    foreach (var possibility in one.Possibilities)
                    {
                        if (!two.Possibilities.Peek(possibility) || possibility == restrictedCommon) continue;

                        List<Cell> coords = new();
                        foreach (var oneCoord in one.Coordinates)
                        {
                            if (strategyManager.PossibilitiesAt(oneCoord.Row, oneCoord.Col).Peek(possibility))
                                coords.Add(oneCoord);
                        }

                        foreach (var twoCoord in two.Coordinates)
                        {
                            if (strategyManager.PossibilitiesAt(twoCoord.Row, twoCoord.Col).Peek(possibility))
                                coords.Add(twoCoord);
                        }
                        
                        foreach (var coord in Cells.SharedSeenCells(coords))
                        {
                            strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, coord.Row, coord.Col);
                        }
                    }
                }

                if (restrictedCommons.Count == 2)
                {
                    foreach (var possibility in one.Possibilities)
                    {
                        if (restrictedCommons.Peek(possibility) || two.Possibilities.Peek(possibility)) continue;

                        List<Cell> where = new();
                        foreach (var coord in one.Coordinates)
                        {
                            if (strategyManager.PossibilitiesAt(coord.Row, coord.Col).Peek(possibility)) where.Add(coord);
                        }
                        
                        foreach (var coord in Cells.SharedSeenCells(where))
                        {
                            strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, coord.Row, coord.Col);
                        }
                    }
                    
                    foreach (var possibility in two.Possibilities)
                    {
                        if (restrictedCommons.Peek(possibility) || one.Possibilities.Peek(possibility)) continue;

                        List<Cell> where = new();
                        foreach (var coord in two.Coordinates)
                        {
                            if (strategyManager.PossibilitiesAt(coord.Row, coord.Col).Peek(possibility)) where.Add(coord);
                        }
                        
                        foreach (var coord in Cells.SharedSeenCells(where))
                        {
                            strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, coord.Row, coord.Col);
                        }
                    }
                }

                if(strategyManager.ChangeBuffer.Push(this, new AlmostLockedSetsReportBuilder(one, two))) return;
            }
        }
    }

    private IPossibilities RestrictedCommons(IStrategyManager strategyManager, AlmostLockedSet one, AlmostLockedSet two)
    {
        IPossibilities result = IPossibilities.NewEmpty();

        foreach (var possibility in one.Possibilities)
        {
            if (!two.Possibilities.Peek(possibility)) continue;

            if (IsPossibilityRestricted(strategyManager, one, two, possibility)) result.Add(possibility);
        }

        return result;
    }

    private bool IsPossibilityRestricted(IStrategyManager strategyManager, AlmostLockedSet one, AlmostLockedSet two,
        int possibility)
    {
        foreach (var oneCoord in one.Coordinates)
        {
            if (!strategyManager.PossibilitiesAt(oneCoord.Row, oneCoord.Col).Peek(possibility)) continue;

            foreach (var twoCoord in two.Coordinates)
            {
                if (!strategyManager.PossibilitiesAt(twoCoord.Row, twoCoord.Col).Peek(possibility)) continue;

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

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
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