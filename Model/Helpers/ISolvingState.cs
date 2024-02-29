using System.Collections.Generic;
using Model.Helpers.Changes;

namespace Model.Helpers;

public interface ISolvingState : ITranslatable
{
    public ISolvingState Apply(IReadOnlyList<SolverProgress> progresses);
    public ISolvingState Apply(SolverProgress progress);
}