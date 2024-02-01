using Model.Utility;

namespace Model.Tectonic;

public interface ISolvable
{
    IReadOnlyTectonic Tectonic { get; }

    Candidates GetCandidates(Cell cell);
}