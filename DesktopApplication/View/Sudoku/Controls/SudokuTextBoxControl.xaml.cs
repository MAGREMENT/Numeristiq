using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DesktopApplication.View.Sudoku.Controls;

public partial class SudokuTextBoxControl
{
    private bool _callNewSudoku = true;
    
    private static readonly PathGeometry _upArrow = new(new []
    {
        new PathFigure(new Point(4, 7), new []
        {
            new LineSegment(new Point(10, 3), true),
            new LineSegment(new Point(16, 7), true)
        }, false)
    });
    
    private static readonly PathGeometry _downArrow = new(new []
    {
        new PathFigure(new Point(4, 3), new []
        {
            new LineSegment(new Point(10, 7), true),
            new LineSegment(new Point(16, 3), true)
        }, false)
    });
    
    public event OnNewSudoku? NewSudoku;
    public event OnShow? Showed;
    
    public SudokuTextBoxControl()
    {
        InitializeComponent();
        Arrow.Data = _downArrow;
    }

    private void Show(object sender, RoutedEventArgs e)
    {
        if (UpperPart.Visibility == Visibility.Visible)
        {
            UpperPart.Visibility = Visibility.Collapsed;
            Arrow.Data = _downArrow;
        }
        else
        {
            UpperPart.Visibility = Visibility.Visible;
            Arrow.Data = _upArrow;
            TextBox.Focus();
            Showed?.Invoke();
        }
    }

    public void SetText(string s)
    {
        _callNewSudoku = false;
        TextBox.Text = s;
        _callNewSudoku = true;
    }

    private void OnTextChange(object sender, TextChangedEventArgs e)
    {
        if(_callNewSudoku) NewSudoku?.Invoke(TextBox.Text);
    }
}

public delegate void OnNewSudoku(string s);
public delegate void OnShow();