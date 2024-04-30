using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using DesktopApplication.Presenter.Sudokus;
using DesktopApplication.Presenter.Sudokus.Play;
using DesktopApplication.Presenter.Sudokus.Solve;
using DesktopApplication.View.Controls;
using DesktopApplication.View.HelperWindows;
using DesktopApplication.View.HelperWindows.Dialog;
using Model.Sudokus.Player;
using Model.Utility;

namespace DesktopApplication.View.Sudokus.Pages;

public partial class PlayPage : ISudokuPlayView
{
    private static readonly Geometry PauseGeometry = Geometry.Parse("M 17.5,5 H 22.5 V 25 H 17.5 Z M 27.5,5 H 32.5 V 25 H 27.5 Z");
    private static readonly Geometry PlayGeometry = Geometry.Parse("M 17,5 33,15 17,25 Z");
    
    private readonly SudokuPlayPresenter _presenter;
    public PlayPage(SudokuApplicationPresenter appPresenter)
    {
        InitializeComponent();
        _presenter = appPresenter.Initialize(this);
        
        InitializeHighlightColorBoxes();
    }

    #region ISudokuPlayView

    public ISudokuPlayerDrawer Drawer => Board;
    public ISudokuSolverDrawer ClueShower => Board;

    public void FocusDrawer()
    {
        Board.Focus();
    }

    public void SetChangeLevelOptions(string[] options, int value)
    {
        ChangeLevelSelector.SetOptions(options, value);
    }

    public void SetIsPlaying(bool isPlaying)
    {
        TimerMiddleButtonPath.Data = isPlaying ? PauseGeometry : PlayGeometry;
    }

    public void SetTimeElapsed(TimeQuantity quantity)
    {
        TimerTime.Dispatcher.Invoke(() => TimerTime.Text = quantity.ToString());
    }

    public void SetHistoricAvailability(bool canMoveBack, bool canMoveForward)
    {
        BackButton.IsEnabled = canMoveBack;
        BackArrow.SetResourceReference(Shape.StrokeProperty, canMoveBack ? "Text" : "Disabled");

        ForwardButton.IsEnabled = canMoveForward;
        ForwardArrow.SetResourceReference(Shape.StrokeProperty, canMoveForward ? "Text" : "Disabled");
    }

    public void ShowClueState(bool isShowing)
    {
        ClueButton.Dispatcher.Invoke(() => ClueButton.Content = isShowing ? "Hide" : "Show");
    }

    public void ShowClueText(string text)
    {
        ClueText.Dispatcher.Invoke(() => ClueText.Text = text);
    }
    
    public void OpenOptionDialog(string name, OptionChosen callback, params string[] options)
    {
        var dialog = new OptionChooserDialog(name, callback, options);
        dialog.Show();
    }

    #endregion

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

    protected override void InternalUpdateSize()
    {
        
    }

    private void SelectCell(int row, int col)
    {
        _presenter.SelectCell(new Cell(row, col));
    }

    private void AddCellToSelection(int row, int col)
    {
        _presenter.AddCellToSelection(new Cell(row, col));
    }
    
    private void DoBoardInput(object sender, KeyEventArgs e)
    {
        if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
        {
            switch (e.Key)
            {
                case Key.V :
                    _presenter.Paste(Clipboard.GetText());
                    break;
            }

            return;
        }
        
        switch (e.Key)
        {
            case Key.D1 :
            case Key.NumPad1 :
                _presenter.ActOnCurrentCells(1);
                break;
            case Key.D2 :
            case Key.NumPad2 :
                _presenter.ActOnCurrentCells(2);
                break;
            case Key.D3 :
            case Key.NumPad3 :
                _presenter.ActOnCurrentCells(3);
                break;
            case Key.D4 :
            case Key.NumPad4 :
                _presenter.ActOnCurrentCells(4);
                break;
            case Key.D5 :
            case Key.NumPad5 :
                _presenter.ActOnCurrentCells(5);
                break;
            case Key.D6 :
            case Key.NumPad6 :
                _presenter.ActOnCurrentCells(6);
                break;
            case Key.D7 :
            case Key.NumPad7 :
                _presenter.ActOnCurrentCells(7);
                break;
            case Key.D8 :
            case Key.NumPad8 :
                _presenter.ActOnCurrentCells(8);
                break;
            case Key.D9 :
            case Key.NumPad9 :
                _presenter.ActOnCurrentCells(9);
                break;
            case Key.D0 :
            case Key.NumPad0 :
            case Key.Back :
                _presenter.RemoveCurrentCells();
                break;
        }
    }

    private void SetChangeLevel(int index)
    {
        _presenter.SetChangeLevel(index);
    }

    private void Start(object sender, RoutedEventArgs e)
    {
        _presenter.Start();
    }

    private void PlayOrPause(object sender, RoutedEventArgs e)
    {
        _presenter.PlayOrPause();
    }

    private void Stop(object sender, RoutedEventArgs e)
    {
        _presenter.Stop();
    }

    public void InitializeHighlightColorBoxes()
    {
        ColorGrid.Children.RemoveRange(1, ColorGrid.Children.Count - 1);
        
        for (int row = 0; row < 2; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                if (row + col == 0) continue;

                var color = (HighlightColor)(row * 4 + col - 1);

                var border = new Border
                {
                    Margin = new Thickness(10),
                    BorderThickness = new Thickness(1),
                    Width = 30,
                    Height = 30,
                    Background = App.Current.ThemeInformation.ToBrush(color)
                };

                border.SetResourceReference(Border.BorderBrushProperty, "Background3");
                Grid.SetRow(border, row);
                Grid.SetColumn(border, col);
                
                border.MouseLeftButtonDown += (_, _) => _presenter.HighlightCurrentCells(color);
                ColorGrid.Children.Add(border);
            }
        }
    }

    private void ClearHighlights(object sender, MouseButtonEventArgs e)
    {
        _presenter.ClearHighlightsFromCurrentCells();
    }

    private void MoveBack(object sender, RoutedEventArgs e)
    {
        _presenter.MoveBack();
    }

    private void MoveForward(object sender, RoutedEventArgs e)
    {
        _presenter.MoveForward();
    }

    private void ChangeClueState(object sender, RoutedEventArgs e)
    {
        _presenter.ChangeClueState();
    }

    private void ComputePossibilities(object sender, RoutedEventArgs e)
    {
        _presenter.ComputePossibilities();
    }
}