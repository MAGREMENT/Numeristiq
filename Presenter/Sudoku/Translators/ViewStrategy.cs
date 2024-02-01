using Model.Sudoku.Solver;
using Model.Sudoku.Solver.Arguments;
using Model.Sudoku.Solver.Helpers.Logs;

namespace Presenter.Sudoku.Translators;

public record ViewStrategy(string Name, Intensity Intensity, bool Used, bool Locked, OnCommitBehavior Behavior,
    IReadOnlyList<ViewStrategyArgument> Arguments);
    
public record ViewStrategyArgument(string Name, IArgumentViewInterface Interface, string CurrentValue);