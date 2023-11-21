using Model.Solver.Possibility;
using Model.Utility;

namespace Model;

public class Sudoku : IReadOnlySudoku
{
    private const int GridSize = 9;

    private readonly int[,] _grid = new int[GridSize, GridSize];

    public Sudoku()
    {
    }
    
    public bool IsComplete()
    {
        foreach (var cell in _grid)
        {
            if (cell == 0) return false;
        }

        return true;
    }

    public bool IsCorrect()
    {
        for (int i = 0; i < GridSize; i++)
        {
            int rowPresence = 0;

            for (int j = 0; j < GridSize; j++)
            {
                var cell = _grid[i, j];
                if (cell == 0) return false;
                if (((rowPresence >> cell) & 1) > 0) return false;
                rowPresence |= 1 << cell;
            }

            int colPresence = 0;
            
            for (int j = 0; j < GridSize; j++)
            {
                var cell = _grid[j, i];
                if (((colPresence >> cell) & 1) > 0) return false;
                colPresence |= 1 << cell;
            }
        }
        
        for(int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int miniPresence = 0;
                int startRow = i * 3;
                int startCol = j * 3;
                
                for (int k = 0; k < 3; k++)
                {
                    for (int l = 0; l < 3; l++)
                    {
                        var cell = _grid[startRow + k, startCol + l];
                        if (((miniPresence >> cell) & 1) > 0) return false;
                        miniPresence |= 1 << cell;
                    }
                }
            }
        }

        return true;
    }

    public int this[int row, int column]
    {
        get => _grid[row, column];

        set
        {
            if (value is < 0 or > GridSize) return;
            _grid[row, column] = value;
        }
    }

    public Possibilities PossibilitiesAt(int row, int col)
    {
        return Possibilities.NewEmpty();
    }

    public int RowCount(int row, int number)
    {
        int result = 0;
        for (int col = 0; col < 9; col++)
        {
            if (_grid[row, col] == number) result++;
        }

        return result;
    }

    public int ColumnCount(int col, int number)
    {
        int result = 0;
        for (int row = 0; row < 9; row++)
        {
            if (_grid[row, col] == number) result++;
        }

        return result;
    }

    public int MiniGridCount(int miniRow, int miniCol, int number)
    {
        int result = 0;
        for (int gridRow = 0; gridRow < 3; gridRow++)
        {
            for (int gridCol = 0; gridCol < 3; gridCol++)
            {
                if (_grid[miniRow * 3 + gridRow, miniCol * 3 + gridCol] == number) result++;
            }
        }

        return result;
    }

    public Sudoku Copy()
    {
        Sudoku result = new Sudoku();
        for (int i = 0; i < GridSize; i++)
        {
            for (int j = 0; j < GridSize; j++)
            {
                result[i, j] = this[i, j];
            }
        }

        return result;
    }

    public override string ToString()
    {
        string result = "";
        for (int i = 0; i < GridSize; i++)
        {
            for (int j = 0; j < GridSize; j++)
            {
                var num = _grid[i, j];
                result += (num == 0 ? " " : num) + ((j + 1) % 3 == 0 && j != 8 ? "|" : " ");
            }

            result += "\n";
            if ((i + 1) % 3 == 0 && i != 8) result += StringUtility.Repeat("-", 19) + "\n";
        }

        return result;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Sudoku sud) return false;
        for (int row = 0; row < GridSize; row++)
        {
            for (int col = 0; col < GridSize; col++)
            {
                if (sud[row, col] != this[row, col]) return false;
            }
        }

        return true;
    }

    public override int GetHashCode()
    {
        return _grid.GetHashCode();
    }
}

public enum Unit
{
    Row, Column, MiniGrid
}

public interface IReadOnlySudoku : ITranslatable
{
    public bool IsCorrect();
    public int RowCount(int row, int number);
    public int ColumnCount(int column, int number);
    public int MiniGridCount(int miniRow, int miniCol, int number);
    public Sudoku Copy();
}