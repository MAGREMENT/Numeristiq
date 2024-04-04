using System;
using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Highlighting;
using Model.Sudoku.Solver.Position;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudoku.Solver.Strategies;

public class WXYZWingStrategy : SudokuStrategy
{
    public const string OfficialName = "WXYZ-Wing";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    public WXYZWingStrategy() : base(OfficialName, StrategyDifficulty.Hard, DefaultInstanceHandling) {}
    
    public override void Apply(IStrategyUser strategyUser)
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
                    var first = strategyUser.PossibilitiesAt(iRow, iCol);
                    if (first.Count is > 4 or < 1) continue;
                    
                    var miniPositions = new MiniGridPositions(miniRow, miniCol);
                    miniPositions.Add(i / 3, i % 3);

                    for (int j = i + 1; j < 9; j++)
                    {
                        int jRow = startRow + j / 3;
                        int jCol = startCol + j % 3;
                        var second = strategyUser.PossibilitiesAt(jRow, jCol);
                        if(second.Count is > 4 or < 1) continue;

                        second |= first;
                        if(second.Count > 4) continue;

                        var miniPositions2 = miniPositions.Copy();
                        miniPositions2.Add(j / 3, j % 3);

                        foreach (var cell in miniPositions2)
                        {
                            if(SearchRow(strategyUser, miniPositions2, second, cell)) return;
                            if(SearchColumn(strategyUser, miniPositions2, second, cell)) return;
                        }

                        for (int k = j + 1; k < 9; k++)
                        {
                            int kRow = startRow + j / 3;
                            int kCol = startCol + j % 3;
                            var third = strategyUser.PossibilitiesAt(kRow, kCol);
                            if(third.Count is > 4 or < 1) continue;

                            third |= second;
                            if(third.Count > 4) continue;

                            var miniPositions3 = miniPositions2.Copy();
                            miniPositions3.Add(j / 3, j % 3);

                            foreach (var cell in miniPositions3)
                            {
                                if(SearchRow(strategyUser, miniPositions3, third, cell)) return;
                                if(SearchColumn(strategyUser, miniPositions3, third, cell)) return;
                            }
                        }
                    }
                }
            }
        }
    }

    private bool SearchRow(IStrategyUser strategyUser, MiniGridPositions miniPositions,
        ReadOnlyBitSet16 possibilities, Cell hinge)
    {
        for (int col = 0; col < 9; col++)
        {
            if(col / 3 == hinge.Column / 3) continue;

            var third = strategyUser.PossibilitiesAt(hinge.Row, col);
            if(third.Count is > 4 or < 1) continue;

            third |= possibilities;
            if (third.Count > 4) continue;

            var rowPositions = new LinePositions();
            rowPositions.Add(col);

            if (miniPositions.Count + rowPositions.Count == 4)
            {
                if(Process(strategyUser, miniPositions, rowPositions, Unit.Row, hinge.Row, third)) return true;
            }
            else
            {
                for (int otherCol = col + 1; otherCol < 9; otherCol++)
                {
                    if(otherCol / 3 == hinge.Column / 3) continue;
                    
                    var fourth = strategyUser.PossibilitiesAt(hinge.Row, otherCol);
                    if(fourth.Count is > 4 or < 1) continue;

                    fourth |= third;
                    if (fourth.Count > 4) continue;

                    var rowPositions2 = rowPositions.Copy();
                    rowPositions2.Add(otherCol);

                    if (Process(strategyUser, miniPositions, rowPositions2, Unit.Row, hinge.Row, fourth))
                        return true;
                }
            }
        }

        return false;
    }
    
    private bool SearchColumn(IStrategyUser strategyUser, MiniGridPositions miniPositions,
        ReadOnlyBitSet16 possibilities, Cell hinge)
    {
        for (int row = 0; row < 9; row++)
        {
            if(row / 3 == hinge.Row / 3) continue;

            var third = strategyUser.PossibilitiesAt(row, hinge.Column);
            if(third.Count is > 4 or < 1) continue;

            third |= possibilities;
            if (third.Count > 4) continue;

            var rowPositions = new LinePositions();
            rowPositions.Add(row);

            if (miniPositions.Count + rowPositions.Count == 4)
            {
                if(Process(strategyUser, miniPositions, rowPositions, Unit.Column, hinge.Column, third)) return true;
            }
            else
            {
                for (int otherRow = row + 1; otherRow < 9; otherRow++)
                {
                    if(otherRow / 3 == hinge.Row / 3) continue;
                    
                    var fourth = strategyUser.PossibilitiesAt(otherRow, hinge.Column);
                    if(fourth.Count is > 4 or < 1) continue;

                    fourth |= third;
                    if (fourth.Count > 4) continue;

                    var rowPositions2 = rowPositions.Copy();
                    rowPositions2.Add(otherRow);

                    if (Process(strategyUser, miniPositions, rowPositions2, Unit.Column, hinge.Column, fourth))
                        return true;
                }
            }
        }

        return false;
    }

    private bool Process(IStrategyUser strategyUser, MiniGridPositions miniPositions, LinePositions linePositions,
        Unit unit, int unitNumber, ReadOnlyBitSet16 possibilities)
    {
        if (possibilities.Count != 4) return false;
        
        int buffer = -1;

        foreach (var possibility in possibilities.EnumeratePossibilities())
        {
            SharedUnits? sharedUnits = null;
            bool nope = false;
            
            foreach (var current in miniPositions)
            {
                if (!strategyUser.PossibilitiesAt(current.Row, current.Column).Contains(possibility)) continue;

                if (sharedUnits is null) sharedUnits = new SharedUnits(current);
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
                
                if (!strategyUser.PossibilitiesAt(current.Row, current.Column).Contains(possibility)) continue;

                if (sharedUnits is null) sharedUnits = new SharedUnits(current);
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
            if (strategyUser.PossibilitiesAt(cell.Row, cell.Column).Contains(buffer)) cells.Add(cell);
        }
            
        foreach (var other in linePositions)
        {
            Cell cell = unit switch
            {
                Unit.Row => new Cell(unitNumber, other),
                Unit.Column => new Cell(other, unitNumber),
                _ => throw new Exception()
            };
                
            if (strategyUser.PossibilitiesAt(cell.Row, cell.Column).Contains(buffer)) cells.Add(cell);
        }

        if (cells.Count == 1) return false;

        foreach (var coord in Cells.SharedSeenCells(cells))
        {
            strategyUser.ChangeBuffer.ProposePossibilityRemoval(buffer, coord.Row, coord.Column);
        }

        return strategyUser.ChangeBuffer.Commit(
            new WXYZWingReportBuilder(miniPositions, linePositions, unit, unitNumber))
            && StopOnFirstPush;
        
    }
}

public class WXYZWingReportBuilder : IChangeReportBuilder<IUpdatableSudokuSolvingState, ISudokuHighlighter>
{
    private readonly MiniGridPositions _mini;
    private readonly LinePositions _line;
    private readonly Unit _unit;
    private readonly int _unitNumber;


    public WXYZWingReportBuilder(MiniGridPositions mini, LinePositions line, Unit unit, int unitNumber)
    {
        _mini = mini;
        _line = line;
        _unit = unit;
        _unitNumber = unitNumber;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
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
                lighter.HighlightCell(cell, ChangeColoration.CauseOffOne);
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}