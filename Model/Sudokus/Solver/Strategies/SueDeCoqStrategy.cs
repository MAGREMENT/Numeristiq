using System.Collections.Generic;
using System.Linq;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Position;
using Model.Sudokus.Solver.PossibilityPosition;
using Model.Sudokus.Solver.Utility;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies;

public class SueDeCoqStrategy : SudokuStrategy
{
    public const string OfficialName = "Sue-De-Coq";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.BestOnly;

    private readonly int _maxNotDrawnCandidates;
    
    public SueDeCoqStrategy(int maxNotDrawnCandidates) : base(OfficialName, StepDifficulty.Hard, DefaultInstanceHandling)
    {
        _maxNotDrawnCandidates = maxNotDrawnCandidates;
    }
    
    public override void Apply(ISudokuSolverData solverData)
    {
        for (int startRow = 0; startRow < 9; startRow += 3)
        {
            for (int startCol = 0; startCol < 9; startCol += 3)
            {
                for (int u = 0; u < 3; u++)
                {
                    var unitRow = startRow + u;
                    var unitColumn = startCol + u;
                    
                    for (int i = 0; i < 2; i++)
                    {
                        var otherColumn1 = startCol + i;
                        var otherRow1 = startRow + i;

                        for (int j = i + 1; j < 3; j++)
                        {
                            var otherColumn2 = startCol + j;
                            var otherRow2 = startRow + j;

                            if (solverData.Sudoku[unitRow, otherColumn1] == 0 &&
                                solverData.Sudoku[unitRow, otherColumn2] == 0)
                            {
                                var c1 = new Cell(unitRow, otherColumn1);
                                var c2 = new Cell(unitRow, otherColumn2);
                                
                                if (Try(solverData, Unit.Row, c1, c2)) return;

                                if (j == 1 && solverData.Sudoku[unitRow, startCol + 2] == 0 &&
                                    Try(solverData, Unit.Row, c1, c2, new Cell(unitRow, startCol + 2))) return;
                            }
                            
                            if (solverData.Sudoku[otherRow1, unitColumn] == 0 &&
                                solverData.Sudoku[otherRow2, unitColumn] == 0)
                            {
                                var c1 = new Cell(otherRow1, unitColumn);
                                var c2 = new Cell(otherRow2, unitColumn);
                                
                                if (Try(solverData, Unit.Column, c1, c2)) return;

                                if (j == 1 && solverData.Sudoku[startRow + 2, unitColumn] == 0 &&
                                    Try(solverData, Unit.Column, c1, c2, new Cell(startRow + 2, unitColumn))) return;
                            }
                        }
                    }
                }
            }
        }
    }

    private bool Try(ISudokuSolverData solverData, Unit unit, params Cell[] cells)
    {
        var possibilities = new ReadOnlyBitSet16();
        foreach (var cell in cells)
        {
            possibilities += solverData.PossibilitiesAt(cell);
        }

        if (possibilities.Count < cells.Length + 2) return false;

        var cellsInBox = CellsInBox(solverData, cells);
        if (cellsInBox.Count == 0) return false;

        var cellsInUnit = CellsInUnit(solverData, cells, unit);
        if (cellsInUnit.Count == 0) return false;
        
        var minimumPossibilitiesDrawn = possibilities.Count - cells.Length;
        var maxCellsPerUnit = minimumPossibilitiesDrawn + _maxNotDrawnCandidates - 1;

        foreach (var boxCombination in CombinationCalculator.EveryCombinationWithMaxCount(maxCellsPerUnit, cellsInBox))
        {
            var forbiddenPositions = new GridPositions();
            var boxPossibilities = new ReadOnlyBitSet16();
            foreach (var cell in boxCombination)
            {
                forbiddenPositions.Add(cell);
                boxPossibilities += solverData.PossibilitiesAt(cell);
            }

            var forbiddenPossibilities = boxPossibilities & possibilities;

            foreach (var unitPP in Combinations(solverData, forbiddenPositions,
                         forbiddenPossibilities, maxCellsPerUnit, cellsInUnit))
            {
                var outOfCenterPossibilities = boxPossibilities | unitPP.Possibilities;
                if ((outOfCenterPossibilities & possibilities).Count < minimumPossibilitiesDrawn) continue;

                var notDrawnPossibilities = possibilities - outOfCenterPossibilities;
                if(unitPP.Possibilities.Count + boxPossibilities.Count + notDrawnPossibilities.Count 
                   != cells.Length + boxCombination.Length + unitPP.PositionsCount) continue;

                var boxPP = new CAPPossibilitiesPositions(boxCombination, boxPossibilities, solverData);
                Process(solverData, boxPP, unitPP, cells, possibilities, cellsInBox, cellsInUnit);

                if (solverData.ChangeBuffer.NotEmpty() && solverData.ChangeBuffer.Commit(
                        new SueDeCoqReportBuilder(boxPP, unitPP, cells)) && StopOnFirstPush) return true;
            }
        }

        return false;
    }

    private void Process(ISudokuSolverData solverData, IPossibilitiesPositions boxPP, IPossibilitiesPositions unitPP,
        Cell[] center, ReadOnlyBitSet16 centerPossibilities, List<Cell> cellsInBox, List<Cell> cellsInUnit)
    {
        var centerGP = new GridPositions();
        foreach (var cell in center)
        {
            centerGP.Add(cell);
        }

        var forbiddenBox = centerGP.Or(boxPP.Positions);
        var forbiddenUnit = centerGP.Or(unitPP.Positions);

        var boxElimination = boxPP.Possibilities | (centerPossibilities - unitPP.Possibilities);
        var unitElimination = unitPP.Possibilities | (centerPossibilities - boxPP.Possibilities);

        foreach (var cell in cellsInBox)
        {
            if (forbiddenBox.Contains(cell)) continue;
            
            foreach (var p in boxElimination.EnumeratePossibilities())
            {
                solverData.ChangeBuffer.ProposePossibilityRemoval(p, cell);
            }
        }

        foreach (var cell in cellsInUnit)
        {
            if (forbiddenUnit.Contains(cell)) continue;

            foreach (var p in unitElimination.EnumeratePossibilities())
            {
                solverData.ChangeBuffer.ProposePossibilityRemoval(p, cell);
            }
        }
    }

    private static List<Cell> CellsInBox(ISudokuSolverData solverData, Cell[] cells)
    {
        var result = new List<Cell>();

        var startRow = cells[0].Row / 3 * 3;
        var startCol = cells[0].Column / 3 * 3;

        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                var cell = new Cell(startRow + r, startCol + c);

                if (cells.Contains(cell) || solverData.Sudoku[cell.Row, cell.Column] != 0) continue;

                result.Add(cell);
            }
        }

        return result;
    }

    private static List<Cell> CellsInUnit(ISudokuSolverData solverData, Cell[] cells, Unit unit)
    {
        var result = new List<Cell>();

        for (int u = 0; u < 9; u++)
        {
            var cell = unit == Unit.Row ? new Cell(cells[0].Row, u) : new Cell(u, cells[0].Column);
            if (cells.Contains(cell) || solverData.Sudoku[cell.Row, cell.Column] != 0) continue;

            result.Add(cell);
        }

        return result;
    }
    
    private static List<IPossibilitiesPositions> Combinations(ISudokuSolverData solverData, GridPositions forbiddenPositions, 
        ReadOnlyBitSet16 forbiddenPossibilities, int max, IReadOnlyList<Cell> sample)
    {
        var result = new List<IPossibilitiesPositions>();

        Combinations(solverData, forbiddenPositions, forbiddenPossibilities, max, 0, sample, result, new List<Cell>(),
            new ReadOnlyBitSet16());

        return result;
    }

    private static void Combinations(ISudokuSolverData solverData, GridPositions forbiddenPositions, 
        ReadOnlyBitSet16 forbiddenPossibilities, int max, int start, IReadOnlyList<Cell> sample,
        List<IPossibilitiesPositions> result, List<Cell> currentCells, ReadOnlyBitSet16 currentPossibilities)
    {
        for (int i = start; i < sample.Count; i++)
        {
            var c = sample[i];
            if (forbiddenPositions.Contains(c)) continue;
            
            var poss = solverData.PossibilitiesAt(c);
            if (forbiddenPossibilities.ContainsAny(poss)) continue;
            
            currentCells.Add(c);
            var newPossibilities = poss | currentPossibilities;
            
            result.Add(new CAPPossibilitiesPositions(currentCells.ToArray(), newPossibilities, solverData)); 
            if (currentCells.Count < max) Combinations(solverData, forbiddenPositions, forbiddenPossibilities, max,
                i + 1, sample, result, currentCells, newPossibilities);

            currentCells.RemoveAt(currentCells.Count - 1);
        }
    }
}

public class SueDeCoqReportBuilder : IChangeReportBuilder<IUpdatableSudokuSolvingState, ISudokuHighlighter>
{
    private readonly IPossibilitiesPositions _boxPP;
    private readonly IPossibilitiesPositions _unitPP;
    private readonly Cell[] _centerCells;


    public SueDeCoqReportBuilder(IPossibilitiesPositions boxPp, IPossibilitiesPositions unitPp, Cell[] centerCells)
    {
        _boxPP = boxPp;
        _unitPP = unitPp;
        _centerCells = centerCells;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>( "", lighter =>
        {
            foreach (var cell in _centerCells)
            {
                lighter.HighlightCell(cell, ChangeColoration.Neutral);

                foreach (var possibility in snapshot.PossibilitiesAt(cell).EnumeratePossibilities())
                {
                    if(_boxPP.Possibilities.Contains(possibility)) lighter.HighlightPossibility(possibility, cell.Row,
                        cell.Column, ChangeColoration.CauseOffOne);
                    else if(_unitPP.Possibilities.Contains(possibility)) lighter.HighlightPossibility(possibility, cell.Row,
                        cell.Column, ChangeColoration.CauseOffTwo);
                    else lighter.HighlightPossibility(possibility, cell.Row, cell.Column, ChangeColoration.CauseOnOne);
                }
            }

            foreach (var cell in _boxPP.EachCell())
            {
                lighter.HighlightCell(cell, ChangeColoration.CauseOffOne);
            }

            foreach (var cell in _unitPP.EachCell())
            {
                lighter.HighlightCell(cell, ChangeColoration.CauseOffTwo);
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}