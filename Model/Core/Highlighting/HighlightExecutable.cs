using System;
using System.Collections.Generic;
using System.Text;
using Model.Core.Changes;
using Model.Core.Graphs;
using Model.Sudokus;
using Model.Sudokus.Solver.PossibilitySets;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.AlmostLockedSets;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;

namespace Model.Core.Highlighting;

/// <summary>
/// This class is made so not too many model class instances are kept in memory just for the logs.
/// I have no idea if this is needed or useful but it has been on my mind for a long time and i wanna do it
/// </summary>
public class HighlightExecutable : IHighlightable<ISudokuHighlighter>
{
    //12-16 = col   16-20 = row   20-24 = possibility   24-28 = type   28-32 = color
    private const int HighlightPossibilityIndex = 0;
    //12-16 = col   16-20 = row   24-28 = type   28-32 = color
    private const int HighlightCellIndex = 1;
    //12-16 = col   16-20 = row   20-24 = possibility   24-28 = type   28-32 = color
    private const int EncirclePossibilityIndex = 2;
    //12-16 = col   16-20 = row   24-28 = type   28-32 = color
    private const int EncircleCellIndex = 3;
    //0-4 = 1 for pr, 0 for pc   4-8 = row/col   8-12 = possibility   12-24 = cols/rows (+1)   24-28 = type   28-32 = color
    private const int HighlightPointingIndex = 4;
    //12-16 = col   16-20 = row   20-24 = possibility   24-28 = type   28-32 = link
    private const int CreateLinkIndex = 5;
    //0-4 = unit   4-8 = number   24-28 = type   28-32 = color
    private const int HighlightNakedSetIndex = 6;
    //0-10 = possibilities   10-14 = number of cell packets   24-28 = type   28-32 = color
    //cell packets => repeat 4 times (0-4 = row + 1   4-8 = col + 1)   
    private const int EncircleHouseIndex = 7;
    
    private readonly int[] _instructions;

    private HighlightExecutable(int[] instructions)
    {
        _instructions = instructions;
    }

    public static HighlightExecutable FromHighlightable(IHighlightable<ISudokuHighlighter> highlightable)
    {
        var instructions = new Instructionalizer();
        highlightable.Highlight(instructions);
        return new HighlightExecutable(instructions.ToArray());
    }
    
    public static HighlightExecutable FromBase16(string s, IAlphabet alphabet)
    {
        var result = new int[s.Length / 8];
        for (int i = 0; i < s.Length; i += 8)
        {
            if (s.Length - i < 8) break;
            result[i / 8] = FromBase16(s.AsSpan(i, 8), alphabet);
        }

        return new HighlightExecutable(result);
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
                case HighlightPointingIndex :
                    highlighter.HighlightElement(PointingFromInt(instruction), (StepColor)((instruction >> 28) & 0xF));
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
                case EncircleHouseIndex :
                    highlighter.EncircleHouse(new House((Unit)(instruction & 0xF), (instruction >> 4) & 0xF),
                        (StepColor)((instruction >> 28) & 0xF));
                    break;
                case HighlightNakedSetIndex :
                    //TODO
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
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

        public void EncircleHouse(House house, StepColor color)
        {
            Add((int)color << 28 | EncircleHouseIndex << 24 | house.Number << 4 | (int)house.Unit);
        }

        public void HighlightElement(ISudokuElement element, StepColor color)
        {
            switch (element)
            {
                case CellPossibility cp : HighlightPossibility(cp.Possibility, cp.Row, cp.Column, color);
                    break;
                case PointingRow pr :
                    Add(ToInt(pr) | ((int)color << 28));
                    break;
                case PointingColumn pc :
                    Add(ToInt(pc) | ((int)color << 28));
                    break;
                case NakedSet ns :
                    var all = ToInt(ns);
                    all[0] |= (int)color << 28;
                    AddRange(all);
                    break;
            }
        }

        public void CreateLink(ISudokuElement from, ISudokuElement to, LinkStrength linkStrength)
        {
            //TODO
        }

        public void CreateLink(IPossibilitySet from, IPossibilitySet to, int link)
        {
            //TODO
        }
    }
    
    private static int ToInt(PointingRow pr)
    {
        var result = 1 | HighlightPointingIndex << 24 | pr.Row << 4 | pr.Possibility << 8;
        var start = 12;
        foreach (var col in pr.EveryColumn())
        {
            result |= (col + 1) << start;
            start += 4;
        }

        return result;
    }
        
    private static int ToInt(PointingColumn pc)
    {
        var result = HighlightPointingIndex << 24 | pc.Column << 4 | pc.Possibility << 8;
        var start = 12;
        foreach (var row in pc.EveryRow())
        {
            result |= (row + 1) << start;
            start += 4;
        }

        return result;
    }
        
    private static ISudokuElement PointingFromInt(int n)
    {
        if ((n & 1) == 1)
        {
            List<int> cols = new();
            for (int i = 12; i <= 20; i += 4)
            {
                var col = (n >> i) & 0xF;
                if (col == 0) break;
                cols.Add(col - 1);
            }

            return new PointingRow((n >> 8) & 0xF, (n >> 4) & 0xF, cols);
        }

        List<int> rows = new();
        for (int i = 12; i <= 20; i += 4)
        {
            var row = (n >> i) & 0xF;
            if (row == 0) break;
            rows.Add(row - 1);
        }

        return new PointingColumn((n >> 8) & 0xF, (n >> 4) & 0xF, rows);
    }

    private static List<int> ToInt(NakedSet set)
    {
        List<int> result = new();
        var packets = ToCellPackets(set.EveryCell());
        result.Add(set.EveryPossibilities().Bits | packets.Count << 10 | HighlightNakedSetIndex << 24);
        result.AddRange(packets);
        return result;
    }

    private static List<int> ToCellPackets(IEnumerable<Cell> cells)
    {
        List<int> result = new();
        int current = 0;
        int start = 0;
        foreach (var cell in cells)
        {
            current |= (cell.Row + 1) << start;
            current |= (cell.Column - 1) << (start + 4);

            if (start == 24)
            {
                start = 0;
                result.Add(current);
                current = 0;
            }
            else start += 8;
        }

        return result;
    }
}



