using Global.Enums;

namespace Presenter.Translators;

public record ViewCommit(string StrategyName, Intensity StrategyIntensity);

public record ViewCommitInformation(string StrategyName, Intensity StrategyIntensity, string Changes,
    string HighlightCursor, int HighlightCount);