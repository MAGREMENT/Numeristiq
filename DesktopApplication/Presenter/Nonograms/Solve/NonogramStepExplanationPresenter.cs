using Model.Core;
using Model.Core.Highlighting;
using Model.Core.Steps;

namespace DesktopApplication.Presenter.Nonograms.Solve;

public class NonogramStepExplanationPresenter : AbstractStepExplanationPresenter<INonogramHighlighter,
    IStep<INonogramHighlighter, IDichotomousSolvingState>, IDichotomousSolvingState>
{
    public NonogramStepExplanationPresenter(IStepExplanationView view, IStep<INonogramHighlighter, IDichotomousSolvingState> step) 
        : base(view, step, new NonogramHighlightTranslator(view.GetDrawer<INonogramDrawer>()))
    {
    }

    public override void LoadStep()
    {
        var state = _numericStep.From;
        var drawer = _view.GetDrawer<INonogramDrawer>();
        
        for (int row = 0; row < state.RowCount; row++)
        {
            for (int col = 0; col < state.ColumnCount; col++)
            {
                if (!state.IsAvailable(row, col)) drawer.SetSolution(row, col);
                else drawer.SetUnavailable(row, col);
            }
        }

        _translator.Translate(_numericStep.HighlightManager, false);
        _view.ShowExplanation(_numericStep.Explanation);
    }
}

public class NonogramStepExplanationPresenterBuilder : IStepExplanationPresenterBuilder
{
    private readonly IStep<INonogramHighlighter, IDichotomousSolvingState> _step;

    public NonogramStepExplanationPresenterBuilder(IStep<INonogramHighlighter, IDichotomousSolvingState> step)
    {
        _step = step;
    }

    public IStepExplanationPresenter Build(IStepExplanationView view)
    {
        return new NonogramStepExplanationPresenter(view, _step);
    }
}