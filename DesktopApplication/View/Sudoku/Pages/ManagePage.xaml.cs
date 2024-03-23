using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DesktopApplication.Presenter.Sudoku;
using DesktopApplication.Presenter.Sudoku.Manage;
using DesktopApplication.View.Settings;
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
        for (int i = 0; i < list.Count; i++)
        {
            var strategy = list[i];
            var tb = new TextBlock
            {
                Text = strategy.Name,
                Foreground = new SolidColorBrush(ColorUtility.ToColor((Intensity)strategy.Difficulty)),
                Padding = new Thickness(5, 5, 0, 5),
                FontSize = 15
            };
            var iForEvent = i;
            tb.MouseLeftButtonDown += (_, _) => _presenter.OnActiveStrategySelection(iForEvent);
            
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

    private void OnSearch(string s)
    {
        _presenter.OnSearch(s);
    }
}