using System;
using System.Collections.Generic;
using System.Linq;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Position;
using Model.Utility;
using Model.Utility.BitSets;
using Model.Utility.Collections;

namespace Model.Sudokus.Solver.Strategies;

public class HiddenSetStrategy : SudokuStrategy
{
    public const string OfficialNameForType2 = "Hidden Doubles";
    public const string OfficialNameForType3 = "Hidden Triples";
    public const string OfficialNameForType4 = "Hidden Quads";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.UnorderedAll;

    private readonly int _type;

    public HiddenSetStrategy(int type) : base("", Difficulty.None, DefaultInstanceHandling)
    {
        _type = type;
        switch (type)
        {
            case 2 : Name = OfficialNameForType2;
                Difficulty = Difficulty.Easy;
                break;
            case 3 : Name = OfficialNameForType3;
                Difficulty = Difficulty.Easy;
                break;
            case 4 : Name = OfficialNameForType4;
                Difficulty = Difficulty.Easy;
                break;
            default : throw new ArgumentException("Type not valid");
        }
    }
    
    public override void Apply(ISudokuSolverData solverData)
    {
        for (int row = 0; row < 9; row++)
        {
            if (RecursiveRowMashing(solverData, 1, new LinePositions(), new ReadOnlyBitSet16(), row)) return;
        }

        for (int col = 0; col < 9; col++)
        {
            if (RecursiveColumnMashing(solverData, 1, new LinePositions(), new ReadOnlyBitSet16(), col)) return;
        }

        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                if (RecursiveMiniGridMashing(solverData, 1, new BoxPositions(miniRow, miniCol), 
                        new ReadOnlyBitSet16(), miniRow, miniCol)) return;
            }
        }
    }

    private bool RecursiveRowMashing(ISudokuSolverData solverData, int start, LinePositions mashed,
        ReadOnlyBitSet16 visited, int row)
    {
        for (int i = start; i <= 9; i++)
        {
            var pos = solverData.RowPositionsAt(row, i);
            if (pos.Count > _type || pos.Count == 0) continue;

            var newMashed = mashed.Or(pos);
            if(newMashed.Count > _type) continue;

            var newVisited = visited;
            newVisited += i;

            if (newVisited.Count == _type && newMashed.Count == _type)
            {
                foreach (var col in newMashed)
                {
                    RemoveAllPossibilitiesExcept(row, col, newVisited, solverData);
                }

                if (solverData.ChangeBuffer.NeedCommit())
                {
                    solverData.ChangeBuffer.Commit(new LineHiddenPossibilitiesReportBuilder(newVisited, newMashed,
                        row, Unit.Row));
                    if (StopOnFirstCommit) return true;
                }
            }
            else if (newVisited.Count < _type &&
                     RecursiveRowMashing(solverData, i + 1, newMashed, newVisited, row)) return true;
        }

        return false;
    }
    
    private bool RecursiveColumnMashing(ISudokuSolverData solverData, int start, LinePositions mashed,
        ReadOnlyBitSet16 visited, int col)
    {
        for (int i = start; i <= 9; i++)
        {
            var pos = solverData.ColumnPositionsAt(col, i);
            if (pos.Count > _type || pos.Count == 0) continue;

            var newMashed = mashed.Or(pos);
            if(newMashed.Count > _type) continue;

            var newVisited = visited;
            newVisited += i;

            if (newVisited.Count == _type && newMashed.Count == _type)
            {
                foreach (var row in newMashed)
                {
                    RemoveAllPossibilitiesExcept(row, col, newVisited, solverData);
                }

                if (solverData.ChangeBuffer.NeedCommit())
                {
                    solverData.ChangeBuffer.Commit(new LineHiddenPossibilitiesReportBuilder(newVisited, newMashed,
                        col, Unit.Column));
                    if (StopOnFirstCommit) return true;
                }
            }
            else if (newVisited.Count < _type &&
                     RecursiveColumnMashing(solverData, i + 1, newMashed, newVisited, col)) return true;
        }

        return false;
    }
    
    private bool RecursiveMiniGridMashing(ISudokuSolverData solverData, int start, BoxPositions mashed,
        ReadOnlyBitSet16 visited, int miniRow, int miniCol)
    {
        for (int i = start; i <= 9; i++)
        {
            var pos = solverData.MiniGridPositionsAt(miniRow, miniCol, i);
            if (pos.Count > _type || pos.Count == 0) continue;

            var newMashed = mashed.Or(pos);
            if(newMashed.Count > _type) continue;

            var newVisited = visited;
            newVisited += i;

            if (newVisited.Count == _type && newMashed.Count == _type)
            {
                foreach (var position in newMashed)
                {
                    RemoveAllPossibilitiesExcept(position.Row, position.Column, newVisited, solverData);
                }

                if (solverData.ChangeBuffer.NeedCommit())
                {
                    solverData.ChangeBuffer.Commit(new MiniGridHiddenPossibilitiesReportBuilder(newVisited, newMashed));
                    if (StopOnFirstCommit) return true;
                }
            }
            else if (newVisited.Count < _type && RecursiveMiniGridMashing(solverData, i + 1, newMashed,
                         newVisited, miniRow, miniCol)) return true;
        }

        return false;
    }

    private void RemoveAllPossibilitiesExcept(int row, int col, ReadOnlyBitSet16 except, ISudokuSolverData solverData)
    {
        for (int number = 1; number <= 9; number++)
        {
            if (!except.Contains(number))
            {
                solverData.ChangeBuffer.ProposePossibilityRemoval(number, row, col);
            }
        }
    }
}

public class LineHiddenPossibilitiesReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly ReadOnlyBitSet16 _possibilities;
    private readonly LinePositions _linePos;
    private readonly int _unitNumber;
    private readonly Unit _unit;


    public LineHiddenPossibilitiesReportBuilder(ReadOnlyBitSet16 possibilities, LinePositions linePos, int unitNumber, Unit unit)
    {
        _possibilities = possibilities;
        _linePos = linePos;
        _unitNumber = unitNumber;
        _unit = unit;
    }
    
    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        var cells = _linePos.ToCellArray(_unit, _unitNumber);
        
        return new ChangeReport<ISudokuHighlighter>(ChangeReportHelper.XSetStrategyDescription(cells, "Hidden",
            _linePos.Count, _possibilities.EnumeratePossibilities()), lighter =>
        {
            foreach (var possibility in _possibilities.EnumeratePossibilities())
            {
                foreach (var cell in cells)
                {
                    if(snapshot.PossibilitiesAt(cell).Contains(possibility))
                        lighter.HighlightPossibility(possibility, cell.Row, cell.Column, StepColor.Cause1);
                }
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}

public class MiniGridHiddenPossibilitiesReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly ReadOnlyBitSet16 _possibilities;
    private readonly BoxPositions _miniPos;

    public MiniGridHiddenPossibilitiesReportBuilder(ReadOnlyBitSet16 possibilities, BoxPositions miniPos)
    {
        _possibilities = possibilities;
        _miniPos = miniPos;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        var cells = _miniPos.ToCellArray();
        
        return new ChangeReport<ISudokuHighlighter>(ChangeReportHelper.XSetStrategyDescription(cells, "Hidden",
            _miniPos.Count, _possibilities.EnumeratePossibilities()), lighter =>
        {
            foreach (var possibility in _possibilities.EnumeratePossibilities())
            {
                foreach (var pos in _miniPos)
                {
                    if (snapshot.PossibilitiesAt(pos.Row, pos.Column).Contains(possibility))
                        lighter.HighlightPossibility(possibility, pos.Row, pos.Column, StepColor.Cause1);
                }
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}