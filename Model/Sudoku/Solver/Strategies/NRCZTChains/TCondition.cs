using System.Collections.Generic;
using Model.Sudoku.Solver.Position;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Sudoku.Solver.StrategiesUtility.NRCZTChains;
using Model.Utility.BitSets;

namespace Model.Sudoku.Solver.Strategies.NRCZTChains;
 
public class TCondition : INRCZTCondition
{ 
    public string Name => "T";
    
    public IEnumerable<NRCZTChain> AnalyzeRow(NRCZTChain current, CellPossibility from, IReadOnlyLinePositions rowPoss)
    {
        var ignorable = new LinePositions();

        foreach (var col in rowPoss)
        {
            var cp = new CellPossibility(from.Row, col, from.Possibility);
            if (cp == from || current.Contains(cp)) ignorable.Add(col);
        }

        NRCZTChain? chain;
        switch (ignorable.Count - rowPoss.Count)
        {
            case 0 :
                foreach (var col in rowPoss)
                {
                    if (col == from.Column) continue;
                    chain = current.TryAdd(from, new CellPossibility(from.Row, col, from.Possibility));
                    if (chain is not null) yield return chain;
                }

                break;
            case 1 :
                var one = rowPoss.Difference(ignorable);
                chain = current.TryAdd(from, new CellPossibility(from.Row, one.First(), from.Possibility));
                if (chain is not null) yield return chain;
                break;
        }
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
}