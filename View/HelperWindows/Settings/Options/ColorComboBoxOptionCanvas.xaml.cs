using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace View.HelperWindows.Settings.Options;

public partial class ColorComboBoxOptionCanvas : OptionCanvas
{
    private static readonly BrushAndNameAssociation[] AvailableColors =
    {
        new(Brushes.Black, "Black"),
        new(Brushes.Gray, "Gray"),
        new(Brushes.Red, "Red"),
        new(Brushes.Green, "Green"),
        new(Brushes.Blue, "Blue"),
    };
    
    private readonly OnChange<Brush> _onChange;
    private readonly bool _callOnChange;
    
    public ColorComboBoxOptionCanvas(string name, string explanation, int startIndex, OnChange<Brush> onChange)
    {
        InitializeComponent();

        Block.Text = name;
        
        _onChange = onChange;
        
        for (int i = 0; i < AvailableColors.Length; i++)
        {
            var brush = AvailableColors[i].Brush;
            var n = AvailableColors[i].Name;

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
                Text = n
            });

            Box.Items.Add(item);
        }

        _callOnChange = false;
        Box.SelectedIndex = startIndex;
        _callOnChange = true;

        Explanation = explanation;
        
    }

    public override string Explanation { get; }

    private void OnSelectionChange(object sender, SelectionChangedEventArgs e)
    {
        if (_callOnChange) _onChange(AvailableColors[Box.SelectedIndex].Brush);
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