using System.Collections.Generic;
using System.Linq;
using Global;
using Global.Enums;
using Model.Solver.Helpers.Changes;
using Model.Solver.Position;
using Model.Solver.Possibility;
using Model.Solver.PossibilityPosition;
using Model.Solver.StrategiesUtility;

namespace Model.Solver.Strategies;

public class SueDeCoqStrategy : AbstractStrategy
{
    public const string OfficialName = "Sue-De-Coq";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.ChooseBest;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;

    private readonly int _maxNotDrawnCandidates;
    
    public SueDeCoqStrategy(int maxNotDrawnCandidates) : base(OfficialName, StrategyDifficulty.Hard, DefaultBehavior)
    {
        _maxNotDrawnCandidates = maxNotDrawnCandidates;
    }
    
    public override void Apply(IStrategyManager strategyManager)
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

                            if (strategyManager.Sudoku[unitRow, otherColumn1] == 0 &&
                                strategyManager.Sudoku[unitRow, otherColumn2] == 0)
                            {
                                var c1 = new Cell(unitRow, otherColumn1);
                                var c2 = new Cell(unitRow, otherColumn2);
                                
                                if (Try(strategyManager, Unit.Row, c1, c2)) return;

                                if (j == 1 && strategyManager.Sudoku[unitRow, startCol + 2] == 0 &&
                                    Try(strategyManager, Unit.Row, c1, c2, new Cell(unitRow, startCol + 2))) return;
                            }
                            
                            if (strategyManager.Sudoku[otherRow1, unitColumn] == 0 &&
                                strategyManager.Sudoku[otherRow2, unitColumn] == 0)
                            {
                                var c1 = new Cell(otherRow1, unitColumn);
                                var c2 = new Cell(otherRow2, unitColumn);
                                
                                if (Try(strategyManager, Unit.Column, c1, c2)) return;

                                if (j == 1 && strategyManager.Sudoku[startRow + 2, unitColumn] == 0 &&
                                    Try(strategyManager, Unit.Column, c1, c2, new Cell(startRow + 2, unitColumn))) return;
                            }
                        }
                    }
                }
            }
        }
    }

    private bool Try(IStrategyManager strategyManager, Unit unit, params Cell[] cells)
    {
        var possibilities = Possibilities.NewEmpty();
        foreach (var cell in cells)
        {
            possibilities.Add(strategyManager.PossibilitiesAt(cell));
        }

        if (possibilities.Count < cells.Length + 2) return false;

        var cellsInBox = CellsInBox(strategyManager, cells);
        if (cellsInBox.Count == 0) return false;

        var cellsInUnit = CellsInUnit(strategyManager, cells, unit);
        if (cellsInUnit.Count == 0) return false;
        
        var minimumPossibilitiesDrawn = possibilities.Count - cells.Length;
        var maxCellsPerUnit = minimumPossibilitiesDrawn + _maxNotDrawnCandidates - 1;

        foreach (var boxCombination in CombinationCalculator.EveryCombinationWithMaxCount(maxCellsPerUnit, cellsInBox))
        {
            var forbiddenPositions = new GridPositions();
            var boxPossibilities = Possibilities.NewEmpty();
            foreach (var cell in boxCombination)
            {
                forbiddenPositions.Add(cell);
                boxPossibilities.Add(strategyManager.PossibilitiesAt(cell));
            }

            var forbiddenPossibilities = boxPossibilities.And(possibilities);

            foreach (var unitPP in Combinations(strategyManager, forbiddenPositions,
                         forbiddenPossibilities, maxCellsPerUnit, cellsInUnit))
            {
                var outOfCenterPossibilities = boxPossibilities.Or(unitPP.Possibilities);
                if (outOfCenterPossibilities.And(possibilities).Count < minimumPossibilitiesDrawn) continue;

                var notDrawnPossibilities = possibilities.Difference(outOfCenterPossibilities);
                if(unitPP.Possibilities.Count + boxPossibilities.Count + notDrawnPossibilities.Count 
                   != cells.Length + boxCombination.Length + unitPP.PositionsCount) continue;

                var boxPP = new CAPPossibilitiesPositions(boxCombination, boxPossibilities, strategyManager);
                Process(strategyManager, boxPP, unitPP, cells, possibilities, cellsInBox, cellsInUnit);

                if (strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
                        new SueDeCoqReportBuilder(boxPP, unitPP, cells)) && OnCommitBehavior == OnCommitBehavior.Return) return true;
            }
        }

        return false;
    }

    private void Process(IStrategyManager strategyManager, IPossibilitiesPositions boxPP, IPossibilitiesPositions unitPP,
        Cell[] center, Possibilities centerPossibilities, List<Cell> cellsInBox, List<Cell> cellsInUnit)
    {
        var centerGP = new GridPositions();
        foreach (var cell in center)
        {
            centerGP.Add(cell);
        }

        var forbiddenBox = centerGP.Or(boxPP.Positions);
        var forbiddenUnit = centerGP.Or(unitPP.Positions);

        var boxElimination = boxPP.Possibilities.Or(centerPossibilities.Difference(unitPP.Possibilities));
        var unitElimination = unitPP.Possibilities.Or(centerPossibilities.Difference(boxPP.Possibilities));

        foreach (var cell in cellsInBox)
        {
            if (forbiddenBox.Peek(cell)) continue;
            
            foreach (var p in boxElimination)
            {
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(p, cell);
            }
        }

        foreach (var cell in cellsInUnit)
        {
            if (forbiddenUnit.Peek(cell)) continue;

            foreach (var p in unitElimination)
            {
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(p, cell);
            }
        }
    }

    private static List<Cell> CellsInBox(IStrategyManager strategyManager, Cell[] cells)
    {
        var result = new List<Cell>();

        var startRow = cells[0].Row / 3 * 3;
        var startCol = cells[0].Column / 3 * 3;

        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                var cell = new Cell(startRow + r, startCol + c);

                if (cells.Contains(cell) || strategyManager.Sudoku[cell.Row, cell.Column] != 0) continue;

                result.Add(cell);
            }
        }

        return result;
    }

    private static List<Cell> CellsInUnit(IStrategyManager strategyManager, Cell[] cells, Unit unit)
    {
        var result = new List<Cell>();

        for (int u = 0; u < 9; u++)
        {
            var cell = unit == Unit.Row ? new Cell(cells[0].Row, u) : new Cell(u, cells[0].Column);
            if (cells.Contains(cell) || strategyManager.Sudoku[cell.Row, cell.Column] != 0) continue;

            result.Add(cell);
        }

        return result;
    }
    
    private static List<IPossibilitiesPositions> Combinations(IStrategyManager strategyManager, GridPositions forbiddenPositions, 
        Possibilities forbiddenPossibilities, int max, IReadOnlyList<Cell> sample)
    {
        var result = new List<IPossibilitiesPositions>();

        Combinations(strategyManager, forbiddenPositions, forbiddenPossibilities, max, 0, sample, result, new List<Cell>(),
            Possibilities.NewEmpty());

        return result;
    }

    private static void Combinations(IStrategyManager strategyManager, GridPositions forbiddenPositions, 
        Possibilities forbiddenPossibilities, int max, int start, IReadOnlyList<Cell> sample,
        List<IPossibilitiesPositions> result, List<Cell> currentCells, Possibilities currentPossibilities)
    {
        for (int i = start; i < sample.Count; i++)
        {
            var c = sample[i];
            if (forbiddenPositions.Peek(c)) continue;
            
            var poss = strategyManager.PossibilitiesAt(c);
            if (forbiddenPossibilities.PeekAny(poss)) continue;
            
            currentCells.Add(c);
            var newPossibilities = poss.Or(currentPossibilities);
            
            result.Add(new CAPPossibilitiesPositions(currentCells.ToArray(), newPossibilities, strategyManager)); 
            if (currentCells.Count < max) Combinations(strategyManager, forbiddenPositions, forbiddenPossibilities, max,
                i + 1, sample, result, currentCells, newPossibilities);

            currentCells.RemoveAt(currentCells.Count - 1);
        }
    }
}

public class SueDeCoqReportBuilder : IChangeReportBuilder
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

    public ChangeReport Build(IReadOnlyList<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            foreach (var cell in _centerCells)
            {
                lighter.HighlightCell(cell, ChangeColoration.Neutral);

                foreach (var possibility in snapshot.PossibilitiesAt(cell))
                {
                    if(_boxPP.Possibilities.Peek(possibility)) lighter.HighlightPossibility(possibility, cell.Row,
                        cell.Column, ChangeColoration.CauseOffOne);
                    else if(_unitPP.Possibilities.Peek(possibility)) lighter.HighlightPossibility(possibility, cell.Row,
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

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}