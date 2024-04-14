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
            if (cp == from)
            {
                ignorable.Add(col);
                continue;
            }

            bool ok = false;
            foreach (var relation in current)
            {
                if (cp == relation.To) yield break;
                if (SudokuCellUtility.AreLinked(cp, relation.To)) ok = true;
            }

            if (ok) ignorable.Add(col);
        }

        NRCZTChain? chain;
        switch (rowPoss.Count - ignorable.Count)
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
        var ignorable = new LinePositions();

        foreach (var row in colPoss)
        {
            var cp = new CellPossibility(row, from.Column, from.Possibility);
            if (cp == from)
            {
                ignorable.Add(row);
                continue;
            }

            bool ok = false;
            foreach (var relation in current)
            {
                if (cp == relation.To) yield break;
                if (SudokuCellUtility.AreLinked(cp, relation.To)) ok = true;
            }

            if (ok) ignorable.Add(row);
        }

        NRCZTChain? chain;
        switch (colPoss.Count - ignorable.Count)
        {
            case 0 :
                foreach (var row in colPoss)
                {
                    if (row == from.Row) continue;
                    chain = current.TryAdd(from, new CellPossibility(row, from.Column, from.Possibility));
                    if (chain is not null) yield return chain;
                }

                break;
            case 1 :
                var one = colPoss.Difference(ignorable);
                chain = current.TryAdd(from, new CellPossibility(one.First(), from.Column, from.Possibility));
                if (chain is not null) yield return chain;
                break;
        }
    }

    public IEnumerable<NRCZTChain> AnalyzeMiniGrid(NRCZTChain current, CellPossibility from, IReadOnlyMiniGridPositions miniPoss)
    {
        var ignorable = new MiniGridPositions(from.Row / 3, from.Column / 3);

        foreach (var cell in miniPoss)
        {
            var cp = new CellPossibility(cell, from.Possibility);
            if (cp == from)
            {
                ignorable.Add(cell.Row % 3, cell.Column % 3);
                continue;
            }

            bool ok = false;
            foreach (var relation in current)
            {
                if (cp == relation.To) yield break;
                if (SudokuCellUtility.AreLinked(cp, relation.To)) ok = true;
            }

            if (ok) ignorable.Add(cell.Row % 3, cell.Column % 3);
        }

        NRCZTChain? chain;
        switch (miniPoss.Count - ignorable.Count)
        {
            case 0 :
                foreach (var cell in miniPoss)
                {
                    if (cell == from.ToCell()) continue;
                    chain = current.TryAdd(from, new CellPossibility(cell, from.Possibility));
                    if (chain is not null) yield return chain;
                }

                break;
            case 1 :
                var one = miniPoss.Difference(ignorable);
                chain = current.TryAdd(from, new CellPossibility(one.First(), from.Possibility));
                if (chain is not null) yield return chain;
                break;
        }
    }

    public IEnumerable<NRCZTChain> AnalyzePossibilities(NRCZTChain current, CellPossibility from, ReadOnlyBitSet16 poss)
    {
        var ignorable = new ReadOnlyBitSet16();

        foreach (var p in poss.EnumeratePossibilities())
        {
            var cp = new CellPossibility(from.Row, from.Column, p);
            if (cp == from)
            {
                ignorable += p;
                continue;
            }

            bool ok = false;
            foreach (var relation in current)
            {
                if (cp == relation.To) yield break;
                if (SudokuCellUtility.AreLinked(cp, relation.To)) ok = true;
            }

            if (ok) ignorable += p;
        }

        NRCZTChain? chain;
        switch (poss.Count - ignorable.Count)
        {
            case 0 :
                foreach (var p in poss.EnumeratePossibilities())
                {
                    if (p == from.Possibility) continue;
                    chain = current.TryAdd(from, new CellPossibility(from.Row, from.Column, p));
                    if (chain is not null) yield return chain;
                }

                break;
            case 1 :
                var one = poss - ignorable;
                chain = current.TryAdd(from, new CellPossibility(from.Row, from.Column, one.FirstPossibility()));
                if (chain is not null) yield return chain;
                break;
        }
    }
}