using System.Windows.Media;
using Model.Logs;

namespace SudokuSolver;

public partial class LogUserControl
{
    private ISolverLog? _log;

    public delegate void OnLogClicked(ISolverLog log);
    public event OnLogClicked? LogClicked;
    
    public LogUserControl()
    {
        InitializeComponent();

        Main.MouseEnter += (_, _) =>
        {
            Main.Background = new SolidColorBrush(Colors.White);
        };
        Main.MouseLeave += (_, _) =>
        {
            Main.Background = new SolidColorBrush(Colors.WhiteSmoke);
        };
        Main.MouseLeftButtonDown += (_, _) =>
        {
            if(_log is not null) LogClicked?.Invoke(_log);
        };
    }

    public void InitLog(ISolverLog log)
    {
        _log = log;

        Number.Text = "#" + log.Id;
        Title.Foreground = new SolidColorBrush(ColorUtil.ToColor(log.Intensity));
        Title.Text = log.Title;
        Text.Text = log.Changes;
    }
}