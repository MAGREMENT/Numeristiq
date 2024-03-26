using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Highlighting;
using Model.Helpers.Changes;
using Model.Sudoku.Solver.PossibilityPosition;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Utility;

namespace Model.Sudoku.Solver.Strategies;

public class UnavoidableRectanglesStrategy : SudokuStrategy
{
    public const string OfficialName = "Unavoidable Rectangles";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    public UnavoidableRectanglesStrategy() : base(OfficialName, StrategyDifficulty.Hard, DefaultInstanceHandling)
    {
        UniquenessDependency = UniquenessDependency.FullyDependent;
    }
    
    public override void Apply(IStrategyUser strategyUser)
    {
        for (int i = 0; i < 81; i++)
        {
            var row1 = i / 9;
            var col1 = i % 9;

            if (strategyUser.Sudoku[row1, col1] == 0 || strategyUser.StartState[row1, col1] != 0) continue;
            
            for (int j = i + 1; j < 81; j++)
            {
                var row2 = j / 9;
                var col2 = j % 9;

                if (strategyUser.Sudoku[row2, col2] == 0 || strategyUser.StartState[row2, col2] != 0) continue;

                if (Search(strategyUser, new BiValue(strategyUser.Sudoku[row1, col1],
                        strategyUser.Sudoku[row2, col2]), new Cell(row1, col1), new Cell(row2, col2))) return;
            }
        }
    }

    private bool Search(IStrategyUser strategyUser, BiValue values, params Cell[] floor)
    {
        foreach (var roof in Cells.DeadlyPatternRoofs(floor))
        {
            if (Try(strategyUser, values, floor, roof)) return true;
        }
        
        return false;
    }

    private bool Try(IStrategyUser strategyUser, BiValue values, Cell[] floor, Cell[] roof)
    {
        if (strategyUser.StartState[roof[0].Row, roof[0].Column] != 0 || strategyUser.StartState[roof[1].Row, roof[1].Column] != 0) return false;
        
        var solved1 = strategyUser.Sudoku[roof[0].Row, roof[0].Column];
        var solved2 = strategyUser.Sudoku[roof[1].Row, roof[1].Column];
        
        switch (solved1, solved2)
        {
            case (not 0, not 0) :
                return false;
            case (0, not 0) :
                if (solved2 == values.One)
                {
                   strategyUser.ChangeBuffer.ProposePossibilityRemoval(values.Two, roof[0]);
                   return strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit(
                              new AvoidableRectanglesReportBuilder(floor, roof)) && StopOnFirstPush;
                }

                return false;
            case(not 0, 0) :
                if (solved1 == values.Two)
                {
                    strategyUser.ChangeBuffer.ProposePossibilityRemoval(values.One, roof[1]);
                    return strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit(
                        new AvoidableRectanglesReportBuilder(floor, roof)) && StopOnFirstPush;
                }
                
                return false;
        }

        var possibilitiesRoofOne = strategyUser.PossibilitiesAt(roof[0]);
        var possibilitiesRoofTwo = strategyUser.PossibilitiesAt(roof[1]);

        if (!possibilitiesRoofOne.Contains(values.Two) || !possibilitiesRoofTwo.Contains(values.One)) return false;

        if (possibilitiesRoofOne.Count == 2 && possibilitiesRoofTwo.Count == 2)
        {
            var and = possibilitiesRoofOne & possibilitiesRoofTwo;
            if (and.Count == 1)
            {
                var possibility = and.FirstPossibility();
                foreach (var cell in Cells.SharedSeenCells(roof[0], roof[1]))
                {
                    strategyUser.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
                }
            }
        }

        if (strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit(
                new AvoidableRectanglesReportBuilder(floor, roof)) && StopOnFirstPush) return true;

        var notBiValuePossibilities = possibilitiesRoofOne | possibilitiesRoofTwo;
        notBiValuePossibilities -= values.One;
        notBiValuePossibilities -= values.Two;
        var ssc = new List<Cell>(Cells.SharedSeenCells(roof[0], roof[1]));
        foreach (var als in strategyUser.AlmostNakedSetSearcher.InCells(ssc))
        {
            if (!als.Possibilities.ContainsAll(notBiValuePossibilities)) continue;

            ProcessArWithAls(strategyUser, roof, als);
            if (strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit(
                    new AvoidableRectanglesWithAlmostLockedSetReportBuilder(floor, roof, als)) &&
                        StopOnFirstPush) return true;
        }

        return false;
    }
    
    private void ProcessArWithAls(IStrategyUser strategyUser, Cell[] roof, IPossibilitiesPositions als)
    {
        List<Cell> buffer = new();
        foreach (var possibility in als.Possibilities.EnumeratePossibilities())
        {
            foreach (var cell in als.EachCell())
            {
                if(strategyUser.PossibilitiesAt(cell).Contains(possibility)) buffer.Add(cell);
            }

            foreach (var r in roof)
            {
                if (strategyUser.PossibilitiesAt(r).Contains(possibility)) buffer.Add(r);
            }

            foreach (var cell in Cells.SharedSeenCells(buffer))
            {
                strategyUser.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
            }
            
            buffer.Clear();
        }
    }
}

public class AvoidableRectanglesReportBuilder : IChangeReportBuilder<IUpdatableSudokuSolvingState, ISudokuHighlighter>
{
    private readonly Cell[] _floor;
    private readonly Cell[] _roof;

    public AvoidableRectanglesReportBuilder(Cell[] floor, Cell[] roof)
    {
        _floor = floor;
        _roof = roof;
    }

    public ChangeReport<ISudokuHighlighter> Build(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
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
}

public class AvoidableRectanglesWithAlmostLockedSetReportBuilder : IChangeReportBuilder<IUpdatableSudokuSolvingState, ISudokuHighlighter>
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

    public ChangeReport<ISudokuHighlighter> Build(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
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

            foreach (var cell in _als.EachCell())
            {
                lighter.HighlightCell(cell, ChangeColoration.CauseOffThree);
            }
            
            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
}