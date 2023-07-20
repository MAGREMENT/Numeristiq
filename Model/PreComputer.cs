using System.Linq;
using Model.Positions;

namespace Model;

public class PreComputer
{
    private readonly LinePositions[,] _rows = new LinePositions[9, 9];
    private readonly LinePositions[,] _cols = new LinePositions[9, 9];
    private readonly MiniGridPositions[,,] _miniGrids = new MiniGridPositions[3, 3, 9];

    public void PrecomputePositions(ISolverView solverView)
    {
        for (int number = 1; number <= 9; number++)
        {
            int numberIndex = number - 1;
            for (int i = 0; i < 9; i++)
            {
                _rows[i, numberIndex] = PossibilityPositionsInRow(solverView, i, number);
                _cols[i, numberIndex] = PossibilityPositionsInColumn(solverView, i, number);
            }
            
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    _miniGrids[i, j, numberIndex] = PossibilityPositionsInMiniGrid(solverView, i, j, number);
                }
                _rows[i, numberIndex] = PossibilityPositionsInRow(solverView, i, number);
                _cols[i, numberIndex] = PossibilityPositionsInColumn(solverView, i, number);
            }
        }
    }

    public void RemovePosition(int possibility, int row, int col)
    {
        int numberIndex = possibility - 1;
        _rows[row, numberIndex].Remove(possibility);
        _cols[col, numberIndex].Remove(possibility);
        int gridRow = row / 3;
        int gridCol = col / 3;
        _miniGrids[gridRow, gridCol, numberIndex].Remove(gridRow, gridCol);
    }

    public void DeletePosition(int possibility, int row, int col)
    {
        int numberIndex = possibility - 1;
        _rows[row, numberIndex].Void();
        _cols[col, numberIndex].Void();
        _miniGrids[row / 3, col / 3, numberIndex].Void();
    }
    
    public LinePositions PossibilityPositionsInRow(int row, int number)
    {
        return _rows[row, number - 1];
    }
    
    public LinePositions PossibilityPositionsInColumn(int col, int number)
    {
        return _cols[col, number - 1];
    }
    
    public MiniGridPositions PossibilityPositionsInMiniGrid(int miniRow, int miniCol, int number)
    {
        return _miniGrids[miniRow, miniCol, number - 1];
    }
    
    private LinePositions PossibilityPositionsInRow(ISolverView solverView, int row, int number)
    {
        LinePositions result = new();
        for (int col = 0; col < 9; col++)
        {
            if (solverView.Sudoku[row, col] == number) return new LinePositions();
            if (solverView.Possibilities[row, col].Peek(number)) result.Add(col);
        }
        return result;
    }
    
    private LinePositions PossibilityPositionsInColumn(ISolverView solverView, int col, int number)
    {
        LinePositions result = new();
        for (int row = 0; row < 9; row++)
        {
            if (solverView.Sudoku[row, col] == number) return new LinePositions();
            if (solverView.Possibilities[row, col].Peek(number)) result.Add(row);
        }

        return result;
    }
    
    private MiniGridPositions PossibilityPositionsInMiniGrid(ISolverView solverView, int miniRow, int miniCol, int number)
    {
        MiniGridPositions result = new(miniRow, miniCol);
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                var realRow = miniRow * 3 + i;
                var realCol = miniCol * 3 + j;

                if (solverView.Sudoku[realRow, realCol] == number) return new MiniGridPositions(miniRow, miniCol);
                if (solverView.Possibilities[realRow, realCol].Peek(number)) result.Add(i, j);
            }
        }

        return result;
    }
}