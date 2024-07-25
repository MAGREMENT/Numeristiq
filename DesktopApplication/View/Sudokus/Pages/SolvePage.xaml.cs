using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DesktopApplication.Presenter.Sudokus;
using DesktopApplication.Presenter.Sudokus.Solve;
using DesktopApplication.View.Controls;
using DesktopApplication.View.HelperWindows;
using DesktopApplication.View.HelperWindows.Dialog;
using DesktopApplication.View.Sudokus.Controls;
using Microsoft.Win32;
using Model.Core.Steps;
using Model.Sudokus.Solver;

namespace DesktopApplication.View.Sudokus.Pages;

public partial class SolvePage : ISudokuSolveView
{
    private readonly SudokuSolvePresenter _presenter;

    private bool _disabled;
    
    public SolvePage(SudokuApplicationPresenter appPresenter)
    {
        InitializeComponent();
        _presenter = appPresenter.Initialize(this);
    }

    #region ISudokuSolveView

    public ISudokuSolverDrawer Drawer => (ISudokuSolverDrawer)EmbeddedDrawer.OptimizableContent!;

    public void SetSudokuAsString(string s)
    {
        SudokuAsString.SetText(s);
    }

    public void Disable()
    {
        SolveButton.IsEnabled = false;
        AdvanceButton.IsEnabled = false;
        ChooseStepButton.IsEnabled = false;
        ClearButton.IsEnabled = false;
        _disabled = true;
    }

    public void Enable()
    {
        SolveButton.IsEnabled = true;
        AdvanceButton.IsEnabled = true;
        ChooseStepButton.IsEnabled = true;
        ClearButton.IsEnabled = true;
        _disabled = false;
    }

    public void AddStep(IStep step, StateShown stateShown)
    {
        LogPanel.Dispatcher.Invoke(() =>
        {
            var lc = new StepControl(step, stateShown);
            LogPanel.Children.Add(lc);
            lc.OpenRequested += _presenter.RequestStepOpening;
            lc.StateShownChanged += _presenter.RequestStateShownChange;
            lc.PageSelector.PageChanged += _presenter.RequestHighlightChange;
            lc.ExplanationAsked += () =>
            {
                var builder = _presenter.RequestExplanation();
                if (builder is null) return;

                var window = new StepExplanationWindow(builder, GetSudokuDrawer());
                window.Show();
            };
        });
        LogViewer.Dispatcher.Invoke(() => LogViewer.ScrollToEnd());
    }

    public void ClearSteps()
    {
        LogPanel.Children.Clear();
    }

    public void OpenStep(int index)
    {
        if (index < 0 || index > LogPanel.Children.Count) return;
        if (LogPanel.Children[index] is not StepControl lc) return;
        
        lc.Open();
    }

    public void CloseStep(int index)
    {
        if (index < 0 || index > LogPanel.Children.Count) return;
        if (LogPanel.Children[index] is not StepControl lc) return;
        
        lc.Close();
    }

    public void SetStepsStateShown(StateShown stateShown)
    {
        foreach (var child in LogPanel.Children)
        {
            if (child is not StepControl lc) continue;

            lc.SetStateShown(stateShown);
        }
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

    public void OpenOptionDialog(string name, OptionChosen callback, params string[] options)
    {
        var dialog = new OptionChooserDialog(name, callback, options);
        dialog.Show();
    }

    #endregion

    private static SudokuBoard GetSudokuDrawer()
    {
        var board = new SudokuBoard
        {
            PossibilitySize = 20, 
            BigLineWidth = 3,
            SmallLineWidth = 1,
            BackgroundBrush = Brushes.Transparent
        };

        board.SetResourceReference(SudokuBoard.DefaultNumberBrushProperty, "Text");
        board.SetResourceReference(SudokuBoard.LineBrushProperty, "Text");
        board.SetResourceReference(SudokuBoard.CursorBrushProperty, "Secondary1");
        board.SetResourceReference(SudokuBoard.SpecialNumberBrushProperty, "Primary1");
        board.SetResourceReference(SudokuBoard.LinkBrushProperty, "Accent");

        return board;
    }

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
        if (_disabled) return;
        
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
                case Key.S :
                    TakeScreenShot();
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

    private void TakeScreenShot()
    {
        var dialog = new SaveFileDialog
        {
            AddExtension = true,
            DefaultExt = "png",
            RestoreDirectory = true,
            Filter = "PNG Image (*.png)|*.png"
        };
        var result = dialog.ShowDialog();

        if (result != true) return;
        
        using var stream = dialog.OpenFile();
        try
        {
            var png = new PngBitmapEncoder();
            png.Frames.Add(((SudokuBoard)EmbeddedDrawer.OptimizableContent!).AsImage());
            png.Save(stream);
        }
        catch (Exception)
        {
            // ignored
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

