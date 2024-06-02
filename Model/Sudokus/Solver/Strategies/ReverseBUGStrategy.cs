using System.Collections.Generic;
using Model.Core;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Highlighting;
using Model.Sudokus.Solver.Position;
using Model.Sudokus.Solver.Utility;

namespace Model.Sudokus.Solver.Strategies;

public class ReverseBUGStrategy : SudokuStrategy
{
    public const string OfficialName = "Reverse BUG";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    public ReverseBUGStrategy() : base(OfficialName, StepDifficulty.Medium, DefaultInstanceHandling)
    {
        UniquenessDependency = UniquenessDependency.FullyDependent;
    }

    public override void Apply(ISudokuStrategyUser strategyUser)
    {
        GridPositions[] positions = { new(), new(), new(), new(), new(), new(), new(), new(), new() };

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var solution = strategyUser.Sudoku[row, col];
                if (solution != 0) positions[solution - 1].Add(row, col);
            }
        }

        for (int n1 = 1; n1 <= 8; n1++)
        {
            var pos1 = positions[n1 - 1];

            for (int n2 = 1; n2 <= 9; n2++)
            {
                var pos2 = positions[n2 - 1];
                var or = pos1.Or(pos2);
                if (or.Count >= 17) continue;

                var soloRow = UniquenessHelper.SearchExceptionInUnit(Unit.Row, 2, or);
                if (soloRow == -1) continue;

                var soloCol = UniquenessHelper.SearchExceptionInUnit(Unit.Column, 2, or);
                if (soloCol == -1) continue;

                var soloMini = UniquenessHelper.SearchExceptionInUnit(Unit.MiniGrid, 2, or);
                if (soloMini == -1) continue;

                var miniRow = soloMini / 3;
                var miniCol = soloCol / 3;

                if (soloRow / 3 == miniRow && soloCol / 3 == miniCol)
                {
                    strategyUser.ChangeBuffer.ProposePossibilityRemoval(n1, soloRow, soloCol);
                    strategyUser.ChangeBuffer.ProposePossibilityRemoval(n2, soloRow, soloCol);
                }

                if (strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit(
                        new ReverseBugReportBuilder(or, n1)) && StopOnFirstPush) return;
            }
        }
    }
}

public class ReverseBugReportBuilder : IChangeReportBuilder<IUpdatableSudokuSolvingState, ISudokuHighlighter>
{
    private readonly GridPositions _gp;
    private readonly int _n1;

    public ReverseBugReportBuilder(GridPositions gp, int n1)
    {
        _gp = gp;
        _n1 = n1;
    }
    
    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>( "", lighter =>
        {
            foreach (var cell in _gp)
            {
                lighter.HighlightCell(cell.Row, cell.Column, snapshot[cell.Row, cell.Column] == _n1
                        ? ChangeColoration.CauseOffOne
                        : ChangeColoration.CauseOffTwo);
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}