using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DesktopApplication.View.Controls;

public partial class HideableTextBox
{
    private bool _callNewSudoku = true;
    
    private static readonly PathGeometry _upArrow = new(new []
    {
        new PathFigure(new Point(5, 7), new []
        {
            new LineSegment(new Point(10, 4), true),
            new LineSegment(new Point(15, 7), true)
        }, false)
    });
    
    private static readonly PathGeometry _downArrow = new(new []
    {
        new PathFigure(new Point(5, 3), new []
        {
            new LineSegment(new Point(10, 6), true),
            new LineSegment(new Point(15, 3), true)
        }, false)
    });
    
    public event OnTextChange? TextChanged;
    public event OnShow? Showed;
    
    public HideableTextBox()
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
        if(_callNewSudoku) TextChanged?.Invoke(TextBox.Text);
    }

    private void Hide(object sender, RoutedEventArgs e)
    {
        UpperPart.Visibility = Visibility.Collapsed;
        Arrow.Data = _downArrow;
    }
}

public delegate void OnTextChange(string s);
public delegate void OnShow();