using System;
using System.Collections.Generic;
using Model.Sudoku.Solver.Position;
using Model.Sudoku.Solver.Possibility;
using Model.Utility;

namespace Model.Sudoku.Solver.StrategiesUtility.Exocet;

public class JuniorExocet
{
    public Cell Base1 { get; }
    public Cell Base2 { get; }
    public Cell Target1 { get; }
    public Cell Target2 { get; }
    public Possibilities BaseCandidates { get; }
    public Cell EscapeCell { get; }
    
    public Cell[] Target1MirrorNodes { get; }
    
    public Cell[] Target2MirrorNodes { get; }
    
    public Dictionary<int, GridPositions> SCells { get; }

    public JuniorExocet(Cell base1, Cell base2, Cell target1, Cell target2, Possibilities baseCandidates,
        Cell escapeCell, Cell[] target1MirrorNodes, Cell[] target2MirrorNodes, Dictionary<int, GridPositions> sCells)
    {
        Base1 = base1;
        Base2 = base2;
        Target1 = target1;
        Target2 = target2;
        BaseCandidates = baseCandidates;
        EscapeCell = escapeCell;
        Target1MirrorNodes = target1MirrorNodes;
        Target2MirrorNodes = target2MirrorNodes;
        SCells = sCells;
    }

    public Unit GetUnit()
    {
        return Base1.Row == Base2.Row ? Unit.Row : Unit.Column;
    }

    public LinePositions SCellsLinePositions()
    {
        LinePositions result = new LinePositions();
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

    public List<Cell> AllPossibleSCells(){
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

    public bool CompatibilityCheck(IStrategyUser strategyUser, int poss1, int poss2)
    {
        if (!BaseCandidates.Peek(poss1) || !BaseCandidates.Peek(poss2))
            throw new ArgumentException("Possibility not in base candidates");
        
        if (poss1 == poss2) return false;

        return GetUnit() == Unit.Row
            ? RowCompatibilityCheck(strategyUser, poss1, poss2)
            : ColumnCompatibilityCheck(strategyUser, poss1, poss2);
    }

    private bool RowCompatibilityCheck(IStrategyUser strategyUser, int poss1, int poss2)
    {
        int urThreatCount = 0;
        var possibilities = Possibilities.NewEmpty();
        possibilities.Add(poss1);
        possibilities.Add(poss2);

        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            if (miniRow == Base1.Row / 3) continue;

            for (int r = 0; r < 3; r++)
            {
                int row = miniRow * 3 + r;
                
                if (strategyUser.Contains(row, EscapeCell.Column, poss1) || strategyUser.Contains(row, EscapeCell.Column, poss2) ||
                    strategyUser.Contains(row, Target1.Column, poss1) || strategyUser.Contains(row, Target1.Column, poss2) ||
                    strategyUser.Contains(row, Target2.Column, poss1) || strategyUser.Contains(row, Target2.Column, poss2)) continue;

                if (!strategyUser.PossibilitiesAt(row, Base1.Column).PeekAll(possibilities) ||
                    !strategyUser.PossibilitiesAt(row, Base2.Column).PeekAll(possibilities)) continue;
                
                urThreatCount++;
                break;
            }
        }

        if (urThreatCount != 2) return true;

        var oneS = SCells[poss1];
        var twoS = SCells[poss2];

        foreach (var diag in Cells.DiagonalMiniGridAssociation(Base1.Row / 3, Base1.Column / 3))
        {
            if (oneS.MiniGridCount(diag.Item1.MiniRow, diag.Item1.MiniColumn) > 0 &&
                twoS.MiniGridCount(diag.Item2.MiniRow, diag.Item2.MiniColumn) > 0) return true;
        }

        return false;
    }
    
    private bool ColumnCompatibilityCheck(IStrategyUser strategyUser, int poss1, int poss2)
    {
        int urThreatCount = 0;
        var possibilities = Possibilities.NewEmpty();
        possibilities.Add(poss1);
        possibilities.Add(poss2);

        for (int miniCol = 0; miniCol < 3; miniCol++)
        {
            if (miniCol == Base1.Column / 3) continue;

            for (int c = 0; c < 3; c++)
            {
                int col = miniCol * 3 + c;
                
                if (strategyUser.Contains(EscapeCell.Row, col, poss1) || strategyUser.Contains(EscapeCell.Row, col, poss2) ||
                    strategyUser.Contains(Target1.Row, col, poss1) || strategyUser.Contains(Target1.Row, col, poss2) ||
                    strategyUser.Contains(Target2.Row, col, poss1) || strategyUser.Contains(Target2.Row, col, poss2)) continue;

                if (!strategyUser.PossibilitiesAt(Base1.Row, col).PeekAll(possibilities) ||
                    !strategyUser.PossibilitiesAt(Base2.Row, col).PeekAll(possibilities)) continue;
                
                urThreatCount++;
                break;
            }
        }

        if (urThreatCount != 2) return true;

        var oneS = SCells[poss1];
        var twoS = SCells[poss2];

        foreach (var diag in Cells.DiagonalMiniGridAssociation(Base1.Row / 3, Base1.Column / 3))
        {
            if (oneS.MiniGridCount(diag.Item1.MiniRow, diag.Item1.MiniColumn) > 0 &&
                twoS.MiniGridCount(diag.Item2.MiniRow, diag.Item2.MiniColumn) > 0) return true;
        }

        return false;
    }

    public Dictionary<int, List<CoverHouse>> ComputeAllCoverHouses()
    {
        var result = new Dictionary<int, List<CoverHouse>>();

        foreach (var possibility in BaseCandidates)
        {
            result.Add(possibility, ComputeCoverHouses(possibility));
        }

        return result;
    }

    public List<CoverHouse> ComputeCoverHouses(int possibility)
    {
        if (!BaseCandidates.Peek(possibility)) return new List<CoverHouse>();

        return SCells[possibility].BestCoverHouses(MethodsInPriorityOrder());
    }

    private IUnitMethods[] MethodsInPriorityOrder()
    {
        return GetUnit() == Unit.Row
            ? new IUnitMethods[] { new RowMethods(), new ColumnMethods() } 
            : new IUnitMethods[] { new ColumnMethods(), new RowMethods() };
    }
}