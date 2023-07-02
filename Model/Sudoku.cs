using System.Text.RegularExpressions;

namespace Model;

public class Sudoku
{
    public const int GridSize = 9;
    public const int MiniGridSize = 3;

    private readonly SudokuCell[,] _grid = new SudokuCell[GridSize, GridSize];

    public Sudoku()
    {
        for (int i = 0; i < GridSize; i++)
        {
            for (int j = 0; j < GridSize; j++)
            {
                _grid[i, j] = new SudokuCell();
            }
        }
    }

    /**
     * [1-9] = Digit
     * x[1-9] = Fixed digit
     * s[1-9]+s = Number of void cells
     */
    public Sudoku(string asString, bool allFixed = false)
    {
        int row = 0;
        int column = 0;
        bool nextFixed = allFixed;
        bool isCounting = false;
        string buffer = "";

        Regex digits = new Regex("[0-9]");

        foreach (var c in asString)
        {
            switch (c)
            {
                case 'x':
                    nextFixed = true;
                    break;
                case 's' when isCounting:
                {
                    var newPos = FillOfVoid(row, column, int.Parse(buffer));
                    row = newPos[0];
                    column = newPos[1];
                    buffer = "";
                    isCounting = false;
                    break;
                }
                case 's':
                    isCounting = true;
                    break;
                case ' ':
                    _grid[row, column] = new SudokuCell();

                    ProgressInSudoku(ref row, ref column);
                    break;
                default:
                {
                    if (!digits.IsMatch(c.ToString()))
                    {
                        FillOfVoid(row, column, 81 - row * GridSize - column);
                        return;
                    }
                    
                    if (isCounting) buffer += c;
                    else
                    {
                        _grid[row, column] = new SudokuCell(int.Parse(c.ToString()), nextFixed);
                        nextFixed = allFixed;
                    
                        ProgressInSudoku(ref row, ref column);
                    }

                    break;
                }
            }
        }

        FillOfVoid(row, column, 81 - row * GridSize - column);
        
    }

    public string AsString()
    {
        string result = "";
        int voidCount = 0;
        for (int i = 0; i < GridSize; i++)
        {
            for (int j = 0; j < GridSize; j++)
            {
                SudokuCell current = _grid[i, j];
                if (current.Number == 0)
                {
                    voidCount++;
                }
                else
                {
                    if (voidCount != 0)
                    {
                        result += voidCount > 3 ? "s" + voidCount + "s" : Repeat(" ", voidCount);
                        voidCount = 0;
                    }

                    result += current.IsFixed ? "x" + current.Number : current.Number;
                }
            }
        }

        return result;
    }

    private int[] FillOfVoid(int rowStart, int columnStart, int number)
    {
        while (number > 0)
        {
            _grid[rowStart, columnStart] = new SudokuCell();
            
            ProgressInSudoku(ref rowStart, ref columnStart);
            
            number--;
        }

        return new[] { rowStart, columnStart };
    }

    private void ProgressInSudoku(ref int row, ref int col)
    {
        if (++col >= GridSize)
        {
            col = 0;
            row++;
        }
    }

    public int[] GetRow(int row)
    {
        int[] result = new int[GridSize];
        for (int i = 0; i < GridSize; i++)
        {
            result[i] = _grid[row, i].Number;
        }

        return result;
    }
    
    public int[] GetColumn(int column)
    {
        int[] result = new int[GridSize];
        for (int i = 0; i < GridSize; i++)
        {
            result[i] = _grid[i, column].Number;
        }

        return result;
    }

    public int[,] GetMiniGrid(int row, int column)
    {
        int[,] result = new int[MiniGridSize, MiniGridSize];
        int startRow = (row / MiniGridSize) * MiniGridSize;
        int startColumn = (column / MiniGridSize) * MiniGridSize;

        for (int i = 0; i < MiniGridSize; i++)
        {
            for (int j = 0; j < MiniGridSize; j++)
            {
                result[i, j] = _grid[startRow + i, startColumn + j].Number;
            }
        }

        return result;
    }

    public bool IsComplete()
    {
        foreach (var cell in _grid)
        {
            if (cell.Number == 0) return false;
        }

        return true;
    }

    public bool IsCorrect()
    {
        for (int i = 0; i < GridSize; i++)
        {
            bool[] rowPresence = { false, false, false, false, false, false, false, false, false, false };

            for (int j = 0; j < GridSize; j++)
            {
                var cell = _grid[i, j];
                if (cell.Number == 0) return false;
                if (rowPresence[cell.Number]) return false;
                rowPresence[cell.Number] = true;
            }
            
            bool[] columnPresence = { false, false, false, false, false, false, false, false, false, false };
            
            for (int j = 0; j < GridSize; j++)
            {
                var cell = _grid[j, i];
                if (columnPresence[cell.Number]) return false;
                columnPresence[cell.Number] = true;
            }
        }
        
        //This is disgusting but whatever
        for(int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {

                bool[] presence = { false, false, false, false, false, false, false, false, false, false };
                
                for (int k = 0; k < 3; k++)
                {
                    for (int l = 0; l < 3; l++)
                    {
                        var cell = _grid[i * 3 + k, j * 3 + l];
                        if (presence[cell.Number]) return false;
                        presence[cell.Number] = true;
                    }
                }
            }
        }

        return true;
    }

    public int this[int row, int column]
    {
        get => _grid[row, column].Number;

        set
        {
            if (value < 0 || value > GridSize || _grid[row, column].IsFixed) return;
            _grid[row, column].Number = value;
        }
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
                var num = _grid[i, j].Number;
                result += (num == 0 ? " " : num) + ((j + 1) % 3 == 0 && j != 8 ? "|" : " ");
            }

            result += "\n";
            if ((i + 1) % 3 == 0 && i != 8) result += Repeat("-", 19) + "\n";
        }

        return result;
    }

    private static string Repeat(string s, int number)
    {
        string result = "";
        for (int i = 0; i < number; i++)
        {
            result += s;
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
}


public class SudokuCell
{
    private int _number;

    public int Number
    {
        set
        {
            if (!IsFixed) _number = value;
        }

        get => _number;
    }

    public bool IsFixed { init; get; }

    public SudokuCell()
    {
        _number = 0;
        IsFixed = false;
    }
        
    public SudokuCell(int number)
    {
        _number = number;
        IsFixed = false;
    }
        
    public SudokuCell(int number, bool isFixed){
        _number = number;
        IsFixed = isFixed;
    }
}