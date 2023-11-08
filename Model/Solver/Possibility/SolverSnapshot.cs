using Model.Solver.Position;

namespace Model.Solver.Possibility;

public class SolverSnapshot : IPossibilitiesHolder
{
    private readonly Sudoku _sudoku;
    private readonly Possibilities[,] _possibilities;
    private readonly GridPositions[] _positions;

    private SolverSnapshot(Sudoku sudoku, Possibilities[,] possibilities, GridPositions[] positions)
    {
        _sudoku = sudoku;
        _positions = positions;
        _possibilities = possibilities;
    }
    
    public static IPossibilitiesHolder TakeSnapshot(IPossibilitiesHolder holder)
    {
        var possibilities = new Possibilities[9, 9];
        var positions = new GridPositions[9];
        
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                possibilities[row, col] = holder.PossibilitiesAt(row, col).Copy();
            }
        }

        for (int number = 1; number <= 9; number++)
        {
            positions[number - 1] = holder.PositionsFor(number).Copy();
        }

        return new SolverSnapshot(holder.Sudoku.Copy(), possibilities, positions);
    }

    public IReadOnlySudoku Sudoku => _sudoku;

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