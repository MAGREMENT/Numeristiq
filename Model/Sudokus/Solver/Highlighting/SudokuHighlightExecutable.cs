using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Core.Changes;
using Model.Core.Graphs;
using Model.Core.Highlighting;
using Model.Utility;
using Model.Utility.Collections;

namespace Model.Sudokus.Solver.Highlighting;

/// <summary>
/// This class is made so not too many model class instances are kept in memory just for the logs.
/// I have no idea if this is needed or useful but it has been on my mind for a long time and i wanna do it
/// </summary>
public class SudokuHighlightExecutable : IHighlightable<ISudokuHighlighter>
{
    //12-16 = col   16-20 = row   20-24 = possibility   24-28 = type   28-32 = color
    private const int HighlightPossibilityIndex = 0;
    
    //12-16 = col   16-20 = row   24-28 = type   28-32 = color
    private const int HighlightCellIndex = 1;
    
    //12-16 = col   16-20 = row   24-28 = type   20-24 = possibility
    private const int EncirclePossibilityIndex = 2;
    
    //12-16 = col   16-20 = row   24-28 = type
    private const int EncircleCellIndex = 3;
    
    //0-4 = col   4-8 = row   8-12 = possibility   12-16 = col   16-20 = row   20-24 = possibility   24-28 = type   28-32 = link
    private const int CreateLinkIndex = 5;
    
    //0-8 = number of CellPossibilities packets   24-28 = type  28-32 = color
    //CellPossibility packet => (0-10 = (possibility * 81 + row * 9 + col) x 3
    private const int DelimitPatchIndex = 6;

    //0-4 = col   4-8 = row   12-16 = col   16-20 = row   24-28 = type   28-32 = color
    private const int EncircleCellRectangleIndex = 7;

    //0-4 = col   4-8 = row   8-12 = possibility   12-16 = col   16-20 = row   20-24 = possibility   24-28 = type   28-32 = color
    private const int EncirclePossibilityRectangleIndex = 8;
    
    private readonly int[] _instructions;

    private SudokuHighlightExecutable(int[] instructions)
    {
        _instructions = instructions;
    }

    public static SudokuHighlightExecutable FromHighlightable(IHighlightable<ISudokuHighlighter> highlightable)
    {
        var instructions = new Instructionalizer();
        highlightable.Highlight(instructions);
        return new SudokuHighlightExecutable(instructions.ToArray());
    }
    
    public static SudokuHighlightExecutable FromBase16(string s, IAlphabet alphabet)
    {
        var result = new int[s.Length / 8];
        for (int i = 0; i < s.Length; i += 8)
        {
            if (s.Length - i < 8) break;
            result[i / 8] = FromBase16(s.AsSpan(i, 8), alphabet);
        }

        return new SudokuHighlightExecutable(result);
    }

    private static int FromBase16(ReadOnlySpan<char> s, IAlphabet alphabet)
    {
        return alphabet.ToInt(s[0]) << 28 |
               alphabet.ToInt(s[1]) << 24 |
               alphabet.ToInt(s[2]) << 20 |
               alphabet.ToInt(s[3]) << 16 |
               alphabet.ToInt(s[4]) << 12 |
               alphabet.ToInt(s[5]) << 8 |
               alphabet.ToInt(s[6]) << 4 |
               alphabet.ToInt(s[7]);
    }
    
    public string ToBase16(IAlphabet alphabet)
    {
        var builder = new StringBuilder();
        foreach (var instruction in _instructions)
        {
            builder.Append(new string(new[]
            {
                alphabet.ToChar((instruction >> 28) & 15),
                alphabet.ToChar((instruction >> 24) & 15),
                alphabet.ToChar((instruction >> 20) & 15),
                alphabet.ToChar((instruction >> 16) & 15),
                alphabet.ToChar((instruction >> 12) & 15),
                alphabet.ToChar((instruction >> 8) & 15),
                alphabet.ToChar((instruction >> 4) & 15),
                alphabet.ToChar(instruction & 15)
            }));
        }

        return builder.ToString();
    }

    public void Highlight(ISudokuHighlighter highlighter)
    {
        for (int i = 0; i < _instructions.Length; i++)
        {
            var instruction = _instructions[i];
            switch ((instruction >> 24) & 0xF)
            {
                case HighlightPossibilityIndex :
                    highlighter.HighlightPossibility((instruction >> 20) & 0xF, (instruction >> 16) & 0xF,
                        (instruction >> 12) & 0xF, (StepColor)((instruction >> 28) & 0xF));
                    break;
                case HighlightCellIndex :
                    highlighter.HighlightCell((instruction >> 16) & 0xF,
                        (instruction >> 12) & 0xF, (StepColor)((instruction >> 28) & 0xF));
                    break;
                case EncirclePossibilityIndex :
                    highlighter.EncirclePossibility((instruction >> 20) & 0xF, (instruction >> 16) & 0xF,
                        (instruction >> 12) & 0xF);
                    break;
                case EncircleCellIndex :
                    highlighter.EncircleCell((instruction >> 16) & 0xF,
                        (instruction >> 12) & 0xF);
                    break;
                case CreateLinkIndex :
                    highlighter.CreateLink(new CellPossibility((instruction >> 16) & 0xF,
                        (instruction >> 12) & 0xF, (instruction >> 20) & 0xF), new CellPossibility((instruction >> 4) & 0xF,
                        instruction & 0xF, (instruction >> 8) & 0xF), (LinkStrength)((instruction >> 28) & 0xF));
                    break;
                case DelimitPatchIndex :
                    var count = instruction & 0xFF;
                    i++;
                    highlighter.DelimitPossibilityPatch(
                        FromCellPossibilityPackets(_instructions, ref i, count)
                            .ToArray(), (StepColor)((instruction >> 28) & 0xF));
                    break;
                case EncircleCellRectangleIndex :
                    highlighter.EncircleRectangle(new Cell((instruction >> 4) & 0xF, instruction & 0xF),
                        new Cell((instruction >> 16) & 0xF, (instruction >> 12) & 0xF), 
                        (StepColor)((instruction >> 28) & 0xF));
                    
                    break;
                case EncirclePossibilityRectangleIndex :
                    highlighter.EncircleRectangle(new CellPossibility( (instruction >> 4) & 0xF, instruction & 0xF, (instruction >> 8) & 0xF),
                        new CellPossibility((instruction >> 16) & 0xF, (instruction >> 12) & 0xF, (instruction >> 20) & 0xF), 
                        (StepColor)((instruction >> 28) & 0xF));
                    break;
            }
        }
    }

    public override bool Equals(object? obj)
    {
        return obj is SudokuHighlightExecutable s && s._instructions.SequenceEqual(_instructions);
    }

    public override int GetHashCode()
    {
        return _instructions.GetSequenceHashCode();
    }

    private class Instructionalizer : List<int>, ISudokuHighlighter
    {
        public void HighlightPossibility(int possibility, int row, int col, StepColor color)
        {
            Add((int)color << 28 | possibility << 20 | row << 16 | col << 12);
        }

        public void HighlightCell(int row, int col, StepColor color)
        {
            Add((int)color << 28 | HighlightCellIndex << 24 | row << 16 | col << 12);
        }

        public void CreateLink(CellPossibility from, CellPossibility to, LinkStrength linkStrength)
        {
            Add((int)linkStrength << 28 | CreateLinkIndex << 24 | from.Possibility << 20 | from.Row << 16
                | from.Column << 12 | to.Possibility << 8 | to.Row << 4 | to.Column);
        }

        public void EncirclePossibility(int possibility, int row, int col)
        {
            Add(EncirclePossibilityIndex << 24 | possibility << 20 | row << 16 | col << 12);
        }

        public void EncircleCell(int row, int col)
        {
            Add(EncircleCellIndex << 24 | row << 16 | col << 12);
        }

        public void EncircleRectangle(CellPossibility topLeft, CellPossibility bottomRight, StepColor color)
        {
            Add(EncirclePossibilityRectangleIndex << 24 | bottomRight.Possibility << 20 | bottomRight.Row << 16 
                | bottomRight.Column << 12 | topLeft.Possibility << 8 | topLeft.Row << 4 | topLeft.Column | (int)color << 28);
        }

        public void EncircleRectangle(Cell topLeft, Cell bottomRight, StepColor color)
        {
            Add(EncircleCellRectangleIndex << 24 | bottomRight.Row << 16 
                | bottomRight.Column << 12 | topLeft.Row << 4 | topLeft.Column | (int)color << 28);
        }

        public void DelimitPossibilityPatch(CellPossibility[] cps, StepColor color)
        {
            Add(DelimitPatchIndex << 24 | (int)color << 28 | cps.Length);
            AddRange(ToCellPossibilityPackets(cps));
        }
    }

    private static List<int> ToCellPossibilityPackets(CellPossibility[] poss)
    {
        var result = new List<int>();
        
        var curr = 0;
        var shift = 0;

        foreach (var p in poss)
        {
            curr |= (p.Possibility * 81 + p.Row * 9 + p.Column) << shift;
            shift += 10;
            
            if (shift == 30)
            {
                result.Add(curr);
                shift = 0;
                curr = 0;
            }
        }

        if (curr != 0) result.Add(curr);

        return result;
    }

    private static CellPossibility[] FromCellPossibilityPackets(IReadOnlyList<int> instructions, ref int curr, int count)
    {
        var result = new CellPossibility[count];
        var shift = 0;

        for (int i = 0; i < count; i++)
        {
            if (shift == 30)
            {
                shift = 0;
                curr++;
            }
            
            var n = (instructions[curr] >> shift) & 0x3FF;

            var p = n / 81;
            n %= 81;
            var r = n / 9;
            var c = n % 9;
            result[i] = new CellPossibility(r, c, p);
            
            shift += 10;
        }
        
        return result;
    }
}



