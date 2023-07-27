using System.Linq;
using Model.Positions;

namespace Model;

public class PreComputer
{
    private readonly ISolverView _view;
    
    private readonly LinePositions?[,] _rows = new LinePositions[9, 9];
    private readonly LinePositions?[,] _cols = new LinePositions[9, 9];
    private readonly MiniGridPositions?[,,] _miniGrids = new MiniGridPositions[3, 3, 9];

    public PreComputer(ISolverView view)
    {
        _view = view;
    }

    public void PrecomputePositions()
    {
        for (int number = 1; number <= 9; number++)
        {
            int numberIndex = number - 1;
            for (int i = 0; i < 9; i++)
            {
                _rows[i, numberIndex] = PossibilityPositionsInRow(i, number);
                _cols[i, numberIndex] = PossibilityPositionsInColumn( i, number);
            }
            
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    _miniGrids[i, j, numberIndex] = PossibilityPositionsInMiniGrid( i, j, number);
                }
            }
        }
    }

    public LinePositions PrecomputedPossibilityPositionsInRow(int row, int number)
    {
        return _rows[row, number - 1] ?? PossibilityPositionsInRow(row, number);
    }
    
    public LinePositions PrecomputedPossibilityPositionsInColumn(int col, int number)
    {
        return _cols[col, number - 1] ?? PossibilityPositionsInColumn(col, number);
    }
    
    public MiniGridPositions PrecomputedPossibilityPositionsInMiniGrid(int miniRow, int miniCol, int number)
    {
        return _miniGrids[miniRow, miniCol, number - 1] ??
               PossibilityPositionsInMiniGrid(miniRow, miniCol, number);
    }
    
    public LinePositions PossibilityPositionsInRow(int row, int number)
    {
        LinePositions result = new();
        for (int col = 0; col < 9; col++)
        {
            if (_view.Sudoku[row, col] == number) return new LinePositions();
            if (_view.Possibilities[row, col].Peek(number)) result.Add(col);
        }
        return result;
    }
    
    public LinePositions PossibilityPositionsInColumn(int col, int number)
    {
        LinePositions result = new();
        for (int row = 0; row < 9; row++)
        {
            if (_view.Sudoku[row, col] == number) return new LinePositions();
            if (_view.Possibilities[row, col].Peek(number)) result.Add(row);
        }

        return result;
    }
    
    public MiniGridPositions PossibilityPositionsInMiniGrid(int miniRow, int miniCol, int number)
    {
        MiniGridPositions result = new(miniRow, miniCol);
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                var realRow = miniRow * 3 + i;
                var realCol = miniCol * 3 + j;

                if (_view.Sudoku[realRow, realCol] == number) return new MiniGridPositions(miniRow, miniCol);
                if (_view.Possibilities[realRow, realCol].Peek(number)) result.Add(i, j);
            }
        }

        return result;
    }
}