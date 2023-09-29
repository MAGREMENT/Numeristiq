namespace RunTester;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow //TODO make into console app
{
    public MainWindow()
    {
        InitializeComponent();

        RunPicker.FilePicked += path => RunLauncher.Path = path;
        RunLauncher.RunEnded += RunResult.Update;
    }
}