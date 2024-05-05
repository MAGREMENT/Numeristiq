using System;
using System.Collections.Generic;
using Model.Sudokus.Solver.Position;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Utility.Exocet;

public class JuniorExocet : Exocet
{
    public Cell EscapeCell { get; }
    public Cell[] Target1MirrorNodes { get; }
    public Cell[] Target2MirrorNodes { get; }
    

    public JuniorExocet(Cell base1, Cell base2, Cell target1, Cell target2, ReadOnlyBitSet16 baseCandidates,
        Cell escapeCell, Cell[] target1MirrorNodes, Cell[] target2MirrorNodes, Dictionary<int, GridPositions> sCells)
    : base(base1, base2, target1, target2, baseCandidates, sCells)
    {
        EscapeCell = escapeCell;
        Target1MirrorNodes = target1MirrorNodes;
        Target2MirrorNodes = target2MirrorNodes;
    }

    public LinePositions SCellsLinePositions()
    {
        var result = new LinePositions();
        if (GetUnit() == Unit.Row)
        {
            result.Add(EscapeCell.Column);
            result.Add(Target1.Column);
            result.Add(Target2.Column);
        }
        else
        {
            result.Add(EscapeCell.Row);
            result.Add(Target1.Row);
            result.Add(Target2.Row);
        }

        return result;
    }

    public override List<Cell> AllPossibleSCells()
    {
        List<Cell> sCells = new();
        if (GetUnit() == Unit.Row)
        {
            for (int row = 0; row < 9; row++)
            {
                if (row / 3 == Base1.Row / 3) continue;
                
                sCells.Add(new Cell(row, Target1.Column));
                sCells.Add(new Cell(row, Target2.Column));
                sCells.Add(new Cell(row, EscapeCell.Column));
            }
        }
        else
        {
            for (int col = 0; col < 9; col++)
            {
                if (col / 3 == Base1.Column / 3) continue;
                
                sCells.Add(new Cell(Target1.Row, col));
                sCells.Add(new Cell(Target2.Row, col));
                sCells.Add(new Cell(EscapeCell.Row, col));
            }
        }

        return sCells;
    }

    public bool CompatibilityCheck(ISudokuStrategyUser strategyUser, int poss1, int poss2)
    {
        if (!BaseCandidates.Contains(poss1) || !BaseCandidates.Contains(poss2))
            throw new ArgumentException("Possibility not in base candidates");
        
        if (poss1 == poss2) return false;

        return GetUnit() == Unit.Row
            ? RowCompatibilityCheck(strategyUser, poss1, poss2)
            : ColumnCompatibilityCheck(strategyUser, poss1, poss2);
    }

    private bool RowCompatibilityCheck(ISudokuStrategyUser strategyUser, int poss1, int poss2)
    {
        int urThreatCount = 0;
        var possibilities = new ReadOnlyBitSet16(poss1, poss2);

        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            if (miniRow == Base1.Row / 3) continue;

            for (int r = 0; r < 3; r++)
            {
                int row = miniRow * 3 + r;
                
                if (strategyUser.Contains(row, EscapeCell.Column, poss1) || strategyUser.Contains(row, EscapeCell.Column, poss2) ||
                    strategyUser.Contains(row, Target1.Column, poss1) || strategyUser.Contains(row, Target1.Column, poss2) ||
                    strategyUser.Contains(row, Target2.Column, poss1) || strategyUser.Contains(row, Target2.Column, poss2)) continue;

                if (!strategyUser.PossibilitiesAt(row, Base1.Column).ContainsAll(possibilities) ||
                    !strategyUser.PossibilitiesAt(row, Base2.Column).ContainsAll(possibilities)) continue;
                
                urThreatCount++;
                break;
            }
        }

        if (urThreatCount != 2) return true;

        var oneS = SCells[poss1];
        var twoS = SCells[poss2];

        foreach (var diag in SudokuCellUtility.DiagonalMiniGridAssociation(Base1.Row / 3, Base1.Column / 3))
        {
            if (oneS.MiniGridCount(diag.Item1.MiniRow, diag.Item1.MiniColumn) > 0 &&
                twoS.MiniGridCount(diag.Item2.MiniRow, diag.Item2.MiniColumn) > 0) return true;
        }

        return false;
    }
    
    private bool ColumnCompatibilityCheck(ISudokuStrategyUser strategyUser, int poss1, int poss2)
    {
        int urThreatCount = 0;
        var possibilities = new ReadOnlyBitSet16(poss1, poss2);

        for (int miniCol = 0; miniCol < 3; miniCol++)
        {
            if (miniCol == Base1.Column / 3) continue;

            for (int c = 0; c < 3; c++)
            {
                int col = miniCol * 3 + c;
                
                if (strategyUser.Contains(EscapeCell.Row, col, poss1) || strategyUser.Contains(EscapeCell.Row, col, poss2) ||
                    strategyUser.Contains(Target1.Row, col, poss1) || strategyUser.Contains(Target1.Row, col, poss2) ||
                    strategyUser.Contains(Target2.Row, col, poss1) || strategyUser.Contains(Target2.Row, col, poss2)) continue;

                if (!strategyUser.PossibilitiesAt(Base1.Row, col).ContainsAll(possibilities) ||
                    !strategyUser.PossibilitiesAt(Base2.Row, col).ContainsAll(possibilities)) continue;
                
                urThreatCount++;
                break;
            }
        }

        if (urThreatCount != 2) return true;

        var oneS = SCells[poss1];
        var twoS = SCells[poss2];

        foreach (var diag in SudokuCellUtility.DiagonalMiniGridAssociation(Base1.Row / 3, Base1.Column / 3))
        {
            if (oneS.MiniGridCount(diag.Item1.MiniRow, diag.Item1.MiniColumn) > 0 &&
                twoS.MiniGridCount(diag.Item2.MiniRow, diag.Item2.MiniColumn) > 0) return true;
        }

        return false;
    }
}