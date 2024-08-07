using System;
using System.Collections.Generic;
using System.Text;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Core.Settings;
using Model.Core.Settings.Types;
using Model.Sudokus.Solver.Position;
using Model.Sudokus.Solver.Utility;
using Model.Utility;
using Model.Utility.BitSets;
using Model.Utility.Collections;

namespace Model.Sudokus.Solver.Strategies;

public class DistributedDisjointSubsetStrategy : SudokuStrategy
{
    public const string OfficialName = "Distributed Disjoint Subset";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    private readonly IntSetting _maxSize;

    public DistributedDisjointSubsetStrategy(int maxSize) : base(OfficialName, Difficulty.Extreme, DefaultInstanceHandling)
    {
        _maxSize = new IntSetting("Maximum Size", "The maximum amount of cells in the pattern",
            new SliderInteractionInterface(2, 9, 1), maxSize);
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
                var positions = new GridPositions
                {
                    { row, col }
                };
                var covers = new CommonHouses[9];
                var poss = solverData.PossibilitiesAt(row, col);
                
                foreach (var p in poss.EnumeratePossibilities())
                {
                    covers[p - 1] = new CommonHouses(current);
                }

                if (Search(solverData, covers, poss.Count, positions, alreadyExplored)) return;
            }
        }
    }

    private bool Search(ISudokuSolverData solverData, CommonHouses[] covers, int coverCount,
        GridPositions positions, HashSet<GridPositions> alreadyExplored)
    {
        Span<CommonHouses> old = stackalloc CommonHouses[9];
        covers.CopyTo(old);
        
        foreach (var cell in positions.AllSeenCells())
        {
            if (solverData.Sudoku[cell.Row, cell.Column] != 0) continue;
            
            positions.Add(cell);
            if (alreadyExplored.Contains(positions))
            {
                positions.Remove(cell);
                continue;
            }
            alreadyExplored.Add(positions.Copy());
            
            int newCoverCount = coverCount;
            var poss = solverData.PossibilitiesAt(cell);
            bool notOk = false;
            
            foreach (var p in poss.EnumeratePossibilities())
            {
                var cover = covers[p - 1];
                var newCover = cover.Adapt(cell);
                if (!newCover.IsValid())
                {
                    old.CopyTo(covers);
                    positions.Remove(cell);
                    notOk = true;
                    break;
                }

                covers[p - 1] = newCover;
                if (!cover.IsValid()) newCoverCount++;
            }

            if (notOk) continue;

            if (positions.Count == newCoverCount)
            {
                if (Process(solverData, positions, covers)) return true;
            }
            
            if (positions.Count < _maxSize.Value && 
                Search(solverData, covers, newCoverCount, positions, alreadyExplored)) return true;

            old.CopyTo(covers);
            positions.Remove(cell);
        }
        
        return false;
    }

    private bool Process(ISudokuSolverData solverData, GridPositions positions, CommonHouses[] covers)
    {
        for(int i = 0; i < covers.Length; i++)
        {
            var cover = covers[i];
            if (!cover.IsValid()) continue;
            
            foreach (var ssc in cover.SeenCells())
            {
                if(!positions.Contains(ssc)) solverData.ChangeBuffer.ProposePossibilityRemoval(i + 1, ssc);
            }
        }

        return solverData.ChangeBuffer.NeedCommit() && solverData.ChangeBuffer.Commit(
                   new DistributedDisjointSubsetReportBuilder(positions, covers)) &&
               StopOnFirstCommit;
    }
}



public class DistributedDisjointSubsetReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly CommonHouses[] _covers = new CommonHouses[9];
    private readonly GridPositions _positions;

    public DistributedDisjointSubsetReportBuilder(GridPositions positions, CommonHouses[] covers)
    {
        _positions = positions.Copy();
        Array.Copy(covers, _covers, covers.Length);
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>(Description(), lighter =>
        {
            int color = (int)StepColor.Cause1;
            for(int i = 0; i < _covers.Length; i++)
            {
                var cover = _covers[i];
                if(!cover.IsValid()) continue;
                
                foreach (var cell in _positions)
                {
                    if(snapshot.PossibilitiesAt(cell).Contains(i + 1)) 
                        lighter.HighlightPossibility(i + 1, cell.Row, cell.Column, (StepColor)color);
                }

                color++;
            }
            
            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }

    private string Description()
    {
        var builder = new StringBuilder($"Distributed Disjoint Subset in {_positions.ToStringSequence(", ")}. ");
        for (int i = 0; i < _covers.Length; i++)
        {
            var cover = _covers[i];
            if (!cover.IsValid()) continue;

            builder.Append($"{i + 1} => ");
            cover.CurrentlyCoveringHouses(builder);
            builder.Append(". ");
        }

        return builder.ToString();
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}