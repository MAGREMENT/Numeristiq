using Model.Solver.Positions;

namespace Model.Solver.Possibilities;

public class PossibilitiesSnapshot : IPossibilitiesHolder
{
    private readonly IPossibilities[,] _possibilities = new IPossibilities[9, 9];
    private readonly GridPositions[] _positions = new GridPositions[9];

    public static IPossibilitiesHolder TakeSnapshot(IPossibilitiesHolder holder)
    {
        PossibilitiesSnapshot snapshot = new PossibilitiesSnapshot();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                snapshot._possibilities[row, col] = holder.PossibilitiesAt(row, col).Copy();
            }
        }

        for (int number = 1; number <= 9; number++)
        {
            snapshot._positions[number - 1] = holder.PositionsFor(number).Copy();
        }

        return snapshot;
    }

    public IReadOnlyPossibilities PossibilitiesAt(int row, int col)
    {
        return _possibilities[row, col];
    }
    
    public IReadOnlyLinePositions RowPositionsAt(int row, int number)
    {
        return _positions[number - 1].RowPositions(row);
    }

    public IReadOnlyLinePositions ColumnPositionsAt(int col, int number)
    {
        return _positions[number - 1].ColumnPositions(col);
    }

    public IReadOnlyMiniGridPositions MiniGridPositionsAt(int miniRow, int miniCol, int number)
    {
        return _positions[number - 1].MiniGridPositions(miniRow, miniCol);
    }

    public IReadOnlyGridPositions PositionsFor(int number)
    {
        return _positions[number - 1];
    }
}