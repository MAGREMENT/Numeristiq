using Model.Core;
using Model.Core.Highlighting;
using Model.Core.Steps;

namespace DesktopApplication.Presenter.Nonograms.Solve;

public class NonogramStepExplanationPresenter : AbstractStepExplanationPresenter<INonogramHighlighter,
    IDichotomousStep<INonogramHighlighter>, IDichotomousSolvingState>
{
    public NonogramStepExplanationPresenter(IStepExplanationView view, IDichotomousStep<INonogramHighlighter> step) 
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
    private readonly IDichotomousStep<INonogramHighlighter> _step;

    public NonogramStepExplanationPresenterBuilder(IDichotomousStep<INonogramHighlighter> step)
    {
        _step = step;
    }

    public IStepExplanationPresenter Build(IStepExplanationView view)
    {
        return new NonogramStepExplanationPresenter(view, _step);
    }
}