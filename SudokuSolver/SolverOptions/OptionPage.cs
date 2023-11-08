using System.Windows.Controls;

namespace SudokuSolver.SolverOptions;

public abstract class OptionPage : Page
{
    public abstract string OptionTitle { get; }
    public ISolverOptionHandler? OptionHandler { get; set; }
    
    protected bool Initializing { get; private set; }

    protected abstract void InitializeOptions();

    public void Initialize()
    {
        Initializing = true;
        InitializeOptions();
        Initializing = false;
    }
}