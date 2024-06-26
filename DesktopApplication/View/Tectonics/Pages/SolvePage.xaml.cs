﻿using System.Windows;
using System.Windows.Input;
using DesktopApplication.Presenter.Tectonics;
using DesktopApplication.Presenter.Tectonics.Solve;
using DesktopApplication.View.Controls;
using DesktopApplication.View.Tectonics.Controls;
using Model.Core.Highlighting;
using Model.Core.Steps;
using Model.Utility;

namespace DesktopApplication.View.Tectonics.Pages;

public partial class SolvePage : ITectonicSolveView
{
    private readonly TectonicSolvePresenter _presenter;

    private int _logOpen;
    
    public SolvePage(TectonicApplicationPresenter appPresenter)
    {
        InitializeComponent();

        _presenter = appPresenter.Initialize(this);
        
        DefaultMode.IsChecked = true; //Do NOT move this to XAML
    }

    public ITectonicDrawer Drawer => (TectonicBoard)EmbeddedDrawer.OptimizableContent!;

    public void SetTectonicString(string s)
    {
        TextBox.SetText(s);
    }

    public void AddLog(IStep step, StateShown stateShown)
    {
        LogPanel.Dispatcher.Invoke(() =>
        {
            var lc = new StepControl(step, stateShown);
            LogPanel.Children.Add(lc);
            lc.OpenRequested += _presenter.RequestLogOpening;
            lc.StateShownChanged += _presenter.RequestStateShownChange;
            lc.HighlightShifted += _presenter.RequestHighlightShift;
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
        if (LogPanel.Children[index] is not StepControl lc) return;

        _logOpen = index;
        lc.Open();
    }

    public void CloseLogs()
    {
        if (_logOpen < 0 || _logOpen >= LogPanel.Children.Count) return;
        if (LogPanel.Children[_logOpen] is not StepControl lc) return;

        _logOpen = -1;
        lc.Close();
    }

    public void SetLogsStateShown(StateShown stateShown)
    {
        foreach (var child in LogPanel.Children)
        {
            if (child is not StepControl lc) continue;

            lc.SetStateShown(stateShown);
        }
    }

    public void SetCursorPosition(int index, string s)
    {
        if (index < 0 || index > LogPanel.Children.Count) return;
        if (LogPanel.Children[index] is not StepControl lc) return;

        lc.SetCursorPosition(s);
    }

    private void CreateNewTectonic(string s)
    {
        _presenter.SetNewTectonic(s);
    }

    private void Solve(object sender, RoutedEventArgs e)
    {
        _presenter.Solve(false);
    }

    private void Advance(object sender, RoutedEventArgs e)
    {
        _presenter.Solve(true);
    }

    private void Reset(object sender, RoutedEventArgs e)
    {
        _presenter.Reset();
    }

    private void Clear(object sender, RoutedEventArgs e)
    {
        _presenter.Clear();
    }

    private void OnRowCountChange(int number)
    {
        RowCount.SetDimension(number);
    }

    private void OnColumnCountChange(int number)
    {
        ColumnCount.SetDimension(number);
    }

    private void OnRowDimensionChangeAsked(int diff)
    {
        _presenter.SetNewRowCount(diff);
    }

    private void OnColumnDimensionChangeAsked(int diff)
    {
        _presenter.SetNewColumnCount(diff);
    }

    private void OnHideableTextboxShowed()
    {
        _presenter.SetTectonicString();
    }

    private void OnCellSelection(int row, int col)
    {
        _presenter.SelectCell(new Cell(row, col));
    }

    private void OnCellAddedToSelection(int row, int col)
    {
        _presenter.SelectCell(new Cell(row, col));
    }

    private void OnSelectionEnd()
    {
        _presenter.EndCellSelection();
    }

    private void ModeToDefault(object sender, RoutedEventArgs e)
    {
        _presenter.SetSelectionMode(SelectionMode.Default);
    }
    
    private void ModeToMerge(object sender, RoutedEventArgs e)
    {
        _presenter.SetSelectionMode(SelectionMode.Merge);
    }
    
    private void ModeToSplit(object sender, RoutedEventArgs e)
    {
        _presenter.SetSelectionMode(SelectionMode.Split);
    }
    
    private void DoBoardInput(object sender, KeyEventArgs e)
    {
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
}