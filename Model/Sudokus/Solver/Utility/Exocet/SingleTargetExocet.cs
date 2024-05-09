using System.Collections.Generic;
using Model.Sudokus.Solver.Position;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Utility.Exocet;

public class SingleTargetExocet : Exocet
{
    public Cell Target { get; }
    public House Cross { get; }
    public Cell EscapeCell { get; }
    public int WildCard { get; }
    
    public SingleTargetExocet(Cell base1, Cell base2, Cell target, House cross, int wildCard, Cell escapeCell,
        ReadOnlyBitSet16 baseCandidates, Dictionary<int, GridPositions> sCells) : base(base1, base2, baseCandidates, sCells)
    {
        Target = target;
        Cross = cross;
        WildCard = wildCard;
        EscapeCell = escapeCell;
    }

    public IEnumerable<Cell> EnumerateAbsentTargets()
    {
        if (GetUnit() == Unit.Row)
        {
            var startRow = Base1.Row / 3 * 3;
            for (int r = 0; r < 3; r++)
            {
                var row = startRow + r;
                if (row != Base1.Row) yield return new Cell(row, Cross.Number);
            }
        }
        else
        {
            var startCol = Base1.Column / 3 * 3;
            for (int c = 0; c < 3; c++)
            {
                var col = startCol + c;
                if (col != Base1.Column) yield return new Cell(Cross.Number, col);
            }
        }
    }

    public override List<Cell> AllPossibleSCells()
    {
        List<Cell> sCells = new();
        if (GetUnit() == Unit.Row)
        {
            for (int row = 0; row < 9; row++)
            {
                if (row / 3 == Base1.Row / 3) continue;
                
                sCells.Add(new Cell(row, Target.Column));
                sCells.Add(new Cell(row, Cross.Number));
                sCells.Add(new Cell(row, EscapeCell.Column));
            }
        }
        else
        {
            for (int col = 0; col < 9; col++)
            {
                if (col / 3 == Base1.Column / 3) continue;
                
                sCells.Add(new Cell(Target.Row, col));
                sCells.Add(new Cell(Cross.Number, col));
                sCells.Add(new Cell(EscapeCell.Row, col));
            }
        }

        return sCells;
    }
}