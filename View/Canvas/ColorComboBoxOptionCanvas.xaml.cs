using System;
using System.Windows;
using System.Windows.Controls;
using Global.Enums;
using View.Utility;

namespace View.Canvas;

public partial class ColorComboBoxOptionCanvas
{
    private readonly OnChange<int> _onChange;
    private readonly bool _callOnChange;
    
    public ColorComboBoxOptionCanvas(string name, string explanation, int startIndex, OnChange<int> onChange)
    {
        InitializeComponent();

        Block.Text = name;
        
        _onChange = onChange;

        foreach (var availableColor in Enum.GetValues<CellColor>())
        {
            var brush = ColorManager.GetCellBrush(availableColor);
            var n = availableColor.ToString();

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
    public override void SetFontSize(int size)
    {
        Block.FontSize = size;
    }

    private void OnSelectionChange(object sender, SelectionChangedEventArgs e)
    {
        if (_callOnChange) _onChange(Box.SelectedIndex);
    }
}