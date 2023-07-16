using System.Collections;
using System.Collections.Generic;

namespace Model.Positions;

public class MiniGridPositions : IEnumerable<int[]>
{
    private readonly int _startRow;
    private readonly int _startCol;

    private int _pos;
    
    public int Count { get; private set; }

    public MiniGridPositions(int miniRow, int miniCol)
    {
        _startRow = miniRow * 3;
        _startCol = miniCol * 3;
    }

    private MiniGridPositions(int pos, int count, int startRow, int startCol)
    {
        _pos = pos;
        Count = count;
        _startRow = startRow;
        _startCol = startCol;
    }

    public void Add(int gridRow, int gridCol)
    {
        if (!PeekFromGridPositions(gridRow, gridCol)) Count++;
        _pos |= 1 << (gridRow * 3 + gridCol);
    }

    public void Add(int gridNumber)
    {
        _pos |= 1 << gridNumber;
    }

    public bool PeekFromGridPositions(int gridRow, int gridCol)
    {
        return ((_pos >> (gridRow * 3 + gridCol)) & 1) > 0;
    }

    public bool PeekFromGridPositions(int gridNumber)
    {
        return ((_pos >> gridNumber) & 1) > 0;
    }

    public bool AreAllInSameRow()
    {
        //000 000 111
        //000 111 000
        //111 000 000
        return Count < 4 && (_pos ^ 0x7) == 0 && (_pos ^ 0x38) == 0 && (_pos ^ 0x1C0) == 0;
    }

    public bool AreAllInSameColumn()
    {
        //001 001 001
        //010 010 010
        //100 100 100
        return Count < 4 && (_pos ^ 0x9) == 0 && (_pos ^ 0x12) == 0 && (_pos ^ 0x24) == 0;
    }

    public IEnumerator<int[]> GetEnumerator()
    {
        for (int i = 0; i < 9; i++)
        {
            if(((_pos >> i) & 1) > 0) yield return new[] {_startRow + i / 3, _startCol + i % 3};
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public MiniGridPositions Copy()
    {
        return new MiniGridPositions(_pos, Count, _startRow, _startCol);
    }
}