using Global;
using Global.Enums;
using Model.SudokuSolving.Solver;

namespace Presenter.Translators;

public record ViewStrategy(string Name, Intensity Intensity, bool Used, bool Locked, OnCommitBehavior Behavior,
    IReadOnlyList<ViewStrategyArgument> Arguments);
    
public record ViewStrategyArgument(string Name, IArgumentViewInterface Interface, string CurrentValue);