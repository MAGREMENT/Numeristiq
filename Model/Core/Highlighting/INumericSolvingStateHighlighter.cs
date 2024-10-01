using Model.Core.Changes;
using Model.Core.Graphs;
using Model.Sudokus;
using Model.Sudokus.Solver.PossibilitySets;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Tectonics.Solver.Utility;
using Model.Utility;

namespace Model.Core.Highlighting;

public interface INumericSolvingStateHighlighter
{
    public void HighlightPossibility(int possibility, int row, int col, StepColor color);

    public void HighlightPossibility(CellPossibility coord, StepColor color)
    {
        HighlightPossibility(coord.Possibility, coord.Row, coord.Column, color);
    }

    public void HighlightPossibility(Cell cell, int possibility, StepColor color)
    {
        HighlightPossibility(possibility, cell.Row, cell.Column, color);
    }
    
    public void HighlightCell(int row, int col, StepColor color);

    public void HighlightCell(Cell coord, StepColor color)
    {
        HighlightCell(coord.Row, coord.Column, color);
    }
    
    public void CreateLink(CellPossibility from, CellPossibility to, LinkStrength linkStrength);
}

public interface ISudokuHighlighter : INumericSolvingStateHighlighter
{
    public void EncirclePossibility(int possibility, int row, int col);

    public void EncirclePossibility(CellPossibility coord)
    {
        EncirclePossibility(coord.Possibility, coord.Row, coord.Column);
    }

    public void EncircleCell(int row, int col);

    public void EncircleCell(Cell coord)
    {
        EncircleCell(coord.Row, coord.Column);
    }

    public void EncircleHouse(House house, StepColor color);

    public void HighlightElement(ISudokuElement element, StepColor color);

    public void CreateLink(ISudokuElement from, ISudokuElement to, LinkStrength linkStrength);

    public void CreateLink(IPossibilitySet from, IPossibilitySet to, int link);
}

public interface ITectonicHighlighter : INumericSolvingStateHighlighter
{
    public void HighlightElement(ITectonicElement element, StepColor color);
    public void CreateLink(ITectonicElement from, ITectonicElement to, LinkStrength linkStrength);
}