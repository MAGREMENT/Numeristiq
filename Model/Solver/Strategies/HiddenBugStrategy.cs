using System.Collections.Generic;
using System.Text;
using Global.Enums;
using Model.Solver.Helpers.Changes;
using Model.Solver.Position;
using Model.Solver.Possibility;
using Model.Solver.StrategiesUtility;

namespace Model.Solver.Strategies;

public class HiddenBugStrategy : AbstractStrategy
{
    public const string OfficialName = "Hidden BUG";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;
    private const int MaxNotInPatternCount = 7;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;

    private int _minPossibilityCount;
    private int _maxPossibilityCount;
    
    public HiddenBugStrategy(int minPossibilityCount, int maxPossibilityCount) : base(OfficialName, StrategyDifficulty.Medium, DefaultBehavior)
    {
        _minPossibilityCount = minPossibilityCount;
        _maxPossibilityCount = maxPossibilityCount;
        UniquenessDependency = UniquenessDependency.FullyDependent;
        ArgumentsList.Add(new MinMaxIntStrategyArgument("Possibility count", 2, 4, 2, 4,
            1, () => _minPossibilityCount, i => _minPossibilityCount = i,
            () => _maxPossibilityCount, i => _maxPossibilityCount = i));
    }
    
    public override void Apply(IStrategyManager strategyManager)
    {
        var positions = BasicPositions(strategyManager);
        var sample = GetSample(positions);
        CellPossibility[] notInPattern = new CellPossibility[MaxNotInPatternCount];
        
        for (int possCount = _minPossibilityCount; possCount <= _maxPossibilityCount; possCount++)
        {
            foreach (var combination in CombinationCalculator.EveryCombinationWithSpecificCount(possCount, sample))
            {
                var count = FillNotInPatternList(strategyManager, positions, notInPattern, combination);
                switch (count)
                {
                    case 0 or > MaxNotInPatternCount:
                        continue;
                    case 1:
                        strategyManager.ChangeBuffer.ProposeSolutionAddition(notInPattern[0]);
                        break;
                    default:
                        foreach (var cp in Cells.SharedSeenExistingPossibilities(strategyManager, notInPattern, count))
                        {
                            strategyManager.ChangeBuffer.ProposePossibilityRemoval(cp);
                        }

                        break;
                }

                if (strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
                        new HiddenBUGReportBuilder(combination, positions)) &&
                            OnCommitBehavior == OnCommitBehavior.Return) return;
            }
        }
    }
    
    private GridPositions[] BasicPositions(IStrategyManager strategyManager)
    {
        var result = new GridPositions[9];

        for (int i = 0; i < 9; i++)
        {
            var gp = new GridPositions();
            gp.Fill();
            result[i] = gp;
        }

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var solved = strategyManager.Sudoku[row, col];
                if (solved == 0) continue;

                var gp = result[solved - 1];
                gp.VoidRow(row);
                gp.VoidColumn(col);
                gp.VoidMiniGrid(row / 3, col / 3);
            }
        }

        return result;
    }

    private int[] GetSample(GridPositions[] positions)
    {
        Possibilities? diff = null;
        for (int i = 1; i <= 9; i++)
        {
            if (positions[i - 1].Count == 0)
            {
                diff ??= new Possibilities();
                diff.Remove(i);
            }
        }

        return diff is null ? CombinationCalculator.NumbersSample : diff.ToArray();
    }

    private int FillNotInPatternList(IStrategyManager strategyManager, GridPositions[] positions,
        CellPossibility[] list, int[] combination)
    {
        int count = 0;
        
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (strategyManager.Sudoku[row, col] != 0) continue;

                var poss = Possibilities.NewEmpty();
                foreach (var p in combination)
                {
                    if (positions[p - 1].Peek(row, col)) poss.Add(p);
                }

                if (poss.Count is 0 or 2) continue;
                
                foreach (var p in poss)
                {
                    if (count == MaxNotInPatternCount) return count + 1;
                    list[count++] = new CellPossibility(row, col, p);
                }
            }
        }

        return count;
    }
}

public class HiddenBUGReportBuilder : IChangeReportBuilder
{
    private readonly int[] _combination;
    private readonly GridPositions[] _positions;

    public HiddenBUGReportBuilder(int[] combination, GridPositions[] positions)
    {
        _combination = combination;
        _positions = positions;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), Explanation(), lighter =>
        {
            int color = (int)ChangeColoration.CauseOffOne;
            foreach (var p in _combination)
            {
                for (int row = 0; row < 9; row++)
                {
                    for (int col = 0; col < 9; col++)
                    {
                        var solved = snapshot.Sudoku[row, col];
                        if (solved == p) lighter.HighlightCell(row, col, (ChangeColoration)color);
                        else if (solved != 0) continue;
                        
                        if(_positions[p - 1].Peek(row, col)) lighter.HighlightPossibility(p, row, col, (ChangeColoration)color);
                    }
                }
                
                color++;
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }

    private string Explanation()
    {
        var builder = new StringBuilder($"Hidden BUG for : {_combination[0]}");

        for (int i = 1; i < _combination.Length; i++)
        {
            builder.Append($", {_combination[i]}");
        }

        return builder.ToString();
    }
}