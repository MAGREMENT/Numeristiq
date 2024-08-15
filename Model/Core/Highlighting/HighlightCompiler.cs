using System.Collections.Generic;
using Model.Core.Changes;
using Model.Core.Graphs;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;

namespace Model.Core.Highlighting;

public static class HighlightCompiler
{
    private static readonly SudokuHighlightCompiler _sudokuCompiler = new();

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
    
    public void HighlightPossibility(int possibility, int row, int col, StepColor color)
    {
        _instructions.Add(new HighlightInstruction(InstructionType.HighlightPossibility, possibility, row, col, color));
    }

    public void EncirclePossibility(int possibility, int row, int col)
    {
        _instructions.Add(new HighlightInstruction(InstructionType.EncirclePossibility, possibility, row, col));
    }

    public void HighlightCell(int row, int col, StepColor color)
    {
        _instructions.Add(new HighlightInstruction(InstructionType.HighlightCell, row, col, color));
    }

    public void EncircleCell(int row, int col)
    {
        _instructions.Add(new HighlightInstruction(InstructionType.EncircleCell, row, col, StepColor.None));
    }
    
    public void EncircleHouse(House house, StepColor color)
    {
        _instructions.Add(new HighlightInstruction(InstructionType.EncircleHouse,
            house.Unit, house.Number, color));
    }

    public void HighlightElement(ISudokuElement element, StepColor color)
    {
        _registers.Add(element);
        _instructions.Add(new HighlightInstruction(InstructionType.HighlightSudokuElement,
            _registers.Count - 1, color));
    }

    public void CreateLink(CellPossibility from, CellPossibility to, LinkStrength linkStrength)
    {
        _instructions.Add(new HighlightInstruction(InstructionType.CreateLink,
            from.Possibility, from.Row, from.Column, to.Possibility, to.Row, to.Column, linkStrength));
    }

    public void CreateLink(ISudokuElement from, ISudokuElement to, LinkStrength linkStrength)
    {
        _registers.Add(from);
        _registers.Add(to);
        _instructions.Add(new HighlightInstruction(InstructionType.CreateSudokuElementLink,
            _registers.Count - 2, _registers.Count - 1, linkStrength));
    }

    private void Clear()
    {
        _instructions.Clear();
        _registers.Clear();
    }
}