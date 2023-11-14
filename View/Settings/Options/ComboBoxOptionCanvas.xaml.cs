using System.Windows.Controls;

namespace View.Settings.Options;

public partial class ComboBoxOptionCanvas : OptionCanvas
{
    private readonly OnChange<int> _onChange;
    private readonly bool _callOnChange;

    public ComboBoxOptionCanvas(string name, string explanation, int startIndex, 
        OnChange<int> onChange, params string[] choices)
    {
        InitializeComponent();

        Block.Text = name;
        
        _onChange = onChange;

        foreach (var choice in choices)
        {
            var item = new ComboBoxItem
            {
                Content = choice
            };

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
        if(_callOnChange) _onChange(Box.SelectedIndex);
    }
}