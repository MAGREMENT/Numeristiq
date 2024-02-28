using System.Windows;
using DesktopApplication.Controllers;

namespace DesktopApplication.View.Sudoku.Controls;

public partial class SudokuTextBoxControl : ISudokuTextBoxView
{
    private readonly SudokuTextBoxController _controller;
    
    public SudokuTextBoxControl()
    {
        InitializeComponent();
        _controller = ControllerDistributor.Initialize(this);
    }

    private void Show(object sender, RoutedEventArgs e)
    {
        UpperPart.Visibility = UpperPart.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        if(UpperPart.Visibility == Visibility.Visible) _controller.OnShow();
    }

    public void SetText(string s)
    {
        TextBox.Text = s;
    }
}