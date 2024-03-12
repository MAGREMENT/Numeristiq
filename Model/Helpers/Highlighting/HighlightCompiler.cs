using System;
using System.Collections.Generic;
using Model.Helpers.Changes;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Sudoku.Solver.StrategiesUtility.Graphs;

namespace Model.Helpers.Highlighting;

public static class HighlightCompiler
{
    private static readonly SudokuHighlightCompiler? _sudokuCompiler = new();
    
    public static IHighlightCompiler<THighlighter> For<THighlighter>()
    {
        if (_sudokuCompiler is IHighlightCompiler<THighlighter> compiler)
        {
            return compiler;
        }
        return DefaultCompiler<THighlighter>.Value;
    }
    
    private class DefaultCompilerImplementation<THighlighter> : IHighlightCompiler<THighlighter>
    {
        public IHighlightable<THighlighter> Compile(Highlight<THighlighter> d)
        {
            return new DelegateHighlightable<THighlighter>(d);
        }
    }

    private static class DefaultCompiler<THighlighter>
    { 
        internal static IHighlightCompiler<THighlighter> Value { get; } = new DefaultCompilerImplementation<THighlighter>();
    }
}

public interface IHighlightCompiler<THighlighter>
{
    public IHighlightable<THighlighter> Compile(Highlight<THighlighter> d);
}

public class SudokuHighlightCompiler : IHighlightCompiler<ISudokuHighlighter>, ISudokuHighlighter
{
    private readonly List<HighlightInstruction> _instructions = new();
    private readonly List<ISudokuElement> _registers = new();

    public IHighlightable<ISudokuHighlighter> Compile(Highlight<ISudokuHighlighter> d)
    {
        d(this);
        var result = new SudokuHighlightExecutable(_instructions.ToArray(), _registers.ToArray());
        
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