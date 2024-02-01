using System;
using System.Text;
using Model.Sudoku.Solver;
using Model.Sudoku.Solver.Possibility;
using Model.Sudoku.Utility;

namespace Model.Sudoku;

public enum SudokuTranslationType
{
    Shortcuts, Zeros, Points
}

public static class SudokuTranslator
{
    public static string TranslateToLine(ITranslatable translatable, SudokuTranslationType type)
    {
        string result = "";
        int voidCount = 0;
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                int current = translatable[i, j];
                if (current == 0)
                {
                    switch (type)
                    {
                        case SudokuTranslationType.Shortcuts :
                            voidCount++;
                            break;
                        case SudokuTranslationType.Zeros :
                            result += "0";
                            break;
                        case SudokuTranslationType.Points :
                            result += ".";
                            break;
                    }
                }
                else
                {
                    if (voidCount != 0)
                    {
                        result += voidCount > 3 ? "s" + voidCount + "s" : StringUtility.Repeat(" ", voidCount);
                        voidCount = 0;
                    }

                    result += current;
                }
            }
        }

        return result;
    }
    
    public static string TranslateToGrid(ITranslatable translatable)
    {
        var maxWidth = 0;
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var width = translatable[row, col] == 0 ? translatable.PossibilitiesAt(row, col).Count : 3;
                maxWidth = Math.Max(width, maxWidth);
            }
        }

        var builder = new StringBuilder();

        for (int row = 0; row < 9; row++)
        {
            if (row % 3 == 0)
            {
                for (int i = 0; i < 9; i++)
                {
                    var first = i % 3 == 0 ? "+" : "-";
                    builder.Append(first + StringUtility.Repeat('-', maxWidth));
                }

                builder.Append("+\n");
            }
            
            for (int col = 0; col < 9; col++)
            {
                var first = col % 3 == 0 ? "|" : " ";
                
                var toPut = translatable[row, col] == 0
                    ? translatable.PossibilitiesAt(row, col).ToSlimString()
                    : $"<{translatable[row, col]}>";
                builder.Append(first + StringUtility.FillWith(toPut, ' ', maxWidth));
            }

            builder.Append("|\n");
        }
        
        for (int i = 0; i < 9; i++)
        {
            var first = i % 3 == 0 ? "+" : "-";
            builder.Append(first + StringUtility.Repeat('-', maxWidth));
        }

        builder.Append("+\n");

        return builder.ToString();
    }
    
    public static Sudoku TranslateToSudoku(string asString)
    {
        Sudoku s = new();
        int n = 0;
        bool isCounting = false;
        string buffer = "";

        try
        {
            foreach (var c in asString)
            {
                switch (c)
                {
                    case 's' when isCounting:
                    {
                        n += int.Parse(buffer);
                        buffer = "";
                        isCounting = false;
                        break;
                    }
                    case 's':
                        isCounting = true;
                        break;
                    case ' ': case '.' :
                        s[n / 9, n % 9] = 0;

                        n++;
                        break;
                    default:
                    {
                        if (isCounting) buffer += c;
                        else
                        {
                            s[n / 9, n % 9] = int.Parse(c.ToString());
                            n++;
                        }

                        break;
                    }
                }
            }
        }
        catch (Exception)
        {
            return new Sudoku();
        }

        return s;
    }

    public static Sudoku TranslateToSudoku(ITranslatable translatable)
    {
        Sudoku result = new();

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                result[row, col] = translatable[row, col];
            }
        }

        return result;
    }

    public static SolverState TranslateToState(string grid, bool soloPossibilityToGiven)
    {
        grid += ' ';
        var cellStates = new CellState[9, 9];
        
        try
        {
            int i = 0;
            int pos = 0;
            var numberBuffer = -1;
            var isNumber = false;
            Possibilities? possibilitiesBuffer = null;
            while (pos < 81 && i < grid.Length)
            {
                var c = grid[i];
                    
                if (c == '<')
                {
                    isNumber = true;
                }
                if (char.IsDigit(c))
                {
                    var asInt = int.Parse(c.ToString());

                    if (isNumber) numberBuffer = asInt;
                    else
                    {
                        possibilitiesBuffer ??= Possibilities.NewEmpty();
                        possibilitiesBuffer.Add(asInt);
                    }
                }
                else
                {
                    var row = pos / 9;
                    var col = pos % 9;
                        
                    if (isNumber && numberBuffer != -1)
                    {
                        cellStates[row, col] = new CellState(numberBuffer);
                        isNumber = false;
                        numberBuffer = -1;
                        pos++;
                    }
                    else if (possibilitiesBuffer is not null)
                    {
                        cellStates[row, col] = soloPossibilityToGiven && possibilitiesBuffer.Count == 1 
                            ? new CellState(possibilitiesBuffer.First()) 
                            : possibilitiesBuffer.ToCellState();
                        possibilitiesBuffer = null;
                        pos++;
                    }
                }

                i++;
            }
        }
        catch (Exception)
        {
            return new SolverState();
        }


        return new SolverState(cellStates);
    }

    public static SudokuStringFormat TryGetFormat(string s)
    {
        return s.Contains('\n') ? SudokuStringFormat.Grid : SudokuStringFormat.Line;
    }
}

public interface ITranslatable
{
    int this[int row, int col] { get; }
    Possibilities PossibilitiesAt(int row, int col);
}

public enum SudokuStringFormat
{
    None, Line, Grid
}