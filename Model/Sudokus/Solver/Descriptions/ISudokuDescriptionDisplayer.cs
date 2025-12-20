using Model.Core;
using Model.Core.Descriptions;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Highlighting;

namespace Model.Sudokus.Solver.Descriptions;

public interface ISudokuDescriptionDisplayer : IDescriptionDisplayer
{
    void AddParagraph(string text, INumericSolvingState state, SudokuCropping cropping,
        IHighlightable<ISudokuHighlighter> highlight, TextDisposition disposition);
}

public enum TextDisposition
{
    Left, Right
}

