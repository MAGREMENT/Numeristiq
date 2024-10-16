using System;
using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Graphs;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Strategies.AlternatingInference;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Sudokus.Solver.Utility.NRCZTChains;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies.NRCZTChains;

public class NRCZTChainStrategy : SudokuStrategy, ICommitComparer<NumericChange>
{
    public const string OfficialNameForDefault = "NRC-Chains";
    public const string OfficialNameForTCondition = "NRCT-Chains";
    public const string OfficialNameForZCondition = "NRCZ-Chains";
    public const string OfficialNameForZAndTCondition = "NRCZT-Chains";
    
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.BestOnly;

    private readonly INRCZTCondition[] _conditions;
    
    public NRCZTChainStrategy(params INRCZTCondition[] conditions) : base("", Difficulty.None, DefaultInstanceHandling)
    {
        _conditions = conditions;

        Difficulty = _conditions.Length > 0 ? Difficulty.Extreme : Difficulty.Hard;

        Name = conditions.Length switch
        {
            0 => OfficialNameForDefault,
            1 => $"NRC{conditions[0].Name}-Chains",
            2 => OfficialNameForZAndTCondition,
            _ => throw new ArgumentException("Too many conditions")
        };
    }

    public override void Apply(ISudokuSolverData solverData)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                foreach (var poss in solverData.PossibilitiesAt(row, col).EnumeratePossibilities())
                {
                    if (Search(solverData, new CellPossibility(row, col, poss))) return;
                }
            }
        }
    }

    private bool Search(ISudokuSolverData solverData, CellPossibility start)
    {
        HashSet<CellPossibility> blockStartVisited = new();
        Queue<NRCZTChain> queue = new();

        blockStartVisited.Add(start);
        foreach (var to in SudokuUtility.GetStrongLinks(solverData, start))
        {
            queue.Enqueue(new NRCZTChain(solverData, start, to));
        }

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            foreach (var from in SudokuUtility.SeenExistingPossibilities(solverData,  current.Last()))
            {
                if (blockStartVisited.Contains(from) || current.Contains(from)) continue;
                
                var rowPoss = solverData.RowPositionsAt(from.Row, from.Possibility);
                if (rowPoss.Count == 2)
                {
                    var cp = new CellPossibility(from.Row, rowPoss.First(from.Column), from.Possibility);
                    var chain = current.TryAdd(from, cp);
                    if (chain is not null)
                    {
                        if (Process(solverData, chain)) return true;
                        queue.Enqueue(chain);
                    }
                }
                else if (_conditions.Length > 0)
                {
                    foreach (var condition in _conditions)
                    {
                        foreach (var chain in condition.AnalyzeRow(current, from, rowPoss))
                        {
                            if (Process(solverData, chain)) return true;
                            queue.Enqueue(chain);
                        }
                    }
                }

                var colPoss = solverData.ColumnPositionsAt(from.Column, from.Possibility);
                if (colPoss.Count == 2)
                {
                    var cp = new CellPossibility(colPoss.First(from.Row), from.Column, from.Possibility);
                    var chain = current.TryAdd(from, cp);
                    if (chain is not null)
                    {
                        if (Process(solverData, chain)) return true;
                        queue.Enqueue(chain);
                    }
                }
                else if (_conditions.Length > 0)
                {
                    foreach (var condition in _conditions)
                    {
                        foreach (var chain in condition.AnalyzeColumn(current, from, colPoss))
                        {
                            if (Process(solverData, chain)) return true;
                            queue.Enqueue(chain);
                        }
                    }
                }

                var boxPoss = solverData.MiniGridPositionsAt(from.Row / 3,
                    from.Column / 3, from.Possibility);
                if (boxPoss.Count == 2)
                {
                    var cp = new CellPossibility(boxPoss.First(from.ToCell()), from.Possibility);
                    var chain = current.TryAdd(from, cp);
                    if (chain is not null)
                    {
                        if (Process(solverData, chain)) return true;
                        queue.Enqueue(chain);
                    }
                }
                else if (_conditions.Length > 0)
                {
                    foreach (var condition in _conditions)
                    {
                        foreach (var chain in condition.AnalyzeMiniGrid(current, from, boxPoss))
                        {
                            if (Process(solverData, chain)) return true;
                            queue.Enqueue(chain);
                        }
                    }
                }

                var poss = solverData.PossibilitiesAt(from.Row, from.Column);
                if (poss.Count == 2)
                {
                    var cp = new CellPossibility(from.Row, from.Column, poss.FirstPossibility(from.Possibility));
                    var chain = current.TryAdd(from, cp);
                    if (chain is not null)
                    {
                        if (Process(solverData, chain)) return true;
                        queue.Enqueue(chain);
                    }
                }
                else if (_conditions.Length > 0)
                {
                    foreach (var condition in _conditions)
                    {
                        foreach (var chain in condition.AnalyzePossibilities(current, from, poss))
                        {
                            if (Process(solverData, chain)) return true;
                            queue.Enqueue(chain);
                        }
                    }
                }

                blockStartVisited.Add(from);
            }
        }
        
        return false;
    }

    private bool Process(ISudokuSolverData solverData, NRCZTChain chain)
    {
        var last = chain.Last();
        foreach (var target in chain.PossibleTargets)
        {
            if (SudokuUtility.AreLinked(target, last)) solverData.ChangeBuffer.ProposePossibilityRemoval(target);
        }

        if (!solverData.ChangeBuffer.NeedCommit()) return false;

        solverData.ChangeBuffer.Commit(new NRCZTChainReportBuilder(chain));
        return StopOnFirstCommit;
    }

    public int Compare(IChangeCommit<NumericChange> first, IChangeCommit<NumericChange> second)
    {
        if (first.TryGetBuilder<IReportBuilderWithChain>(out var f) ||
            second.TryGetBuilder<IReportBuilderWithChain>(out var s)) return 0;

        return s!.Length() - f!.Length();
    }
}

public class NRCZTChainReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>, IReportBuilderWithChain
{
    private readonly NRCZTChain _chain;

    public NRCZTChainReportBuilder(NRCZTChain chain)
    {
        _chain = chain;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>(_chain.ToString(), lighter =>
        {
            foreach (var relation in _chain)
            {
                lighter.HighlightPossibility(relation.From, StepColor.Cause1);
                lighter.HighlightPossibility(relation.To, StepColor.On);
                lighter.CreateLink(relation.From, relation.To, LinkStrength.Strong);
            }
            
            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }

    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
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