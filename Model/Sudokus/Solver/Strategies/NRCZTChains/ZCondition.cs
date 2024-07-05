using System.Collections.Generic;
using Model.Sudokus.Solver.Position;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.NRCZTChains;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies.NRCZTChains;

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
        if (colPoss.Count != 3) yield break;
        
        int cursor = 0;
        foreach (var row in colPoss)
        {
            if (row == from.Row) continue;
            _buffer[cursor++] = new CellPossibility(row, from.Column, from.Possibility);
        }

        var chain = current.TryAdd(from, _buffer[0], _buffer[1]);
        if (chain is not null) yield return chain;
        
        current.TryAdd(from, _buffer[1], _buffer[0]);
        if (chain is not null) yield return chain;
    }

    public IEnumerable<NRCZTChain> AnalyzeMiniGrid(NRCZTChain current, CellPossibility from, IReadOnlyBoxPositions miniPoss)
    {
        if (miniPoss.Count != 3) yield break;
        
        int cursor = 0;
        foreach (var cell in miniPoss)
        {
            if (cell == from.ToCell()) continue;
            _buffer[cursor++] = new CellPossibility(cell, from.Possibility);
        }

        var chain = current.TryAdd(from, _buffer[0], _buffer[1]);
        if (chain is not null) yield return chain;
        
        current.TryAdd(from, _buffer[1], _buffer[0]);
        if (chain is not null) yield return chain;
    }

    public IEnumerable<NRCZTChain> AnalyzePossibilities(NRCZTChain current, CellPossibility from, ReadOnlyBitSet16 poss)
    {
        if (poss.Count != 3) yield break;
        
        int cursor = 0;
        foreach (var p in poss.EnumeratePossibilities())
        {
            if (p == from.Possibility) continue;
            _buffer[cursor++] = new CellPossibility(from.Row, from.Column, p);
        }

        var chain = current.TryAdd(from, _buffer[0], _buffer[1]);
        if (chain is not null) yield return chain;
        
        current.TryAdd(from, _buffer[1], _buffer[0]);
        if (chain is not null) yield return chain;
    }
}