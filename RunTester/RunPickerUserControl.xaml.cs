using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace RunTester;

public partial class RunPickerUserControl : UserControl
{

    public delegate void OnFilePicked(string path);
    public event OnFilePicked? FilePicked;
    
    public RunPickerUserControl()
    {
        InitializeComponent();
    }

    private void PickFile(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog()
        {
            DefaultExt = ".txt",
            Filter = "Text documents (.txt)|*.txt"
        };

        var result = dialog.ShowDialog();

        if (result == true)
        {
            FileName.Text = dialog.FileName;
            FilePicked?.Invoke(FileName.Text);
        }
    }
}