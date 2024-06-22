using System;
using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Position;
using Model.Utility;

namespace Model.Sudokus.Solver.Strategies;

/// <summary>
/// This class generalize the XWing, Swordfish and JellyFish strategy
/// </summary>
public class GridFormationStrategy : SudokuStrategy
{
    public const string OfficialNameForType2 = "X-Wing";
    public const string OfficialNameForType3 = "Swordfish";
    public const string OfficialNameForType4 = "Jellyfish";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.UnorderedAll;

    private readonly int _type;

    public GridFormationStrategy(int type) : base("", StepDifficulty.None, DefaultInstanceHandling)
    {
        _type = type;
        switch (type)
        {
            case 2 : Name =  OfficialNameForType2;
                Difficulty = StepDifficulty.Medium;
                break;
            case 3 : Name = OfficialNameForType3;
                Difficulty = StepDifficulty.Medium;
                break;
            case 4 : Name = OfficialNameForType4;
                Difficulty = StepDifficulty.Medium;
                break;
            default : throw new ArgumentException("Type not valid");
        }
    }

    public override void Apply(ISudokuSolverData solverData)
    { 
        for (int number = 1; number <= 9; number++)
        {
            if (Search(solverData, 0, Unit.Row, number, new LinePositions(), new LinePositions())) return;
            if (Search(solverData, 0, Unit.Column, number, new LinePositions(), new LinePositions())) return;
        }
    }

    private bool Search(ISudokuSolverData solverData, int start, Unit unit, int number, LinePositions or,
        LinePositions visited)
    {
        for (int i = start; i < 9; i++)
        {
            var current = unit == Unit.Row
                ? solverData.RowPositionsAt(i, number)
                : solverData.ColumnPositionsAt(i, number);
            if (current.Count > _type || current.Count < 1) continue;

            var newOr = or.Or(current);
            if(newOr.Count > _type) continue;

            var newVisited = visited.Copy();
            newVisited.Add(i);

            if (newVisited.Count == _type)
            {
                if (newOr.Count == _type && Process(solverData, newVisited, newOr, number, unit)) return true;
            }
            else Search(solverData, i + 1, unit, number, newOr, newVisited);
        }

        return false;
    }

    private bool Process(ISudokuSolverData solverData, LinePositions visited, LinePositions toRemove, int number, Unit unit)
    {
        foreach (var first in toRemove)
        {
            for (int other = 0; other < 9; other++)
            {
                if (visited.Contains(other)) continue;

                if (unit == Unit.Row) solverData.ChangeBuffer.ProposePossibilityRemoval(number, other, first);
                else solverData.ChangeBuffer.ProposePossibilityRemoval(number, first, other);
            }
        }

        return solverData.ChangeBuffer.Commit( unit == Unit.Row
                ? new GridFormationReportBuilder(visited, toRemove, number)
                : new GridFormationReportBuilder(toRemove, visited, number)) 
               && StopOnFirstPush;
    }
}

public class GridFormationReportBuilder : IChangeReportBuilder<NumericChange, IUpdatableSudokuSolvingState, ISudokuHighlighter>
{
    private readonly LinePositions _rows;
    private readonly LinePositions _cols;
    private readonly int _number;

    public GridFormationReportBuilder(LinePositions rows, LinePositions cols, int number)
    {
        _rows = rows;
        _cols = cols;
        _number = number;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, IUpdatableSudokuSolvingState snapshot)
    {
        List<Cell> coords = new();
        foreach (var row in _rows)
        {
            foreach (var col in _cols)
            {
                if (snapshot.PossibilitiesAt(row, col).Contains(_number)) coords.Add(new Cell(row, col));
            }
        }
        return new ChangeReport<ISudokuHighlighter>( "", lighter =>
        {
            foreach (var coord in coords)
            {
                lighter.HighlightPossibility(_number, coord.Row, coord.Column, ChangeColoration.CauseOffOne);
            }
            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}