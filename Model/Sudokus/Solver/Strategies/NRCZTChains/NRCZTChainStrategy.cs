using System;
using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Changes.Buffers;
using Model.Helpers.Highlighting;
using Model.Sudokus.Solver.Strategies.AlternatingInference;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Sudokus.Solver.Utility.NRCZTChains;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies.NRCZTChains;

public class NRCZTChainStrategy : SudokuStrategy, ICommitComparer
{
    public const string OfficialNameForDefault = "NRC-Chains";
    public const string OfficialNameForTCondition = "NRCT-Chains";
    public const string OfficialNameForZCondition = "NRCZ-Chains";
    public const string OfficialNameForZAndTCondition = "NRCZT-Chains";
    
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.BestOnly;

    private readonly INRCZTCondition[] _conditions;
    
    public NRCZTChainStrategy(params INRCZTCondition[] conditions) : base("", StepDifficulty.None, DefaultInstanceHandling)
    {
        _conditions = conditions;

        Difficulty = _conditions.Length > 0 ? StepDifficulty.Extreme : StepDifficulty.Hard;

        Name = conditions.Length switch
        {
            0 => OfficialNameForDefault,
            1 => $"NRC{conditions[0].Name}-Chains",
            2 => OfficialNameForZAndTCondition,
            _ => throw new ArgumentException("Too many conditions")
        };
    }

    public override void Apply(ISudokuStrategyUser strategyUser)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                foreach (var poss in strategyUser.PossibilitiesAt(row, col).EnumeratePossibilities())
                {
                    if (Search(strategyUser, new CellPossibility(row, col, poss))) return;
                }
            }
        }
    }

    private bool Search(ISudokuStrategyUser strategyUser, CellPossibility start)
    {
        HashSet<CellPossibility> blockStartVisited = new();
        Queue<NRCZTChain> queue = new();

        blockStartVisited.Add(start);
        foreach (var to in SudokuCellUtility.DefaultStrongLinks(strategyUser, start))
        {
            queue.Enqueue(new NRCZTChain(strategyUser, start, to));
        }

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            foreach (var from in SudokuCellUtility.SeenExistingPossibilities(strategyUser,  current.Last()))
            {
                if (blockStartVisited.Contains(from) || current.Contains(from)) continue;
                
                var rowPoss = strategyUser.RowPositionsAt(from.Row, from.Possibility);
                if (rowPoss.Count == 2)
                {
                    var cp = new CellPossibility(from.Row, rowPoss.First(from.Column), from.Possibility);
                    var chain = current.TryAdd(from, cp);
                    if (chain is not null)
                    {
                        if (Process(strategyUser, chain)) return true;
                        queue.Enqueue(chain);
                    }
                }
                else if (_conditions.Length > 0)
                {
                    foreach (var condition in _conditions)
                    {
                        foreach (var chain in condition.AnalyzeRow(current, from, rowPoss))
                        {
                            if (Process(strategyUser, chain)) return true;
                            queue.Enqueue(chain);
                        }
                    }
                }

                var colPoss = strategyUser.ColumnPositionsAt(from.Column, from.Possibility);
                if (colPoss.Count == 2)
                {
                    var cp = new CellPossibility(colPoss.First(from.Row), from.Column, from.Possibility);
                    var chain = current.TryAdd(from, cp);
                    if (chain is not null)
                    {
                        if (Process(strategyUser, chain)) return true;
                        queue.Enqueue(chain);
                    }
                }
                else if (_conditions.Length > 0)
                {
                    foreach (var condition in _conditions)
                    {
                        foreach (var chain in condition.AnalyzeColumn(current, from, colPoss))
                        {
                            if (Process(strategyUser, chain)) return true;
                            queue.Enqueue(chain);
                        }
                    }
                }

                var boxPoss = strategyUser.MiniGridPositionsAt(from.Row / 3,
                    from.Column / 3, from.Possibility);
                if (boxPoss.Count == 2)
                {
                    var cp = new CellPossibility(boxPoss.First(from.ToCell()), from.Possibility);
                    var chain = current.TryAdd(from, cp);
                    if (chain is not null)
                    {
                        if (Process(strategyUser, chain)) return true;
                        queue.Enqueue(chain);
                    }
                }
                else if (_conditions.Length > 0)
                {
                    foreach (var condition in _conditions)
                    {
                        foreach (var chain in condition.AnalyzeMiniGrid(current, from, boxPoss))
                        {
                            if (Process(strategyUser, chain)) return true;
                            queue.Enqueue(chain);
                        }
                    }
                }

                var poss = strategyUser.PossibilitiesAt(from.Row, from.Column);
                if (poss.Count == 2)
                {
                    var cp = new CellPossibility(from.Row, from.Column, poss.FirstPossibility(from.Possibility));
                    var chain = current.TryAdd(from, cp);
                    if (chain is not null)
                    {
                        if (Process(strategyUser, chain)) return true;
                        queue.Enqueue(chain);
                    }
                }
                else if (_conditions.Length > 0)
                {
                    foreach (var condition in _conditions)
                    {
                        foreach (var chain in condition.AnalyzePossibilities(current, from, poss))
                        {
                            if (Process(strategyUser, chain)) return true;
                            queue.Enqueue(chain);
                        }
                    }
                }

                blockStartVisited.Add(from);
            }
        }
        
        return false;
    }

    private bool Process(ISudokuStrategyUser strategyUser, NRCZTChain chain)
    {
        var last = chain.Last();
        foreach (var target in chain.PossibleTargets)
        {
            if (SudokuCellUtility.AreLinked(target, last)) strategyUser.ChangeBuffer.ProposePossibilityRemoval(target);
        }

        return strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit(new NRCZTChainReportBuilder(chain))
                                                    && StopOnFirstPush;
    }

    public int Compare(IChangeCommit first, IChangeCommit second)
    {
        if (first.TryGetBuilder<IReportBuilderWithChain>(out var f) ||
            second.TryGetBuilder<IReportBuilderWithChain>(out var s)) return 0;

        return s.Length() - f.Length();
    }
}

public class NRCZTChainReportBuilder : IChangeReportBuilder<IUpdatableSudokuSolvingState, ISudokuHighlighter>, IReportBuilderWithChain
{
    private readonly NRCZTChain _chain;

    public NRCZTChainReportBuilder(NRCZTChain chain)
    {
        _chain = chain;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>(_chain.ToString(), lighter =>
        {
            foreach (var relation in _chain)
            {
                lighter.HighlightPossibility(relation.From, ChangeColoration.CauseOffOne);
                lighter.HighlightPossibility(relation.To, ChangeColoration.CauseOnOne);
                lighter.CreateLink(relation.From, relation.To, LinkStrength.Strong);
            }
            
            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }

    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }

    public int MaxRank()
    {
        return _chain.Last().DifficultyRank;
    }

    public int Length()
    {
        return _chain.Count;
    }
}