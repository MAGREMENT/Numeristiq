using System.Collections.Generic;
using Model.Sudoku.Solver.Position;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Sudoku.Solver.StrategiesUtility.NRCZTChains;
using Model.Utility.BitSets;

namespace Model.Sudoku.Solver.Strategies.NRCZTChains;

public class ZCondition : INRCZTCondition
{
    private readonly CellPossibility[] _buffer = new CellPossibility[2];
    
    public string Name => "Z";
    
    public IEnumerable<NRCZTChain> AnalyzeRow(NRCZTChain current, CellPossibility from, IReadOnlyLinePositions rowPoss)
    {
        if (rowPoss.Count != 3) yield break;
        
        int cursor = 0;
        foreach (var col in rowPoss)
        {
            if (col == from.Column) continue;
            _buffer[cursor++] = new CellPossibility(from.Row, col, from.Possibility);
        }

        var chain = current.TryAdd(from, _buffer[0], _buffer[1]);
        if (chain is not null) yield return chain;
        
        current.TryAdd(from, _buffer[1], _buffer[0]);
        if (chain is not null) yield return chain;
    }

    public IEnumerable<NRCZTChain> AnalyzeColumn(NRCZTChain current, CellPossibility from, IReadOnlyLinePositions colPoss)
    {
        throw new System.NotImplementedException();
    }

    public IEnumerable<NRCZTChain> AnalyzeMiniGrid(NRCZTChain current, CellPossibility from, IReadOnlyMiniGridPositions miniPoss)
    {
        throw new System.NotImplementedException();
    }

    public IEnumerable<NRCZTChain> AnalyzePossibilities(NRCZTChain current, CellPossibility from, ReadOnlyBitSet16 poss)
    {
        throw new System.NotImplementedException();
    }

    public IEnumerable<NRCZTChain> AnalyzeRow()
    {
        throw new System.NotImplementedException();
    }

    public IEnumerable<NRCZTChain> AnalyzeColumn()
    {
        throw new System.NotImplementedException();
    }

    public IEnumerable<NRCZTChain> AnalyzeMiniGrid()
    {
        throw new System.NotImplementedException();
    }

    public IEnumerable<NRCZTChain> AnalyzePossibilities()
    {
        throw new System.NotImplementedException();
    }
}