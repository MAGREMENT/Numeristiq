using System.Collections.Generic;
using Model.Kakuros;
using Model.Utility;

namespace Model.Nonograms;

public interface INonogramSolverData
{
    IReadOnlyNonogram Nonogram { get; }
    bool IsAvailable(int row, int col);
    IEnumerable<LineSpace> EnumerateSpaces(Orientation orientation, int index);
}

public readonly struct LineSpace
{
    public int Start { get; }
    public int End { get; }
    
    public LineSpace(int start, int end)
    {
        Start = start;
        End = end;
    }

    public int GetLength => End - Start + 1;

    public IEnumerable<Cell> EnumerateCells(Orientation orientation, int unit)
    {
        for (int i = Start; i <= End; i++)
        {
            yield return orientation == Orientation.Horizontal
                ? new Cell(unit, i)
                : new Cell(i, unit);
        }
    }
}