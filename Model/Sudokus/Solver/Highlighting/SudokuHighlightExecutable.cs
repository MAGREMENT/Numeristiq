using System;
using System.Collections.Generic;
using System.Text;
using Model.Core.Changes;
using Model.Core.Graphs;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.PossibilitySets;
using Model.Sudokus.Solver.Utility;
using Model.Utility;
using Model.Utility.BitSets;

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
    //0-8 = number of CellPossibilities packets   24-28 = type   28-32 = color
    //CellPossibilities packet => (0-7 = (row * 9 + col + 1)   7-16 = possibilities) x 2
    private const int EncircleHouseIndex = 7;
    //0-8 = size of from   8-16 = size of to   24-28 = type   28-32 = link
    private const int CreateElementLinkIndex = 8;
    //0-8 = size of from   8-16 = size of to   24-28 = type   28-32 = link
    private const int CreatePossibilitySetLinkIndex = 9;
    //0-4 = possibility   4-12 = number of Cell packets   24-28 = type   28-32 = color
    //Cell packets => (0-4 = row   4-8 = col) x 4
    private const int HighlightCellsPossibilityIndex = 10;
    
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
                    var count = instruction & 0xFF;
                    highlighter.HighlightElement(
                        new ArrayPossibilitySet(FromCellPossibilitiesPackets(_instructions, i + 1, i + count)
                            .ToArray()), (StepColor)((instruction >> 28) & 0xF));

                    i += count;
                    break;
                case CreateElementLinkIndex :
                    var c1 = instruction & 0xFF;
                    var c2 = (instruction >> 8) & 0xFF;

                    highlighter.CreateLink(FromInstruction(_instructions, i + 1),
                        FromInstruction(_instructions, i + c1 + 1), (LinkStrength)((instruction >> 28) & 0xF));
                    i += c1 + c2;
                    break;
                case CreatePossibilitySetLinkIndex :
                    var c3 = instruction & 0xFF;
                    var c4 = (instruction >> 8) & 0xFF;

                    highlighter.CreateLink(
                        new ArrayPossibilitySet(FromCellPossibilitiesPackets(_instructions, i + 2, i + c3).ToArray()),
                        new ArrayPossibilitySet(FromCellPossibilitiesPackets(_instructions, i + 2 + c3, i + c3 + c4).ToArray()), 
                        (instruction >> 28) & 0xF);
                    i += c3 + c4;
                    break;
                case HighlightCellsPossibilityIndex :
                    var count2 = (instruction >> 4) & 0xFF;
                    highlighter.HighlightElement(new CellsPossibility(instruction & 0xF,
                        FromCellPackets(_instructions, i + 1, i + count2).ToArray()),
                        (StepColor)((instruction >> 28) & 0xF));
                    
                    i += count2;
                    break;
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
            var all = ToInstruction(element);
            if (all.Count == 0) return;
            
            all[0] |= (int)color << 28;
            AddRange(all);
        }

        public void CreateLink(ISudokuElement from, ISudokuElement to, LinkStrength linkStrength)
        {
            var one = ToInstruction(from);
            var two = ToInstruction(to);
            
            Add(one.Count | two.Count << 8 | CreateElementLinkIndex << 24 | (int)linkStrength << 28);
            AddRange(one);
            AddRange(two);
        }

        public void CreateLink(IPossibilitySet from, IPossibilitySet to, int link)
        {
            var one = ToInt(from.EveryCellPossibilities());
            var two = ToInt(to.EveryCellPossibilities());
            
            Add(one.Count | two.Count << 8 | CreatePossibilitySetLinkIndex << 24 | link << 28);
            AddRange(one);
            AddRange(two);
        }
    }

    private static List<int> ToInstruction(ISudokuElement element)
    {
        switch (element)
        {
            case CellPossibility cp :
                return new List<int> { cp.Possibility << 20 | cp.Row << 16 | cp.Column << 12 };
            case PointingRow pr :
                return new List<int> { ToInt(pr) };
            case PointingColumn pc :
                return new List<int> { ToInt(pc) };
            case IPossibilitySet ns :
                return ToInt(ns.EveryCellPossibilities());
            case CellsPossibility csp :
                return ToInt(csp);
            default: return new List<int>();
        }
    }

    private static ISudokuElement FromInstruction(IReadOnlyList<int> instruction, int from)
    {
        switch ((instruction[from] >> 24) & 0xF)
        {
            case HighlightPointingIndex :
                return PointingFromInt(instruction[from]);
            case HighlightPossibilityIndex:
                return new CellPossibility((instruction[from] >> 16) & 0xF,
                    (instruction[from] >> 12) & 0xF, (instruction[from] >> 20) & 0xF);
            case HighlightNakedSetIndex:
                return new ArrayPossibilitySet(FromCellPossibilitiesPackets(instruction, from + 1,
                    from + (instruction[from] & 0xFF)).ToArray());
            case HighlightCellsPossibilityIndex:
                return new CellsPossibility(instruction[from] & 0xF, FromCellPackets(
                    instruction, from + 1, from + ((instruction[from] >> 4) & 0xFF)).ToArray());
            default: throw new ArgumentOutOfRangeException();
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

    private static List<int> ToInt(CellsPossibility cp)
    {
        var packets = ToCellPackets(cp.Cells);
        packets.Insert(0, cp.Possibility | packets.Count << 4 | HighlightCellsPossibilityIndex << 24);
        return packets;
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

    private static List<int> ToInt(IEnumerable<CellPossibilities> cps)
    {
        List<int> result = new();
        var packets = ToCellPossibilitiesPackets(cps);
        result.Add(packets.Count | HighlightNakedSetIndex << 24);
        result.AddRange(packets);
        return result;
    }

    private static List<int> ToCellPossibilitiesPackets(IEnumerable<CellPossibilities> cells)
    {
        List<int> result = new();
        int current = 0;
        int start = 0;
        foreach (var cell in cells)
        {
            current |= (cell.Cell.Row * 9 + cell.Cell.Column + 1) << start;
            current |= cell.Possibilities.Bits << (start + 6);

            if (start == 16)
            {
                start = 0;
                result.Add(current);
                current = 0;
            }
            else start += 16;
        }

        if (start == 16) result.Add(current);

        return result;
    }

    private static List<CellPossibilities> FromCellPossibilitiesPackets(IReadOnlyList<int> packets,
        int from, int to)
    {
        List<CellPossibilities> result = new();

        for (int i = from; i <= to; i++)
        {
            int p = packets[i];
            
            var cell = p & 0x7F;
            if (cell == 0) break;

            cell -= 1;
            result.Add(new CellPossibilities(new Cell(cell / 9, cell % 9), 
                ReadOnlyBitSet16.FromBits((ushort)(((p >> 7) & 0x1FF) << 1))));
            
            cell = (p >> 16) & 0x7F;
            if (cell == 0) break;

            cell -= 1;
            result.Add(new CellPossibilities(new Cell(cell / 9, cell % 9), 
                ReadOnlyBitSet16.FromBits((ushort)(((p >> 23) & 0x1FF) << 1))));
        }
        
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
            current |= (cell.Column + 1) << (start + 4);

            if (start == 24)
            {
                start = 0;
                result.Add(current);
                current = 0;
            }
            else start += 8;
        }

        if (start != 0) result.Add(current);

        return result;
    }

    private static List<Cell> FromCellPackets(IReadOnlyList<int> packets,
        int from, int to)
    {
        List<Cell> result = new();
        for (int i = from; i <= to; i++)
        {
            int p = packets[i];
            
            bool stop = false;
            for (int start = 0; start < 32; start += 8)
            {
                var row = (p >> start) & 0xF;
                var col = (p >> (start + 4)) & 0xF;
                if (row == 0 || col == 0)
                {
                    stop = true;
                    break;
                }
                
                result.Add(new Cell(row - 1, col - 1));
            }

            if (stop) break;
        }

        return result;
    }
}



