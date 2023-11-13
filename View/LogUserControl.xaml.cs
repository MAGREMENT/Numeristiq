﻿using System.Windows;
using System.Windows.Media;
using Model.Solver.Helpers.Logs;
using View.Utils;

namespace View;

public partial class LogUserControl
{
    public ISolverLog? Log { get; private set; }

    public delegate void OnLogClicked(ISolverLog log);
    public event OnLogClicked? LogClicked;

    public delegate void OnShownTypeChanged(StateShownType type);
    public event OnShownTypeChanged? ShownTypeChanged;

    private bool _invokeShowTypeEvent = true;
    
    public LogUserControl()
    {
        InitializeComponent();

        Main.MouseEnter += (_, _) =>
        {
            Main.Background = new SolidColorBrush(Colors.White);
        };
        Main.MouseLeave += (_, _) =>
        {
            Main.Background = new SolidColorBrush(Colors.WhiteSmoke);
        };
        Main.MouseLeftButtonDown += (_, _) =>
        {
            if(Log is not null) LogClicked?.Invoke(Log);
        };
    }

    public void InitLog(ISolverLog log)
    {
        Log = log;

        Number.Text = "#" + log.Id;
        Title.Foreground = new SolidColorBrush(ColorManager.ToColor(log.Intensity));
        Title.Text = log.Title;
        Text.Text = log.Changes;

        HighlightsNumber.Text = log.HighlightManager.CursorPosition();
        if (log.HighlightManager.Count == 1)
        {
            LeftArrow.IsEnabled = false;
            RightArrow.IsEnabled = false;
        }
    }

    public void CurrentlyShowed()
    {
        Highlights.Visibility = Visibility.Visible;
    }

    public void NotShowedAnymore()
    {
        Highlights.Visibility = Visibility.Hidden;
    }

    private void ShiftLeft(object sender, RoutedEventArgs e)
    {
        if (Log is null) return;
        
        Log.HighlightManager.ShiftLeft();
        HighlightsNumber.Text = Log.HighlightManager.CursorPosition();
        LogClicked?.Invoke(Log);
    }

    private void ShiftRight(object sender, RoutedEventArgs e)
    {
        if (Log is null) return;
        
        Log.HighlightManager.ShiftRight();
        HighlightsNumber.Text = Log.HighlightManager.CursorPosition();
        LogClicked?.Invoke(Log);
    }

    private void TypeBefore_OnChecked(object sender, RoutedEventArgs e)
    {
        if(_invokeShowTypeEvent) ShownTypeChanged?.Invoke(StateShownType.Before);
    }

    private void TypeAfter_OnChecked(object sender, RoutedEventArgs e)
    {
        if(_invokeShowTypeEvent) ShownTypeChanged?.Invoke(StateShownType.After);
    }

    public void SetShownType(StateShownType type)
    {
        _invokeShowTypeEvent = false;
        if (type == StateShownType.After) TypeAfter.IsChecked = true;
        else TypeBefore.IsChecked = true;
        _invokeShowTypeEvent = true;
    }
}