using Global.Enums;
using Model.Solver;

namespace Presenter.Translator;

public record ViewStrategy(string Name, Intensity Intensity, bool Used, bool Locked, OnCommitBehavior Behavior);