using System;
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
using Model.Core;
using Model.Sudokus.Player;
using Model.Utility;

namespace DesktopApplication.View.Sudokus.Pages;

public partial class PlayPage : ISudokuPlayView
{
    private static readonly Geometry PauseGeometry = Geometry.Parse("M 17.5,5 H 22.5 V 25 H 17.5 Z M 27.5,5 H 32.5 V 25 H 27.5 Z");
    private static readonly Geometry PlayGeometry = Geometry.Parse("M 17,5 33,15 17,25 Z");
    
    private readonly SudokuPlayPresenter _presenter;

    private bool _disabled;
    private readonly bool _initialized;
    
    public PlayPage(SudokuApplicationPresenter appPresenter)
    {
        InitializeComponent();
        _presenter = appPresenter.Initialize(this);
        
        InitializeDifficultyComboBox();
        InitializeHighlightColorBoxes();
        _initialized = true;
    }

    #region ISudokuPlayView

    public ISudokuPlayerDrawer Drawer => (ISudokuPlayerDrawer)Embedded.OptimizableContent!;
    public ISudokuSolverDrawer ClueShower => (ISudokuSolverDrawer)Embedded.OptimizableContent!;

    public void FocusDrawer()
    {
        ((FrameworkElement)Embedded.OptimizableContent!).Focus();
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
        ClueButton.Dispatcher.Invoke(() => ClueButton.Content = isShowing ? "Hide Clue" : "Show Clue");
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

    public void UnselectPossibilityCursor()
    {
        foreach (var child in PossibilityCursorPanel.Children)
        {
            if (child is RadioButton rb) rb.IsChecked = false;
        }
    }
    
    public void InitializeHighlightColorBoxes()
    {
        ColorGrid.Children.RemoveRange(1, ColorGrid.Children.Count - 1);
        
        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 2; col++)
            {
                if (row + col == 0) continue;

                var color = (HighlightColor)(row * 2 + col - 1);

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

    private void InitializeDifficultyComboBox()
    {
        foreach (var v in Enum.GetValues<Difficulty>())
        {
            if(v == Difficulty.None) continue;

            var item = new ComboBoxItem
            {
                Content = SpaceConverter.Instance.Convert(v.ToString()),
                VerticalContentAlignment = VerticalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 16
            };
            item.SetResourceReference(ForegroundProperty, ThemeInformation.ResourceNameFor(v));
            
            DifficultyComboBox.Items.Add(item);
        }
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
        if (_disabled) return;
        
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

    private void Select1s(object sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton rb) return;

        rb.IsChecked = _presenter.SelectAllPossibilities(1);
    }
    
    private void Select2s(object sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton rb) return;

        rb.IsChecked = _presenter.SelectAllPossibilities(2);
    }
    
    private void Select3s(object sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton rb) return;

        rb.IsChecked = _presenter.SelectAllPossibilities(3);
    }
    
    private void Select4s(object sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton rb) return;

        rb.IsChecked = _presenter.SelectAllPossibilities(4);
    }
    
    private void Select5s(object sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton rb) return;

        rb.IsChecked = _presenter.SelectAllPossibilities(5);
    }
    
    private void Select6s(object sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton rb) return;

        rb.IsChecked = _presenter.SelectAllPossibilities(6);
    }
    
    private void Select7s(object sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton rb) return;

        rb.IsChecked = _presenter.SelectAllPossibilities(7);
    }
    
    private void Select8s(object sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton rb) return;

        rb.IsChecked = _presenter.SelectAllPossibilities(8);
    }
    
    private void Select9s(object sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton rb) return;

        rb.IsChecked = _presenter.SelectAllPossibilities(9);
    }

    public void Enable()
    {
        ComputePossibilitiesButton.IsEnabled = true;
        ClueButton.IsEnabled = true;
        TimerGrid.IsEnabled = true;
        ColorGrid.IsEnabled = true;
        ForwardButton.IsEnabled = true;
        BackButton.IsEnabled = true;
        _disabled = false;
    }

    public void Disable()
    {
        ComputePossibilitiesButton.IsEnabled = false;
        ClueButton.IsEnabled = false;
        TimerGrid.IsEnabled = false;
        ColorGrid.IsEnabled = false;
        ForwardButton.IsEnabled = false;
        BackButton.IsEnabled = false;
        _disabled = true;
    }

    private void ChangeLevelToSolution(object sender, RoutedEventArgs e)
    {
        if(_initialized) _presenter.SetChangeLevel(ChangeLevel.Solution);
    }
    
    private void ChangeLevelToTop(object sender, RoutedEventArgs e)
    {
        if(_initialized) _presenter.SetChangeLevel(ChangeLevel.TopPossibilities);
    }
    
    private void ChangeLevelToCenter(object sender, RoutedEventArgs e)
    {
        if(_initialized) _presenter.SetChangeLevel(ChangeLevel.MiddlePossibilities);
    }
    
    private void ChangeLevelToBottom(object sender, RoutedEventArgs e)
    {
        if(_initialized) _presenter.SetChangeLevel(ChangeLevel.BottomPossibilities);
    }

    private void LoadFromBank(object sender, RoutedEventArgs e)
    {
        if (DifficultyComboBox.SelectedIndex == -1) return;
        
        _presenter.LoadFromBank((Difficulty)(DifficultyComboBox.SelectedIndex + 1));
    }
}