using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using DesktopApplication.Presenter;
using DesktopApplication.Presenter.Sudokus.Generate;
using DesktopApplication.View.Controls;
using DesktopApplication.View.HelperWindows;
using DesktopApplication.View.HelperWindows.Dialog;
using Model.Sudokus;
using Model.Sudokus.Generator;

namespace DesktopApplication.View.Sudokus.Pages;

public partial class GeneratePage : ISudokuGenerateView
{
    private readonly SudokuGeneratePresenter _presenter;
    private readonly bool _initialized;
    
    public GeneratePage()
    {
        InitializeComponent();
        _presenter = PresenterFactory.Instance.Initialize(this);

        RenderOptions.SetBitmapScalingMode(Bin, BitmapScalingMode.Fant);
        _initialized = true;
    }

    public override string Header => "Generate";

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

    public void ActivateFilledSudokuGenerator(bool activated)
    {
        FSG.Dispatcher.Invoke(() => FSG.Activate(activated));
    }

    public void ActivateRandomDigitRemover(bool activated)
    {
        RDR.Dispatcher.Invoke(() => RDR.Activate(activated));
    }

    public void ActivatePuzzleEvaluator(bool activated)
    {
        Evaluator.Dispatcher.Invoke(() => Evaluator.Activate(activated));
    }

    public async void ShowTransition(TransitionPlace place)
    {
        var path = place switch
        {
            TransitionPlace.ToEvaluator => ToEvaluator,
            TransitionPlace.ToFinalList => ToFinalList,
            TransitionPlace.ToRDR => ToRDR,
            TransitionPlace.ToBin => ToBin,
            _ => throw new ArgumentOutOfRangeException()
        };
        
        path.Dispatcher.Invoke(() => path.SetResourceReference(Shape.StrokeProperty, "Primary1"));
        await Task.Delay(TimeSpan.FromMilliseconds(250));
        path.Dispatcher.Invoke(() => path.SetResourceReference(Shape.StrokeProperty, "Text"));
    }

    public void UpdateEvaluatedList(IEnumerable<GeneratedSudokuPuzzle> sudokus)
    {
        Evaluated.Dispatcher.Invoke(() =>
        {
            Evaluated.Children.Clear();
            Evaluated.RowDefinitions.Clear();

            var r = 0;
            foreach (var sudoku in sudokus)
            {
                Evaluated.RowDefinitions.Add(new RowDefinition
                {
                    Height = GridLength.Auto
                });
                
                var tb = new TextBlock
                {
                    Text = sudoku.Id.ToString(),
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                tb.SetResourceReference(ForegroundProperty, "Text");

                var border = new Border
                {
                    CornerRadius = new CornerRadius(5),
                    Width = 40,
                    Height = 40,
                    VerticalAlignment = VerticalAlignment.Center,
                    Child = tb,
                    Margin = new Thickness(5)
                };
                border.SetResourceReference(BackgroundProperty, "Background3");
                Grid.SetRow(border, r);
                Grid.SetColumn(border, 0);
                Evaluated.Children.Add(border);

                var asString = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontSize = 16,
                    Text = sudoku.AsString(),
                    Margin = new Thickness(5, 0, 5, 0)
                };
                asString.SetResourceReference(ForegroundProperty, "Text");
                Grid.SetColumn(asString, 1);
                Grid.SetRow(asString, r);
                Evaluated.Children.Add(asString);

                var rating = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontSize = 18,
                    FontWeight = FontWeights.SemiBold,
                    Text = Math.Round(sudoku.Rating, 2).ToString(CultureInfo.CurrentUICulture)
                };
                rating.SetResourceReference(ForegroundProperty, "Text");
                Grid.SetRow(rating, r);
                Grid.SetColumn(rating, 2);
                Evaluated.Children.Add(rating);

                var h = sudoku.Hardest;
                if (h is not null)
                {
                    var hardest = new TextBlock
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        FontSize = 16,
                        Text = h.Name
                    };
                    hardest.SetResourceReference(ForegroundProperty,
                        ThemeInformation.ResourceNameFor(h.Difficulty));
                    Grid.SetRow(hardest, r);
                    Grid.SetColumn(hardest, 3);
                    Evaluated.Children.Add(hardest);
                }
                
                var grid = new Grid
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                grid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = GridLength.Auto
                });
                grid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = GridLength.Auto
                });
                
                var p1 = new Path
                {
                    Data = Geometry.Parse("M 5,25 V 8 H 20 V 25 H 5 M 10,8 V 4 H 25 V 21 H 20"),
                    Width = 30,
                    Height = 30,
                    StrokeThickness = 2
                };
                p1.SetResourceReference(Shape.StrokeProperty, "Text");
                var b1 = new Button
                {
                    Template = (ControlTemplate)FindResource("RoundedButton"),
                    Content = p1,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 10, 0)
                };
                b1.SetResourceReference(BackgroundProperty, "Primary1");
                b1.Click += (_, _) => Clipboard.SetText(sudoku.AsString());
                Grid.SetColumn(b1, 0);
                grid.Children.Add(b1);
                
                var p2 = new Path
                {
                    Data = Geometry.Parse("M 5,15 C 12,7 18,7 25,15 C 18,23 12,23 5,15 M 13,15 A 2,2 0 1 1 17,15 A 2,2 0 1 1 13,15"),
                    Width = 30,
                    Height = 30,
                    StrokeThickness = 2
                };
                p2.SetResourceReference(Shape.StrokeProperty, "Text");
                var b2 = new Button
                {
                    Template = (ControlTemplate)FindResource("RoundedButton"),
                    Content = p2,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
                b2.SetResourceReference(BackgroundProperty, "Secondary1");
                b2.Click += (_, _) =>
                {
                    var dialog = new ShowSudokuDialog(sudoku.Puzzle);
                    dialog.Show();
                };
                Grid.SetColumn(b2, 1);
                grid.Children.Add(b2);

                Grid.SetRow(grid, r);
                Grid.SetColumn(grid, 4);
                Evaluated.Children.Add(grid);

                r++;
            }
        });
    }

    public void SetCriteriaList(IReadOnlyList<EvaluationCriteria> criteriaList)
    {
        Evaluator.SetCriteriaList(criteriaList);
    }

    public void ShowSudoku(Sudoku sudoku)
    {
        var dialog = new ShowSudokuDialog(sudoku);
        dialog.Show();
    }

    public void CopyToClipboard(string s)
    {
        Clipboard.SetText(s);
    }

    public void OnGenerationStart()
    {
        GenerateButton.Content = "Stop";
        GenerateButton.Click -= Generate;
        GenerateButton.Click += Stop;
    }

    public void OnGenerationStop()
    {
        GenerateButton.Content = "Generate";
        GenerateButton.Click += Generate;
        GenerateButton.Click -= Stop;
        GenerateButton.IsEnabled = true;
    }

    private void Generate(object sender, RoutedEventArgs e)
    {
        _presenter.Generate();
    }
    
    private void Stop(object sender, RoutedEventArgs e)
    {
        _presenter.Stop();
        GenerateButton.IsEnabled = false;
    }

    private void OnValueChange(int value)
    {
        if(_initialized) _presenter.SetGenerationCount(value);
    }

    private void ChangeSymmetry(bool value)
    {
        if(_initialized) _presenter.SetKeepSymmetry(value);
    }
    
    private void ChangeUnique(bool value)
    {
        if(_initialized) _presenter.SetKeepUniqueness(value);
    }

    private void OpenManageCriteriaWindow()
    {
        var window = new ManageCriteriaWindow(_presenter.ManageCriteria());
        window.Closed += (_, _) => _presenter.UpdateCriterias();
        window.Show();
    }

    private void OnRandomSelection()
    {
        _presenter.SetRandomFilled();
    }

    private void OnSeedSelection(string s, SudokuStringFormat format)
    {
        _presenter.SetSeedFilled(s, format);
    }

    private void ShowSeed()
    {
        _presenter.ShowSeed();
    }

    private void CopyAll(object sender, RoutedEventArgs e)
    {
        _presenter.CopyAll();
    }
}