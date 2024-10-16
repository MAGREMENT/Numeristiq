﻿using System;
using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Position;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies;

public class ReverseBUGLiteStrategy : SudokuStrategy
{
    public const string OfficialName = "Reverse BUG-Lite";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    public ReverseBUGLiteStrategy() : base(OfficialName, Difficulty.Medium, DefaultInstanceHandling)
    {
        UniquenessDependency = UniquenessDependency.FullyDependent;
    }
    
    public override void Apply(ISudokuSolverData solverData)
    {
        for (int row1 = 0; row1 < 9; row1++)
        {
            var startR = row1 / 3 * 3;
            var miniR = row1 % 3;
            
            for (int r = miniR + 1; r < 3; r++)
            {
                var row2 = startR + r;
                
                var poss1 = new ReadOnlyBitSet16();
                var poss2 = new ReadOnlyBitSet16();

                var cols1 = new LinePositions();
                var cols2 = new LinePositions();

                for (int col = 0; col < 9; col++)
                {
                    var current = solverData.Sudoku[row1, col];
                    if (current != 0)
                    {
                        poss1 += current;
                        cols1.Add(col);
                    }
                    
                    current = solverData.Sudoku[row2, col];
                    if (current != 0)
                    {
                        poss2 += current;
                        cols2.Add(col);
                    }
                }
                
                var solo = (poss1 | poss2) - (poss1 & poss2);
                if (solo.Count != 1) continue;

                var or = cols1.Or(cols2);
                if (or.Count != Math.Max(cols1.Count, cols2.Count)) continue;

                var p = solo.FirstPossibility();
                foreach (var col in or)
                {
                    if (!cols1.Contains(col))
                    {
                        solverData.ChangeBuffer.ProposePossibilityRemoval(p, row1, col);
                        break;
                    }

                    if (!cols2.Contains(col))
                    {
                        solverData.ChangeBuffer.ProposePossibilityRemoval(p, row2, col);
                        break;
                    }
                }

                if (solverData.ChangeBuffer.NeedCommit())
                {
                    solverData.ChangeBuffer.Commit(new ReverseBUGLiteChangeReport(row1, row2, cols1, cols2, Unit.Row));
                    if (StopOnFirstCommit) return;
                }
            }
        }
        
        for (int col1 = 0; col1 < 9; col1++)
        {
            var startC = col1 / 3 * 3;
            var miniC = col1 % 3;
            
            for (int c = miniC + 1; c < 3; c++)
            {
                var col2 = startC + c;

                var poss1 = new ReadOnlyBitSet16();
                var poss2 = new ReadOnlyBitSet16();

                var rows1 = new LinePositions();
                var rows2 = new LinePositions();

                for (int row = 0; row < 9; row++)
                {
                    var current = solverData.Sudoku[row, col1];
                    if (current != 0)
                    {
                        poss1 += current;
                        rows1.Add(row);
                    }
                    
                    current = solverData.Sudoku[row, col2];
                    if (current != 0)
                    {
                        poss2 += current;
                        rows2.Add(row);
                    }
                }

                var solo = (poss1 | poss2) - (poss1 & poss2);
                if (solo.Count != 1) continue;

                var or = rows1.Or(rows2);
                if (or.Count != Math.Max(rows1.Count, rows2.Count)) continue;

                var p = solo.FirstPossibility();
                foreach (var row in or)
                {
                    if (!rows1.Contains(row))
                    {
                        solverData.ChangeBuffer.ProposePossibilityRemoval(p, row, col1);
                        break;
                    }

                    if (!rows2.Contains(row))
                    {
                        solverData.ChangeBuffer.ProposePossibilityRemoval(p, row, col2);
                        break;
                    }
                }

                if (solverData.ChangeBuffer.NeedCommit())
                {
                    solverData.ChangeBuffer.Commit(new ReverseBUGLiteChangeReport(col1, col2, rows1, rows2, Unit.Column));
                    if (StopOnFirstCommit) return;
                }
            }
        }
    }
}

public class ReverseBUGLiteChangeReport : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly int _unit1;
    private readonly int _unit2;
    private readonly LinePositions _others1;
    private readonly LinePositions _others2;
    private readonly Unit _unit;

    public ReverseBUGLiteChangeReport(int unit1, int unit2, LinePositions others1, LinePositions others2, Unit unit)
    {
        _unit1 = unit1;
        _unit2 = unit2;
        _others1 = others1;
        _others2 = others2;
        _unit = unit;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>( "", lighter =>
        {
            foreach (var o in _others1)
            {
                if (_unit == Unit.Row) lighter.HighlightCell(_unit1, o, StepColor.Cause1);
                else lighter.HighlightCell(o, _unit1, StepColor.Cause1);
            }
            
            foreach (var o in _others2)
            {
                if (_unit == Unit.Row) lighter.HighlightCell(_unit2, o, StepColor.Cause1);
                else lighter.HighlightCell(o, _unit2, StepColor.Cause1);
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}