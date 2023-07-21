using System;
using System.Collections.Generic;
using Model.Positions;
using Model.Possibilities;

namespace Model.Strategies;

// ReSharper disable once InconsistentNaming
public class XYZWingStrategy : IStrategy
{
    public string Name { get; } = "XYZWing";
    
    public StrategyLevel Difficulty { get; } = StrategyLevel.Hard;
    public int Score { get; set; }

    /// <summary>
    /// Conditions for xyz wing :
    /// -One cell is a triple and two cells are doubles
    /// -Each double must have a different combination of the triple's possibilities
    /// -Not all in the same unit
    /// -Each double share a unit with the triple
    /// </summary>
    /// <param name="solverView"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void ApplyOnce(ISolverView solverView)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (solverView.Possibilities[row, col].Count == 3)
                {
                    IPossibilities hinge = solverView.Possibilities[row, col];

                    LinePositions rowCandidates = CandidateForXyzWingInRow(solverView, row, hinge);
                    LinePositions colCandidates = CandidateForXyzWingInColumn(solverView, col, hinge);
                    MiniGridPositions miniGridCandidates = CandidateForXyzWingInMiniGrid(solverView, row / 3, col / 3, hinge);
                    
                    //Rows
                    foreach (var candidateCol in rowCandidates)
                    {
                        foreach (var pos in miniGridCandidates)
                        {
                            if (ShareOnlyOne(solverView.Possibilities[row, candidateCol],
                                    solverView.Possibilities[pos[0], pos[1]]) &&
                                !AreAllInSameUnit(row, col, row,
                                    candidateCol, pos[0], pos[1]) &&
                                Process(solverView, row, col, row,
                                    candidateCol, pos[0], pos[1])) return;
                        }
                    }
                    
                    //Cols
                    foreach (var candidateRow in colCandidates)
                    {
                        foreach (var pos in miniGridCandidates)
                        {
                            if (ShareOnlyOne(solverView.Possibilities[candidateRow, col],
                                    solverView.Possibilities[pos[0], pos[1]]) &&
                                !AreAllInSameUnit(row, col, candidateRow,
                                    col, pos[0], pos[1]) &&
                                Process(solverView, row, col, candidateRow,
                                    col, pos[0], pos[1])) return;
                        }
                    }
                }
            }
        }
    }

    private LinePositions CandidateForXyzWingInRow(ISolverView solverView, int row, IPossibilities hinge)
    {
        LinePositions result = new();
        for (int col = 0; col < 9; col++)
        {
            if (solverView.Possibilities[row, col].Count == 2 && IsSubset(solverView.Possibilities[row, col], hinge))
            {
                result.Add(col);
            }
        }

        return result;
    }
    
    private LinePositions CandidateForXyzWingInColumn(ISolverView solverView, int col, IPossibilities hinge)
    {
        LinePositions result = new();
        for (int row = 0; row < 9; row++)
        {
            if (solverView.Possibilities[row, col].Count == 2 && IsSubset(solverView.Possibilities[row, col], hinge))
            {
                result.Add(row);
            }
        }

        return result;
    }

    private MiniGridPositions CandidateForXyzWingInMiniGrid(ISolverView solverView, int miniRow, int miniCol, IPossibilities hinge)
    {
        MiniGridPositions result = new(miniRow, miniCol);
        for (int gridRow = 0; gridRow < 3; gridRow++)
        {
            for (int gridCol = 0; gridCol < 3; gridCol++)
            {
                int row = miniRow * 3 + gridRow;
                int col = miniCol * 3 + gridCol;
                
                if (solverView.Possibilities[row, col].Count == 2 && IsSubset(solverView.Possibilities[row, col], hinge))
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

    private bool Process(ISolverView solverView, int hingeRow, int hingeCol, int row1, int col1, int row2, int col2)
    {
        bool wasProgressMade = false;

        int toRemove = OneInCommon(solverView.Possibilities[hingeRow, hingeCol], solverView.Possibilities[row1, col1],
            solverView.Possibilities[row2, col2]);
        foreach (var pos in MatchingCells(hingeRow, hingeCol, row1, col1, row2, col2))
        {
            if (solverView.RemovePossibility(toRemove, pos[0], pos[1], this)) wasProgressMade = true;
        }

        return wasProgressMade;
    }

    private IEnumerable<int[]> MatchingCells(int hingeRow, int hingeCol, int row1, int col1, int row2, int col2)
    {
        //Note : One of the coordinate has to be in the same box as the hinge
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