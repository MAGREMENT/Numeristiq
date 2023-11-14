using System.Windows;
using System.Windows.Controls;

namespace View.Settings.Options;

public partial class MultiChoiceOptionCanvas : OptionCanvas
{
    public MultiChoiceOptionCanvas(string name, string explanation, int startIndex, 
        OnChange<int> onChange, params string[] choices)
    {
        InitializeComponent();

        TextBlock.Text = name;
        for (int i = 0; i < choices.Length; i++)
        {
            var radioButton = new RadioButton
            {
                Margin = new Thickness(0, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Content = choices[i]
            };

            if (startIndex == i) radioButton.IsChecked = true;
            var iForEvent = i;
            radioButton.Checked += (_, _) => onChange(iForEvent);

            Panel.Children.Add(radioButton);
        }

        Explanation = explanation;
    }

    public override string Explanation { get; }
}