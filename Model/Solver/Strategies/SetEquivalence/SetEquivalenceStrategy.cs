using System;
using System.Collections.Generic;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.StrategiesUtil;

namespace Model.Solver.Strategies.SetEquivalence;

public class SetEquivalenceStrategy : AbstractStrategy
{
    public const string OfficialName = "Set Equivalence";

    private readonly ISetEquivalenceSearcher _searcher;
    
    public SetEquivalenceStrategy(ISetEquivalenceSearcher searcher)
        : base(OfficialName, StrategyDifficulty.Extreme)
    {
        _searcher = searcher;
    }

    public override void ApplyOnce(IStrategyManager strategyManager)
    {
        foreach (var equivalence in _searcher.Search(strategyManager))
        {
            int[] solved1 = new int[9];
            int[] solved2 = new int[9];
            int count1 = 0;
            int count2 = 0;

            foreach (var cell in equivalence.FirstGeometry)
            {
                var solved = strategyManager.Sudoku[cell.Row, cell.Col];
                if (solved == 0) continue;
                
                solved1[solved - 1]++;
                count1++;
            }

            foreach (var cell in equivalence.SecondGeometry)
            {
                var solved = strategyManager.Sudoku[cell.Row, cell.Col];
                if (solved == 0) continue;
                
                solved2[solved - 1]++;
                count2++;
            }
            
            var order = equivalence.FirstOrder - equivalence.SecondOrder;

            switch (order)
            {
                case 0 : break;
                case > 0 :
                    for (int i = 0; i < solved2.Length; i++)
                    {
                        if (solved2[i] > 0)
                        {
                            solved2[i] += order;
                            count2 += order;
                        }
                    }
                    break;
                case < 0 :
                    for (int i = 0; i < solved1.Length; i++)
                    {
                        if (solved1[i] > 0)
                        {
                            solved1[i] -= order;
                            count1 -= order;
                        }
                    }
                    break;
            }

            int[] difference1 = new int[9];
            Array.Copy(solved1, difference1, solved1.Length);
            int[] difference2 = new int[9];
            Array.Copy(solved2, difference2, solved1.Length);
            int differenceCount1 = count1;
            int differenceCount2 = count2;

            for (int i = 0; i < solved1.Length; i++)
            {
                int buffer = difference2[i];
                difference2[i] -= solved1[i];
                difference2[i] = Math.Max(0, difference2[i]);

                differenceCount2 -= buffer - difference2[i];
            }
            
            for (int i = 0; i < solved2.Length; i++)
            {
                int buffer = difference1[i];
                difference1[i] -= solved2[i];
                difference1[i] = Math.Max(0, difference1[i]);

                differenceCount1 -= buffer - difference1[i];
            }

            if (count1 + differenceCount2 >= equivalence.FirstGeometry.Count)
            {
                foreach (var cell in equivalence.FirstGeometry)
                {
                    var possibilities = strategyManager.PossibilitiesAt(cell);
                    if(possibilities.Count == 0) continue;

                    foreach (var possibility in possibilities)
                    {
                        if (difference2[possibility - 1] == 0)
                            strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, cell.Row, cell.Col);
                    }
                }
            }

            if (count2 + differenceCount1 >= equivalence.SecondGeometry.Count)
            {
                foreach (var cell in equivalence.SecondGeometry)
                {
                    var possibilities = strategyManager.PossibilitiesAt(cell);
                    if(possibilities.Count == 0) continue;

                    foreach (var possibility in possibilities)
                    {
                        if (difference1[possibility - 1] == 0)
                            strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, cell.Row, cell.Col);
                    }
                }
            }

            if (strategyManager.ChangeBuffer.NotEmpty())
            {
                strategyManager.ChangeBuffer.Push(this, new GeometricEquivalenceReportBuilder(equivalence));
                return;
            }
        }
    }
}

public class SetEquivalence
{
    public SetEquivalence(List<Cell> firstGeometry, int firstOrder, List<Cell> secondGeometry, int secondOrder)
    {
        FirstGeometry = firstGeometry;
        FirstOrder = firstOrder;
        SecondGeometry = secondGeometry;
        SecondOrder = secondOrder;
    }

    public List<Cell> FirstGeometry { get; }
    public int FirstOrder { get; }
    
    public List<Cell> SecondGeometry { get; }
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
            foreach (var cell in _equivalence.FirstGeometry)
            {
                lighter.HighlightCell(cell, ChangeColoration.CauseOnOne);
            }
            
            foreach (var cell in _equivalence.SecondGeometry)
            {
                lighter.HighlightCell(cell, ChangeColoration.CauseOffOne);
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}