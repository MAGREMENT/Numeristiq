using System;
using System.Collections.Generic;
using Model.Solver.Helpers;
using Model.Solver.Helpers.Changes;
using Model.Solver.Positions;
using Model.Solver.Possibilities;
using Model.Solver.StrategiesUtil;
using Model.StrategiesUtil;

namespace Model.Solver.Strategies;

public class WXYZWingStrategy : IStrategy
{
    public string Name => "WXYZWing";
    public StrategyLevel Difficulty => StrategyLevel.Hard;
    public StatisticsTracker Tracker { get; } = new();
    public void ApplyOnce(IStrategyManager strategyManager)
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
                    var first = strategyManager.PossibilitiesAt(iRow, iCol);
                    if (first.Count is > 4 or < 1) continue;
                    
                    var miniPositions = new MiniGridPositions(miniRow, miniCol);
                    miniPositions.Add(i / 3, i % 3);

                    for (int j = i + 1; j < 9; j++)
                    {
                        int jRow = startRow + j / 3;
                        int jCol = startCol + j % 3;
                        var second = strategyManager.PossibilitiesAt(jRow, jCol);
                        if(second.Count is > 4 or < 1) continue;

                        second = second.Or(first);
                        if(second.Count > 4) continue;

                        var miniPositions2 = miniPositions.Copy();
                        miniPositions2.Add(j / 3, j % 3);

                        foreach (var cell in miniPositions2)
                        {
                            if(SearchRow(strategyManager, miniPositions2, second, cell)) return;
                            if(SearchColumn(strategyManager, miniPositions2, second, cell)) return;
                        }

                        for (int k = j + 1; k < 9; k++)
                        {
                            int kRow = startRow + j / 3;
                            int kCol = startCol + j % 3;
                            var third = strategyManager.PossibilitiesAt(kRow, kCol);
                            if(third.Count is > 4 or < 1) continue;

                            third = second.Or(third);
                            if(third.Count > 4) continue;

                            var miniPositions3 = miniPositions2.Copy();
                            miniPositions3.Add(j / 3, j % 3);

                            foreach (var cell in miniPositions3)
                            {
                                if(SearchRow(strategyManager, miniPositions3, third, cell)) return;
                                if(SearchColumn(strategyManager, miniPositions3, third, cell)) return;
                            }
                        }
                    }
                }
            }
        }
    }

    private bool SearchRow(IStrategyManager strategyManager, MiniGridPositions miniPositions,
        IReadOnlyPossibilities possibilities, Cell hinge)
    {
        for (int col = 0; col < 9; col++)
        {
            if(col / 3 == hinge.Col / 3) continue;

            var third = strategyManager.PossibilitiesAt(hinge.Row, col);
            if(third.Count is > 4 or < 1) continue;

            third = possibilities.Or(third);
            if (third.Count > 4) continue;

            var rowPositions = new LinePositions();
            rowPositions.Add(col);

            if (miniPositions.Count + rowPositions.Count == 4)
            {
                if(Process(strategyManager, miniPositions, rowPositions, Unit.Row, hinge.Row, third)) return true;
            }
            else
            {
                for (int otherCol = col + 1; otherCol < 9; otherCol++)
                {
                    if(otherCol / 3 == hinge.Col / 3) continue;
                    
                    var fourth = strategyManager.PossibilitiesAt(hinge.Row, otherCol);
                    if(fourth.Count is > 4 or < 1) continue;

                    fourth = fourth.Or(third);
                    if (fourth.Count > 4) continue;

                    var rowPositions2 = rowPositions.Copy();
                    rowPositions2.Add(otherCol);

                    if (Process(strategyManager, miniPositions, rowPositions2, Unit.Row, hinge.Row, fourth))
                        return true;
                }
            }
        }

        return false;
    }
    
    private bool SearchColumn(IStrategyManager strategyManager, MiniGridPositions miniPositions,
        IReadOnlyPossibilities possibilities, Cell hinge)
    {
        for (int row = 0; row < 9; row++)
        {
            if(row / 3 == hinge.Row / 3) continue;

            var third = strategyManager.PossibilitiesAt(row, hinge.Col);
            if(third.Count is > 4 or < 1) continue;

            third = possibilities.Or(third);
            if (third.Count > 4) continue;

            var rowPositions = new LinePositions();
            rowPositions.Add(row);

            if (miniPositions.Count + rowPositions.Count == 4)
            {
                if(Process(strategyManager, miniPositions, rowPositions, Unit.Column, hinge.Col, third)) return true;
            }
            else
            {
                for (int otherRow = row + 1; otherRow < 9; otherRow++)
                {
                    if(otherRow / 3 == hinge.Row / 3) continue;
                    
                    var fourth = strategyManager.PossibilitiesAt(otherRow, hinge.Col);
                    if(fourth.Count is > 4 or < 1) continue;

                    fourth = fourth.Or(third);
                    if (fourth.Count > 4) continue;

                    var rowPositions2 = rowPositions.Copy();
                    rowPositions2.Add(otherRow);

                    if (Process(strategyManager, miniPositions, rowPositions2, Unit.Column, hinge.Col, fourth))
                        return true;
                }
            }
        }

        return false;
    }

    private bool Process(IStrategyManager strategyManager, MiniGridPositions miniPositions, LinePositions linePositions,
        Unit unit, int unitNumber, IReadOnlyPossibilities possibilities)
    {
        if (possibilities.Count != 4) return false;
        
        int buffer = -1;

        foreach (var possibility in possibilities)
        {
            SharedUnits? sharedUnits = null;
            bool nope = false;
            
            foreach (var current in miniPositions)
            {
                if (!strategyManager.PossibilitiesAt(current.Row, current.Col).Peek(possibility)) continue;

                if (sharedUnits is null) sharedUnits = new SharedUnits(current);
                else
                {
                    sharedUnits.IsShared(current);
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
                
                if (!strategyManager.PossibilitiesAt(current.Row, current.Col).Peek(possibility)) continue;

                if (sharedUnits is null) sharedUnits = new SharedUnits(current);
                else
                {
                    sharedUnits.IsShared(current);
                    if (sharedUnits.Count != 0) continue;

                    if (buffer != -1) return false;
                    
                    buffer = possibility;
                    break;

                }
            }
        }

        if (buffer != -1)
        {
            List<Cell> cells = new();
            foreach (var cell in miniPositions)
            {
                if (strategyManager.PossibilitiesAt(cell.Row, cell.Col).Peek(buffer)) cells.Add(cell);
            }
            
            foreach (var other in linePositions)
            {
                Cell cell = unit switch
                {
                    Unit.Row => new Cell(unitNumber, other),
                    Unit.Column => new Cell(other, unitNumber),
                    _ => throw new Exception()
                };
                
                if (strategyManager.PossibilitiesAt(cell.Row, cell.Col).Peek(buffer)) cells.Add(cell);
            }

            if (cells.Count == 1) return false;

            foreach (var coord in Cells.SharedSeenCells(cells))
            {
                strategyManager.ChangeBuffer.AddPossibilityToRemove(buffer, coord.Row, coord.Col);
            }

            return strategyManager.ChangeBuffer.Push(this,
                new WXYZWingReportBuilder(miniPositions, linePositions, unit, unitNumber));
        }

        return false;
    }
}

public class WXYZWingReportBuilder : IChangeReportBuilder
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

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
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

        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            foreach (var cell in cells)
            {
                lighter.HighlightCell(cell, ChangeColoration.CauseOffOne);
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}