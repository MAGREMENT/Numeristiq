using System.Windows;
using System.Windows.Controls;
using Model.Sudoku.Solver.Settings;
using View.Themes;

namespace View.Canvas;

public partial class MultiChoiceOptionCanvas
{
    private readonly SetSetting<int> _setter;
    private readonly GetSetting<int> _getter;
    
    public MultiChoiceOptionCanvas(string name, string explanation, GetSetting<int> getter, 
        SetSetting<int> setter, params string[] choices)
    {
        InitializeComponent();

        _setter = setter;
        _getter = getter;

        TextBlock.Text = name;
        for (int i = 0; i < choices.Length; i++)
        {
            var radioButton = new RadioButton
            {
                Margin = new Thickness(0, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Content = choices[i]
            };

            var iForEvent = i;
            radioButton.Checked += (_, _) => TryChange(iForEvent);

            Panel.Children.Add(radioButton);
        }

        Explanation = explanation;
    }

    public override string Explanation { get; }
    public override void SetFontSize(int size)
    {
        TextBlock.FontSize = size;
    }

    protected override void InternalRefresh()
    {
        ((RadioButton)Panel.Children[_getter() + 1]).IsChecked = true;
    }

    private void TryChange(int i)
    {
        if (ShouldCallSetter) _setter(i);
    }
}