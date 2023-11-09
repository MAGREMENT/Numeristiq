using System.Windows.Media;
using Model;
using Model.Solver;
using Model.Solver.Helpers.Changes;

namespace SudokuSolver.SolverOptions;

public interface ISolverOptionHandler
{
    public int DelayBefore { get; set; }
    public int DelayAfter { get; set; }
    public SudokuTranslationType TranslationType { get; set; }
    public bool StepByStep { get; set; }
    public bool UniquenessAllowed { get; set; }
    public OnInstanceFound OnInstanceFound { get; set; }
    public ChangeType ActionOnKeyboardInput { get; set; }
    public Brush GivenForegroundColor { get; set; }
    public Brush SolvingForegroundColor { get; set; }
}