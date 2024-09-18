using Model.Core;
using Model.Core.Descriptions;
using Model.Core.Highlighting;

namespace Model.Sudokus.Solver.Descriptions;

public interface SudokuDescriptionDisplayer : IDescriptionDisplayer
{
    void AddParagraph(string text, INumericSolvingState state, SudokuCropping cropping,
        IHighlightable<ISudokuHighlighter> highlight, TextDisposition disposition);
}

public enum TextDisposition
{
    Left, Right
}

