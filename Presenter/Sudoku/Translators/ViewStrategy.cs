using Model.Helpers.Logs;
using Model.Helpers.Settings;
using Model.Sudoku.Solver;

namespace Presenter.Sudoku.Translators;

public record ViewStrategy(string Name, Intensity Intensity, bool Used, bool Locked, OnCommitBehavior Behavior,
    IReadOnlyList<ViewStrategyArgument> Arguments);
    
public record ViewStrategyArgument(string Name, ISettingViewInterface Interface, SettingValue CurrentValue);