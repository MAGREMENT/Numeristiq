using System;
using Model.Core.Changes;
using Model.Core.Graphs;
using Model.Sudokus;
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
    private readonly ISudokuElement[] _register;

    public SudokuHighlightExecutable(HighlightInstruction[] instructions, ISudokuElement[] register)
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
    //0-4 = col           |
    //4-8 = row           | 0-12 = register                 
    //8-12 = possibility  |
    
    //12-16 = col         |
    //16-20 = row         | 12-24 = register                 
    //20-24 = possibility |
    
    //24-28 = type
    //28-32 = coloration  | 28-32 = link strength
    private readonly int _bits;

    public HighlightInstruction(InstructionType type, int possibility, int row, int col,
        StepColor color = StepColor.None)
    {
        _bits = (int)color << 28 | (int)type << 24 | possibility << 20 | row << 16 | col << 12;
    }
    
    public HighlightInstruction(InstructionType type, int row, int col,
        StepColor color = StepColor.None)
    {
        _bits = (int)color << 28 | (int)type << 24 | row << 16 | col << 12;
    }
    
    public HighlightInstruction(InstructionType type, int register, StepColor color = StepColor.None)
    {
        _bits = (int)color << 28 | (int)type << 24 | register << 12;
    }
    
    
    public HighlightInstruction(InstructionType type, int register1, int register2,
        LinkStrength strength = LinkStrength.None)
    {
        _bits = (int)strength << 28 | (int)type << 24 | register1 << 12 | register2;
    }
    
    public HighlightInstruction(InstructionType type, int possibility1, int row1, int col1, int possibility2, int row2,
        int col2, LinkStrength strength = LinkStrength.None)
    {
        _bits = (int)strength << 28 | (int)type << 24 | possibility1 << 20 | row1 << 16
                | col1 << 12 | possibility2 << 8 | row2 << 4 | col2;
    }

    public HighlightInstruction(InstructionType type, Unit unit, int number, StepColor color)
    {
        _bits = (int)color << 28 | (int)type << 24 | number << 4 | (int)unit;
    }
    
    public void Apply(ISudokuHighlighter highlighter, ISudokuElement[] registers)
    {
        switch ((InstructionType)((_bits >> 24) & 0xF))
        {
            case InstructionType.HighlightPossibility :
                highlighter.HighlightPossibility((_bits >> 20) & 0xF, (_bits >> 16) & 0xF,
                    (_bits >> 12) & 0xF, (StepColor)((_bits >> 28) & 0xF));
                break;
            case InstructionType.HighlightCell :
                highlighter.HighlightCell((_bits >> 16) & 0xF,
                    (_bits >> 12) & 0xF, (StepColor)((_bits >> 28) & 0xF));
                break;
            case InstructionType.EncirclePossibility :
                highlighter.EncirclePossibility((_bits >> 20) & 0xF, (_bits >> 16) & 0xF,
                    (_bits >> 12) & 0xF);
                break;
            case InstructionType.EncircleCell :
                highlighter.EncircleCell((_bits >> 16) & 0xF,
                    (_bits >> 12) & 0xF);
                break;
            case InstructionType.HighlightSudokuElement :
                highlighter.HighlightElement(registers[(_bits >> 12) & 0xFFF],
                    (StepColor)((_bits >> 28) & 0xF));
                break;
            case InstructionType.CreateLink :
                highlighter.CreateLink(new CellPossibility((_bits >> 16) & 0xF,
                    (_bits >> 12) & 0xF, (_bits >> 20) & 0xF), new CellPossibility((_bits >> 4) & 0xF,
                    _bits & 0xF, (_bits >> 8) & 0xF), (LinkStrength)((_bits >> 28) & 0xF));
                break;
            case InstructionType.CreateSudokuElementLink :
                highlighter.CreateLink(registers[(_bits >> 12) & 0xFFF], registers[_bits & 0xFFF],
                    (LinkStrength)((_bits >> 28) & 0xF));
                break;
            case InstructionType.EncircleHouse:
                highlighter.EncircleHouse(new House((Unit)(_bits & 0xF), (_bits >> 4) & 0xF),
                    (StepColor)((_bits >> 28) & 0xF));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

public enum InstructionType
{
    HighlightPossibility = 0, HighlightCell = 1, EncirclePossibility = 2, EncircleCell = 3, 
    HighlightSudokuElement = 4, CreateLink = 5, CreateSudokuElementLink = 6, EncircleHouse = 7,
}



