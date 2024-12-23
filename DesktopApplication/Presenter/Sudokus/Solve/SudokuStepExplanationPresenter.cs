﻿using Model.Core;
using Model.Core.Highlighting;
using Model.Core.Steps;
using Model.Utility.BitSets;

namespace DesktopApplication.Presenter.Sudokus.Solve;

public class SudokuStepExplanationPresenter : AbstractStepExplanationPresenter<ISudokuHighlighter, 
    IStep<ISudokuHighlighter, INumericSolvingState>, INumericSolvingState>
{ 
    public SudokuStepExplanationPresenter(IStepExplanationView view,
        IStep<ISudokuHighlighter, INumericSolvingState> numericStep, Settings settings) : base(view, numericStep,
        new SudokuHighlighterTranslator(view.GetDrawer<ISudokuSolverDrawer>(), settings))
    {
    }

    public override void LoadStep()
    {
        var state = _numericStep.From;
        var drawer = _view.GetDrawer<ISudokuSolverDrawer>();
        
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var number = state[row, col];
                if (number == 0) drawer.ShowPossibilities(row, col, state.PossibilitiesAt(row, col).EnumeratePossibilities());
                else drawer.ShowSolution(row, col, number);
            }
        }

        _translator.Translate(_numericStep.HighlightCollection, false);
        _view.ShowExplanation(_numericStep.Explanation);
    }
}

public class SudokuStepExplanationPresenterBuilder : IStepExplanationPresenterBuilder
{
    private readonly IStep<ISudokuHighlighter, INumericSolvingState> _numericStep;
    private readonly Settings _settings;

    public SudokuStepExplanationPresenterBuilder(IStep<ISudokuHighlighter, INumericSolvingState> numericStep, Settings settings)
    {
        _numericStep = numericStep;
        _settings = settings;
    }

    public IStepExplanationPresenter Build(IStepExplanationView view)
    {
        return new SudokuStepExplanationPresenter(view, _numericStep, _settings);
    }
}