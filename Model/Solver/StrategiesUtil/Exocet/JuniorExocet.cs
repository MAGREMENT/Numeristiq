using System;
using System.Collections.Generic;
using Model.Solver.Position;
using Model.Solver.Possibility;

namespace Model.Solver.StrategiesUtil.Exocet;

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

    public List<Cell> GetSCells(){
        List<Cell> sCells = new();
        if (GetUnit() == Unit.Row)
        {
            for (int row = 0; row < 9; row++)
            {
                if (row / 3 == Base1.Row / 3) continue;
                
                sCells.Add(new Cell(row, Target1.Col));
                sCells.Add(new Cell(row, Target2.Col));
                sCells.Add(new Cell(row, EscapeCell.Col));
            }
        }
        else
        {
            for (int col = 0; col < 9; col++)
            {
                if (col / 3 == Base1.Col / 3) continue;
                
                sCells.Add(new Cell(Target1.Row, col));
                sCells.Add(new Cell(Target2.Row, col));
                sCells.Add(new Cell(EscapeCell.Row, col));
            }
        }

        return sCells;
    }

    public bool CompatibilityCheck(IStrategyManager strategyManager, int poss1, int poss2)
    {
        if (!BaseCandidates.Peek(poss1) || !BaseCandidates.Peek(poss2))
            throw new ArgumentException("Possibility not in base candidates");
        
        if (poss1 == poss2) return false;

        return GetUnit() == Unit.Row
            ? RowCompatibilityCheck(strategyManager, poss1, poss2)
            : ColumnCompatibilityCheck(strategyManager, poss1, poss2);
    }

    private bool RowCompatibilityCheck(IStrategyManager strategyManager, int poss1, int poss2)
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
                
                if (strategyManager.Contains(row, EscapeCell.Col, poss1) || strategyManager.Contains(row, EscapeCell.Col, poss2) ||
                    strategyManager.Contains(row, Target1.Col, poss1) || strategyManager.Contains(row, Target1.Col, poss2) ||
                    strategyManager.Contains(row, Target2.Col, poss1) || strategyManager.Contains(row, Target2.Col, poss2)) continue;

                if (!strategyManager.PossibilitiesAt(row, Base1.Col).PeekAll(possibilities) ||
                    !strategyManager.PossibilitiesAt(row, Base2.Col).PeekAll(possibilities)) continue;
                
                urThreatCount++;
                break;
            }
        }

        if (urThreatCount != 2) return true;

        var oneS = SCells[poss1];
        var twoS = SCells[poss2];

        foreach (var diag in Cells.DiagonalMiniGridAssociation(Base1.Row / 3, Base1.Col / 3))
        {
            if (oneS.MiniGridCount(diag.Key[0], diag.Key[1]) > 0 &&
                twoS.MiniGridCount(diag.Value[0], diag.Value[1]) > 0) return true;
        }

        return false;
    }
    
    private bool ColumnCompatibilityCheck(IStrategyManager strategyManager, int poss1, int poss2)
    {
        //TODO
        return true;
    }
}