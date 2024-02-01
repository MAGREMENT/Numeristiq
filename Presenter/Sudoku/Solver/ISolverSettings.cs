using Model.Sudoku;
using Model.Sudoku.Solver.Helpers.Changes;

namespace Presenter.Sudoku.Solver;

public interface ISolverSettings
{
    public event OnSettingChange? ShownStateChanged;
    public event OnSettingChange? TranslationTypeChanged;
    public event OnSettingChange? UniquenessAllowedChanged;
    public event OnSettingChange? RedrawNeeded;
    
    public StateShown StateShown { get; set; }
    public SudokuTranslationType TranslationType { get; set; }
    public int DelayBeforeTransition { get; set; }
    public int DelayAfterTransition { get; set; }
    public bool UniquenessAllowed { get; set; }
    public ChangeType ActionOnCellChange { get; set; }
    public bool TransformSoloPossibilityIntoGiven { get; set; }
    public CellColor GivenColor { get; set; }
    public CellColor SolvingColor { get; set; }
    public LinkOffsetSidePriority SidePriority { get; set; }
    public bool ShowSameCellLinks { get; set; }
}