using System.Windows;
using System.Windows.Controls;
using DesktopApplication.Presenter.Sudoku.Solve;

namespace DesktopApplication.View.HelperWindows;

public partial class OptionChooserDialog
{
    public OptionChooserDialog(string name, OptionChosen callback, params string[] options)
    {
        InitializeComponent();

        OptionName.Text = name;

        for (int i = 0; i < options.Length; i++)
        {
            var iForEvent = i;

            var tb = new TextBlock
            {
                Style = (Style)FindResource("SearchResult"),
                Text = options[i]
            };

            tb.MouseLeftButtonDown += (_, _) =>
            {
                callback(iForEvent);
                CloseOnDeactivate = false;
                Close();
            };

            Panel.Children.Add(tb);
        }
    }
}

