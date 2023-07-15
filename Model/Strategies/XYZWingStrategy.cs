using System;
using System.Collections.Generic;

namespace Model.Strategies;

// ReSharper disable once InconsistentNaming
public class XYZWingStrategy : IStrategy
{
    /// <summary>
    /// Conditions for xyz wing :
    /// -One cell is a triple and two cells are doubles
    /// -Each double must have a different combination of the triple's possibilities
    /// -Not all in the same unit
    /// -Each double share a unit with the triple
    /// </summary>
    /// <param name="solver"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void ApplyOnce(ISolver solver)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (solver.Possibilities[row, col].Count == 3)
                {
                    IPossibilities hinge = solver.Possibilities[row, col];

                    Positions rowCandidates = CandidateForXyzWingInRow(solver, row, hinge);
                    Positions colCandidates = CandidateForXyzWingInColumn(solver, col, hinge);
                    List<int[]> miniGridCandidates = CandidateForXyzWingInMiniGrid(solver, row / 3, col / 3, hinge);
                    
                    //Rows
                    foreach (var candidateCol in rowCandidates)
                    {
                        foreach (var pos in miniGridCandidates)
                        {
                            if (ShareOnlyOne(solver.Possibilities[row, candidateCol],
                                    solver.Possibilities[pos[0], pos[1]]) &&
                                !AreAllInSameUnit(row, col, row,
                                    candidateCol, pos[0], pos[1]) &&
                                Process(solver, row, col, row,
                                    candidateCol, pos[0], pos[1])) return;
                        }
                    }
                    
                    //Cols
                    foreach (var candidateRow in colCandidates)
                    {
                        foreach (var pos in miniGridCandidates)
                        {
                            if (ShareOnlyOne(solver.Possibilities[candidateRow, col],
                                    solver.Possibilities[pos[0], pos[1]]) &&
                                !AreAllInSameUnit(row, col, candidateRow,
                                    col, pos[0], pos[1]) &&
                                Process(solver, row, col, candidateRow,
                                    col, pos[0], pos[1])) return;
                        }
                    }
                }
            }
        }
    }

    private Positions CandidateForXyzWingInRow(ISolver solver, int row, IPossibilities hinge)
    {
        Positions result = new();
        for (int col = 0; col < 9; col++)
        {
            if (solver.Possibilities[row, col].Count == 2 && IsSubset(solver.Possibilities[row, col], hinge))
            {
                result.Add(col);
            }
        }

        return result;
    }
    
    private Positions CandidateForXyzWingInColumn(ISolver solver, int col, IPossibilities hinge)
    {
        Positions result = new();
        for (int row = 0; row < 9; row++)
        {
            if (solver.Possibilities[row, col].Count == 2 && IsSubset(solver.Possibilities[row, col], hinge))
            {
                result.Add(row);
            }
        }

        return result;
    }

    private List<int[]> CandidateForXyzWingInMiniGrid(ISolver solver, int miniRow, int miniCol, IPossibilities hinge)
    {
        List<int[]> result = new();
        for (int gridRow = 0; gridRow < 3; gridRow++)
        {
            for (int gridCol = 0; gridCol < 3; gridCol++)
            {
                int row = miniRow * 3 + gridRow;
                int col = miniCol * 3 + gridCol;
                
                if (solver.Possibilities[row, col].Count == 2 && IsSubset(solver.Possibilities[row, col], hinge))
                {
                    result.Add(new[] {row, col});
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

    private bool Process(ISolver solver, int hingeRow, int hingeCol, int row1, int col1, int row2, int col2)
    {
        bool wasProgressMade = false;

        int toRemove = OneInCommon(solver.Possibilities[hingeRow, hingeCol], solver.Possibilities[row1, col1],
            solver.Possibilities[row2, col2]);
        foreach (var pos in MatchingCells(hingeRow, hingeCol, row1, col1, row2, col2))
        {
            if (solver.RemovePossibility(toRemove, pos[0], pos[1],
                new XYZWingLog(toRemove, pos[0], pos[1], hingeRow, hingeCol,
                    row1, col1, row2, col2))) wasProgressMade = true;
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

public class XYZWingLog : ISolverLog
{
    public string AsString { get; }
    public StrategyLevel Level { get; } = StrategyLevel.Hard;

    public XYZWingLog(int number, int row, int col, int hingeRow, int hingeCol,
        int row1, int col1, int row2, int col2)
    {
        AsString = $"[{row + 1}, {col + 1}] {number} removed from possibilities because of XYZ-Wing at " +
                   $"[{hingeRow + 1}, {hingeCol + 1}], [{row1 + 1}, {col1 + 1}] and [{row2 + 1}, {col2 + 1}]";
    }
}