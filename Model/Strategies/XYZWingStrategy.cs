using System;
using System.Collections.Generic;
using Model.Changes;
using Model.Positions;
using Model.Possibilities;
using Model.Solver;

namespace Model.Strategies;

public class XYZWingStrategy : IStrategy
{
    public string Name => "XYZWing";
    
    public StrategyLevel Difficulty => StrategyLevel.Medium;
    public StatisticsTracker Tracker { get; } = new();

    /// <summary>
    /// Conditions for xyz wing :
    /// -One cell is a triple and two cells are doubles
    /// -Each double must have a different combination of the triple's possibilities
    /// -Not all in the same unit
    /// -Each double share a unit with the triple
    /// </summary>
    /// <param name="strategyManager"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void ApplyOnce(IStrategyManager strategyManager)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (strategyManager.Possibilities[row, col].Count == 3)
                {
                    IPossibilities hinge = strategyManager.Possibilities[row, col];

                    LinePositions rowCandidates = CandidateForXyzWingInRow(strategyManager, row, hinge);
                    LinePositions colCandidates = CandidateForXyzWingInColumn(strategyManager, col, hinge);
                    MiniGridPositions miniGridCandidates = CandidateForXyzWingInMiniGrid(strategyManager, row / 3, col / 3, hinge);
                    
                    //Rows
                    foreach (var candidateCol in rowCandidates)
                    {
                        foreach (var pos in miniGridCandidates)
                        {
                            if (ShareOnlyOne(strategyManager.Possibilities[row, candidateCol],
                                    strategyManager.Possibilities[pos.Row, pos.Col]) &&
                                !AreAllInSameUnit(row, col, row,
                                    candidateCol, pos.Row, pos.Col) &&
                                Process(strategyManager, row, col, row,
                                    candidateCol, pos.Row, pos.Col)) return;
                        }
                    }
                    
                    //Cols
                    foreach (var candidateRow in colCandidates)
                    {
                        foreach (var pos in miniGridCandidates)
                        {
                            if (ShareOnlyOne(strategyManager.Possibilities[candidateRow, col],
                                    strategyManager.Possibilities[pos.Row, pos.Col]) &&
                                !AreAllInSameUnit(row, col, candidateRow,
                                    col, pos.Row, pos.Col) &&
                                Process(strategyManager, row, col, candidateRow,
                                    col, pos.Row, pos.Col)) return;
                        }
                    }
                }
            }
        }
    }

    private LinePositions CandidateForXyzWingInRow(IStrategyManager strategyManager, int row, IPossibilities hinge)
    {
        LinePositions result = new();
        for (int col = 0; col < 9; col++)
        {
            if (strategyManager.Possibilities[row, col].Count == 2 && IsSubset(strategyManager.Possibilities[row, col], hinge))
            {
                result.Add(col);
            }
        }

        return result;
    }
    
    private LinePositions CandidateForXyzWingInColumn(IStrategyManager strategyManager, int col, IPossibilities hinge)
    {
        LinePositions result = new();
        for (int row = 0; row < 9; row++)
        {
            if (strategyManager.Possibilities[row, col].Count == 2 && IsSubset(strategyManager.Possibilities[row, col], hinge))
            {
                result.Add(row);
            }
        }

        return result;
    }

    private MiniGridPositions CandidateForXyzWingInMiniGrid(IStrategyManager strategyManager, int miniRow, int miniCol, IPossibilities hinge)
    {
        MiniGridPositions result = new(miniRow, miniCol);
        for (int gridRow = 0; gridRow < 3; gridRow++)
        {
            for (int gridCol = 0; gridCol < 3; gridCol++)
            {
                int row = miniRow * 3 + gridRow;
                int col = miniCol * 3 + gridCol;
                
                if (strategyManager.Possibilities[row, col].Count == 2 && IsSubset(strategyManager.Possibilities[row, col], hinge))
                {
                    result.Add(gridRow, gridCol);
                }
            }
        }

        return result;
    }

    private static bool ShareOnlyOne(IPossibilities one, IPossibilities two)
    {
        bool sharedFound = false;
        foreach (var n in one)
        {
            if (two.Peek(n))
            {
                if (!sharedFound) sharedFound = true;
                else return false;
            }
        }

        return sharedFound;
    }

    private bool IsSubset(IPossibilities sub, IPossibilities hinge)
    {
        foreach (var n in sub)
        {
            if (!hinge.Peek(n)) return false;
        }

        return true;
    }
    
    private static bool AreAllInSameUnit(int row1, int col1, int row2, int col2, int row3, int col3)
    {
        return (row1 == row2 && row1 == row3) || (col1 == col2 && col1 == col3) ||
               (row1 / 3 == row2 / 3 && col1 / 3 == col2 / 3 && row1 / 3 == row3 / 3 &&
                col1 / 3 == col3 / 3);
    }

    private bool Process(IStrategyManager strategyManager, int hingeRow, int hingeCol, int row1, int col1, int row2, int col2)
    {
        int toRemove = OneInCommon(strategyManager.Possibilities[hingeRow, hingeCol], strategyManager.Possibilities[row1, col1],
            strategyManager.Possibilities[row2, col2]);
        foreach (var pos in MatchingCells(hingeRow, hingeCol, row1, col1, row2, col2))
        {
            strategyManager.ChangeBuffer.AddPossibilityToRemove(toRemove, pos[0], pos[1]);
        }

        return strategyManager.ChangeBuffer.Push(this,
            new XYZWingReportBuilder(hingeRow, hingeCol, row1, col1, row2, col2));
    }

    private IEnumerable<int[]> MatchingCells(int hingeRow, int hingeCol, int row1, int col1, int row2, int col2)
    {
        //Note : One of the cell has to be in the same box as the hinge
        if (row1 / 3 == hingeRow / 3 && col1 / 3 == hingeCol / 3)
        {
            if (row2 == hingeRow)
            {
                int start = hingeCol / 3 * 3;
                for (int i = 0; i < 3; i++)
                {
                    if (start + i != hingeCol) yield return new[] {row2, start + i};
                }
            }
            else if (col2 == hingeCol)
            {
                int start = hingeRow / 3 * 3;
                for (int i = 0; i < 3; i++)
                {
                    if (start + i != hingeRow) yield return new[] {start + i, col2};
                }
            }
            else throw new Exception("Wtf big problem");
        }
        else if (row2 / 3 == hingeRow / 3 && col2 / 3 == hingeCol / 3)
        {
            if (row1 == hingeRow)
            {
                int start = hingeCol / 3 * 3;
                for (int i = 0; i < 3; i++)
                {
                    if (start + i != hingeCol) yield return new[] {row1, start + i};
                }
            }
            else if (col1 == hingeCol)
            {
                int start = hingeRow / 3 * 3;
                for (int i = 0; i < 3; i++)
                {
                    if (start + i != hingeRow) yield return new[] {start + i, col1};
                }
            }
            else throw new Exception("Wtf big problem");
        }
        else throw new Exception("Wtf big problem");
    }

    private int OneInCommon(IPossibilities hinge, IPossibilities one, IPossibilities two)
    {
        foreach (var n in one)
        {
            if (hinge.Peek(n) && two.Peek(n)) return n;
        }

        throw new Exception("Wtf big problem");
    }
}

public class XYZWingReportBuilder : IChangeReportBuilder
{
    private readonly int _hingeRow;
    private readonly int _hingeCol;
    private readonly int _row1;
    private readonly int _col1;
    private readonly int _row2;
    private readonly int _col2;

    public XYZWingReportBuilder(int hingeRow, int hingeCol, int row1, int col1, int row2, int col2)
    {
        _hingeRow = hingeRow;
        _hingeCol = hingeCol;
        _row1 = row1;
        _col1 = col1;
        _row2 = row2;
        _col2 = col2;
    }
    
    public ChangeReport Build(List<SolverChange> changes, IChangeManager manager)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            lighter.HighlightCell(_hingeRow, _hingeCol, ChangeColoration.CauseOffTwo);
            lighter.HighlightCell(_row1, _col1, ChangeColoration.CauseOffOne);
            lighter.HighlightCell(_row2, _col2, ChangeColoration.CauseOffOne);

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}