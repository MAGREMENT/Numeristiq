using System;
using Model.Core;
using Model.Core.Descriptions;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Highlighting;
using Model.Utility;

namespace Model.Sudokus.Solver.Descriptions;

public class SudokuStepDescription : IDescription<ISudokuDescriptionDisplayer>
{
    private readonly string _text;
    private readonly TextDisposition _disposition;
    private readonly INumericSolvingState _state;
    private readonly SudokuCropping _cropping;
    private readonly IHighlightable<ISudokuHighlighter> _highlight;

    public SudokuStepDescription(string text, string state32, int rowFrom, int rowTo, int colFrom, int colTo,
        string highlight, TextDisposition disposition) : this(text, state32, 
        new SudokuCropping(rowFrom, colFrom, rowTo, colTo), highlight, disposition) { }

    public SudokuStepDescription(string text, string state32, SudokuCropping cropping, string highlight,
        TextDisposition disposition)
    {
        _state = SudokuTranslator.TranslateBase32Format(state32, DefaultBase32Alphabet.Instance);
        _text = text;
        _cropping = cropping;
        _disposition = disposition;
        _highlight = SudokuHighlightExecutable.FromBase16(highlight, DefaultBase16Alphabet.Instance);
    }

    public void Display(ISudokuDescriptionDisplayer displayer)
    {
        displayer.AddParagraph(_text, _state, _cropping, _highlight, _disposition);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_text, _state.GetHashCode(), _disposition.GetHashCode(),
            _cropping.GetHashCode(), _highlight.GetHashCode());
    }

    public override bool Equals(object? obj)
    {
        return obj is SudokuStepDescription s && s._text.Equals(_text) && s._state.Equals(_state)
               && _disposition == s._disposition && _cropping == s._cropping && _highlight.Equals(s._highlight);
    }
}

public readonly struct SudokuCropping
{
    public SudokuCropping(int rowFrom, int columnFrom, int rowTo, int columnTo)
    {
        RowFrom = rowFrom;
        ColumnFrom = columnFrom;
        RowTo = rowTo;
        ColumnTo = columnTo;
    }

    public static SudokuCropping Default() => new(0, 0, 8, 8);

    public int RowFrom { get; }
    public int ColumnFrom { get; }
    public int RowTo { get; }
    public int ColumnTo { get; }

    public static bool operator ==(SudokuCropping left, SudokuCropping right)
    {
        return left.RowFrom == right.RowFrom && left.ColumnFrom == right.ColumnFrom
                                             && left.RowTo == right.RowTo && left.ColumnTo == right.ColumnTo;
    }
    
    public static bool operator !=(SudokuCropping left, SudokuCropping right)
    {
        return left.RowFrom != right.RowFrom || left.ColumnFrom != right.ColumnFrom
                                             || left.RowTo != right.RowTo || left.ColumnTo != right.ColumnTo;
    }

    public override bool Equals(object? obj)
    {
        return obj is SudokuCropping sc && sc == this;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(RowFrom, ColumnFrom, RowTo, ColumnTo);
    }
}