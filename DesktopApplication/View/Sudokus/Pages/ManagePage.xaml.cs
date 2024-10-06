using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DesktopApplication.Presenter;
using DesktopApplication.Presenter.Sudokus.Manage;
using DesktopApplication.Presenter.Sudokus.Solve;
using DesktopApplication.View.Settings;
using DesktopApplication.View.Sudokus.Controls;
using DesktopApplication.View.Utility;
using Microsoft.Win32;
using Model.Core;
using Model.Core.Descriptions;
using Model.Core.Highlighting;
using Model.Sudokus.Solver;
using Model.Sudokus.Solver.Descriptions;

namespace DesktopApplication.View.Sudokus.Pages;

public partial class ManagePage : ISudokuManageView, ISudokuDescriptionDisplayer
{
    private const int ToleranceForDragScroll = 80;
    private const int DragScrollOffset = 60;
    
    private readonly SudokuManagePresenter _presenter;
    private readonly bool _initialized;
    
    public ManagePage()
    {
        InitializeComponent();
        
        _presenter = PresenterFactory.Instance.Initialize(this);
        _presenter.Initialize();

        _initialized = true;
    }

    public void ClearSearchResults()
    {
        Search.ClearResult();
    }

    public void AddSearchResult(string s)
    {
        var tb = new TextBlock
        {
            Text = s,
            Style = (Style)FindResource("SearchResult")
        };
        tb.MouseLeftButtonDown += (_, _) => _presenter.OnSearchResultSelection(s);
        tb.MouseMove += (_, args) =>
        {
            if(args.LeftButton == MouseButtonState.Pressed)
                DragDrop.DoDragDrop(tb, new StrategyDragDropData(s, -1), DragDropEffects.Move);
        };
        
        Search.AddResult(tb);
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
                AllowDrop = true,
                Style = (Style)FindResource("SimpleHoverBlock")
            };
            tb.SetResourceReference(ForegroundProperty, ThemeInformation.ResourceNameFor(strategy.Difficulty));
            
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
            tb.Drop += (_, args) => DropOn(tb, iForEvent, args);
            tb.DragOver += (_, args) =>
            {
                if (args.Data.GetData(typeof(StrategyDragDropData)) is not StrategyDragDropData) return;

                tb.Padding = tb.IsUnderHalfHeight(args) 
                    ? new Thickness(5, 5, 0, 20) 
                    : new Thickness(5, 20, 0, 5);
            };
            tb.DragLeave += (_, _) =>
            {
                tb.Padding = new Thickness(5, 5, 0, 5);
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
        var count = 0;
        
        foreach (var s in presenter)
        {
            var control = SettingTranslator.Translate(presenter, s.Item1, s.Item2);
            if (control is not null)
            {
                control.AutoSet = true;
                control.Margin = new Thickness(10, 10, 0, count == 1 ? 10 : 0);
                InfoPanel.Children.Add(control);
            }

            count++;
        }
    }

    public void SetNotFoundSettings()
    {
        InfoPanel.Children.Clear();

        var tb = new TextBlock
        {
            FontSize = 15,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            FontWeight = FontWeights.SemiBold,
            Text = "Add the strategy to your list to see its settings."
        };
        tb.SetResourceReference(ForegroundProperty, "Text");

        InfoPanel.Children.Add(tb);
    }

    public void SetStrategyDescription(IDescription<ISudokuDescriptionDisplayer> description)
    {
        InfoPanel.Children.Clear();

        description.Display(this);
    }

    public void ClearSelectedStrategy()
    {
        StrategyName.Text = "No Strategy Selected";
        InfoPanel.Children.Clear();
    }

    public Stream? GetUploadPresetStream()
    {
        var dialog = new SaveFileDialog
        {
            AddExtension = true,
            DefaultExt = "json",
            RestoreDirectory = true,
            Filter = "JSON File (*.json)|*.json"
        };
        var result = dialog.ShowDialog();

        if (result != true) return null;
        
        return dialog.OpenFile();
    }

    public Stream? GetDownloadPresetStream()
    {
        var dialog = new OpenFileDialog
        {
            AddExtension = true,
            Filter = "JSON File (*.json)|*.json",
            RestoreDirectory = true,
            DefaultExt = "json"
        };
        var result = dialog.ShowDialog();

        if (result != true) return null;
        
        return dialog.OpenFile();
    }

    public override string Header => "Manage";

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
        if(sender is FrameworkElement element) element.SetResourceReference(BackgroundProperty, "Background1");
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

    private void DropOn(TextBlock tb, int index, DragEventArgs args)
    {
        if (args.Data.GetData(typeof(StrategyDragDropData)) is not StrategyDragDropData data) return;
        
        if (tb.IsUnderHalfHeight(args)) index++;
        if (data.Index == -1) _presenter.AddStrategy(data.Name, index);
        else _presenter.InterchangeStrategies(data.Index, index);

        args.Handled = true;
    }

    private void Upload(object sender, RoutedEventArgs e)
    {
        _presenter.UploadPreset();
    }
    
    private void Download(object sender, RoutedEventArgs e)
    {
        _presenter.DownloadPreset();
    }

    private void OnDragEnter(object sender, DragEventArgs e)
    {
        if (sender is not FrameworkElement element 
            || e.Data.GetData(typeof(StrategyDragDropData)) is not StrategyDragDropData data
            || data.Index == -1) return;
        element.SetResourceReference(BackgroundProperty, "BackgroundHighlighted");
    }

    private void OnDragLeave(object sender, DragEventArgs e)
    {
        if (sender is not FrameworkElement element) return;
        element.SetResourceReference(BackgroundProperty, "Background1");
    }

    private void OnCheckedSettings(object sender, RoutedEventArgs e)
    {
        if (_initialized) _presenter.ChangeDisplay(ShownInfo.Settings);
    }
    
    private void OnCheckedDocumentation(object sender, RoutedEventArgs e)
    {
        if (_initialized) _presenter.ChangeDisplay(ShownInfo.Documentation);
    }

    public void AddParagraph(string s)
    {
        var tb = new TextBlock
        {
            FontSize = 15,
            Text = s,
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Left,
            Margin = new Thickness(10, 0, 10, 20)
        };
                
        tb.SetResourceReference(ForegroundProperty, "Text");
        InfoPanel.Children.Add(tb);
    }

    public void AddParagraph(string text, INumericSolvingState state, SudokuCropping cropping,
        IHighlightable<ISudokuHighlighter> highlight, TextDisposition disposition)
    {
        var grid = new Grid
        {
            Margin = new Thickness(0, 0, 0, 20)
        };
        grid.ColumnDefinitions.Add(new ColumnDefinition
        {
            Width = new GridLength(1, GridUnitType.Star)
        });
        grid.ColumnDefinitions.Add(new ColumnDefinition
        {
            Width = new GridLength(1, GridUnitType.Star)
        });
                
        var tb = new TextBlock
        {
            FontSize = 15,
            Text = text,
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 10, 0)
        };
                
        tb.SetResourceReference(ForegroundProperty, "Text");
        Grid.SetColumn(tb, disposition == TextDisposition.Left ? 0 : 1);
        grid.Children.Add(tb);

        var board = CreateBoard(state, highlight, cropping);
        Grid.SetColumn(board, disposition == TextDisposition.Left ? 1 : 0);
        grid.Children.Add(board);

        InfoPanel.Children.Add(grid);
    }

    private SudokuBoard CreateBoard(INumericSolvingState state,
        IHighlightable<ISudokuHighlighter> highlight, SudokuCropping cropping) //TODO adapt CroppedSudokuBoard to size
    {
        var board = new CroppedSudokuBoard(cropping)
        {
            PossibilitySize = 12,
            BigLineWidth = 3,
            SmallLineWidth = 1,
        };

        board.SetResourceReference(DrawingBoard.BackgroundBrushProperty, "Background1");
        board.SetResourceReference(DrawingBoard.ClueNumberBrushProperty, "Primary");
        board.SetResourceReference(DrawingBoard.DefaultNumberBrushProperty, "Text");
        board.SetResourceReference(DrawingBoard.LineBrushProperty, "Text");
        board.SetResourceReference(DrawingBoard.LinkBrushProperty, "Accent");

        SudokuSolvePresenter.SetShownState(board, state, cropping, false, true);
        _presenter.Highlight(board, highlight);
        
        return board;
    }
}

public record StrategyDragDropData(string Name, int Index);