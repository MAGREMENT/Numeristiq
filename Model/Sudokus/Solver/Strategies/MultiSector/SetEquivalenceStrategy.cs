using System;
using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Highlighting;
using Model.Utility;

namespace Model.Sudokus.Solver.Strategies.MultiSector;

public class SetEquivalenceStrategy : SudokuStrategy
{
    public const string OfficialName = "Set Equivalence";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    private readonly ISetEquivalenceSearcher[] _searchers;
    
    public SetEquivalenceStrategy(params ISetEquivalenceSearcher[] searchers)
        : base(OfficialName, StrategyDifficulty.Extreme, DefaultInstanceHandling)
    {
        _searchers = searchers;
    }

    public override void Apply(IStrategyUser strategyUser)
    {
        foreach (var searcher in _searchers)
        {
            foreach (var equivalence in searcher.Search(strategyUser))
            {
                int[] solved1 = new int[9];
                int[] solved2 = new int[9];
                int count1 = 0;
                int count2 = 0;
                int removed = 0;

                foreach (var cell in equivalence.FirstSet)
                {
                    var solved = strategyUser.Sudoku[cell.Row, cell.Column];
                    if (solved == 0) continue;
                
                    solved1[solved - 1]++;
                    count1++;
                }

                foreach (var cell in equivalence.SecondSet)
                {
                    var solved = strategyUser.Sudoku[cell.Row, cell.Column];
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
                        var possibilities = strategyUser.PossibilitiesAt(cell);
                        if(possibilities.Count == 0) continue;

                        foreach (var possibility in possibilities.EnumeratePossibilities())
                        {
                            if (solved1[possibility - 1] == 0)
                                strategyUser.ChangeBuffer.ProposePossibilityRemoval(possibility, cell.Row, cell.Column);
                        }
                    }
                }

                if (count2 - order * digitCount2 >= equivalence.FirstSet.Length - removed - count1)
                {
                    foreach (var cell in equivalence.FirstSet)
                    {
                        var possibilities = strategyUser.PossibilitiesAt(cell);
                        if(possibilities.Count == 0) continue;

                        foreach (var possibility in possibilities.EnumeratePossibilities())
                        {
                            if (solved2[possibility - 1] == 0)
                                strategyUser.ChangeBuffer.ProposePossibilityRemoval(possibility, cell.Row, cell.Column);
                        }
                    }
                }

                if (strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit(
                        new GeometricEquivalenceReportBuilder(equivalence)) &&
                            StopOnFirstPush) return;
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

public class GeometricEquivalenceReportBuilder : IChangeReportBuilder<IUpdatableSudokuSolvingState, ISudokuHighlighter>
{
    private readonly SetEquivalence _equivalence;

    public GeometricEquivalenceReportBuilder(SetEquivalence equivalence)
    {
        _equivalence = equivalence;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>( "", lighter =>
        {
            foreach (var cell in _equivalence.FirstSet)
            {
                lighter.HighlightCell(cell, ChangeColoration.CauseOnOne);
            }
            
            foreach (var cell in _equivalence.SecondSet)
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