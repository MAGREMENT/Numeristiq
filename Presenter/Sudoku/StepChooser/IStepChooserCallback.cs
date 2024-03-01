using Model;
using Model.Helpers.Changes;
using Model.Sudoku;
using Presenter.Sudoku.Solver;

namespace Presenter.Sudoku.StepChooser;

public interface IStepChooserCallback
{
    ISolverSettings Settings { get; }
    void EnableActionsBack();
    void ApplyCommit(BuiltChangeCommit commit);
    public CellColor GetCellColor(int row, int col);

}