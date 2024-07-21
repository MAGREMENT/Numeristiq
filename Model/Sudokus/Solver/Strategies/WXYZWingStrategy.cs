using System;
using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Position;
using Model.Sudokus.Solver.Utility;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies;

public class WXYZWingStrategy : SudokuStrategy
{
    public const string OfficialName = "WXYZ-Wing";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    public WXYZWingStrategy() : base(OfficialName, StepDifficulty.Hard, DefaultInstanceHandling) {}
    
    public override void Apply(ISudokuSolverData solverData)
    {
        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                int startRow = miniRow * 3;
                int startCol = miniCol * 3;
                
                for (int i = 0; i < 9; i++)
                {
                    int iRow = startRow + i / 3;
                    int iCol = startCol + i % 3;
                    var first = solverData.PossibilitiesAt(iRow, iCol);
                    if (first.Count is > 4 or < 1) continue;
                    
                    var miniPositions = new BoxPositions(miniRow, miniCol);
                    miniPositions.Add(i / 3, i % 3);

                    for (int j = i + 1; j < 9; j++)
                    {
                        int jRow = startRow + j / 3;
                        int jCol = startCol + j % 3;
                        var second = solverData.PossibilitiesAt(jRow, jCol);
                        if(second.Count is > 4 or < 1) continue;

                        second |= first;
                        if(second.Count > 4) continue;

                        var miniPositions2 = miniPositions.Copy();
                        miniPositions2.Add(j / 3, j % 3);

                        foreach (var cell in miniPositions2)
                        {
                            if(SearchRow(solverData, miniPositions2, second, cell)) return;
                            if(SearchColumn(solverData, miniPositions2, second, cell)) return;
                        }

                        for (int k = j + 1; k < 9; k++)
                        {
                            int kRow = startRow + j / 3;
                            int kCol = startCol + j % 3;
                            var third = solverData.PossibilitiesAt(kRow, kCol);
                            if(third.Count is > 4 or < 1) continue;

                            third |= second;
                            if(third.Count > 4) continue;

                            var miniPositions3 = miniPositions2.Copy();
                            miniPositions3.Add(j / 3, j % 3);

                            foreach (var cell in miniPositions3)
                            {
                                if(SearchRow(solverData, miniPositions3, third, cell)) return;
                                if(SearchColumn(solverData, miniPositions3, third, cell)) return;
                            }
                        }
                    }
                }
            }
        }
    }

    private bool SearchRow(ISudokuSolverData solverData, BoxPositions miniPositions,
        ReadOnlyBitSet16 possibilities, Cell hinge)
    {
        for (int col = 0; col < 9; col++)
        {
            if(col / 3 == hinge.Column / 3) continue;

            var third = solverData.PossibilitiesAt(hinge.Row, col);
            if(third.Count is > 4 or < 1) continue;

            third |= possibilities;
            if (third.Count > 4) continue;

            var rowPositions = new LinePositions();
            rowPositions.Add(col);

            if (miniPositions.Count + rowPositions.Count == 4)
            {
                if(Process(solverData, miniPositions, rowPositions, Unit.Row, hinge.Row, third)) return true;
            }
            else
            {
                for (int otherCol = col + 1; otherCol < 9; otherCol++)
                {
                    if(otherCol / 3 == hinge.Column / 3) continue;
                    
                    var fourth = solverData.PossibilitiesAt(hinge.Row, otherCol);
                    if(fourth.Count is > 4 or < 1) continue;

                    fourth |= third;
                    if (fourth.Count > 4) continue;

                    var rowPositions2 = rowPositions.Copy();
                    rowPositions2.Add(otherCol);

                    if (Process(solverData, miniPositions, rowPositions2, Unit.Row, hinge.Row, fourth))
                        return true;
                }
            }
        }

        return false;
    }
    
    private bool SearchColumn(ISudokuSolverData solverData, BoxPositions miniPositions,
        ReadOnlyBitSet16 possibilities, Cell hinge)
    {
        for (int row = 0; row < 9; row++)
        {
            if(row / 3 == hinge.Row / 3) continue;

            var third = solverData.PossibilitiesAt(row, hinge.Column);
            if(third.Count is > 4 or < 1) continue;

            third |= possibilities;
            if (third.Count > 4) continue;

            var rowPositions = new LinePositions();
            rowPositions.Add(row);

            if (miniPositions.Count + rowPositions.Count == 4)
            {
                if(Process(solverData, miniPositions, rowPositions, Unit.Column, hinge.Column, third)) return true;
            }
            else
            {
                for (int otherRow = row + 1; otherRow < 9; otherRow++)
                {
                    if(otherRow / 3 == hinge.Row / 3) continue;
                    
                    var fourth = solverData.PossibilitiesAt(otherRow, hinge.Column);
                    if(fourth.Count is > 4 or < 1) continue;

                    fourth |= third;
                    if (fourth.Count > 4) continue;

                    var rowPositions2 = rowPositions.Copy();
                    rowPositions2.Add(otherRow);

                    if (Process(solverData, miniPositions, rowPositions2, Unit.Column, hinge.Column, fourth))
                        return true;
                }
            }
        }

        return false;
    }

    private bool Process(ISudokuSolverData solverData, BoxPositions miniPositions, LinePositions linePositions,
        Unit unit, int unitNumber, ReadOnlyBitSet16 possibilities)
    {
        if (possibilities.Count != 4) return false;
        
        int buffer = -1;

        foreach (var possibility in possibilities.EnumeratePossibilities())
        {
            SharedHouses? sharedUnits = null;
            bool nope = false;
            
            foreach (var current in miniPositions)
            {
                if (!solverData.PossibilitiesAt(current.Row, current.Column).Contains(possibility)) continue;

                if (sharedUnits is null) sharedUnits = new SharedHouses(current);
                else
                {
                    sharedUnits.Share(current);
                    if (sharedUnits.Count != 0) continue;

                    if (buffer != -1) return false;
                    
                    buffer = possibility;
                    nope = true;
                    break;
                }
            }

            if (nope) break;

            foreach (var other in linePositions)
            {
                Cell current = unit switch
                {
                    Unit.Row => new Cell(unitNumber, other),
                    Unit.Column => new Cell(other, unitNumber),
                    _ => throw new Exception()
                };
                
                if (!solverData.PossibilitiesAt(current.Row, current.Column).Contains(possibility)) continue;

                if (sharedUnits is null) sharedUnits = new SharedHouses(current);
                else
                {
                    sharedUnits.Share(current);
                    if (sharedUnits.Count != 0) continue;

                    if (buffer != -1) return false;
                    
                    buffer = possibility;
                    break;

                }
            }
        }

        if (buffer == -1) return false;
        
        List<Cell> cells = new();
        foreach (var cell in miniPositions)
        {
            if (solverData.PossibilitiesAt(cell.Row, cell.Column).Contains(buffer)) cells.Add(cell);
        }
            
        foreach (var other in linePositions)
        {
            Cell cell = unit switch
            {
                Unit.Row => new Cell(unitNumber, other),
                Unit.Column => new Cell(other, unitNumber),
                _ => throw new Exception()
            };
                
            if (solverData.PossibilitiesAt(cell.Row, cell.Column).Contains(buffer)) cells.Add(cell);
        }

        if (cells.Count == 1) return false;

        foreach (var coord in SudokuCellUtility.SharedSeenCells(cells))
        {
            solverData.ChangeBuffer.ProposePossibilityRemoval(buffer, coord.Row, coord.Column);
        }

        return solverData.ChangeBuffer.Commit(
            new WXYZWingReportBuilder(miniPositions, linePositions, unit, unitNumber))
            && StopOnFirstPush;
        
    }
}

public class WXYZWingReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly BoxPositions _mini;
    private readonly LinePositions _line;
    private readonly Unit _unit;
    private readonly int _unitNumber;


    public WXYZWingReportBuilder(BoxPositions mini, LinePositions line, Unit unit, int unitNumber)
    {
        _mini = mini;
        _line = line;
        _unit = unit;
        _unitNumber = unitNumber;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        List<Cell> cells = new();

        foreach (var cell in _mini)
        {
            cells.Add(cell);
        }
        foreach (var other in _line)
        {
            cells.Add(_unit switch
            {
                Unit.Row => new Cell(_unitNumber, other),
                Unit.Column => new Cell(other, _unitNumber),
                _ => throw new Exception()
            });
        }

        return new ChangeReport<ISudokuHighlighter>( "", lighter =>
        {
            foreach (var cell in cells)
            {
                lighter.HighlightCell(cell, StepColor.Cause1);
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}