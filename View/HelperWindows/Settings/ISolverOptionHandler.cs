using System.Windows.Media;
using Global.Enums;
using View.Utility;

namespace View.HelperWindows.Settings;

public interface ISolverOptionHandler
{
    public int DelayBeforeTransition { get; set; }
    public int DelayAfterTransition { get; set; }
    public SudokuTranslationType TranslationType { get; set; }
    public bool StepByStep { get; set; }
    public bool UniquenessAllowed { get; set; }
    public OnInstanceFound OnInstanceFound { get; set; }
    public ChangeType ActionOnKeyboardInput { get; set; }
    public Brush GivenForegroundColor { get; set; }
    public Brush SolvingForegroundColor { get; set; }
    public LinkOffsetSidePriority SidePriority { get; set; }
}