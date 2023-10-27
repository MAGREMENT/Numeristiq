using System;
using System.Collections.Generic;
using System.Text;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.Positions;

namespace Model.Solver.Strategies;

public class MultiSectorLockedSetsStrategy : AbstractStrategy //Not optimal
{
    public const string OfficialName = "Multi-Sector Locket Sets";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;

    private readonly int _minimumUnitCount;
    private readonly int _maximumUnitCount;
    private readonly int _maximumUnitDifference;
    private readonly int _maximumFixedCells;

    public MultiSectorLockedSetsStrategy(int minimumUnitCount, int maximumUnitCount, int maximumUnitDifference,
        int maximumFixedCells) : base(OfficialName, StrategyDifficulty.Extreme, DefaultBehavior)
    {
        _minimumUnitCount = minimumUnitCount;
        _maximumUnitDifference = maximumUnitDifference;
        _maximumFixedCells = maximumFixedCells;
        _maximumUnitCount = maximumUnitCount;
    }

    public override void ApplyOnce(IStrategyManager strategyManager)
    {
        for (int rowCount = 1; rowCount <= 9; rowCount++)
        {
            if (rowCount < _minimumUnitCount || rowCount > _maximumUnitCount) continue;
            
            for (int colCount = 1; colCount <= 9; colCount++)
            {
                if(colCount < _minimumUnitCount
                   || colCount > _maximumUnitCount
                   || Math.Abs(rowCount - colCount) > _maximumUnitDifference) continue;

                if (SearchCombinations(strategyManager, rowCount, colCount)) return;
            }
        }
    }

    private bool SearchCombinations(IStrategyManager strategyManager, int rowCount, int colCount)
    {
        var rowCombinations = EachCombinations(rowCount);
        var colCombinations = EachCombinations(colCount);
        
        foreach (var rows in rowCombinations)
        {
            foreach (var cols in colCombinations)
            {
                if (SearchCombination(strategyManager, rows, cols)) return true;
            }
        }

        return false;
    }

    private List<LinePositions> EachCombinations(int number)
    {
        List<LinePositions> result = new();

        EachCombinations(result, new LinePositions(),number, 0, 0);

        return result;
    }

    private void EachCombinations(List<LinePositions> result, LinePositions current, int number, int count, int start)
    {
        for (int i = start; i < 9; i++)
        {
            current.Add(i);
            
            if (count + 1 == number) result.Add(current.Copy());
            else EachCombinations(result, current, number, count + 1, i + 1);

            current.Remove(i);
        }
    }

    private bool SearchCombination(IStrategyManager strategyManager, LinePositions rows, LinePositions cols)
    {
        GridPositions[] possibilitiesPositions = { new(), new(), new(), new(), new(), new(), new(), new(), new() };
        int fixedCount = 0;

        foreach (var row in rows)
        {
            foreach (var col in cols)
            {
                var possibilities = strategyManager.PossibilitiesAt(row, col);
                
                if (possibilities.Count == 0)
                {
                    fixedCount++;
                    if(fixedCount > _maximumFixedCells) return false;
                }
                else
                {
                    foreach (var possibility in possibilities)
                    {
                        possibilitiesPositions[possibility - 1].Add(row, col);
                    }
                }
            }
        }

        List<RowPossibility> rowCovers = new();
        List<ColumnPossibility> colCovers = new();
        List<MiniGridPossibility> miniCovers = new();

        for (int number = 1; number <= 9; number++)
        {
            var positions = possibilitiesPositions[number - 1];
            if (positions.Count == 0) continue;

            var copy = positions.Copy();
            foreach (var cell in positions)
            {
                if(!copy.Peek(cell)) continue;

                int rowCount = copy.RowCount(cell.Row);
                int colCount = copy.ColumnCount(cell.Col);
                int miniCount = copy.MiniGridCount(cell.Row / 3, cell.Col / 3);

                if (rowCount >= colCount && rowCount >= miniCount)
                {
                    rowCovers.Add(new RowPossibility(cell.Row, number));
                    copy.VoidRow(cell.Row);
                }
                else if(colCount >= miniCount)
                {
                    colCovers.Add(new ColumnPossibility(cell.Col, number));
                    copy.VoidColumn(cell.Col);
                }
                else
                {
                    miniCovers.Add(new MiniGridPossibility(cell.Row / 3, cell.Col / 3, number));
                }
            }
        }

        int crossCellsTotal = rows.Count * cols.Count - fixedCount;
        int coverHousesTotal = rowCovers.Count + colCovers.Count + miniCovers.Count;

        if (crossCellsTotal != coverHousesTotal) return false;
        //MSLS found !

        foreach (var row in rowCovers)
        {
            for (int col = 0; col < 9; col++)
            {
                if (!cols.Peek(col)) strategyManager.ChangeBuffer.AddPossibilityToRemove(row.Possibility, row.Row, col);
            }
        }

        foreach (var col in colCovers)
        {
            for (int row = 0; row < 9; row++)
            {
                if (!rows.Peek(row))
                    strategyManager.ChangeBuffer.AddPossibilityToRemove(col.Possibility, row, col.Column);
            }
        }

        foreach (var mini in miniCovers)
        {
            for (int gridRow = 0; gridRow < 3; gridRow++)
            {
                for (int gridCol = 0; gridCol < 3; gridCol++)
                {
                    int row = mini.MiniRow * 3 + gridRow;
                    int col = mini.MiniCol * 3 + gridCol;

                    if (!rows.Peek(row) && !cols.Peek(col))
                        strategyManager.ChangeBuffer.AddPossibilityToRemove(mini.Possibility, row, col);
                }
            }
        }

        return strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer
            .Commit(this, new MultiSectorLockedSetsReportBuilder(rows, cols, rowCovers, colCovers, miniCovers))
            && OnCommitBehavior == OnCommitBehavior.Return;
    }
}

public struct RowPossibility
{
    public RowPossibility(int row, int possibility)
    {
        Row = row;
        Possibility = possibility;
    }

    public int Row { get; }
    public int Possibility { get; }
}

public struct ColumnPossibility
{
    public ColumnPossibility(int column, int possibility)
    {
        Column = column;
        Possibility = possibility;
    }

    public int Column { get; }
    public int Possibility { get; }
}

public struct MiniGridPossibility
{
    public MiniGridPossibility(int miniRow, int miniCol, int possibility)
    {
        MiniRow = miniRow;
        MiniCol = miniCol;
        Possibility = possibility;
    }

    public int MiniRow { get; }
    public int MiniCol { get; }
    public int Possibility { get; }
}

public class MultiSectorLockedSetsReportBuilder : IChangeReportBuilder
{
    private readonly LinePositions _rows;
    private readonly LinePositions _cols;
    private readonly List<RowPossibility> _rowCovers;
    private readonly List<ColumnPossibility> _colCovers;
    private readonly List<MiniGridPossibility> _miniCovers;

    public MultiSectorLockedSetsReportBuilder(LinePositions rows, LinePositions cols, List<RowPossibility> rowCovers, List<ColumnPossibility> colCovers, List<MiniGridPossibility> miniCovers)
    {
        _rows = rows;
        _cols = cols;
        _rowCovers = rowCovers;
        _colCovers = colCovers;
        _miniCovers = miniCovers;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), Explanation(), lighter =>
        {
            foreach (var row in _rows)
            {
                foreach (var col in _cols)
                {
                    if(snapshot.Sudoku[row, col] == 0) lighter.HighlightCell(row, col, ChangeColoration.Neutral);
                }
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }

    private string Explanation()
    {
        var builder = new StringBuilder("Covers : \n");
        bool yes = false;

        foreach (var row in _rowCovers)
        {
            if (yes) builder.Append(", ");
            else yes = true;

            builder.Append($"Row {row.Row + 1} for {row.Possibility}");
        }
        
        foreach (var col in _colCovers)
        {
            if (yes) builder.Append(", ");
            else yes = true;

            builder.Append($"Column {col.Column + 1} for {col.Possibility}");
        }
        
        foreach (var mini in _miniCovers)
        {
            if (yes) builder.Append(", ");
            else yes = true;

            builder.Append($"Row {mini.MiniRow * 3 + mini.MiniCol + 1} for {mini.Possibility}");
        }

        return builder.ToString();
    }
}