using Model.Sudoku;
using Model.Sudoku.Solver.Helpers.Changes;
using Presenter.Sudoku.Solver;

namespace Presenter.Sudoku.StepChooser;

public interface IStepChooserCallback
{
    ISolverSettings Settings { get; }
    void EnableActionsBack();
    void ApplyCommit(BuiltChangeCommit commit);
    public CellColor GetCellColor(int row, int col);

}