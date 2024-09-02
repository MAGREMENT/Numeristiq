using System;
using Model.Core.Changes;
using Model.Core.Graphs;
using Model.Sudokus;
using Model.Sudokus.Solver.PossibilitySets;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;

namespace Model.Core.Highlighting;

/// <summary>
/// This class is made so not too many model class instances are kept in memory just for the logs.
/// I have no idea if this is needed or useful but it has been on my mind for a long time and i wanna do it
/// </summary>
public class SudokuHighlightExecutable : IHighlightable<ISudokuHighlighter>
{
    private readonly HighlightInstruction[] _instructions;
    private readonly object[] _register;

    public SudokuHighlightExecutable(HighlightInstruction[] instructions, object[] register)
    {
        _instructions = instructions;
        _register = register;
    }

    public void Highlight(ISudokuHighlighter highlighter)
    {
        foreach (var instruction in _instructions)
        {
            instruction.Apply(highlighter, _register);
        }
    }
}

public readonly struct HighlightInstruction
{
    private const int HighlightPossibilityIndex = 0;
    private const int HighlightCellIndex = 1;
    private const int EncirclePossibilityIndex = 2;
    private const int EncircleCellIndex = 3;
    private const int HighlightSudokuElementIndex = 4;
    private const int CreateLinkIndex = 5;
    private const int CreateSudokuElementLinkIndex = 6;
    private const int EncircleHouseIndex = 7;
    private const int CreatePossibilitySetLinkIndex = 8;
    
    //0-4 = col           |
    //4-8 = row           | 0-12 = register                 
    //8-12 = possibility  |
    
    //12-16 = col         |
    //16-20 = row         | 12-24 = register                 
    //20-24 = possibility |
    
    //24-28 = type        | 
    //28-32 = coloration  | 28-32 = link
    private readonly int _bits;

    private HighlightInstruction(int bits)
    {
        _bits = bits;
    }
    
    public static HighlightInstruction HighlightPossibility(int possibility, int row, int col,
        StepColor color) => new((int)color << 28 | possibility << 20 | row << 16 | col << 12);
    
    public static HighlightInstruction EncirclePossibility(int possibility, int row, int col) => 
        new(EncirclePossibilityIndex << 24 | possibility << 20 | row << 16 | col << 12);

    public static HighlightInstruction HighlightCell(int row, int col, StepColor color) =>
        new((int)color << 28 | HighlightCellIndex << 24 | row << 16 | col << 12);
    
    public static HighlightInstruction EncircleCell(int row, int col) =>
        new(EncircleCellIndex << 24 | row << 16 | col << 12);

    public static HighlightInstruction HighlightSudokuElement(int register, StepColor color) =>
        new((int)color << 28 | HighlightSudokuElementIndex << 24 | register << 12);

    public static HighlightInstruction CreateSudokuElementLink(int register1, int register2,
        LinkStrength strength) => new((int)strength << 28 | CreateSudokuElementLinkIndex << 24 | register1 << 12 | register2);

    public static HighlightInstruction CreatePossibilitySetLink(int register1, int register2, int n)
        => new(n << 28 | CreatePossibilitySetLinkIndex << 24 | register1 << 12 | register2);

    public static HighlightInstruction CreateLink(int possibility1, int row1, int col1, int possibility2, int row2,
        int col2, LinkStrength strength) => new((int)strength << 28 | CreateLinkIndex << 24 | possibility1 << 20 | row1 << 16
                                                | col1 << 12 | possibility2 << 8 | row2 << 4 | col2);

    public static HighlightInstruction EncircleHouse(Unit unit, int number, StepColor color)
        => new((int)color << 28 | EncircleHouseIndex << 24 | number << 4 | (int)unit);
    
    public void Apply(ISudokuHighlighter highlighter, object[] registers)
    {
        switch ((_bits >> 24) & 0xF)
        {
            case HighlightPossibilityIndex :
                highlighter.HighlightPossibility((_bits >> 20) & 0xF, (_bits >> 16) & 0xF,
                    (_bits >> 12) & 0xF, (StepColor)((_bits >> 28) & 0xF));
                break;
            case HighlightCellIndex :
                highlighter.HighlightCell((_bits >> 16) & 0xF,
                    (_bits >> 12) & 0xF, (StepColor)((_bits >> 28) & 0xF));
                break;
            case EncirclePossibilityIndex :
                highlighter.EncirclePossibility((_bits >> 20) & 0xF, (_bits >> 16) & 0xF,
                    (_bits >> 12) & 0xF);
                break;
            case EncircleCellIndex :
                highlighter.EncircleCell((_bits >> 16) & 0xF,
                    (_bits >> 12) & 0xF);
                break;
            case HighlightSudokuElementIndex :
                highlighter.HighlightElement((ISudokuElement)registers[(_bits >> 12) & 0xFFF],
                    (StepColor)((_bits >> 28) & 0xF));
                break;
            case CreateLinkIndex :
                highlighter.CreateLink(new CellPossibility((_bits >> 16) & 0xF,
                    (_bits >> 12) & 0xF, (_bits >> 20) & 0xF), new CellPossibility((_bits >> 4) & 0xF,
                    _bits & 0xF, (_bits >> 8) & 0xF), (LinkStrength)((_bits >> 28) & 0xF));
                break;
            case CreateSudokuElementLinkIndex :
                highlighter.CreateLink((ISudokuElement)registers[(_bits >> 12) & 0xFFF], (ISudokuElement)registers[_bits & 0xFFF],
                    (LinkStrength)((_bits >> 28) & 0xF));
                break;
            case EncircleHouseIndex :
                highlighter.EncircleHouse(new House((Unit)(_bits & 0xF), (_bits >> 4) & 0xF),
                    (StepColor)((_bits >> 28) & 0xF));
                break;
            case CreatePossibilitySetLinkIndex :
                highlighter.CreateLink((IPossibilitySet)registers[(_bits >> 12) & 0xFFF], (IPossibilitySet)registers[_bits & 0xFFF],
                    (_bits >> 28) & 0xF);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}



