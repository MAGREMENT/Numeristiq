using System;
using System.Text;
using Model.Core;
using Model.Sudokus.Solver.States;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus;

public enum SudokuStringFormat
{
    Line, Grid, Base32
}

public enum SudokuLineFormatEmptyCellRepresentation
{
    Shortcuts, Zeros, Points
}

public static class SudokuTranslator
{
    public static string TranslateLineFormat(INumericSolvingState numericSolvingState, SudokuLineFormatEmptyCellRepresentation type)
    {
        string result = "";
        int voidCount = 0;
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                int current = numericSolvingState[i, j];
                if (current == 0)
                {
                    switch (type)
                    {
                        case SudokuLineFormatEmptyCellRepresentation.Shortcuts :
                            voidCount++;
                            break;
                        case SudokuLineFormatEmptyCellRepresentation.Zeros :
                            result += "0";
                            break;
                        case SudokuLineFormatEmptyCellRepresentation.Points :
                            result += ".";
                            break;
                    }
                }
                else
                {
                    if (voidCount != 0)
                    {
                        result += voidCount > 3 ? "s" + voidCount + "s" : StringExtensions.Repeat(" ", voidCount);
                        voidCount = 0;
                    }

                    result += current;
                }
            }
        }

        return result;
    }
    
    public static Sudoku TranslateLineFormat(string asString)
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
    
    public static string TranslateGridFormat(INumericSolvingState numericSolvingState)
    {
        var maxWidth = 0;
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var width = numericSolvingState[row, col] == 0 ? numericSolvingState.PossibilitiesAt(row, col).Count : 3;
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
                    builder.Append(first + StringExtensions.Repeat('-', maxWidth));
                }

                builder.Append("+\n");
            }
            
            for (int col = 0; col < 9; col++)
            {
                var first = col % 3 == 0 ? "|" : " ";
                
                var toPut = numericSolvingState[row, col] == 0
                    ? numericSolvingState.PossibilitiesAt(row, col).ToValuesString()
                    : $"<{numericSolvingState[row, col]}>";
                builder.Append(first + StringExtensions.FillRightWith(toPut, ' ', maxWidth));
            }

            builder.Append("|\n");
        }
        
        for (int i = 0; i < 9; i++)
        {
            var first = i % 3 == 0 ? "+" : "-";
            builder.Append(first + StringExtensions.Repeat('-', maxWidth));
        }

        builder.Append("+\n");

        return builder.ToString();
    }

    public static StateArraySolvingState TranslateGridFormat(string grid, bool soloPossibilityToGiven)
    {
        grid += ' ';
        var result = new StateArraySolvingState();
        
        try
        {
            int i = 0;
            int pos = 0;
            var numberBuffer = -1;
            var isNumber = false;
            ReadOnlyBitSet16? possibilitiesBuffer = null;
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
                        possibilitiesBuffer ??= new ReadOnlyBitSet16();
                        possibilitiesBuffer += asInt;
                    }
                }
                else
                {
                    var row = pos / 9;
                    var col = pos % 9;
                        
                    if (isNumber && numberBuffer != -1)
                    {
                        result.Set(row, col, new CellState(numberBuffer));
                        isNumber = false;
                        numberBuffer = -1;
                        pos++;
                    }
                    else if (possibilitiesBuffer is not null)
                    {
                        result.Set(row, col, soloPossibilityToGiven && possibilitiesBuffer.Value.Count == 1 
                            ? new CellState(possibilitiesBuffer.Value.FirstPossibility()) 
                            : CellState.FromBits(possibilitiesBuffer.Value.Bits));
                        possibilitiesBuffer = null;
                        pos++;
                    }
                }

                i++;
            }
        }
        catch (Exception)
        {
            return result;
        }


        return result;
    }

    public static StateArraySolvingState TranslateBase32Format(string s, IBase32Translator translator)
    {
        var result = new StateArraySolvingState();

        for (int i = 0; i < s.Length / 2 && i < 81; i++)
        {
            var row = i / 9;
            var col = i % 9;

            var bits = translator.ToInt(s[i * 2]) << 5 | translator.ToInt(s[i * 2 + 1]);
            var bitSet = ReadOnlyBitSet16.FromBits((ushort)(bits & ~1));
            
            result.Set(row, col, (bits & 1) > 0 
                ? new CellState(bitSet.FirstPossibility()) 
                : CellState.FromBits(bitSet.Bits));
        }

        return result;
    }

    public static string TranslateBase32Format(INumericSolvingState numericSolvingState, IBase32Translator translator)
    {
        var builder = new StringBuilder();

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var solved = numericSolvingState[row, col];
                int bits = solved == 0 ? numericSolvingState.PossibilitiesAt(row, col).Bits : (1 << solved) | 1;

                builder.Append(translator.ToChar((bits >> 5) & 0x1F));
                builder.Append(translator.ToChar(bits & 0x1F));
            }
        }

        return builder.ToString();
    }
    
    public static Sudoku TranslateSolvingState(INumericSolvingState numericSolvingState)
    {
        Sudoku result = new();

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                result[row, col] = numericSolvingState[row, col];
            }
        }

        return result;
    }

    public static SudokuStringFormat GuessFormat(string s)
    {
        if (s.Contains('\n')) return SudokuStringFormat.Grid;

        return s.Length == 162 ? SudokuStringFormat.Base32 : SudokuStringFormat.Line;
    }
}

public interface IBase32Translator
{
    public int ToInt(char c);
    public char ToChar(int n);
}

public class RFC4648Base32Translator : IBase32Translator
{
    public int ToInt(char c)
    {
        //65-90 == uppercase letters
        if (c < 91 && c > 64)
        {
            return c - 65;
        }
        //50-55 == numbers 2-7
        if (c < 56 && c > 49)
        {
            return c - 24;
        }

        return 0;
    }

    public char ToChar(int n)
    {
        return n switch
        {
            < 26 => (char)(n + 'A'),
            < 32 => (char)(n + 24),
            _ => 'A'
        };
    }
}

public class AlphabeticalBase32Translator : IBase32Translator
{
    public int ToInt(char c)
    {
        return c switch
        {
            >= '0' and <= '9' => c - '0',
            >= 'a' and <= 'w' => c - 'a' + 10,
            _ => '0'
        };
    }

    public char ToChar(int n)
    {
        if (n < 10) return (char)(n + '0');
        return (char)(n - 10 + 'a');
    }
}