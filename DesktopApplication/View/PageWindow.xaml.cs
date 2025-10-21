using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace DesktopApplication.View;

public partial class PageWindow
{
    private readonly ManagedPage[] _pages;

    private int _currentPage = -1;
    private bool _cancelNavigation = true;
    
    public PageWindow(double width, double height, params ManagedPage[] pages)
    {
        InitializeComponent();

        MinWidth = width;
        Width = width;
        MinHeight = height;
        Height = height;
        
        TitleBar.RefreshMaximizeRestoreButton(WindowState);
        StateChanged += (_, _) => TitleBar.RefreshMaximizeRestoreButton(WindowState);

        _pages = pages;
        MainGrid.ColumnDefinitions.Add(new ColumnDefinition
        {
            Width = new GridLength(1, GridUnitType.Star)
        });
        for (int i = 0; i < _pages.Length; i++)
        {
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = GridLength.Auto
            });
            
            var rb = new RadioButton
            {
                Style = (Style)FindResource("BottomSelectionRadioButtonStyle"),
                Content = _pages[i].Header
            };
            Grid.SetRow(rb, 2);
            Grid.SetColumn(rb, i + 1);
            rb.Checked += SwapPage;
            rb.IsChecked = i == 0;

            MainGrid.Children.Add(rb);
        }
        MainGrid.ColumnDefinitions.Add(new ColumnDefinition
        {
            Width = new GridLength(1, GridUnitType.Star)
        });
        
        Grid.SetColumnSpan(TitleBar, _pages.Length + 2);
        Grid.SetColumnSpan(Frame, _pages.Length + 2);
    }
    
    private void Minimize()
    {
        WindowState = WindowState.Minimized;
    }

    private void ChangeSize()
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void SwapPage(object sender, EventArgs args)
    {
        if (sender is not FrameworkElement fe) return;
        
        _cancelNavigation = false;
        if(_currentPage != -1) _pages[_currentPage].OnClose();

        _currentPage = Grid.GetColumn(fe) - 1;
        var newOne = _pages[_currentPage];

        TitleBar.InsideContent = newOne.TitleBarContent();
        newOne.OnShow();
        Frame.Content = newOne;

        _cancelNavigation = true;
    }

    private void CancelNavigation(object sender, NavigatingCancelEventArgs e)
    {
        if(_cancelNavigation) e.Cancel = true;
    }

    private void ReturnToWelcomeWindow()
    {
        var window = new WelcomeWindow();
        window.Show();
        Close();
    }
}

public abstract class ManagedPage : Page
{
    public abstract string Header { get; }
    public abstract void OnShow();
    public abstract void OnClose();
    public abstract object? TitleBarContent();
}