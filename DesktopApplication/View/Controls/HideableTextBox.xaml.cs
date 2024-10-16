﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DesktopApplication.View.Controls;

public partial class HideableTextBox
{
    private bool _callNewSudoku = true;
    
    private static readonly PathGeometry _upArrow = new(new []
    {
        new PathFigure(new Point(5, 6), new []
        {
            new LineSegment(new Point(10, 3), true),
            new LineSegment(new Point(15, 6), true)
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

    private void Copy(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(TextBox.Text);
    }
    
    private void Paste(object sender, RoutedEventArgs e)
    {
        TextBox.Text = Clipboard.GetText();
    }
}

public delegate void OnTextChange(string s);
public delegate void OnShow();