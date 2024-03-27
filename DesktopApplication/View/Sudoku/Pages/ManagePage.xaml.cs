using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DesktopApplication.Presenter.Sudoku;
using DesktopApplication.Presenter.Sudoku.Manage;
using DesktopApplication.View.Settings;
using DesktopApplication.View.Sudoku.Controls;
using DesktopApplication.View.Utility;
using Model.Helpers.Logs;
using Model.Sudoku.Solver;

namespace DesktopApplication.View.Sudoku.Pages;

public partial class ManagePage : ISudokuManageView //TODO visuals for drag & drop
{
    private const int ToleranceForDragScroll = 80;
    private const int DragScrollOffset = 60;
    
    private readonly SudokuManagePresenter _presenter;
    
    public ManagePage(SudokuApplicationPresenter appPresenter)
    {
        InitializeComponent();
        _presenter = appPresenter.Initialize(this);
        _presenter.InitStrategies();
        
        RenderOptions.SetBitmapScalingMode(Bin, BitmapScalingMode.Fant);
    }

    public void ClearSearchResults()
    {
        Search.ClearResult();
    }

    public void AddSearchResult(string s)
    {
        Search.AddResult(s);
    }

    public void SetStrategyList(IReadOnlyList<SudokuStrategy> list)
    {
        StrategyPanel.Children.Clear();
        for (int i = 0; i < list.Count; i++)
        {
            var strategy = list[i];
            
            var tb = new TextBlock
            {
                Text = strategy.Name,
                Foreground = new SolidColorBrush(ColorUtility.ToColor((Intensity)strategy.Difficulty)),
                Padding = new Thickness(5, 5, 0, 5),
                FontSize = 15,
                AllowDrop = true
            };
            var iForEvent = i;
            tb.MouseLeftButtonDown += (_, _) =>
            {
                _presenter.OnActiveStrategySelection(iForEvent);
            };
            tb.MouseMove += (_, args) =>
            {
                if (args.LeftButton == MouseButtonState.Pressed)
                {
                    StrategyPanel.Children.RemoveAt(iForEvent);
                    DragDrop.DoDragDrop(tb, new StrategyDragDropData(strategy.Name, iForEvent), DragDropEffects.Move);
                }
            };
            tb.Drop += (_, args) =>
            {
                if (args.Data.GetData(typeof(StrategyDragDropData)) is not StrategyDragDropData data) return;
            
                var relativePosition = tb.IsUnderHalfHeight(args) ? iForEvent + 1 : iForEvent;
                if (data.Index == -1) _presenter.AddStrategy(data.Name, relativePosition);
                else _presenter.InterchangeStrategies(data.Index, relativePosition);
                
                args.Handled = true;
            };
            
            StrategyPanel.Children.Add(tb);
        }
    }

    public void SetSelectedStrategyName(string name)
    {
        StrategyName.Text = name;
    }

    public void SetManageableSettings(StrategySettingsPresenter presenter)
    {
        InfoPanel.Children.Clear();
        foreach (var s in presenter)
        {
            var control = SettingTranslator.Translate(presenter, s.Item1, s.Item2);
            if (control is not null)
            {
                control.AutoSet = true;
                control.Margin = new Thickness(5, 5, 0, 5);
                InfoPanel.Children.Add(control);
            }
        }
    }

    public void ClearSelectedStrategy()
    {
        StrategyName.Text = "No Strategy Selected";
        InfoPanel.Children.Clear();
    }

    public override void OnShow()
    {
        _presenter.OnShow();
    }

    public override void OnClose()
    {
       
    }

    public override object? TitleBarContent()
    {
        return null;
    }

    private void OnSearch(string s)
    {
        _presenter.OnSearch(s);
    }

    private void DropInBin(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(typeof(StrategyDragDropData)) is not StrategyDragDropData data) return;

        if (data.Index != -1) _presenter.RemoveStrategy(data.Index);
    }
    
    private void ScrollOnDrag(object sender, DragEventArgs e)
    {
        var pos = e.GetPosition(StrategyScrollViewer).Y;

        if (pos < ToleranceForDragScroll) StrategyScrollViewer.ScrollToVerticalOffset(
            StrategyScrollViewer.VerticalOffset - DragScrollOffset * (ToleranceForDragScroll - pos) / ToleranceForDragScroll);
        else if (pos > StrategyScrollViewer.ActualHeight - ToleranceForDragScroll) StrategyScrollViewer
            .ScrollToVerticalOffset(StrategyScrollViewer.VerticalOffset + DragScrollOffset * (pos - 
                StrategyScrollViewer.ActualHeight + ToleranceForDragScroll) / ToleranceForDragScroll);     
        
    }
}