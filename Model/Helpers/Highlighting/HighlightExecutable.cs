using System;
using System.Collections.Generic;
using Model.Helpers.Changes;
using Model.Sudoku;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Sudoku.Solver.StrategiesUtility.Graphs;

namespace Model.Helpers.Highlighting;

/// <summary>
/// This class is made so not too many model class instances are kept in memory just for the logs.
/// I have no idea if this is needed or useful but it has been on my mind for a long time and i wanna do it
/// </summary>
public class HighlightExecutable : IHighlightable
{
    private readonly HighlightInstruction[] _instructions;
    private readonly ISudokuElement[] _register;

    public HighlightExecutable(HighlightInstruction[] instructions, ISudokuElement[] register)
    {
        _instructions = instructions;
        _register = register;
    }

    public void Highlight(IHighlighter highlighter)
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
        ChangeColoration coloration = ChangeColoration.None)
    {
        _bits = (int)coloration << 28 | (int)type << 24 | possibility << 20 | row << 16 | col << 12;
    }
    
    public HighlightInstruction(InstructionType type, int row, int col,
        ChangeColoration coloration = ChangeColoration.None)
    {
        _bits = (int)coloration << 28 | (int)type << 24 | row << 16 | col << 12;
    }
    
    public HighlightInstruction(InstructionType type, int register, ChangeColoration coloration = ChangeColoration.None)
    {
        _bits = (int)coloration << 28 | (int)type << 24 | register << 12;
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
    
    public HighlightInstruction(InstructionType type, int possibility1, int row1, int col1, int possibility2, int row2,
        int col2, ChangeColoration coloration)
    {
        _bits = (int)coloration << 28 | (int)type << 24 | possibility1 << 20 | row1 << 16
                | col1 << 12 | possibility2 << 8 | row2 << 4 | col2;
    }

    public HighlightInstruction(InstructionType type, Unit unit, int number, ChangeColoration coloration)
    {
        _bits = (int)coloration << 28 | (int)type << 24 | number << 4 | (int)unit;
    }
    
    public void Apply(IHighlighter highlighter, ISudokuElement[] registers)
    {
        switch ((InstructionType)((_bits >> 24) & 0xF))
        {
            case InstructionType.HighlightPossibility :
                highlighter.HighlightPossibility((_bits >> 20) & 0xF, (_bits >> 16) & 0xF,
                    (_bits >> 12) & 0xF, (ChangeColoration)((_bits >> 28) & 0xF));
                break;
            case InstructionType.HighlightCell :
                highlighter.HighlightCell((_bits >> 16) & 0xF,
                    (_bits >> 12) & 0xF, (ChangeColoration)((_bits >> 28) & 0xF));
                break;
            case InstructionType.CirclePossibility :
                highlighter.EncirclePossibility((_bits >> 20) & 0xF, (_bits >> 16) & 0xF,
                    (_bits >> 12) & 0xF);
                break;
            case InstructionType.CircleCell :
                highlighter.EncircleCell((_bits >> 16) & 0xF,
                    (_bits >> 12) & 0xF);
                break;
            case InstructionType.HighlightLinkGraphElement :
                highlighter.HighlightLinkGraphElement(registers[(_bits >> 12) & 0xFFF],
                    (ChangeColoration)((_bits >> 28) & 0xF));
                break;
            case InstructionType.CreateSimpleLink :
                highlighter.CreateLink(new CellPossibility((_bits >> 16) & 0xF,
                    (_bits >> 12) & 0xF, (_bits >> 20) & 0xF), new CellPossibility((_bits >> 4) & 0xF,
                    _bits & 0xF, (_bits >> 8) & 0xF), (LinkStrength)((_bits >> 28) & 0xF));
                break;
            case InstructionType.CreateGroupLink :
                highlighter.CreateLink(registers[(_bits >> 12) & 0xFFF], registers[_bits & 0xFFF],
                    (LinkStrength)((_bits >> 28) & 0xF));
                break;
            case InstructionType.CircleRectangle:
                highlighter.EncircleRectangle(new CellPossibility((_bits >> 16) & 0xF,
                    (_bits >> 12) & 0xF, (_bits >> 20) & 0xF), new CellPossibility((_bits >> 4) & 0xF,
                    _bits & 0xF, (_bits >> 8) & 0xF), (ChangeColoration)((_bits >> 28) & 0xF));
                break;
            case InstructionType.CircleRectangleFromCoverHouse:
                highlighter.EncircleRectangle(new CoverHouse((Unit)(_bits & 0xF), (_bits >> 4) & 0xF),
                    (ChangeColoration)((_bits >> 28) & 0xF));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

public enum InstructionType
{
    HighlightPossibility = 0, HighlightCell = 1, CirclePossibility = 2, CircleCell = 3, 
    HighlightLinkGraphElement = 4, CreateSimpleLink = 5, CreateGroupLink = 6, CircleRectangle = 7,
    CircleRectangleFromCoverHouse = 8
}

public class HighlightCompiler : IHighlighter
{
    private readonly List<HighlightInstruction> _instructions = new();
    private readonly List<ISudokuElement> _registers = new();

    public bool Enabled { get; set; } = true;

    private static HighlightCompiler? _instance;

    public static HighlightCompiler GetInstance()
    {
        _instance ??= new HighlightCompiler();
        return _instance;
    }
    
    private HighlightCompiler() {}
    
    public IHighlightable Compile(Highlight d)
    {
        if (!Enabled) return new DelegateHighlightable(d);
        
        d(this);
        var result = new HighlightExecutable(_instructions.ToArray(), _registers.ToArray());
        
        Clear();
        return result;
    }
    
    public void HighlightPossibility(int possibility, int row, int col, ChangeColoration coloration)
    {
        _instructions.Add(new HighlightInstruction(InstructionType.HighlightPossibility, possibility, row, col, coloration));
    }

    public void EncirclePossibility(int possibility, int row, int col)
    {
        _instructions.Add(new HighlightInstruction(InstructionType.CirclePossibility, possibility, row, col));
    }

    public void HighlightCell(int row, int col, ChangeColoration coloration)
    {
        _instructions.Add(new HighlightInstruction(InstructionType.HighlightCell, row, col, coloration));
    }

    public void EncircleCell(int row, int col)
    {
        _instructions.Add(new HighlightInstruction(InstructionType.CircleCell, row, col, ChangeColoration.None));
    }

    public void EncircleRectangle(CellPossibility from, CellPossibility to, ChangeColoration coloration)
    {
        _instructions.Add(new HighlightInstruction(InstructionType.CircleRectangle, from.Possibility, from.Row,
            from.Column, to.Possibility, to.Row, to.Column, coloration));
    }

    public void EncircleRectangle(CoverHouse house, ChangeColoration coloration)
    {
        _instructions.Add(new HighlightInstruction(InstructionType.CircleRectangleFromCoverHouse,
            house.Unit, house.Number, coloration));
    }

    public void HighlightLinkGraphElement(ISudokuElement element, ChangeColoration coloration)
    {
        _registers.Add(element);
        _instructions.Add(new HighlightInstruction(InstructionType.HighlightLinkGraphElement,
            _registers.Count - 1, coloration));
    }

    public void CreateLink(CellPossibility from, CellPossibility to, LinkStrength linkStrength)
    {
        _instructions.Add(new HighlightInstruction(InstructionType.CreateSimpleLink,
            from.Possibility, from.Row, from.Column, to.Possibility, to.Row, to.Column, linkStrength));
    }

    public void CreateLink(ISudokuElement from, ISudokuElement to, LinkStrength linkStrength)
    {
        _registers.Add(from);
        _registers.Add(to);
        _instructions.Add(new HighlightInstruction(InstructionType.CreateGroupLink,
            _registers.Count - 2, _registers.Count - 1, linkStrength));
    }

    private void Clear()
    {
        _instructions.Clear();
        _registers.Clear();
    }
}

