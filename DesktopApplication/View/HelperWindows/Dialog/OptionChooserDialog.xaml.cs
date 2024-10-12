using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using DesktopApplication.Presenter.Sudokus.Solve;

namespace DesktopApplication.View.HelperWindows.Dialog;

public partial class OptionChooserDialog
{
    public event OptionChosen? OptionChosen;
    
    public static OptionChooserDialog? TryCreate(string name, OptionCollection collection)
    {
        return collection switch
        {
            OptionCollection.SudokuStringFormat => new OptionChooserDialog(name, SudokuStringFormatOptions()),
            OptionCollection.SudokuSolverCopy => new OptionChooserDialog(name, SudokuSolverCopyOptions()),
            _ => null
        };
    }
    
    private static IEnumerable<(Geometry?, string, int)> SudokuStringFormatOptions()
    {
        yield return (null, "Line", 0); //TODO
        yield return (Geometry.Parse("M 1,1 H 19 V 19 H 1 Z M 1,10 H 19 M 10,1 V 19"), "Grid", 1);
        yield return (null, "Base32", 2);
    }
    
    private static IEnumerable<(Geometry?, string, int)> SudokuSolverCopyOptions()
    {
        return SudokuStringFormatOptions().Append((Geometry.Parse("M 7,5 3,10 7,15 M 13,5 17,10 13,15"), "XML", 3));
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
                CloseOnDeactivate = false;
                Close();
            };
            
            Panel.Children.Add(grid);
        }
    }
}

