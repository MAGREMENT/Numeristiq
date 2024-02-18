using System.Windows.Controls;
using Model.Sudoku.Solver.Settings;
using View.Themes;

namespace View.Canvas;

public partial class ComboBoxOptionCanvas : OptionCanvas
{
    private readonly SetSetting<int> _setter;
    private readonly GetSetting<int> _getter;

    public ComboBoxOptionCanvas(string name, string explanation, GetSetting<int> getter, 
        SetSetting<int> setter, params string[] choices)
    {
        InitializeComponent();

        Block.Text = name;
        
        _setter = setter;
        _getter = getter;

        foreach (var choice in choices)
        {
            var item = new ComboBoxItem
            {
                Content = choice
            };

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
        if(ShouldCallSetter) _setter(Box.SelectedIndex);
    }
}