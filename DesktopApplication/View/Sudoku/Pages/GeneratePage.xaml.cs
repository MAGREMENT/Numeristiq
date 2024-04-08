using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using DesktopApplication.Presenter.Sudoku;
using DesktopApplication.Presenter.Sudoku.Generate;
using DesktopApplication.View.Controls;
using DesktopApplication.View.HelperWindows;
using DesktopApplication.View.Sudoku.Controls;
using Model.Sudoku;
using Model.Sudoku.Generator;

namespace DesktopApplication.View.Sudoku.Pages;

public partial class GeneratePage : ISudokuGenerateView
{
    private readonly SudokuGeneratePresenter _presenter;
    public GeneratePage(SudokuApplicationPresenter appPresenter)
    {
        InitializeComponent();
        _presenter = appPresenter.Initialize(this);
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

    public void UpdateNotEvaluatedList(IEnumerable<Model.Sudoku.Sudoku> sudokus)
    {
        NotEvaluated.Dispatcher.Invoke(() =>
        {
            NotEvaluated.Children.Clear();

            foreach (var sudoku in sudokus)
            {
                var tb = new TextBlock
                {
                    FontSize = 14,
                    Padding = new Thickness(5),
                    Text = SudokuTranslator.TranslateLineFormat(sudoku,
                        SudokuLineFormatEmptyCellRepresentation.Zeros)
                };
                
                tb.SetResourceReference(ForegroundProperty, "Text");
                NotEvaluated.Children.Add(tb);
            }
        });
    }

    public void UpdateEvaluatedList(IEnumerable<EvaluatedGeneratedPuzzle> sudokus)
    {
        Evaluated.Dispatcher.Invoke(() =>
        {
            Evaluated.Children.Clear();

            foreach (var sudoku in sudokus)
            {
                Evaluated.Children.Add(new EvaluatedGeneratedPuzzleControl(sudoku));
            }
        });
    }

    public void UpdateCurrentlyEvaluated(Model.Sudoku.Sudoku? sudoku)
    {
        CurrentlyEvaluated.Dispatcher.Invoke(() =>
        {
            var s = sudoku is null
                ? string.Empty
                : SudokuTranslator.TranslateLineFormat(sudoku, SudokuLineFormatEmptyCellRepresentation.Zeros);
            CurrentlyEvaluated.Text = s;
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
        _presenter?.SetGenerationCount(value);
    }
}