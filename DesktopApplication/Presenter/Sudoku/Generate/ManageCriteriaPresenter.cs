using Model.Sudoku.Generator;

namespace DesktopApplication.Presenter.Sudoku.Generate;

public class ManageCriteriaPresenter
{
    private readonly IManageCriteriaView _view;
    private readonly SudokuEvaluator _evaluator;

    public ManageCriteriaPresenter(IManageCriteriaView view, SudokuEvaluator evaluator)
    {
        _view = view;
        _evaluator = evaluator;
    }
}