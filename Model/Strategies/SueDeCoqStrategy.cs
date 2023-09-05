using System.Collections.Generic;
using System.Linq;
using Model.Changes;
using Model.Positions;
using Model.Possibilities;
using Model.Solver;
using Model.StrategiesUtil;

namespace Model.Strategies;

public class SueDeCoqStrategy : IStrategy
{
    public string Name => "Sue-De-Coq";
    public StrategyLevel Difficulty => StrategyLevel.Extreme;
    public int Score { get; set; }
    public void ApplyOnce(IStrategyManager strategyManager)
    {
        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                for (int gridRow = 0; gridRow < 3; gridRow++)
                {
                    int row = miniRow * 3 + gridRow;
                    IPossibilities possibilities = IPossibilities.NewEmpty();
                    LinePositions cols = new LinePositions();

                    for (int gridCol = 0; gridCol < 3; gridCol++)
                    {
                        int col = miniCol * 3 + gridCol;
                        if (strategyManager.Sudoku[row, col] != 0) continue;
                        
                        cols.Add(col);
                        foreach (var possibility in strategyManager.Possibilities[row, col])
                        {
                            possibilities.Add(possibility);
                        }
                    }

                    if (cols.Count < 2) continue;
                    if(possibilities.Count - 2 < cols.Count) continue;

                    List<Coordinate> rowCoords = new();
                    List<Coordinate> miniCoords = new();
                    for (int col = 0; col < 9; col++)
                    {
                        if (col / 3 == miniCol) continue;
                        if (strategyManager.Sudoku[row, col] != 0) continue;

                        rowCoords.Add(new Coordinate(row, col));
                    }

                    for (int gridRow2 = 0; gridRow2 < 3; gridRow2++)
                    {
                        if (gridRow2 == gridRow) continue;
                        for (int gridCol = 0; gridCol < 3; gridCol++)
                        {
                            int row2 = miniRow * 3 + gridRow2;
                            int col = miniCol * 3 + gridCol;
                            if (strategyManager.Sudoku[row2, col] != 0) continue;

                            miniCoords.Add(new Coordinate(row2, col));
                        }
                    }

                    var rowAls = AlmostLockedSet.SearchForAls(strategyManager, rowCoords, 4);
                    var miniAls = AlmostLockedSet.SearchForAls(strategyManager, miniCoords, 4);

                    foreach (var rAls in rowAls)
                    {
                        foreach (var mAls in miniAls)
                        {
                            if (rAls.Possibilities.Mash(mAls.Possibilities).Equals(possibilities))
                                ProcessSueDeCoq(strategyManager, row, cols, rAls, mAls, Unit.Row);
                        }
                    }
                }
                
                for (int gridCol = 0; gridCol < 3; gridCol++)
                {
                    int col = miniCol * 3 + gridCol;
                    IPossibilities possibilities = IPossibilities.NewEmpty();
                    LinePositions rows = new LinePositions();

                    for (int gridRow = 0; gridRow < 3; gridRow++)
                    {
                        int row = miniRow * 3 + gridRow;
                        if (strategyManager.Sudoku[row, col] != 0) continue;
                        
                        rows.Add(row);
                        foreach (var possibility in strategyManager.Possibilities[row, col])
                        {
                            possibilities.Add(possibility);
                        }
                    }

                    if (rows.Count < 2) continue;
                    if(possibilities.Count - 2 < rows.Count) continue;

                    List<Coordinate> colCoords = new();
                    List<Coordinate> miniCoords = new();
                    for (int row = 0; row < 9; row++)
                    {
                        if (row / 3 == miniRow) continue;
                        if (strategyManager.Sudoku[row, col] != 0) continue;

                        colCoords.Add(new Coordinate(row, col));
                    }

                    for (int gridCol2 = 0; gridCol2 < 3; gridCol2++)
                    {
                        if (gridCol2 == gridCol) continue;
                        for (int gridRow = 0; gridRow < 3; gridRow++)
                        {
                            int col2 = miniCol * 3 + gridCol2;
                            int row = miniRow * 3 + gridRow;
                            if (strategyManager.Sudoku[row, col2] != 0) continue;

                            miniCoords.Add(new Coordinate(row, col2));
                        }
                    }

                    var colAls = AlmostLockedSet.SearchForAls(strategyManager, colCoords, 4);
                    var miniAls = AlmostLockedSet.SearchForAls(strategyManager, miniCoords, 4);

                    foreach (var cAls in colAls)
                    {
                        foreach (var mAls in miniAls)
                        {
                            if (cAls.Possibilities.Mash(mAls.Possibilities).Equals(possibilities))
                                ProcessSueDeCoq(strategyManager, col, rows, cAls, mAls, Unit.Column);
                        }
                    }
                }
            }
        }
    }

    private void ProcessSueDeCoq(IStrategyManager strategyManager, int unitNumber, LinePositions center,
       AlmostLockedSet unitAls, AlmostLockedSet miniAls, Unit unit)
    {
        for (int other = 0; other < 9; other++)
        {
            Coordinate current = unit == Unit.Row ?
                new Coordinate(unitNumber, other) : new Coordinate(other, unitNumber);
            if(unitAls.Contains(current)) continue;
            if(center.Peek(other)) continue;

            foreach (var possibility in unitAls.Possibilities)
            {
                strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, current.Row, current.Col);
            }
        }

        var unitStart = unitNumber / 3 * 3;
        var otherStart = center.First() / 3 * 3;
        for (int gridUnit = 0; gridUnit < 3; gridUnit++)
        {
            if (gridUnit + unitStart == unitNumber) continue;
            for (int gridOther = 0; gridOther < 3; gridOther++)
            {
                Coordinate current = unit == Unit.Row ?
                    new Coordinate(unitStart + gridUnit, otherStart + gridOther) :
                    new Coordinate(otherStart + gridOther, unitStart + gridUnit);
                if (miniAls.Contains(current)) continue;
                
                foreach (var possibility in miniAls.Possibilities)
                {
                    strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, current.Row, current.Col);
                }
            }
        }

        strategyManager.ChangeBuffer.Push(this, new SueDeCoqReportBuilder(unitNumber, center, miniAls, unitAls, unit));
    }
}

public class SueDeCoqReportBuilder : IChangeReportBuilder
{
    private readonly int _unitNumber;
    private readonly LinePositions _positions;
    private readonly AlmostLockedSet _miniAls;
    private readonly AlmostLockedSet _unitAls;
    private readonly Unit _unit;

    public SueDeCoqReportBuilder(int unitNumber, LinePositions positions, AlmostLockedSet miniAls,
        AlmostLockedSet unitAls, Unit unit)
    {
        _unitNumber = unitNumber;
        _positions = positions;
        _miniAls = miniAls;
        _unitAls = unitAls;
        _unit = unit;
    }

    public ChangeReport Build(List<SolverChange> changes, IChangeManager manager)
    {
        List<Coordinate> center = new(_positions.Count);
        foreach (var other in _positions)
        {
            center.Add(_unit == Unit.Row ?
                new Coordinate(_unitNumber, other) :
                new Coordinate(other, _unitNumber));
        }
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "",
            lighter =>
            {
                foreach (var coord in center)
                {
                    lighter.HighlightCell(coord.Row, coord.Col, ChangeColoration.Neutral);
                }

                foreach (var coord in _unitAls.Coordinates)
                {
                    lighter.HighlightCell(coord.Row, coord.Col, ChangeColoration.CauseOffOne);
                }

                foreach (var coord in _miniAls.Coordinates)
                {
                    lighter.HighlightCell(coord.Row, coord.Col, ChangeColoration.CauseOffTwo);
                }
                IChangeReportBuilder.HighlightChanges(lighter, changes);
            });
    }
}