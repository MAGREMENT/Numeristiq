using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Model.Sudokus;

namespace DesktopApplication.View.HelperWindows.Dialog;

public partial class OptionChooserDialog
{
    public event OnOptionChosen? OptionChosen;
    
    public static OptionChooserDialog? TryCreate(string name, Type type, params int[] except)
    {
        if (type == typeof(SudokuStringFormat))
            return new OptionChooserDialog(name, SudokuStringFormatOptions(except));

        return null;
    }
    
    private static IEnumerable<(Geometry?, string, int)> SudokuStringFormatOptions(int[] except)
    {
        if (!except.Contains(0)) yield return (null, "Line", 0);
        if (!except.Contains(1)) yield return (Geometry.Parse("M 1,1 H 19 V 19 H 1 Z M 1,10 H 19 M 10,1 V 19"), "Grid", 1);
        if (!except.Contains(2)) yield return (null, "Base32", 2);
    }
    
    private OptionChooserDialog(string name, IEnumerable<(Geometry?, string, int)> collection)
    {
        InitializeComponent();

        OptionName.Text = name;
        foreach (var (g, n, i) in collection)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = GridLength.Auto
            });
            grid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(1, GridUnitType.Star)
            });

            var drawing = new Path
            {
                StrokeThickness = 2,
                Width = 20,
                Height = 20,
                Margin = new Thickness(5),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            drawing.SetResourceReference(Shape.StrokeProperty, "Text");
            Grid.SetColumn(drawing, 0);
            if (g is not null) drawing.Data = g;
            grid.Children.Add(drawing);

            var tb = new TextBlock
            {
                FontSize = 16,
                Text = n,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 0, 0, 0)
            };

            tb.SetResourceReference(ForegroundProperty, "Text");
            Grid.SetColumn(tb, 1);
            grid.Children.Add(tb);

            grid.MouseEnter += (_, _) => grid.SetResourceReference(BackgroundProperty, "BackgroundHighlighted");
            grid.MouseLeave += (_, _) => grid.Background = Brushes.Transparent;
            grid.MouseLeftButtonDown += (_, _) =>
            {
                OptionChosen?.Invoke(i);
                Close();
            };
            
            Panel.Children.Add(grid);
        }
    }
}

public delegate void OnOptionChosen(int index);

