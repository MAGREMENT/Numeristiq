using System.Collections.Generic;
using Model.Sudokus.Solver.Position;
using Model.Sudokus.Solver.StrategiesUtility;
using Model.Sudokus.Solver.StrategiesUtility.NRCZTChains;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies.NRCZTChains;

public interface INRCZTCondition
{
    public string Name { get; }

    public IEnumerable<NRCZTChain> AnalyzeRow(NRCZTChain current, CellPossibility from, IReadOnlyLinePositions rowPoss);
    
    public IEnumerable<NRCZTChain> AnalyzeColumn(NRCZTChain current, CellPossibility from, IReadOnlyLinePositions colPoss);
    
    public IEnumerable<NRCZTChain> AnalyzeMiniGrid(NRCZTChain current, CellPossibility from, IReadOnlyMiniGridPositions miniPoss);
    
    public IEnumerable<NRCZTChain> AnalyzePossibilities(NRCZTChain current, CellPossibility from, ReadOnlyBitSet16 poss);
}



