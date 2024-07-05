using System;
using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Position;
using Model.Utility;
using Model.Utility.BitSets;
using Model.Utility.Collections;

namespace Model.Sudokus.Solver.Strategies;

public class NakedSetStrategy : SudokuStrategy
{
    public const string OfficialNameForType2 = "Naked Doubles";
    public const string OfficialNameForType3 = "Naked Triples";
    public const string OfficialNameForType4 = "Naked Quads";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.UnorderedAll;

    private readonly int _type;

    public NakedSetStrategy(int type) : base("", StepDifficulty.None, DefaultInstanceHandling)
    {
        _type = type;
        switch (type)
        {
            case 2 : Name = OfficialNameForType2;
                Difficulty = StepDifficulty.Easy;
                break;
            case 3 : Name = OfficialNameForType3;
                Difficulty = StepDifficulty.Easy;
                break;
            case 4 : Name = OfficialNameForType4;
                Difficulty = StepDifficulty.Easy;
                break;
            default : throw new ArgumentException("Type not valid");
        }
    }
    
    
    public override void Apply(ISudokuSolverData solverData)
    {
        for (int row = 0; row < 9; row++)
        {
            var possibleCols = EveryRowCellWithLessPossibilities(solverData, row, _type + 1);
            if (RecursiveRowMashing(solverData, new ReadOnlyBitSet16(), possibleCols, -1, row,
                    new LinePositions())) return;
        }
        
        for (int col = 0; col < 9; col++)
        {
            var possibleRows = EveryColumnCellWithLessPossibilities(solverData, col, _type + 1);
            if (RecursiveColumnMashing(solverData, new ReadOnlyBitSet16(), possibleRows, -1, col,
                    new LinePositions())) return;
        }
        
        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                var possibleGridNumbers = EveryMiniGridCellWithLessPossibilities(solverData, miniRow, miniCol, _type + 1);
                if (RecursiveMiniGridMashing(solverData, new ReadOnlyBitSet16(), possibleGridNumbers, -1,
                        miniRow, miniCol, new MiniGridPositions(miniRow, miniCol))) return;
            }
        }
    }

    private LinePositions EveryRowCellWithLessPossibilities(ISudokuSolverData solverData, int row, int than)
    {
        LinePositions result = new();
        for (int col = 0; col < 9; col++)
        {
            if (solverData.Sudoku[row, col] == 0 && solverData.PossibilitiesAt(row, col).Count < than)
                result.Add(col);
        }

        return result;
    }

    private bool RecursiveRowMashing(ISudokuSolverData solverData, ReadOnlyBitSet16 current,
        LinePositions possibleCols, int cursor, int row, LinePositions visited)
    {
        int col;
        while ((col = possibleCols.Next(ref cursor)) != -1)
        {
            var possibilities = solverData.PossibilitiesAt(row, col);
            if(possibilities.Count > _type) continue;
            
            var newCurrent = current | possibilities;
            if (newCurrent.Count > _type) continue;
            
            var newVisited = visited.Copy();
            newVisited.Add(col);

            if (newVisited.Count == _type && newCurrent.Count == _type)
            {
                if (RemovePossibilitiesFromRow(solverData, row, newCurrent, newVisited)) return true;
            }
               
            else if (newVisited.Count < _type)
            {
                if (RecursiveRowMashing(solverData, newCurrent, possibleCols, cursor, row, newVisited))
                    return true;
            }
        }

        return false;
    }

    private bool RemovePossibilitiesFromRow(ISudokuSolverData solverData, int row, ReadOnlyBitSet16 toRemove, LinePositions except)
    {
        foreach (var n in toRemove.EnumeratePossibilities())
        {
            for (int col = 0; col < 9; col++)
            {
                if (!except.Contains(col)) solverData.ChangeBuffer.ProposePossibilityRemoval(n, row, col);
            }
        }
        
        return solverData.ChangeBuffer.Commit( new LineNakedPossibilitiesReportBuilder(toRemove,
            except, row, Unit.Row)) && StopOnFirstPush;
    }
    
    private LinePositions EveryColumnCellWithLessPossibilities(ISudokuSolverData solverData, int col, int than)
    {
        LinePositions result = new();
        for (int row = 0; row < 9; row++)
        {
            if (solverData.Sudoku[row, col] == 0 && solverData.PossibilitiesAt(row, col).Count < than) 
                result.Add(row);
        }

        return result;
    }
    
    private bool RecursiveColumnMashing(ISudokuSolverData solverData, ReadOnlyBitSet16 current,
        LinePositions possibleRows, int cursor, int col, LinePositions visited)
    {
        int row;
        while((row = possibleRows.Next(ref cursor)) != -1)
        {
            var possibilities = solverData.PossibilitiesAt(row, col);
            if(possibilities.Count > _type) continue;
            
            var newCurrent = current | possibilities;
            if (newCurrent.Count > _type) continue;
            
            var newVisited = visited.Copy();
            newVisited.Add(row);

            if (newVisited.Count == _type && newCurrent.Count == _type)
            {
                if (RemovePossibilitiesFromColumn(solverData, col, newCurrent, newVisited)) return true;
            }
            else if (newVisited.Count < _type)
            {
                if (RecursiveColumnMashing(solverData, newCurrent, possibleRows, cursor, col, newVisited))
                    return true;
            }
        }

        return false;
    }

    private bool RemovePossibilitiesFromColumn(ISudokuSolverData solverData, int col, ReadOnlyBitSet16 toRemove, LinePositions except)
    {
        foreach (var n in toRemove.EnumeratePossibilities())
        {
            for (int row = 0; row < 9; row++)
            {
                if (!except.Contains(row)) solverData.ChangeBuffer.ProposePossibilityRemoval(n, row, col);
            }
        }
        
        return solverData.ChangeBuffer.Commit( new LineNakedPossibilitiesReportBuilder(toRemove, except,
            col, Unit.Column)) && StopOnFirstPush;
    }
    
    private MiniGridPositions EveryMiniGridCellWithLessPossibilities(ISudokuSolverData solverData, int miniRow, int miniCol, int than)
    {
        MiniGridPositions result = new(miniRow, miniCol);
        for (int gridRow = 0; gridRow < 3; gridRow++)
        {
            for (int gridCol = 0; gridCol < 3; gridCol++)
            {
                int row = miniRow * 3 + gridRow;
                int col = miniCol * 3 + gridCol;
            
                if (solverData.Sudoku[row, col] == 0 && solverData.PossibilitiesAt(row, col).Count < than) 
                    result.Add(gridRow, gridCol);
            }
        }
        
        return result;
    }
    
    private bool RecursiveMiniGridMashing(ISudokuSolverData solverData, ReadOnlyBitSet16 current,
        MiniGridPositions possiblePos, int cursor, int miniRow, int miniCol, MiniGridPositions visited)
    {
        Cell pos;
        while((pos = possiblePos.Next(ref cursor)).Row != -1)
        {
            var possibilities = solverData.PossibilitiesAt(pos.Row, pos.Column);
            if(possibilities.Count > _type) continue;
            
            var newCurrent = current | possibilities;
            if (newCurrent.Count > _type) continue;
            
            var newVisited = visited.Copy();
            newVisited.Add(pos.Row % 3, pos.Column % 3);

            if (newVisited.Count == _type && newCurrent.Count == _type)
            {
                if (RemovePossibilitiesFromMiniGrid(solverData, miniRow, miniCol, newCurrent, newVisited))
                    return true;
            }
            else if (newVisited.Count < _type)
            {
                if (RecursiveMiniGridMashing(solverData, newCurrent, possiblePos, cursor, miniRow, miniCol,
                        newVisited)) return true;
            }
        }

        return false;
    }
    
    private bool RemovePossibilitiesFromMiniGrid(ISudokuSolverData solverData, int miniRow, int miniCol, ReadOnlyBitSet16 toRemove,
        MiniGridPositions except)
    {
        foreach (var n in toRemove.EnumeratePossibilities())
        {
            for (int gridRow = 0; gridRow < 3; gridRow++)
            {
                for (int gridCol = 0; gridCol < 3; gridCol++)
                {
                    int row = miniRow * 3 + gridRow;
                    int col = miniCol * 3 + gridCol;
                
                    if (!except.Contains(gridRow, gridCol)) solverData.ChangeBuffer.ProposePossibilityRemoval(n, row, col);
                }
            }
        }
        
        return solverData.ChangeBuffer.Commit( new MiniGridNakedPossibilitiesReportBuilder(toRemove,
            except)) && StopOnFirstPush;
    }
}

public class LineNakedPossibilitiesReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly ReadOnlyBitSet16 _possibilities;
    private readonly LinePositions _linePos;
    private readonly int _unitNumber;
    private readonly Unit _unit;


    public LineNakedPossibilitiesReportBuilder(ReadOnlyBitSet16 possibilities, LinePositions linePos, int unitNumber, Unit unit)
    {
        _possibilities = possibilities;
        _linePos = linePos;
        _unitNumber = unitNumber;
        _unit = unit;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        var cells = _linePos.ToCellArray(_unit, _unitNumber);
        
        return new ChangeReport<ISudokuHighlighter>(ChangeReportHelper.XSetStrategyDescription(cells, "Naked",
            _linePos.Count, _possibilities.EnumeratePossibilities()), lighter =>
        {
            foreach (var possibility in _possibilities.EnumeratePossibilities())
            {
                foreach (var cell in cells)
                {
                    if(snapshot.PossibilitiesAt(cell).Contains(possibility))
                        lighter.HighlightPossibility(possibility, cell.Row, cell.Column, ChangeColoration.CauseOffOne);
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

public class MiniGridNakedPossibilitiesReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly ReadOnlyBitSet16 _possibilities;
    private readonly MiniGridPositions _miniPos;

    public MiniGridNakedPossibilitiesReportBuilder(ReadOnlyBitSet16 possibilities, MiniGridPositions miniPos)
    {
        _possibilities = possibilities;
        _miniPos = miniPos;
    }
    
    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        var cells = _miniPos.ToCellArray();
        
        return new ChangeReport<ISudokuHighlighter>(ChangeReportHelper.XSetStrategyDescription(cells, "Naked",
            _miniPos.Count, _possibilities.EnumeratePossibilities()), lighter =>
        {
            foreach (var pos in _miniPos)
            {
                foreach (var possibility in _possibilities.EnumeratePossibilities())
                {
                    if(snapshot.PossibilitiesAt(pos.Row, pos.Column).Contains(possibility))
                        lighter.HighlightPossibility(possibility, pos.Row, pos.Column, ChangeColoration.CauseOffOne);
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