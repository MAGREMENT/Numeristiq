using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DesktopApplication.View.HelperWindows.Dialog;
using Model.Sudokus;
using Model.Utility;

namespace DesktopApplication.View.Sudokus.Controls;

public partial class FilledSudokuGeneratorControl
{
    private bool _isRandom = true;

    public event OnRandomSelection? RandomSelected;
    public event OnSeedSelection? SeedSelected;
    public event OnShowSeedAsked? ShowSeedAsked;
    
    public FilledSudokuGeneratorControl()
    {
        InitializeComponent();
    }
    
    public void Activate(bool activated)
    {
        ActivatedLamp.SetResourceReference(BackgroundProperty, activated ? "On" : "Off");
    }

    private void OnSeedMouseEnter(object sender, MouseEventArgs e)
    {
        if (_isRandom)
        {
            Seed.SetResourceReference(BackgroundProperty, "BackgroundHighlighted");
        }
    }

    private void OnSeedMouseLeave(object sender, MouseEventArgs e)
    {
        Seed.Background = Brushes.Transparent;
    }
    
    private void OnRandomMouseEnter(object sender, MouseEventArgs e)
    {
        if (!_isRandom)
        {
            Random.SetResourceReference(BackgroundProperty, "BackgroundHighlighted");
        }
    }

    private void OnRandomMouseLeave(object sender, MouseEventArgs e)
    {
        Random.Background = Brushes.Transparent;
    }

    private void OnSeedClick(object sender, MouseButtonEventArgs e)
    {
        var window = OptionChooserDialog.TryCreate("Copy", typeof(SudokuStringFormat));
        if (window is null) return;

        window.OptionChosen += i =>
        {
            SeedText.SetResourceReference(ForegroundProperty, "Text");
            Seed.SetResourceReference(BorderBrushProperty, "Primary");
            SeedView.Visibility = Visibility.Visible;

            RandomText.SetResourceReference(ForegroundProperty, "Disabled");
            Random.BorderBrush = Brushes.Transparent;

            _isRandom = false;
            SeedSelected?.Invoke(Clipboard.GetText(), (SudokuStringFormat)i);
        };
        
        window.Show();
    }

    private void OnRandomClick(object sender, MouseButtonEventArgs e)
    {
        RandomText.SetResourceReference(ForegroundProperty, "Text");
        Random.SetResourceReference(BorderBrushProperty, "Primary");

        SeedView.Visibility = Visibility.Collapsed;
        Seed.BorderBrush = Brushes.Transparent;
        SeedText.SetResourceReference(ForegroundProperty, "Disabled");

        _isRandom = true;
        RandomSelected?.Invoke();
    }

    private void AskToShowSeed(object sender, RoutedEventArgs e)
    {
        ShowSeedAsked?.Invoke();
    }
}

public delegate void OnRandomSelection();
public delegate void OnSeedSelection(string s, SudokuStringFormat format);
public delegate void OnShowSeedAsked();