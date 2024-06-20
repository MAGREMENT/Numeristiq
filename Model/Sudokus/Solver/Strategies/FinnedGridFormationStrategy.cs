using System;
using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Position;
using Model.Utility;

namespace Model.Sudokus.Solver.Strategies;

public class FinnedGridFormationStrategy : SudokuStrategy
{
    public const string OfficialNameForType3 = "Finned Swordfish";
    public const string OfficialNameForType4 = "Finned Jellyfish";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.UnorderedAll;

    private readonly int _type;

    public FinnedGridFormationStrategy(int type) : base("", StepDifficulty.Hard, DefaultInstanceHandling)
    {
        _type = type;
        Name = type switch
        {
            3 => OfficialNameForType3,
            4 => OfficialNameForType4,
            _ => throw new ArgumentException("Type not valid")
        };
    }
    public override void Apply(ISudokuSolverData solverData)
    {
        for (int number = 1; number <= 9; number++)
        {
            for (int row = 0; row < 9; row++)
            {
                var ppic = solverData.RowPositionsAt(row, number);
                if (ppic.Count == 0) continue;

                var here = new LinePositions { row };

                if(SearchRowCandidate(solverData, row + 1, ppic, here, number)) return;
            }
            
            for (int col = 0; col < 9; col++)
            {
                var ppir = solverData.ColumnPositionsAt(col, number);
                if (ppir.Count == 0) continue;

                var here = new LinePositions { col };

                if (SearchColumnCandidate(solverData, col + 1, ppir, here, number)) return;
            }
        }
    }

    private bool SearchRowCandidate(ISudokuSolverData solverData, int start, IReadOnlyLinePositions mashed,
        LinePositions visited, int number)
    {
        for (int row = start; row < 9; row++)
        {
            var ppic = solverData.RowPositionsAt(row, number);
            if (ppic.Count > _type) continue;

            var newMashed = mashed.Or(ppic);
            if (newMashed.Count > _type || newMashed.Count == mashed.Count + ppic.Count) continue;
            
            var newVisited = visited.Copy();
            newVisited.Add(row);

            if (newVisited.Count == newMashed.Count - 1 && newMashed.Count == _type)
            {
                if (SearchRowFinned(solverData, newMashed, newVisited, number)) return true;
            }
            else if(newVisited.Count < _type) SearchRowCandidate(solverData, row + 1, newMashed, newVisited, number);
        }

        return false;
    }

    private bool SearchRowFinned(ISudokuSolverData solverData, IReadOnlyLinePositions mashed, LinePositions visited,
        int number)
    {
        for (int row = 0; row < 9; row++)
        {
            if (visited.Contains(row)) continue;

            var ppic = solverData.RowPositionsAt(row, number);

            int miniCol = -1;

            foreach (var col in ppic)
            {
                if (mashed.Contains(col)) continue;
                if (miniCol == -1) miniCol = col / 3;
                else if(col / 3 != miniCol)
                {
                    miniCol = -1;
                    break;
                }
            }

            if (miniCol == -1) continue;
            
            foreach (var col in mashed)
            {
                if (col / 3 != miniCol) continue;
                int startRow = row / 3 * 3;

                for (int gridRow = 0; gridRow < 3; gridRow++)
                {
                    int eliminationRow = startRow + gridRow;
                    if (visited.Contains(eliminationRow) || row == eliminationRow) continue;

                    solverData.ChangeBuffer.ProposePossibilityRemoval(number, eliminationRow, col);
                }
            }

            if (solverData.ChangeBuffer.NotEmpty() && solverData.ChangeBuffer.Commit(
                    new FinnedGridFormationReportBuilder(mashed, visited, row, number, Unit.Row)) &&
                    StopOnFirstPush) return true;
        }

        return false;
    }
    
    private bool SearchColumnCandidate(ISudokuSolverData solverData, int start, IReadOnlyLinePositions mashed,
        LinePositions visited, int number)
    {
        for (int col = start; col < 9; col++)
        {
            var ppir = solverData.ColumnPositionsAt(col, number);
            if(ppir.Count > _type) continue;

            var newMashed = mashed.Or(ppir);
            if (newMashed.Count > _type || newMashed.Count == mashed.Count + ppir.Count) continue;

            var newVisited = visited.Copy();
            newVisited.Add(col);

            if (newVisited.Count == newMashed.Count - 1 && newMashed.Count == _type)
            {
                if (SearchColumnFinned(solverData, newMashed, newVisited, number)) return true;
            }
            else if(newVisited.Count < _type) SearchColumnCandidate(solverData, col + 1, newMashed, newVisited, number);
        }

        return false;
    }
    
    private bool SearchColumnFinned(ISudokuSolverData solverData, IReadOnlyLinePositions mashed, LinePositions visited,
        int number)
    {
        for (int col = 0; col < 9; col++)
        {
            if (visited.Contains(col)) continue;

            var ppic = solverData.ColumnPositionsAt(col, number);

            int miniRow = -1;

            foreach (var row in ppic)
            {
                if (mashed.Contains(row)) continue;
                if (miniRow == -1) miniRow = row / 3;
                else if(row / 3 != miniRow)
                {
                    miniRow = -1;
                    break;
                }
            }

            if (miniRow  == -1) continue;
            
            foreach (var row in mashed)
            {
                if (row / 3 != miniRow) continue;
                int startCol = col / 3 * 3;

                for (int gridCol = 0; gridCol < 3; gridCol++)
                {
                    int eliminationCol = startCol + gridCol;
                    if (visited.Contains(eliminationCol) || col == eliminationCol) continue;

                    solverData.ChangeBuffer.ProposePossibilityRemoval(number, row, eliminationCol);
                }
            }

            if (solverData.ChangeBuffer.NotEmpty() && solverData.ChangeBuffer.Commit(
                    new FinnedGridFormationReportBuilder(mashed, visited, col, number, Unit.Column))
                    && StopOnFirstPush) return true;
        }

        return false;
    }
}

public class FinnedGridFormationReportBuilder : IChangeReportBuilder<IUpdatableSudokuSolvingState, ISudokuHighlighter>
{
    private readonly IReadOnlyLinePositions _mashed;
    private readonly IReadOnlyLinePositions _visited;
    private readonly int _fin;
    private readonly int _number;
    private readonly Unit _unit;

    public FinnedGridFormationReportBuilder(IReadOnlyLinePositions mashed, IReadOnlyLinePositions visited, int fin, int number, Unit unit)
    {
        _mashed = mashed;
        _visited = visited;
        _fin = fin;
        _unit = unit;
        _number = number;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        List<Cell> normal = new();
        List<Cell> finned = new();

        foreach (var visited in _visited)
        {
            foreach (var mashed in _mashed)
            {
                int row = _unit == Unit.Row ? visited : mashed;
                int col = _unit == Unit.Row ? mashed : visited;

                if (snapshot.PossibilitiesAt(row, col).Contains(_number)) normal.Add(new Cell(row, col));
            }
        }

        for (int n = 0; n < 9; n++)
        {
            int row = _unit == Unit.Row ? _fin : n;
            int col = _unit == Unit.Row ? n : _fin;

            if (snapshot.PossibilitiesAt(row, col).Contains(_number))
            {
                if (_mashed.Contains(n)) normal.Add(new Cell(row, col));
                else finned.Add(new Cell(row, col));
            }
        }
        
        return new ChangeReport<ISudokuHighlighter>( "",
            lighter =>
            {
                foreach (var coord in normal)
                {
                    lighter.HighlightPossibility(_number, coord.Row, coord.Column, ChangeColoration.CauseOffOne);
                }
                
                foreach (var coord in finned)
                {
                    lighter.HighlightPossibility(_number, coord.Row, coord.Column, ChangeColoration.CauseOffTwo);
                }
                
                ChangeReportHelper.HighlightChanges(lighter, changes);
            });
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}