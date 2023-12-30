using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Global;
using Global.Enums;
using Presenter;
using Presenter.Player;
using View.HelperWindows.Settings;
using View.Pages.Player.UserControls;
using View.Themes;
using View.Utility;

namespace View.Pages.Player;

public partial class PlayerPage : IPlayerView
{
    private const double ModeWidth = 100;
    
    private readonly IPageHandler _pageHandler;
    private readonly PlayerPresenter _presenter;
    private readonly SudokuGrid _grid;

    private ModeUserControl? _currentChangeMode;
    private ModeUserControl? _currentLocationMode;
    private Key _changeModeUp = Key.Z;
    private Key _changeModeDown = Key.S;
    private Key _locationModeUp = Key.A;
    private Key _locationModeDown = Key.Q;

    public PlayerPage(IPageHandler handler, ApplicationPresenter factory)
    {
        InitializeComponent();

        _presenter = factory.Create(this);
        
        _pageHandler = handler;

        _grid = new SudokuGrid(24, 1, 3)
        {
            Margin = new Thickness(10)
        };
        Panel.Children.Insert(0, _grid);

        _grid.CellSelected += (row, col) => _presenter.RestartSelection(row, col);
        _grid.CellAddedToSelection += _presenter.AddToSelection;
        _grid.KeyDown += AnalyzeKeyDown;
        Colors.HighlightChosen += _presenter.Highlight;
        
        InitModes();
        _presenter.Bind();
    }

    public override void OnShow()
    {
        _grid.Focus();
    }

    public override void OnQuit()
    {
        
    }

    public override void ApplyTheme(Theme theme)
    {
        
    }

    private void GoBack(object sender, RoutedEventArgs e)
    {
        _pageHandler.ShowPage(PagesName.First);
    }

    public void ShowPossibilities(int row, int col, int[] possibilities, PossibilitiesLocation location)
    {
        _grid.ShowLinePossibilities(row, col, possibilities, location, Brushes.Black);
    }

    public void ShowNumber(int row, int col, int number, CellColor color)
    {
        _grid.ShowSolution(row, col, number, ColorUtility.ToBrush(color));
    }

    public void ClearNumbers()
    {
        _grid.ClearNumbers();
    }

    public void PutCursorOn(HashSet<Cell> cells)
    {
        _grid.PutCursorOn(cells);
    }

    public void ClearCursor()
    {
        _grid.ClearCursor();
    }

    public void Refresh()
    {
        _grid.Refresh();
    }

    public void ClearDrawings()
    {
        _grid.ClearHighlighting();
    }

    public void HighlightCell(int row, int col, HighlightColor color)
    {
        _grid.FillCell(row, col, ColorUtility.ToColor(color));
    }

    public void HighlightCell(int row, int col, HighlightColor[] colors, double startAngle, RotationDirection direction)
    {
        _grid.FillCell(row, col, startAngle, direction == RotationDirection.ClockWise ? -1 : 1, ColorUtility.ToColors(colors));
    }

    private void InitModes()
    {
        foreach (var m in Enum.GetValues<LocationMode>())
        {
            var muc = new ModeUserControl(m.ToString(), ModeWidth)
            {
                Margin = new Thickness(0, 0, 0, 5)
            };
            muc.Selected += selection =>
            {
                if(_currentLocationMode is not null) _currentLocationMode.ShowUnSelection();
                selection.ShowSelection();
                _currentLocationMode = selection;
                _presenter.SetLocationMode(m);
            };

            LocationModes.Children.Add(muc);
        }
        
        foreach (var m in Enum.GetValues<ChangeMode>())
        {
            var muc = new ModeUserControl(m.ToString(), ModeWidth)
            {
                Margin = new Thickness(0, 0, 0, 5)
            };
            muc.Selected += selection =>
            {
                if(_currentChangeMode is not null) _currentChangeMode.ShowUnSelection();
                selection.ShowSelection();
                _currentChangeMode = selection;
                _presenter.SetChangeMode(m);
            };
            
            ChangeModes.Children.Add(muc);
        }
    }

    public void SetChangeMode(ChangeMode mode)
    {
        var muc = (ModeUserControl)ChangeModes.Children[(int)mode];
        if(_currentChangeMode is not null) _currentChangeMode.ShowUnSelection();
        muc.ShowSelection();
        _currentChangeMode = muc;
    }

    public void SetLocationMode(LocationMode mode)
    {
        var muc = (ModeUserControl)LocationModes.Children[(int)mode];
        if(_currentLocationMode is not null) _currentLocationMode.ShowUnSelection();
        muc.ShowSelection();
        _currentLocationMode = muc;
    }

    public void SetMoveAvailability(bool canMoveBack, bool canMoveForward)
    {
        LeftArrow.IsEnabled = canMoveBack;
        RightArrow.IsEnabled = canMoveForward;
    }

    private void AnalyzeKeyDown(object? sender, KeyEventArgs args)
    {
        if (args.KeyboardDevice.Modifiers == ModifierKeys.Control)
        {
            switch (args.Key)
            {
                case Key.V :
                    _presenter.Paste(Clipboard.GetText());
                    break;
            }
        }
        else
        {
            switch (args.Key)
            {
                case Key.D1 :
                case Key.NumPad1 : _presenter.ApplyChange(1);
                    break;
                case Key.D2 :
                case Key.NumPad2 : _presenter.ApplyChange(2);
                    break;
                case Key.D3 :
                case Key.NumPad3 : _presenter.ApplyChange(3);
                    break;
                case Key.D4 :
                case Key.NumPad4 : _presenter.ApplyChange(4);
                    break;
                case Key.D5 :
                case Key.NumPad5 : _presenter.ApplyChange(5);
                    break;
                case Key.D6 :
                case Key.NumPad6 : _presenter.ApplyChange(6);
                    break;
                case Key.D7 :
                case Key.NumPad7 : _presenter.ApplyChange(7);
                    break;
                case Key.D8 :
                case Key.NumPad8 : _presenter.ApplyChange(8);
                    break;
                case Key.D9 :
                case Key.NumPad9 : _presenter.ApplyChange(9);
                    break;
                case Key.Back : _presenter.Remove();
                    break;
                default:
                    if (args.Key == _locationModeUp) MoveUp(LocationModes.Children, _currentLocationMode)?.InvokeSelection();
                    else if (args.Key == _locationModeDown) MoveDown(LocationModes.Children, _currentLocationMode)?.InvokeSelection();
                    else if (args.Key == _changeModeUp) MoveUp(ChangeModes.Children, _currentChangeMode)?.InvokeSelection();
                    else if (args.Key == _changeModeDown) MoveDown(ChangeModes.Children, _currentChangeMode)?.InvokeSelection();
                    break;
            } 
        }
    }
    
    private ModeUserControl? MoveUp(UIElementCollection collection, ModeUserControl? muc)
    {
        var i = collection.IndexOf(muc);
        if (i == -1) return null;
        i = i - 1 < 0 ? collection.Count - 1 : i - 1;
        return (ModeUserControl)collection[i];
    }
    
    private ModeUserControl? MoveDown(UIElementCollection collection, ModeUserControl? muc)
    {
        var i = collection.IndexOf(muc);
        if (i == -1) return null;
        i = (i + 1) % collection.Count;
        return (ModeUserControl)collection[i];
    }

    private void MoveBack(object sender, RoutedEventArgs e)
    {
        _presenter.MoveBack();
    }

    private void MoveForward(object sender, RoutedEventArgs e)
    {
        _presenter.MoveForward();
    }

    private void ClearNumbers(object sender, RoutedEventArgs e)
    {
        _presenter.ClearNumbers();
    }

    private void ClearHighlights(object sender, RoutedEventArgs e)
    {
        _presenter.ClearHighlightings();
    }

    private void ComputeDefault(object sender, RoutedEventArgs e)
    {
        _presenter.ComputeDefaultPossibilities();
    }

    private void Paste(object sender, RoutedEventArgs e)
    {
        _presenter.Paste(Clipboard.GetText());
    }

    private void Settings(object sender, RoutedEventArgs e)
    {
        var settings = SettingsWindow.From(_presenter.Settings);
        settings.Show();
    }
}