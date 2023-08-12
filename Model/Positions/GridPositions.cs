using Model.StrategiesUtil;

namespace Model.Positions;

public class GridPositions
{
    private int _first; //0 to 62
    private int _second; // 63 to 80
    
    public GridPositions() {}

    private GridPositions(int first, int second)
    {
        _first = first;
        _second = second;
    }

    public void Add(int row, int col)
    {
        int n = row * 9 + col;
        if (n > 62) _second |= 1 << n;
        else _first |= 1 << n;
    }

    public void Add(Coordinate coord)
    {
        Add(coord.Row, coord.Col);
    }

    public bool Peek(int row, int col)
    {
        int n = row * 9 + col;
        return n > 62 ? ((_second >> n) & 1) > 0 : ((_first >> n) & 1) > 0;
    }

    public bool Peek(Coordinate coord)
    {
        return Peek(coord.Row, coord.Col);
    }

    public int RowCount(int row)
    {
        int result = 0;
        for (int col = 0; col < 9; col++)
        {
            if (Peek(row, col)) result++;
        }

        return result;
    }

    public int ColumnCount(int column)
    {
        int result = 0;
        for (int row = 0; row < 9; row++)
        {
            if (Peek(row, column)) result++;
        }

        return result;
    }

    public int MiniGridCount(int miniRow, int miniCol)
    {
        int result = 0;
        for (int gridRow = 0; gridRow < 3; gridRow++)
        {
            for (int gridCol = 0; gridCol < 3; gridCol++)
            {
                if (Peek(miniRow * 3 + gridRow, miniCol * 3 + gridCol)) result++;
            }
        }

        return result;
    }

    public GridPositions Copy()
    {
        return new GridPositions(_first, _second);
    }
}