using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Model;

namespace RunTester;

public partial class RunLauncherUserControl
{
    private readonly Model.RunTester _runTester = new();

    private int _count;

    public delegate void OnRunEnd(RunResult rr);
    public event OnRunEnd? RunEnded;

    public string Path { get; set; } = "";
    
    public RunLauncherUserControl()
    {
        InitializeComponent();

        _runTester.SolveDone += (number, line, success) =>
        {
            Console.Dispatcher.Invoke(() => Console.Text = _count + " done");
        };

        _runTester.RunStatusChanged += running =>
        {
            Console.Dispatcher.Invoke(() => RunStatus.Background = running ? Brushes.Green : Brushes.Red);
            if(!running) RunEnded?.Invoke(_runTester.LastRunResult);
        };

    }

    private void Start(object sender, RoutedEventArgs e)
    {
        Console.Text = "";
        if (!File.Exists(Path))
        {
            Console.Text = "File not found";
            return;
        }

        _count = 0;
        _runTester.Path = Path;
        _runTester.Start();
    }

    private void Cancel(object sender, RoutedEventArgs e)
    {
        _runTester.Cancel();
    }
}