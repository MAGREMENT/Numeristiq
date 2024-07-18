using System;
using System.Collections.Generic;
using System.Text;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Core.Settings;
using Model.Core.Settings.Types;
using Model.Sudokus.Solver.Position;
using Model.Utility;
using Model.Utility.BitSets;
using Model.Utility.Collections;

namespace Model.Sudokus.Solver.Strategies;

public class DistributedDisjointSubsetStrategy : SudokuStrategy
{
    public const string OfficialName = "Distributed Disjoint Subset";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    private readonly IntSetting _maxSize;

    public DistributedDisjointSubsetStrategy(int maxSize) : base(OfficialName, StepDifficulty.Extreme, DefaultInstanceHandling)
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
                var positions = new GridPositions { { row, col } };
                var covers = new DistributedDisjointSubsetCover[9];
                var poss = solverData.PossibilitiesAt(row, col);
                
                foreach (var p in poss.EnumeratePossibilities())
                {
                    covers[p - 1] = new DistributedDisjointSubsetCover(current);
                }

                if (Search(solverData, covers, poss.Count, positions, alreadyExplored)) return;
            }
        }
    }

    private bool Search(ISudokuSolverData solverData, DistributedDisjointSubsetCover[] covers, int coverCount,
        GridPositions positions, HashSet<GridPositions> alreadyExplored)
    {
        if (positions.Count == _maxSize.Value) return false;
        
        Span<DistributedDisjointSubsetCover> old = stackalloc DistributedDisjointSubsetCover[9];
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
            
            if (Search(solverData, covers, newCoverCount, positions, alreadyExplored)) return true;

            old.CopyTo(covers);
            positions.Remove(cell);
        }
        
        return false;
    }

    private bool Process(ISudokuSolverData solverData, GridPositions positions, DistributedDisjointSubsetCover[] covers)
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

        return solverData.ChangeBuffer.NotEmpty() && solverData.ChangeBuffer.Commit(
                   new DistributedDisjointSubsetReportBuilder(positions, covers)) &&
               StopOnFirstPush;
    }
}

public readonly struct DistributedDisjointSubsetCover
{
    private const uint RowRemove = ~(uint)0b1000;
    private const uint ColumnRemove = ~(uint)0b100;
    private const uint BoxRemove = ~(uint)0b10;
    
    private readonly uint _sharedHouses;
    private readonly int _row;
    private readonly int _col;

    public DistributedDisjointSubsetCover(Cell cell)
    {
        _row = cell.Row;
        _col = cell.Column;
        //RowBit - ColumnBit - BoxBit - InitializationBit
        _sharedHouses = 0b1111;
    }

    private DistributedDisjointSubsetCover(int row, int col, uint sharedHouses)
    {
        _row = row;
        _col = col;
        _sharedHouses = sharedHouses;
    }
    
    public bool IsValid() => _sharedHouses > 1;

    public DistributedDisjointSubsetCover Adapt(Cell cell)
    {
        if (_sharedHouses == 0) return new DistributedDisjointSubsetCover(cell);
        
        var sh = _sharedHouses;
        if (_row != cell.Row) sh &= RowRemove;
        if (_col != cell.Column) sh &= ColumnRemove;
        if (_row / 3 != cell.Row / 3 || _col / 3 != cell.Column / 3) sh &= BoxRemove;

        return new DistributedDisjointSubsetCover(_row, _col, sh);
    }

    public void CurrentlyCoveringHouses(StringBuilder builder)
    {
        bool alreadyOne = false;
        if (((_sharedHouses >> 3) & 1) > 0)
        {
            alreadyOne = true;
            builder.Append("r" + (_row + 1));
        }
        
        if (((_sharedHouses >> 2) & 1) > 0)
        {
            if (alreadyOne) builder.Append(" or ");
            alreadyOne = true;
            builder.Append("c" + (_col + 1));
        }

        if (((_sharedHouses >> 1) & 1) > 0)
        {
            if (alreadyOne) builder.Append(" or ");
            builder.Append("b" + (_row / 3 * 3 + _col / 3));
        }
    }

    public IEnumerable<Cell> SeenCells()
    {
        if (((_sharedHouses >> 3) & 1) > 0)
        {
            for (int c = 0; c < 9; c++)
            {
                yield return new Cell(_row, c);
            }
        }
        
        if (((_sharedHouses >> 2) & 1) > 0)
        {
            for (int r = 0; r < 9; r++)
            {
                yield return new Cell(r, _col);
            }
        }

        if (((_sharedHouses >> 1) & 1) > 0)
        {
            var sr = _row / 3 * 3;
            var sc = _col / 3 * 3;

            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    yield return new Cell(sr + r, sc + c);
                }
            }
        }
    }
}

public class DistributedDisjointSubsetReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly DistributedDisjointSubsetCover[] _covers = new DistributedDisjointSubsetCover[9];
    private readonly GridPositions _positions;

    public DistributedDisjointSubsetReportBuilder(GridPositions positions, DistributedDisjointSubsetCover[] covers)
    {
        _positions = positions.Copy();
        Array.Copy(covers, _covers, covers.Length);
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>(Description(), lighter =>
        {
            int color = (int)ChangeColoration.CauseOffOne;
            for(int i = 0; i < _covers.Length; i++)
            {
                var cover = _covers[i];
                if(!cover.IsValid()) continue;
                
                foreach (var cell in _positions)
                {
                    if(snapshot.PossibilitiesAt(cell).Contains(i + 1)) 
                        lighter.HighlightPossibility(i + 1, cell.Row, cell.Column, (ChangeColoration)color);
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