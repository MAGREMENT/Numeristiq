using Global.Enums;
using Model.SudokuSolving.Solver.Helpers.Changes;
using Presenter.Solver;

namespace Presenter.StepChooser;

public interface IStepChooserCallback
{
    ISolverSettings Settings { get; }
    void EnableActionsBack();
    void ApplyCommit(BuiltChangeCommit commit);
    public CellColor GetCellColor(int row, int col);

}