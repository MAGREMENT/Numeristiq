using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DesktopApplication.Presenter.Sudoku;
using DesktopApplication.Presenter.Sudoku.Manage;
using DesktopApplication.View.Utility;
using Model.Helpers.Logs;
using Model.Sudoku.Solver;

namespace DesktopApplication.View.Sudoku.Pages;

public partial class ManagePage : ISudokuManageView
{
    private readonly SudokuManagePresenter _presenter;
    public ManagePage(SudokuApplicationPresenter appPresenter)
    {
        InitializeComponent();
        _presenter = appPresenter.Initialize(this);
        
        RenderOptions.SetBitmapScalingMode(Bin, BitmapScalingMode.Fant);
        
        _presenter.InitStrategies();
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
        foreach (var strategy in list)
        {
            StrategyPanel.Children.Add(new TextBlock
            {
                Text = strategy.Name,
                Foreground = new SolidColorBrush(ColorUtility.ToColor((Intensity)strategy.Difficulty)),
                Padding = new Thickness(5, 5, 0, 5),
                FontSize = 15
            });
        }
    }

    private void OnSearch(string s)
    {
        _presenter.OnSearch(s);
    }
}