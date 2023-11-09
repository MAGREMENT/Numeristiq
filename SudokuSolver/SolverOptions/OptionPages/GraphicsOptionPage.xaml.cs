using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SudokuSolver.SolverOptions.OptionPages;

public partial class GraphicsOptionPage
{
    private readonly BrushAndNameAssociation[] _availableColors =
    {
        new(Brushes.Black, "Black"),
        new(Brushes.Gray, "Gray"),
        new(Brushes.Red, "Red"),
        new(Brushes.Green, "Green"),
        new(Brushes.Blue, "Blue"),
    };

    public override string OptionTitle => "Graphics";

    public GraphicsOptionPage()
    {
        InitializeComponent();

        InitializeColorComboBox(GivenBox);
        InitializeColorComboBox(SolvingBox);
    }

    private void InitializeColorComboBox(ComboBox box)
    {
        for (int i = 0; i < _availableColors.Length; i++)
        {
            var brush = _availableColors[i].Brush;
            var name = _availableColors[i].Name;

            var item = new ComboBoxItem();
            var sp = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            item.Content = sp;

            sp.Children.Add(new TextBlock
            {
                Width = 10,
                Height = 10,
                Background = brush,
                Margin = new Thickness(0, 0, 5, 0)
            });
            sp.Children.Add(new TextBlock
            {
                Text = name
            });

            box.Items.Add(item);
        }
    }
    
    protected override void InitializeOptions()
    {
        if (OptionHandler is null) return;
        DelayAfterSlider.Value = OptionHandler.DelayAfter;
        DelayBeforeSlider.Value = OptionHandler.DelayBefore;
    }

    private void SetSolverDelayBefore(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (OptionHandler is null || Initializing) return;
        OptionHandler.DelayBefore = (int)DelayBeforeSlider.Value;
    }

    private void SetSolverDelayAfter(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (OptionHandler is null || Initializing) return;
        OptionHandler.DelayAfter = (int)DelayAfterSlider.Value;
    }

    private void SelectedGivenColor(object sender, SelectionChangedEventArgs e)
    {
        if (OptionHandler is null || Initializing) return;
        OptionHandler.GivenForegroundColor = _availableColors[GivenBox.SelectedIndex].Brush;
    }

    private void SelectedSolvingColor(object sender, SelectionChangedEventArgs e)
    {
        if (OptionHandler is null || Initializing) return;
        OptionHandler.SolvingForegroundColor = _availableColors[SolvingBox.SelectedIndex].Brush;
    }
    
    private class BrushAndNameAssociation
    {
        public BrushAndNameAssociation(Brush brush, string name)
        {
            Brush = brush;
            Name = name;
        }

        public Brush Brush { get; }
        public string Name { get; }
    }
}

