using System.Collections.Generic;
using Model.Solver.Helpers;
using Model.Solver.Helpers.Changes;
using Model.Solver.Positions;

namespace Model.Solver.Strategies;

/// <summary>
/// Hidden doubles are a special case of hidden possibilities where there are only 2 candidates concerned. See the doc
/// of HiddenPossibilities.cs for more information.
/// </summary>
public class HiddenDoublesStrategy : IStrategy
{
    public string Name => "Hidden doubles";
    public StrategyDifficulty Difficulty => StrategyDifficulty.Easy;
    public StatisticsTracker Tracker { get; } = new();
    public void ApplyOnce(IStrategyManager strategyManager)
    {
        Dictionary<IReadOnlyLinePositions, int> lines = new();
        Dictionary<IReadOnlyMiniGridPositions, int> minis = new();
        for (int row = 0; row < 9; row++)
        {
            for (int number = 1; number <= 9; number++)
            {
                var pos = strategyManager.RowPositionsAt(row, number);
                if (pos.Count != 2) continue;

                if (lines.TryGetValue(pos, out var n)) ProcessRow(strategyManager, pos, row, number, n);
                else lines.Add(pos, number);
            }

            lines.Clear();
        }

        for (int col = 0; col < 9; col++)
        {
            for (int number = 1; number <= 9; number++)
            {
                var pos = strategyManager.ColumnPositionsAt(col, number);
                if (pos.Count != 2) continue;

                if (lines.TryGetValue(pos, out var n)) ProcessColumn(strategyManager, pos, col, number, n);
                else lines.Add(pos, number);
            }

            lines.Clear();
        }

        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                for (int number = 1; number <= 9; number++)
                {
                    var pos = strategyManager.MiniGridPositionsAt(miniRow, miniCol, number);
                    if (pos.Count != 2) continue;

                    if (minis.TryGetValue(pos, out var n)) ProcessMiniGrid(strategyManager, pos, number, n);
                    else minis.Add(pos, number);
                }

                minis.Clear();
            }
        }
    }
    
    private void ProcessRow(IStrategyManager strategyManager, IReadOnlyLinePositions positions, int row, int n1,
        int n2)
    {
        foreach (var col in positions)
        {
            foreach (var possibility in strategyManager.PossibilitiesAt(row, col))
            {
                if(possibility == n1 || possibility == n2) continue;

                strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, row, col);
            }
        }

        strategyManager.ChangeBuffer.Push(this,
            new LineHiddenDoublesReportBuilder(row, positions, n1, n2, Unit.Row));
    }

    private void ProcessColumn(IStrategyManager strategyManager, IReadOnlyLinePositions positions, int col,
        int n1, int n2)
    {
        foreach(var row in positions)
        {
            foreach (var possibility in strategyManager.PossibilitiesAt(row, col))
            {
                if(possibility == n1 || possibility == n2) continue;

                strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, row, col);
            }
        }

        strategyManager.ChangeBuffer.Push(this,
            new LineHiddenDoublesReportBuilder(col, positions, n1, n2, Unit.Column));
    }

    private void ProcessMiniGrid(IStrategyManager strategyManager, IReadOnlyMiniGridPositions positions, int n1, int n2)
    {
        foreach (var cell in positions)
        {
            foreach (var possibility in strategyManager.PossibilitiesAt(cell.Row, cell.Col))
            {
                if(possibility == n1 || possibility == n2) continue;

                strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, cell.Row, cell.Col);
            }
        }

        strategyManager.ChangeBuffer.Push(this,
            new MiniGridHiddenDoublesReportBuilder(positions, n1, n2));
    }
}

public class LineHiddenDoublesReportBuilder : IChangeReportBuilder
{
    private readonly int _unitNumber;
    private readonly IReadOnlyLinePositions _pos;
    private readonly int _n1;
    private readonly int _n2;
    private readonly Unit _unit;

    public LineHiddenDoublesReportBuilder(int unitNumber, IReadOnlyLinePositions pos, int n1, int n2, Unit unit)
    {
        _unitNumber = unitNumber;
        _pos = pos;
        _n1 = n1;
        _n2 = n2;
        _unit = unit;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        var cells = _pos.ToCellArray(_unit, _unitNumber);

        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), Explanation(), lighter =>
        {
            foreach (var cell in cells)
            {
                lighter.HighlightPossibility(_n1, cell.Row, cell.Col, ChangeColoration.CauseOffOne);
                lighter.HighlightPossibility(_n2, cell.Row, cell.Col, ChangeColoration.CauseOffOne);
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }

    private string Explanation()
    {
        return ""; //TODO
    }
}

public class MiniGridHiddenDoublesReportBuilder : IChangeReportBuilder
{
    private readonly IReadOnlyMiniGridPositions _pos;
    private readonly int _n1;
    private readonly int _n2;

    public MiniGridHiddenDoublesReportBuilder(IReadOnlyMiniGridPositions pos, int n1, int n2)
    {
        _pos = pos;
        _n1 = n1;
        _n2 = n2;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        var cells = _pos.ToCellArray();

        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), Explanation(), lighter =>
        {
            foreach (var cell in cells)
            {
                lighter.HighlightPossibility(_n1, cell.Row, cell.Col, ChangeColoration.CauseOffOne);
                lighter.HighlightPossibility(_n2, cell.Row, cell.Col, ChangeColoration.CauseOffOne);
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
    
    private string Explanation()
    {
        return ""; //TODO
    }
}