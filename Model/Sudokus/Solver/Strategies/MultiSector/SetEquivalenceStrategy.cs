using System;
using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies.MultiSector;

public class SetEquivalenceStrategy : SudokuStrategy
{
    public const string OfficialName = "Set Equivalence";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    private readonly ISetEquivalenceSearcher[] _searchers;
    
    public SetEquivalenceStrategy(params ISetEquivalenceSearcher[] searchers)
        : base(OfficialName, Difficulty.Extreme, DefaultInstanceHandling)
    {
        _searchers = searchers;
    }

    public override void Apply(ISudokuSolverData solverData)
    {
        foreach (var searcher in _searchers)
        {
            foreach (var equivalence in searcher.Search(solverData))
            {
                int[] solved1 = new int[9];
                int[] solved2 = new int[9];
                int count1 = 0;
                int count2 = 0;
                int removed = 0;

                foreach (var cell in equivalence.FirstSet)
                {
                    var solved = solverData.Sudoku[cell.Row, cell.Column];
                    if (solved == 0) continue;
                
                    solved1[solved - 1]++;
                    count1++;
                }

                foreach (var cell in equivalence.SecondSet)
                {
                    var solved = solverData.Sudoku[cell.Row, cell.Column];
                    if (solved == 0) continue;
                
                    solved2[solved - 1]++;
                    count2++;
                }
            
                var digitCount1 = 0;
                var digitCount2 = 0;
            
                for (int i = 0; i < 9; i++)
                {
                    var toRemove = Math.Min(solved1[i], solved2[i]);

                    solved1[i] -= toRemove;
                    solved2[i] -= toRemove;
                    count1 -= toRemove;
                    count2 -= toRemove;
                    removed += toRemove;

                    if (solved1[i] > 0) digitCount1++;
                    if (solved2[i] > 0) digitCount2++;
                }

                var order = equivalence.SecondOrder - equivalence.FirstOrder;

                if (count1 + order * digitCount1 >= equivalence.SecondSet.Length - removed - count2)
                {
                    foreach (var cell in equivalence.SecondSet)
                    {
                        var possibilities = solverData.PossibilitiesAt(cell);
                        if(possibilities.Count == 0) continue;

                        foreach (var possibility in possibilities.EnumeratePossibilities())
                        {
                            if (solved1[possibility - 1] == 0)
                                solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, cell.Row, cell.Column);
                        }
                    }
                }

                if (count2 - order * digitCount2 >= equivalence.FirstSet.Length - removed - count1)
                {
                    foreach (var cell in equivalence.FirstSet)
                    {
                        var possibilities = solverData.PossibilitiesAt(cell);
                        if(possibilities.Count == 0) continue;

                        foreach (var possibility in possibilities.EnumeratePossibilities())
                        {
                            if (solved2[possibility - 1] == 0)
                                solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, cell.Row, cell.Column);
                        }
                    }
                }

                if (solverData.ChangeBuffer.NeedCommit())
                {
                    solverData.ChangeBuffer.Commit(new SetEquivalenceReportBuilder(equivalence));
                    if (StopOnFirstCommit) return;
                }
            }
        }
    }
}

public class SetEquivalence
{
    public SetEquivalence(Cell[] firstSet, int firstOrder, Cell[] secondSet, int secondOrder)
    {
        FirstSet = firstSet;
        FirstOrder = firstOrder;
        SecondSet = secondSet;
        SecondOrder = secondOrder;
    }

    public Cell[] FirstSet { get; }
    public int FirstOrder { get; }
    
    public Cell[] SecondSet { get; }
    public int SecondOrder { get; }
}

public class SetEquivalenceReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly SetEquivalence _equivalence;

    public SetEquivalenceReportBuilder(SetEquivalence equivalence)
    {
        _equivalence = equivalence;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>( "", lighter =>
        {
            foreach (var cell in _equivalence.FirstSet)
            {
                lighter.HighlightCell(cell, StepColor.On);
            }
            
            foreach (var cell in _equivalence.SecondSet)
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