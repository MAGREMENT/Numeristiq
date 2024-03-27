using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using DesktopApplication.Presenter.Sudoku;
using DesktopApplication.Presenter.Sudoku.Solve;
using DesktopApplication.View.Controls;
using DesktopApplication.View.HelperWindows;
using DesktopApplication.View.Sudoku.Controls;
using Model;
using Model.Helpers.Highlighting;
using Model.Helpers.Logs;
using Model.Sudoku.Solver;

namespace DesktopApplication.View.Sudoku.Pages;

public partial class SolvePage : ISudokuSolveView
{
    private readonly SudokuSolvePresenter _presenter;

    private int _logOpen = -1;
    
    public SolvePage(SudokuApplicationPresenter appPresenter)
    {
        InitializeComponent();
        _presenter = appPresenter.Initialize(this);
    }

    #region ISudokuSolveView

    public ISudokuDrawer Drawer => Board;

    public void SetSudokuAsString(string s)
    {
        SudokuAsString.SetText(s);
    }

    public void DisableSolveActions()
    {
        SolveButton.IsEnabled = false;
        AdvanceButton.IsEnabled = false;
        ChooseStepButton.IsEnabled = false;
        ClearButton.IsEnabled = false;
    }

    public void EnableSolveActions()
    {
        SolveButton.IsEnabled = true;
        AdvanceButton.IsEnabled = true;
        ChooseStepButton.IsEnabled = true;
        ClearButton.IsEnabled = true;
    }

    public void AddLog(ISolverLog<ISudokuHighlighter> log, StateShown stateShown)
    {
        LogPanel.Dispatcher.Invoke(() =>
        {
            var lc = new LogControl(log, stateShown);
            LogPanel.Children.Add(lc);
            lc.OpenRequested += _presenter.RequestLogOpening;
            lc.StateShownChanged += _presenter.RequestStateShownChange;
            lc.HighlightShifted += _presenter.RequestHighlightShift;
            lc.ExplanationAsked += () =>
            {
                var builder = _presenter.RequestExplanation();
                if (builder is null) return;

                var window = new StepExplanationWindow(builder);
                window.Show();
            };
        });
        LogViewer.Dispatcher.Invoke(() => LogViewer.ScrollToEnd());
    }

    public void ClearLogs()
    {
        LogPanel.Children.Clear();
    }

    public void OpenLog(int index)
    {
        if (index < 0 || index > LogPanel.Children.Count) return;
        if (LogPanel.Children[index] is not LogControl lc) return;

        _logOpen = index;
        lc.Open();
    }

    public void CloseLogs()
    {
        if (_logOpen < 0 || _logOpen > LogPanel.Children.Count) return;
        if (LogPanel.Children[_logOpen] is not LogControl lc) return;

        _logOpen = -1;
        lc.Close();
    }

    public void SetLogsStateShown(StateShown stateShown)
    {
        foreach (var child in LogPanel.Children)
        {
            if (child is not LogControl lc) continue;

            lc.SetStateShown(stateShown);
        }
    }

    public void SetCursorPosition(int index, string s)
    {
        if (index < 0 || index > LogPanel.Children.Count) return;
        if (LogPanel.Children[index] is not LogControl lc) return;

        lc.SetCursorPosition(s);
    }

    public void InitializeStrategies(IReadOnlyList<SudokuStrategy> strategies)
    {
        StrategyPanel.Children.Clear();
        for (int i = 0; i < strategies.Count; i++)
        {
            var iForEvent = i;
            var control = new StrategyControl(strategies[i]);
            control.StrategyEnabled += b => _presenter.EnableStrategy(iForEvent, b);
            StrategyPanel.Children.Add(control);
        }
    }

    public void HighlightStrategy(int index)
    {
        Dispatcher.Invoke(() =>
        {
            if(index < 0 || index >= StrategyPanel.Children.Count) return;
            ((StrategyControl)StrategyPanel.Children[index]).SetHighlight(true);
        });
    }

    public void UnHighlightStrategy(int index)
    {
        Dispatcher.Invoke(() =>
        {
            if(index < 0 || index >= StrategyPanel.Children.Count) return;
            ((StrategyControl)StrategyPanel.Children[index]).SetHighlight(false);
        });
    }

    public void CopyToClipBoard(string s)
    {
        Clipboard.SetText(s);
    }

    public void EnableStrategy(int index, bool enabled)
    {
        if (index < 0 || index >= StrategyPanel.Children.Count ||
            StrategyPanel.Children[index] is not StrategyControl sc) return;

        sc.EnableStrategy(enabled);
    }

    public void LockStrategy(int index)
    {
        if (index < 0 || index >= StrategyPanel.Children.Count ||
            StrategyPanel.Children[index] is not StrategyControl sc) return;

        sc.LockStrategy();
    }

    #endregion

    private void SetNewSudoku(string s)
    {
        _presenter.SetNewSudoku(s);
    }

    private void SudokuTextBoxShowed()
    {
        
        _presenter.OnSudokuAsStringBoxShowed();
    }

    private void Solve(object sender, RoutedEventArgs e)
    {
        _presenter.Solve(false);
    }

    private void Advance(object sender, RoutedEventArgs e)
    {
        _presenter.Solve(true);
    }

    private async void ChooseStep(object sender, RoutedEventArgs e)
    {
        var p = await _presenter.ChooseStep();
        var window = new ChooseStepWindow(p);
        window.Closed += (_, _) => _presenter.OnStoppedChoosingStep();
        window.Show();
    }

    private void Clear(object sender, RoutedEventArgs e)
    {
        _presenter.Clear();
    }

    private void SelectCell(int row, int col)
    {
        _presenter.SelectCell(row, col);
    }

    private void DoBoardInput(object sender, KeyEventArgs e)
    {
        if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
        {
            switch (e.Key)
            {
                case Key.C :
                    _presenter.Copy();
                    break;
                case Key.V :
                    _presenter.Paste(Clipboard.GetText());
                    break;
            }

            return;
        }
        
        switch (e.Key)
        {
            case Key.D1 :
            case Key.NumPad1 :
                _presenter.SetCurrentCell(1);
                break;
            case Key.D2 :
            case Key.NumPad2 :
                _presenter.SetCurrentCell(2);
                break;
            case Key.D3 :
            case Key.NumPad3 :
                _presenter.SetCurrentCell(3);
                break;
            case Key.D4 :
            case Key.NumPad4 :
                _presenter.SetCurrentCell(4);
                break;
            case Key.D5 :
            case Key.NumPad5 :
                _presenter.SetCurrentCell(5);
                break;
            case Key.D6 :
            case Key.NumPad6 :
                _presenter.SetCurrentCell(6);
                break;
            case Key.D7 :
            case Key.NumPad7 :
                _presenter.SetCurrentCell(7);
                break;
            case Key.D8 :
            case Key.NumPad8 :
                _presenter.SetCurrentCell(8);
                break;
            case Key.D9 :
            case Key.NumPad9 :
                _presenter.SetCurrentCell(9);
                break;
            case Key.D0 :
            case Key.NumPad0 :
            case Key.Back :
                _presenter.DeleteCurrentCell();
                break;
        }
    }

    public override void OnShow()
    {
        _presenter.OnShow();
    }

    public override void OnClose()
    {
        
    }

    public override object TitleBarContent()
    {
        var settings = new SettingsButton();
        settings.Clicked += () =>
        {
            var window = new SettingWindow(_presenter.SettingsPresenter);
            window.Show();
        };

        return settings;
    }
}

