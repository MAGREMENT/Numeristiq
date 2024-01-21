using Global.Enums;

namespace Presenter.Translators;

public record ViewLog(int Id, string Title, string Explanation, string Changes, Intensity Intensity, string HighlightCursor,
    int HighlightCount);