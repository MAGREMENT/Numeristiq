using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Core.Steps;
using Model.Tectonics;

namespace DesktopApplication.Presenter.Tectonics.Solve;

public class TectonicStepExplanationPresenter : AbstractStepExplanationPresenter<ITectonicHighlighter, 
    INumericStep<ITectonicHighlighter>, INumericSolvingState>
{
    private readonly IReadOnlyTectonic _tectonic;
    
    public TectonicStepExplanationPresenter(IStepExplanationView view,
        INumericStep<ITectonicHighlighter> numericStep, IReadOnlyTectonic tectonic) : base(view, numericStep,
        new TectonicHighlightTranslator(view.GetDrawer<ITectonicDrawer>()))
    {
        _tectonic = tectonic;
    }

    public override void LoadStep()
    {
        var state = _numericStep.From;
        var drawer = _view.GetDrawer<ITectonicDrawer>();
        
        drawer.RowCount = state.RowCount;
        drawer.ColumnCount = state.ColumnCount;
        
        drawer.ClearBorderDefinitions();
        for (int row = 0; row < state.RowCount; row++)
        {
            for (int col = 0; col < state.ColumnCount - 1; col++)
            {
                drawer.AddBorderDefinition(row, col, BorderDirection.Vertical,
                    _tectonic.GetZone(row, col).Equals(_tectonic.GetZone(row, col + 1)));
            }
        }

        for (int col = 0; col < state.ColumnCount; col++)
        {
            for (int row = 0; row < state.RowCount - 1; row++)
            {
                drawer.AddBorderDefinition(row, col, BorderDirection.Horizontal,
                    _tectonic.GetZone(row, col).Equals(_tectonic.GetZone(row + 1, col)));
            }
        }
        
        drawer.ClearNumbers();
        drawer.ClearHighlights();
        for (int row = 0; row < state.RowCount; row++)
        {
            for (int col = 0; col < state.ColumnCount; col++)
            {
                var number = state[row, col];
                if (number == 0)
                {
                    var zoneSize = _tectonic.GetZone(row, col).Count;
                    drawer.ShowPossibilities(row, col, state.PossibilitiesAt(row, col).Enumerate(1, zoneSize));
                }
                else drawer.ShowSolution(row, col, number);
            }
        }

        _translator.Translate(_numericStep.HighlightManager, false);
        _view.ShowExplanation(_numericStep.Explanation);
    }
}

public class TectonicStepExplanationPresenterBuilder : IStepExplanationPresenterBuilder
{
    private readonly INumericStep<ITectonicHighlighter> _numericStep;
    private readonly IReadOnlyTectonic _tectonic;

    public TectonicStepExplanationPresenterBuilder(INumericStep<ITectonicHighlighter> numericStep, IReadOnlyTectonic tectonic)
    {
        _numericStep = numericStep;
        _tectonic = tectonic;
    }

    public IStepExplanationPresenter Build(IStepExplanationView view)
    {
        return new TectonicStepExplanationPresenter(view, _numericStep, _tectonic);
    }
}