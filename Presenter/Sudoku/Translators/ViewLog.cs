using Model.Sudoku.Solver.Helpers.Logs;

namespace Presenter.Sudoku.Translators;

public record ViewLog(int Id, string Title, string Explanation, string Changes, Intensity Intensity, string HighlightCursor,
    int HighlightCount);