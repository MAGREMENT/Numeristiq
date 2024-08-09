using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using DesktopApplication.Presenter;
using DesktopApplication.Presenter.Sudokus;
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
            if(Evaluated.Children.Count > 4)
                Evaluated.Children.RemoveRange(4, Evaluated.Children.Count - 4);
            if(Evaluated.RowDefinitions.Count > 1) 
                Evaluated.RowDefinitions.RemoveRange(1, Evaluated.RowDefinitions.Count - 1);

            var r = 1;
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

                var grid = new Grid
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };
                grid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(1, GridUnitType.Star)
                });
                grid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = GridLength.Auto
                });
                grid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = GridLength.Auto
                });

                var asString = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    FontSize = 16,
                    Text = sudoku.AsString(),
                    Margin = new Thickness(5, 0, 5, 0)
                };
                asString.SetResourceReference(ForegroundProperty, "Text");
                Grid.SetColumn(asString, 0);
                grid.Children.Add(asString);

                var p1 = new Path
                {
                    Data = Geometry.Parse("M 7,35 V 10 H 27 V 35 H 7 M 15,10 V 5 H 35 V 30 H 27"),
                    Width = 40,
                    Height = 40
                };
                p1.SetResourceReference(Shape.StrokeProperty, "Text");
                var b1 = new Button
                {
                    Style = (Style)FindResource("SimpleHoverButton"),
                    Content = p1,
                    Visibility = Visibility.Collapsed
                };
                b1.Click += (_, _) => Clipboard.SetText(sudoku.AsString());
                Grid.SetColumn(b1, 1);
                grid.Children.Add(b1);
                
                var p2 = new Path
                {
                    Data = Geometry.Parse("M 3,20 C 15,8 25,8 37,20 C 25,32 15,32 3,20 M 20,20 M 16,20 A 4,4 0 1 1 24,20 A 4,4 0 1 1 16,20"),
                    Width = 40,
                    Height = 40
                };
                p2.SetResourceReference(Shape.StrokeProperty, "Text");
                var b2 = new Button
                {
                    Style = (Style)FindResource("SimpleHoverButton"),
                    Content = p2,
                    Visibility = Visibility.Collapsed
                };
                b2.Click += (_, _) =>
                {
                    var dialog = new ShowSudokuDialog(sudoku.Puzzle);
                    dialog.Show();
                };
                Grid.SetColumn(b2, 2);
                grid.Children.Add(b2);

                grid.MouseEnter += (_, _) =>
                {
                    b1.Visibility = Visibility.Visible;
                    b2.Visibility = Visibility.Visible;
                };
                
                grid.MouseLeave += (_, _) =>
                {
                    b1.Visibility = Visibility.Collapsed;
                    b2.Visibility = Visibility.Collapsed;
                };

                Grid.SetRow(grid, r);
                Grid.SetColumn(grid, 1);
                Evaluated.Children.Add(grid);

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

                r++;
            }
        });
    }

    public void AllowGeneration(bool allowed)
    {
        GenerateButton.IsEnabled = allowed;
    }

    public void AllowCancel(bool allowed)
    {
        StopButton.Dispatcher.Invoke(() => StopButton.IsEnabled = allowed);
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

    private void Generate(object sender, RoutedEventArgs e)
    {
        _presenter.Generate();
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

    private void Stop(object sender, RoutedEventArgs e)
    {
        _presenter.Stop();
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