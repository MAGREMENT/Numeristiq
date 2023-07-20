using Model.StrategiesUtil;

namespace Model.Positions;

public class GridPositions
{
    private int _first = 0; //0 to 62
    private int _second = 0; // 63 to 80

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
}