﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Model.Core;
using Model.Core.Highlighting;
using Model.Core.Steps;
using Model.Nonograms;
using Model.Nonograms.Solver;

namespace DesktopApplication.Presenter.Nonograms.Solve;

public class NonogramSolvePresenter : StepManagingPresenter<INonogramHighlighter,
    IStep<INonogramHighlighter, IDichotomousSolvingState>, IDichotomousSolvingState>
{
    private readonly INonogramSolveView _view;
    private readonly NonogramSolver _solver;

    public NonogramSolvePresenter(INonogramSolveView view, NonogramSolver solver) : base(
        new DefaultHighlightTranslator<INonogramDrawer ,INonogramHighlighter>(view.Drawer))
    {
        _view = view;
        _solver = solver;
    }

    public void SetNewNonogram(string s)
    {
        _solver.SetNonogram(NonogramTranslator.TranslateLineFormat(s));
        SetUpNewNonogram();
        SetShownState(_solver, false, false);
        ClearSteps();
    }

    public void ShowNonogramAsString()
    {
        _view.ShowNonogramAsString(NonogramTranslator.TranslateLineFormat(_solver.Nonogram));
    }

    public async void Solve(bool stopAtProgress)
    {
        _solver.StrategyEnded += OnStrategyEnd;
        await Task.Run(() => _solver.Solve(stopAtProgress));
        _solver.StrategyEnded -= OnStrategyEnd;
    }

    public void SetSizeTo(int rDiff, int cDiff)
    {
        var n = _solver.Nonogram.CopyWithoutDichotomy();
        n.SetSizeTo(n.RowCount + rDiff, n.ColumnCount + cDiff);
        
        _solver.SetNonogram(n);
        SetUpNewNonogram();
        SetShownState(_solver, true, false);
    }

    private void OnStrategyEnd(Strategy strategy, int index, int p, int s)
    {
        if (p + s == 0) return;
        
        SetShownState(_solver, false, false);
        UpdateSteps();
    }

    private void SetUpNewNonogram()
    {
        var drawer = _view.Drawer;
        drawer.SetRows(_solver.Nonogram.HorizontalLines.Enumerate());
        drawer.SetColumns(_solver.Nonogram.VerticalLines.Enumerate());
    }

    protected override void SetShownState(IDichotomousSolvingState state, bool solutionAsClues, bool showPossibilities)
    {
        var drawer = _view.Drawer;
        drawer.ClearSolutions();
        drawer.ClearUnavailable();
        drawer.ClearHighlights();
        
        for (int row = 0; row < state.RowCount; row++)
        {
            for (int col = 0; col < state.ColumnCount; col++)
            {
                if (state[row, col]) drawer.SetSolution(row, col);
                else if (!state.IsAvailable(row, col)) drawer.SetUnavailable(row, col);
            }
        }
        
        drawer.Refresh();
    }

    protected override IReadOnlyList<IStep<INonogramHighlighter, IDichotomousSolvingState>> Steps => _solver.Steps;
    protected override IStepManagingView View => _view;
    public override IStepExplanationPresenterBuilder? RequestExplanation()
    {
        
        if (_currentlyOpenedStep < 0 || _currentlyOpenedStep >= _solver.Steps.Count) return null;

        return new NonogramStepExplanationPresenterBuilder(_solver.Steps[_currentlyOpenedStep]);
    }

    protected override IDichotomousSolvingState GetCurrentState()
    {
        return _solver;
    }
}