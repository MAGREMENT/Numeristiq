using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Utility.BitSets;

namespace Model.Kakuros;

public class KakuroSolvingState : IUpdatableSolvingState //TODO
{
    public int this[int row, int col] => throw new System.NotImplementedException();

    public ReadOnlyBitSet16 PossibilitiesAt(int row, int col)
    {
        throw new System.NotImplementedException();
    }

    public IUpdatableSolvingState Apply(IReadOnlyList<SolverProgress> progresses)
    {
        throw new System.NotImplementedException();
    }

    public IUpdatableSolvingState Apply(SolverProgress progress)
    {
        throw new System.NotImplementedException();
    }
}