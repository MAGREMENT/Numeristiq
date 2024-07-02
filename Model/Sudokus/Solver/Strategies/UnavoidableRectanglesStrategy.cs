using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.PossibilityPosition;
using Model.Sudokus.Solver.Utility;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies;

public class UnavoidableRectanglesStrategy : SudokuStrategy
{
    public const string OfficialName = "Unavoidable Rectangles";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    public UnavoidableRectanglesStrategy() : base(OfficialName, StepDifficulty.Hard, DefaultInstanceHandling)
    {
        UniquenessDependency = UniquenessDependency.FullyDependent;
    }
    
    public override void Apply(ISudokuSolverData solverData)
    {
        for (int i = 0; i < 81; i++)
        {
            var row1 = i / 9;
            var col1 = i % 9;

            if (solverData.Sudoku[row1, col1] == 0 || solverData.StartState![row1, col1] != 0) continue;
            
            for (int j = i + 1; j < 81; j++)
            {
                var row2 = j / 9;
                var col2 = j % 9;

                if (solverData.Sudoku[row2, col2] == 0 || solverData.StartState[row2, col2] != 0) continue;

                if (Search(solverData, new BiValue(solverData.Sudoku[row1, col1],
                        solverData.Sudoku[row2, col2]), new Cell(row1, col1), new Cell(row2, col2))) return;
            }
        }
    }

    private bool Search(ISudokuSolverData solverData, BiValue values, params Cell[] floor)
    {
        foreach (var roof in SudokuCellUtility.DeadlyPatternRoofs(floor))
        {
            if (Try(solverData, values, floor, roof)) return true;
        }
        
        return false;
    }

    private bool Try(ISudokuSolverData solverData, BiValue values, Cell[] floor, Cell[] roof)
    {
        if (solverData.StartState![roof[0].Row, roof[0].Column] != 0 || solverData.StartState[roof[1].Row, roof[1].Column] != 0) return false;
        
        var solved1 = solverData.Sudoku[roof[0].Row, roof[0].Column];
        var solved2 = solverData.Sudoku[roof[1].Row, roof[1].Column];
        
        switch (solved1, solved2)
        {
            case (not 0, not 0) :
                return false;
            case (0, not 0) :
                if (solved2 == values.One)
                {
                   solverData.ChangeBuffer.ProposePossibilityRemoval(values.Two, roof[0]);
                   return solverData.ChangeBuffer.NotEmpty() && solverData.ChangeBuffer.Commit(
                              new AvoidableRectanglesReportBuilder(floor, roof)) && StopOnFirstPush;
                }

                return false;
            case(not 0, 0) :
                if (solved1 == values.Two)
                {
                    solverData.ChangeBuffer.ProposePossibilityRemoval(values.One, roof[1]);
                    return solverData.ChangeBuffer.NotEmpty() && solverData.ChangeBuffer.Commit(
                        new AvoidableRectanglesReportBuilder(floor, roof)) && StopOnFirstPush;
                }
                
                return false;
        }

        var possibilitiesRoofOne = solverData.PossibilitiesAt(roof[0]);
        var possibilitiesRoofTwo = solverData.PossibilitiesAt(roof[1]);

        if (!possibilitiesRoofOne.Contains(values.Two) || !possibilitiesRoofTwo.Contains(values.One)) return false;

        if (possibilitiesRoofOne.Count == 2 && possibilitiesRoofTwo.Count == 2)
        {
            var and = possibilitiesRoofOne & possibilitiesRoofTwo;
            if (and.Count == 1)
            {
                var possibility = and.FirstPossibility();
                foreach (var cell in SudokuCellUtility.SharedSeenCells(roof[0], roof[1]))
                {
                    solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
                }
            }
        }

        if (solverData.ChangeBuffer.NotEmpty() && solverData.ChangeBuffer.Commit(
                new AvoidableRectanglesReportBuilder(floor, roof)) && StopOnFirstPush) return true;

        var notBiValuePossibilities = possibilitiesRoofOne | possibilitiesRoofTwo;
        notBiValuePossibilities -= values.One;
        notBiValuePossibilities -= values.Two;
        var ssc = new List<Cell>(SudokuCellUtility.SharedSeenCells(roof[0], roof[1]));
        foreach (var als in solverData.AlmostNakedSetSearcher.InCells(ssc))
        {
            if (!als.Possibilities.ContainsAll(notBiValuePossibilities)) continue;

            ProcessArWithAls(solverData, roof, als);
            if (solverData.ChangeBuffer.NotEmpty() && solverData.ChangeBuffer.Commit(
                    new AvoidableRectanglesWithAlmostLockedSetReportBuilder(floor, roof, als)) &&
                        StopOnFirstPush) return true;
        }

        return false;
    }
    
    private void ProcessArWithAls(ISudokuSolverData solverData, Cell[] roof, IPossibilitiesPositions als)
    {
        List<Cell> buffer = new();
        foreach (var possibility in als.Possibilities.EnumeratePossibilities())
        {
            foreach (var cell in als.EnumerateCells())
            {
                if(solverData.PossibilitiesAt(cell).Contains(possibility)) buffer.Add(cell);
            }

            foreach (var r in roof)
            {
                if (solverData.PossibilitiesAt(r).Contains(possibility)) buffer.Add(r);
            }

            foreach (var cell in SudokuCellUtility.SharedSeenCells(buffer))
            {
                solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
            }
            
            buffer.Clear();
        }
    }
}

public class AvoidableRectanglesReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly Cell[] _floor;
    private readonly Cell[] _roof;

    public AvoidableRectanglesReportBuilder(Cell[] floor, Cell[] roof)
    {
        _floor = floor;
        _roof = roof;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>( "", lighter =>
        {
            foreach (var floor in _floor)
            {
                lighter.HighlightCell(floor, ChangeColoration.CauseOffTwo);
            }

            foreach (var roof in _roof)
            {
                lighter.HighlightCell(roof, snapshot[roof.Row, roof.Column] == 0 ? ChangeColoration.CauseOffOne
                    : ChangeColoration.CauseOffTwo);
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}

public class AvoidableRectanglesWithAlmostLockedSetReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly Cell[] _floor;
    private readonly Cell[] _roof;
    private readonly IPossibilitiesPositions _als;

    public AvoidableRectanglesWithAlmostLockedSetReportBuilder(Cell[] floor, Cell[] roof, IPossibilitiesPositions als)
    {
        _floor = floor;
        _roof = roof;
        _als = als;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>( "", lighter =>
        {
            foreach (var floor in _floor)
            {
                lighter.HighlightCell(floor, ChangeColoration.CauseOffTwo);
            }

            foreach (var roof in _roof)
            {
                lighter.HighlightCell(roof, ChangeColoration.CauseOffOne);
            }

            foreach (var cell in _als.EnumerateCells())
            {
                lighter.HighlightCell(cell, ChangeColoration.CauseOffThree);
            }
            
            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}