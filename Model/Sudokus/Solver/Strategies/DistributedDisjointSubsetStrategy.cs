using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Position;
using Model.Sudokus.Solver.Utility;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies;

public class DistributedDisjointSubsetStrategy : SudokuStrategy
{
    public const string OfficialName = "Distributed Disjoint Subset";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    public DistributedDisjointSubsetStrategy() : base(OfficialName, StepDifficulty.Extreme, DefaultInstanceHandling)
    {
    }
    
    public override void Apply(ISudokuSolverData solverData)
    {
        HashSet<GridPositions> alreadyExplored = new();
        
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (solverData.Sudoku[row, col] != 0) continue;

                var current = new Cell(row, col);
                
                GridPositions positions = new GridPositions();
                positions.Add(row, col);
                Dictionary<int, List<Cell>> possibilitiesCells = new();
                foreach (var p in solverData.PossibilitiesAt(row, col).EnumeratePossibilities())
                {
                    possibilitiesCells.Add(p, new List<Cell>{current});
                }

                if (Search(solverData, possibilitiesCells, positions, alreadyExplored)) return;
            }
        }
    }

    private bool Search(ISudokuSolverData solverData, Dictionary<int, List<Cell>> possibilitiesCells,
        GridPositions positions, HashSet<GridPositions> alreadyExplored)
    {
        foreach (var cell in positions.AllSeenCells())
        {
            if (solverData.Sudoku[cell.Row, cell.Column] != 0 ||
                !ShareAUnitWithAll(solverData, cell, possibilitiesCells)) continue;
            
            positions.Add(cell);
            if (alreadyExplored.Contains(positions))
            {
                positions.Remove(cell);
                continue;
            }

            alreadyExplored.Add(positions.Copy());
            
            foreach (var p in solverData.PossibilitiesAt(cell).EnumeratePossibilities())
            {
                if (!possibilitiesCells.TryGetValue(p, out var list))
                {
                    list = new List<Cell>();
                    possibilitiesCells[p] = list;
                }

                list.Add(cell);
            }

            if (positions.Count == possibilitiesCells.Count)
            {
                if (Process(solverData, possibilitiesCells)) return true;
            }
            
            if (Search(solverData, possibilitiesCells, positions, alreadyExplored)) return true;

            positions.Remove(cell);
            foreach (var p in solverData.PossibilitiesAt(cell).EnumeratePossibilities())
            {
                var list = possibilitiesCells[p];

                list.RemoveAt(list.Count - 1);
                if (list.Count == 0) possibilitiesCells.Remove(p);
            }
        }
        
        return false;
    }

    private bool Process(ISudokuSolverData solverData, Dictionary<int, List<Cell>> possibilitiesCells)
    {
        foreach (var entry in possibilitiesCells)
        {
            foreach (var ssc in SudokuCellUtility.SharedSeenCells(entry.Value))
            {
                solverData.ChangeBuffer.ProposePossibilityRemoval(entry.Key, ssc);
            }
        }

        return solverData.ChangeBuffer.NotEmpty() && solverData.ChangeBuffer.Commit(
                   new DistributedDisjointSubsetReportBuilder(PossibilitiesCellsDeepCopy(possibilitiesCells))) &&
               StopOnFirstPush;
    }

    private Dictionary<int, List<Cell>> PossibilitiesCellsDeepCopy(Dictionary<int, List<Cell>> original)
    {
        var result = new Dictionary<int, List<Cell>>();

        foreach (var entry in original)
        {
            result.Add(entry.Key, new List<Cell>(entry.Value));
        }
        
        return result;
    }

    private bool ShareAUnitWithAll(ISudokuSolverData solverData, Cell cell, Dictionary<int, List<Cell>> possibilitiesCells)
    {
        bool ok = false;
        foreach (var poss in solverData.PossibilitiesAt(cell).EnumeratePossibilities())
        {
            if (!possibilitiesCells.TryGetValue(poss, out var toShareAUnitWith)) continue;

            ok = true;
            foreach (var c in toShareAUnitWith)
            {
                if (!SudokuCellUtility.ShareAUnit(cell, c)) return false;
            }
        }

        return ok;
    }
}

public class DistributedDisjointSubsetReportBuilder : IChangeReportBuilder<NumericChange, IUpdatableSudokuSolvingState, ISudokuHighlighter>
{
    private readonly Dictionary<int, List<Cell>> _possibilitiesCells;

    public DistributedDisjointSubsetReportBuilder(Dictionary<int, List<Cell>> possibilitiesCells)
    {
        _possibilitiesCells = possibilitiesCells;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>( "", lighter =>
        {
            int color = (int)ChangeColoration.CauseOffOne;
            foreach (var entry in _possibilitiesCells)
            {
                foreach (var cell in entry.Value)
                {
                    lighter.HighlightPossibility(entry.Key, cell.Row, cell.Column, (ChangeColoration)color);
                }

                color++;
            }
            
            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}