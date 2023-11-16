using Global.Enums;

namespace Presenter.Translator;

public record ViewLog(int Id, string Title, string Changes, Intensity Intensity, string HighlightCursor,
    int HighlightCount);