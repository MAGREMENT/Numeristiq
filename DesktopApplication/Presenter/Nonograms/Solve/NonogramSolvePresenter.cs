﻿using System.Threading.Tasks;
using Model.Core;
using Model.Nonograms;
using Model.Nonograms.Solver;
using Model.Nonograms.Solver.Strategies;

namespace DesktopApplication.Presenter.Nonograms.Solve;

public class NonogramSolvePresenter
{
    private readonly INonogramSolveView _view;
    private readonly NonogramSolver _solver;
    
    private int _stepCount;
    private StateShown _stateShown = StateShown.Before;

    public NonogramSolvePresenter(INonogramSolveView view)
    {
        _view = view;
        _solver = new NonogramSolver();
        _solver.StrategyManager.AddStrategies(new PerfectSpaceStrategy(), new NotEnoughSpaceStrategy(),
            new BridgingStrategy(), new EdgeValueStrategy(), new ValueOverlayStrategy());
    }

    public void SetNewNonogram(string s)
    {
        _solver.SetNonogram(NonogramTranslator.TranslateLineFormat(s));
        SetUpNewNonogram();
        ShowCurrentState();
        ClearLogs();
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
    
    /*public void RequestLogOpening(int id)
    {
        var index = id - 1;
        if (index < 0 || index > _solver.Steps.Count) return;
        
        _view.CloseLogs();

        if (_currentlyOpenedStep == index)
        {
            _currentlyOpenedStep = -1;
            SetShownState(_solver, false, true);
        }
        else
        {
            _view.OpenLog(index);
            _currentlyOpenedStep = index;

            var log = _solver.Steps[index];
            SetShownState(_stateShown == StateShown.Before ? log.From : log.To, false, true); 
            _translator.Translate(log.HighlightManager); 
        }
    }

    public void RequestStateShownChange(StateShown ss)
    {
        _stateShown = ss;
        _view.SetLogsStateShown(ss);
        if (_currentlyOpenedStep < 0 || _currentlyOpenedStep > _solver.Steps.Count) return;
        
        var log = _solver.Steps[_currentlyOpenedStep];
        SetShownState(_stateShown == StateShown.Before ? log.From : log.To, false, true); 
        _translator.Translate(log.HighlightManager);
    }

    public void RequestHighlightShift(bool isLeft)
    {
        if (_currentlyOpenedStep < 0 || _currentlyOpenedStep > _solver.Steps.Count) return;
        
        var log = _solver.Steps[_currentlyOpenedStep];
        if(isLeft) log.HighlightManager.ShiftLeft();
        else log.HighlightManager.ShiftRight();
        
        _view.Drawer.ClearHighlights();
        _translator.Translate(log.HighlightManager);
        _view.SetCursorPosition(_currentlyOpenedStep, log.HighlightManager.CursorPosition());
    }*/

    private void OnStrategyEnd(Strategy strategy, int index, int p, int s)
    {
        if (p + s == 0) return;
        
        ShowCurrentState();
        UpdateLogs();
    }
    
    private void ClearLogs()
    {
        _view.ClearLogs();
        _stepCount = 0;
    }
    
    private void UpdateLogs()
    {
        if (_solver.Steps.Count < _stepCount)
        {
            ClearLogs();
            return;
        }

        for (;_stepCount < _solver.Steps.Count; _stepCount++)
        {
            _view.AddLog(_solver.Steps[_stepCount], _stateShown);
        }
    }

    private void SetUpNewNonogram()
    {
        var drawer = _view.Drawer;
        drawer.SetRows(_solver.Nonogram.HorizontalLineCollection);
        drawer.SetColumns(_solver.Nonogram.VerticalLineCollection);
        ShowCurrentState();
    }

    private void ShowCurrentState()
    {
        var drawer = _view.Drawer;
        drawer.ClearSolutions();
        drawer.ClearUnavailable();
        
        for (int row = 0; row < _solver.Nonogram.RowCount; row++)
        {
            for (int col = 0; col < _solver.Nonogram.ColumnCount; col++)
            {
                if (_solver.Nonogram[row, col]) drawer.SetSolution(row, col);
                else if (!_solver.IsAvailable(row, col)) drawer.SetUnavailable(row, col);
            }
        }
        
        drawer.Refresh();
    }
}