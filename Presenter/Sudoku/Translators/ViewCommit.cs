using Model.Sudoku.Solver.Helpers.Logs;

namespace Presenter.Sudoku.Translators;

public record ViewCommit(string StrategyName, Intensity StrategyIntensity);

public record ViewCommitInformation(string StrategyName, Intensity StrategyIntensity, string Changes,
    string HighlightCursor, int HighlightCount);