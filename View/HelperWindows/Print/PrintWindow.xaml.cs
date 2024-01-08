using System.IO;
using System.Windows;
using System.Windows.Forms;
using Clipboard = System.Windows.Forms.Clipboard;

namespace View.HelperWindows.Print;

public partial class PrintWindow
{
    private readonly string _toPrint;
    
    public PrintWindow(string title, string toPrint)
    {
        InitializeComponent();

        Title.Text = title;
        _toPrint = toPrint;
    }

    private void OpenFolderDialog(object sender, RoutedEventArgs e)
    {
        FolderBrowserDialog dialog = new();
        var result = dialog.ShowDialog();
        if (!string.IsNullOrWhiteSpace(result.ToString()))
        {
            FileLocation.Text = dialog.SelectedPath;
            OkButton.IsEnabled = true;
        }
    }

    private void Ok(object sender, RoutedEventArgs e)
    {
        if (ClipboardRadioButton.IsChecked == true)
        {
            Clipboard.SetText(_toPrint);
        }
        else if (TextFileRadioButton.IsChecked == true)
        {
            var file = FileLocation.Text + @$"\{Title.Text}.txt";
            using var writer = new StreamWriter(file, new FileStreamOptions
            {
                Mode = FileMode.Create,
                Access = FileAccess.ReadWrite,
                Share = FileShare.None
            });

            writer.Write(_toPrint);
        }

        Close();
    }

    private void ToClipboard(object sender, RoutedEventArgs e)
    {
        OkButton.IsEnabled = true;
    }

    private void ToTextFile(object sender, RoutedEventArgs e)
    {
        FolderChooser.IsEnabled = true;
    }

    private void DisableFolderChooser(object sender, RoutedEventArgs e)
    {
        FolderChooser.IsEnabled = false;
    }
}