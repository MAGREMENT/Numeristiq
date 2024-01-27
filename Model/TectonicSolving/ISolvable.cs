using System.Collections.Generic;
using Global;

namespace Model.TectonicSolving;

public interface ISolvable
{
    IReadOnlyTectonic Tectonic { get; }

    Candidates GetCandidates(Cell cell);
}