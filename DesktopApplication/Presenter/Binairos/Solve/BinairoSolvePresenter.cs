using System.Collections.Generic;
using System.Threading.Tasks;
using Model.Binairos;
using Model.Core;
using Model.Core.Highlighting;
using Model.Core.Steps;

namespace DesktopApplication.Presenter.Binairos.Solve;

public class BinairoSolvePresenter : SolveWithStepsPresenter<IBinairoHighlighter, IBinaryStep<IBinairoHighlighter>,
    IBinarySolvingState>
{
    private readonly IBinairoSolveView _view;
    private readonly BinairoSolver _solver;
    
    public SettingsPresenter SettingsPresenter { get; }
    
    public BinairoSolvePresenter(IBinairoSolveView view, BinairoSolver solver, Settings settings) : 
        base(new BinairoHighlightTranslator(view.Drawer))
    {
        _view = view;
        _solver = solver;
        SettingsPresenter = new SettingsPresenter(settings, SettingCollections.BinairoSolvePage);

        _view.Drawer.LinkOffsetSidePriority = (LinkOffsetSidePriority)settings.LinkOffsetSidePriority.Get().ToInt();
        _view.Drawer.AreSolutionNumbers = settings.AreSolutionNumbers.Get().ToBool();

        settings.LinkOffsetSidePriority.ValueChanged += v =>
        {
            _view.Drawer.LinkOffsetSidePriority = (LinkOffsetSidePriority)v.ToInt();
            _view.Drawer.Refresh();
        };
        settings.AreSolutionNumbers.ValueChanged += v =>
        {
            _view.Drawer.AreSolutionNumbers = v.ToBool();
            _view.Drawer.Refresh();
        };
    }

    public void SetNewBinairo(string s)
    {
        SetNewBinairo(BinairoTranslator.TranslateLineFormat(s));
    }

    public void ShowBinairoAsString() =>
        _view.SetBinairoAsString(BinairoTranslator.TranslateLineFormat(_solver.Binairo));

    public async void Solve(bool stopOnProgress)
    {
        _solver.StrategyEnded += OnStrategyEnd;

        await Task.Run(() => _solver.Solve(stopOnProgress));
        
        _solver.StrategyEnded -= OnStrategyEnd;
    }

    protected override IReadOnlyList<IBinaryStep<IBinairoHighlighter>> Steps => _solver.Steps;
    protected override ISolveWithStepsView View => _view;
    public override IStepExplanationPresenterBuilder? RequestExplanation()
    {
        return null; //TODO
    }

    private void OnStrategyEnd(Strategy s, int i, int a, int b)
    {
        if (a + b == 0) return;
        
        SetShownState(_solver, false, true);
        UpdateSteps();
    }

    private void SetNewBinairo(Binairo binairo)
    {
        _solver.SetBinairo(binairo);
        _view.Drawer.RowCount = binairo.RowCount;
        _view.Drawer.ColumnCount = binairo.ColumnCount;
        SetShownState(_solver, true, true);
        ClearSteps();
    }

    protected override void SetShownState(IBinarySolvingState numericSolvingState, bool solutionAsClues, bool showPossibilities)
    {
        var drawer = _view.Drawer;
        
        drawer.ClearSolutions();
        for (int row = 0; row < numericSolvingState.RowCount; row++)
        {
            for (int col = 0; col < numericSolvingState.ColumnCount; col++)
            {
                var n = numericSolvingState[row, col];
                if (solutionAsClues) drawer.SetClue(row, col, n != 0);
                if (n != 0) drawer.ShowSolution(n, row, col);
            }
        }
        
        drawer.Refresh();
    }

    protected override IBinarySolvingState GetCurrentState()
    {
        return _solver;
    }
}