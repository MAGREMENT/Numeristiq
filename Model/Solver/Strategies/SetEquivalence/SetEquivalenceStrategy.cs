using System;
using System.Collections.Generic;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.StrategiesUtil;

namespace Model.Solver.Strategies.SetEquivalence;

public class SetEquivalenceStrategy : AbstractStrategy
{
    public const string OfficialName = "Set Equivalence";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;

    private readonly ISetEquivalenceSearcher _searcher;
    
    public SetEquivalenceStrategy(ISetEquivalenceSearcher searcher)
        : base(OfficialName, StrategyDifficulty.Extreme, DefaultBehavior)
    {
        _searcher = searcher;
    }

    public override void ApplyOnce(IStrategyManager strategyManager)
    {
        foreach (var equivalence in _searcher.Search(strategyManager))
        {
            //if(equivalence.FirstOrder != equivalence.SecondOrder) continue;
            
            int[] solved1 = new int[9];
            int[] solved2 = new int[9];
            int count1 = 0;
            int count2 = 0;
            int removed = 0;

            foreach (var cell in equivalence.FirstSet)
            {
                var solved = strategyManager.Sudoku[cell.Row, cell.Col];
                if (solved == 0) continue;
                
                solved1[solved - 1]++;
                count1++;
            }

            foreach (var cell in equivalence.SecondSet)
            {
                var solved = strategyManager.Sudoku[cell.Row, cell.Col];
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
                    var possibilities = strategyManager.PossibilitiesAt(cell);
                    if(possibilities.Count == 0) continue;

                    foreach (var possibility in possibilities)
                    {
                        if (solved1[possibility - 1] == 0)
                            strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, cell.Row, cell.Col);
                    }
                }
            }

            if (count2 - order * digitCount2 >= equivalence.FirstSet.Length - removed - count1)
            {
                foreach (var cell in equivalence.FirstSet)
                {
                    var possibilities = strategyManager.PossibilitiesAt(cell);
                    if(possibilities.Count == 0) continue;

                    foreach (var possibility in possibilities)
                    {
                        if (solved2[possibility - 1] == 0)
                            strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, cell.Row, cell.Col);
                    }
                }
            }

            if (strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
                    new GeometricEquivalenceReportBuilder(equivalence)) &&
                    OnCommitBehavior == OnCommitBehavior.Return)
                return;
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

public class GeometricEquivalenceReportBuilder : IChangeReportBuilder
{
    private readonly SetEquivalence _equivalence;

    public GeometricEquivalenceReportBuilder(SetEquivalence equivalence)
    {
        _equivalence = equivalence;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            foreach (var cell in _equivalence.FirstSet)
            {
                lighter.HighlightCell(cell, ChangeColoration.CauseOnOne);
            }
            
            foreach (var cell in _equivalence.SecondSet)
            {
                lighter.HighlightCell(cell, ChangeColoration.CauseOffOne);
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}