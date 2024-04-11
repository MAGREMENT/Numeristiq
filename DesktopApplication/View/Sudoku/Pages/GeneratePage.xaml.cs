using System.Collections.Generic;
using System.Windows;
using DesktopApplication.Presenter.Sudoku;
using DesktopApplication.Presenter.Sudoku.Generate;
using DesktopApplication.View.Controls;
using DesktopApplication.View.HelperWindows;
using DesktopApplication.View.Sudoku.Controls;
using Model.Sudoku.Generator;

namespace DesktopApplication.View.Sudoku.Pages;

public partial class GeneratePage : ISudokuGenerateView
{
    private readonly SudokuGeneratePresenter _presenter;
    private readonly bool _initialized;
    
    public GeneratePage(SudokuApplicationPresenter appPresenter)
    {
        InitializeComponent();
        _presenter = appPresenter.Initialize(this);

        _initialized = true;
    }

    public override void OnShow()
    {
        
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

    public void UpdateEvaluatedList(IEnumerable<GeneratedSudokuPuzzle> sudokus)
    {
        Evaluated.Dispatcher.Invoke(() =>
        {
            Evaluated.Children.Clear();

            foreach (var sudoku in sudokus)
            {
                Evaluated.Children.Add(new GeneratedPuzzleControl(sudoku));
            }
        });
    }

    public void AllowGeneration(bool allowed)
    {
        GenerateButton.IsEnabled = allowed;
    }

    private void Generate(object sender, RoutedEventArgs e)
    {
        _presenter.Generate();
    }

    private void OnValueChange(int value)
    {
        if(_initialized) _presenter.SetGenerationCount(value);
    }
}