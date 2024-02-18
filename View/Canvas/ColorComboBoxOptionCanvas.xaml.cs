using System;
using System.Windows;
using System.Windows.Controls;
using Model.Sudoku;
using Model.Sudoku.Solver.Settings;
using View.Themes;
using View.Utility;

namespace View.Canvas;

public partial class ColorComboBoxOptionCanvas
{
    private readonly SetSetting<int> _setter;
    private readonly GetSetting<int> _getter;
    
    public ColorComboBoxOptionCanvas(string name, string explanation, GetSetting<int> getter, SetSetting<int> setter)
    {
        InitializeComponent();

        Block.Text = name;
        
        _setter = setter;
        _getter = getter;

        foreach (var availableColor in Enum.GetValues<CellColor>())
        {
            var brush = ColorUtility.GetCellBrush(availableColor);
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

        Explanation = explanation;
        
    }

    public override string Explanation { get; }
    public override void SetFontSize(int size)
    {
        Block.FontSize = size;
    }

    protected override void InternalRefresh()
    {
        Box.SelectedIndex = _getter();
    }

    private void OnSelectionChange(object sender, SelectionChangedEventArgs e)
    {
        if (ShouldCallSetter) _setter(Box.SelectedIndex);
    }
}