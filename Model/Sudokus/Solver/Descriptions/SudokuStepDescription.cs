using System;
using Model.Core;
using Model.Core.Descriptions;
using Model.Core.Highlighting;
using Model.Utility;

namespace Model.Sudokus.Solver.Descriptions;

public class SudokuStepDescription : IDescription<SudokuDescriptionDisplayer>
{
    private readonly string _text;
    private readonly TextDisposition _disposition;
    private readonly INumericSolvingState _state;
    private readonly SudokuCropping _cropping;
    private readonly IHighlightable<ISudokuHighlighter> _highlight;

    public SudokuStepDescription(string text, string state32, int rowFrom, int rowTo, int colFrom, int colTo,
        string highlight, TextDisposition disposition)
    {
        _state = SudokuTranslator.TranslateBase32Format(state32, DefaultBase32Alphabet.Instance);
        _text = text;
        _cropping = new SudokuCropping(rowFrom, colFrom, rowTo, colTo);
        _disposition = disposition;
        _highlight = HighlightExecutable.FromBase16(highlight, DefaultBase16Alphabet.Instance);
    }

    public void Display(SudokuDescriptionDisplayer displayer)
    {
        displayer.AddParagraph(_text, _state, _cropping, _highlight, _disposition);
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
}