using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.Positions;
using Model.Solver.StrategiesUtil;

namespace Model.Solver.Strategies;

public class PatternOverlayStrategy : AbstractStrategy
{
    public const string OfficialName = "Pattern Overlay";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;

    private readonly int _maxCombinationSize;
    private readonly int _maxPatternNumber;

    public PatternOverlayStrategy(int maxCombinationSize, int maxPatternNumber)
        : base(OfficialName, StrategyDifficulty.Extreme, DefaultBehavior)
    {
        _maxCombinationSize = maxCombinationSize;
        _maxPatternNumber = maxPatternNumber;
    }

    public override void Apply(IStrategyManager strategyManager)
    {
        var allPatterns = GetPatterns(strategyManager);

        for (int number = 1; number <= 9; number++)
        {
            if (SearchForElimination(strategyManager, number, allPatterns[number - 1])) return;
        }
        
        foreach (var p in allPatterns)
        {
            if (p.Count > _maxPatternNumber) return;
        }

        for (int combinationSize = 1; combinationSize <= _maxCombinationSize; combinationSize++)
        {
            for (int i = 0; i < 9; i++)
            {
                var currentPatterns = allPatterns[i];
                var combinations = CombinationsOfSize(combinationSize, i);

                foreach (var combination in combinations)
                {
                    var enumerator = new PatternCombinationEnumerator(allPatterns, combination);

                    currentPatterns.RemoveAll(pattern =>
                    {
                        bool toRemove = true;

                        while (enumerator.MoveNext())
                        {
                            var current = enumerator.Current;

                            if (!pattern.PeakAny(current))
                            {
                                toRemove = false;
                                break;
                            }
                        }
                        
                        enumerator.Reset();
                        return toRemove;
                    });
                }
                
                if (SearchForElimination(strategyManager, i + 1, allPatterns[i])) return;
            }
        }
    }

    private List<LinePositions> CombinationsOfSize(int size, int except)
    {
        List<LinePositions> result = new();

        SearchCombinationsOfSize(size, except, 0, new LinePositions(), result);
        
        return result;
    }

    private void SearchCombinationsOfSize(int size, int except, int start, LinePositions current, List<LinePositions> result)
    {
        for (int i = start; i < 9; i++)
        {
            if (i == except) continue;

            current.Add(i);
            
            if (current.Count == size) result.Add(current.Copy());
            else SearchCombinationsOfSize(size, except, i + 1, current, result);

            current.Remove(i);
        }
    }

    private bool SearchForElimination(IStrategyManager strategyManager, int number, List<GridPositions> patterns)
    {
        if (patterns.Count == 0) return false;
        
        foreach (var cell in patterns[0].And(patterns))
        {
            strategyManager.ChangeBuffer.ProposeSolutionAddition(number, cell.Row, cell.Col);
        }

        foreach (var cell in strategyManager.PositionsFor(number).Difference(patterns[0].Or(patterns)))
        {
            strategyManager.ChangeBuffer.ProposePossibilityRemoval(number, cell.Row, cell.Col);
        }

        return strategyManager.ChangeBuffer.Commit(this, new PatternOverlayReportBuilder(patterns, number))
            && OnCommitBehavior == OnCommitBehavior.Return;
    }

    private List<GridPositions>[] GetPatterns(IStrategyManager strategyManager)
    {
        List<GridPositions>[] result = new List<GridPositions>[9];

        for (int i = 0; i < 9; i++)
        {
            List<GridPositions> currentResult = new();

            SearchForPattern(strategyManager, new LinePositions(), new LinePositions(),
                new GridPositions(), i + 1, currentResult, 0);

            result[i] = currentResult;
        }

        return result;
    }

    private void SearchForPattern(IStrategyManager strategyManager, LinePositions colsUsed, LinePositions miniColsUsed,
        GridPositions current, int number, List<GridPositions> result, int row)
    {
        if (row == 9)
        {
            result.Add(current.Copy());
            return;
        }
        
        var cols = strategyManager.RowPositionsAt(row, number);
        LinePositions nextMCU;
        
        if (cols.Count != 0)
        {
            foreach (var col in cols)
            {
                if (colsUsed.Peek(col) || miniColsUsed.Peek(col)) continue;

                var cell = new Cell(row, col);
                current.Add(cell);
                
                colsUsed.Add(col);
                if ((row + 1) % 3 == 0) nextMCU = new LinePositions();
                else
                {
                    nextMCU = miniColsUsed.Copy();
                    nextMCU.FillMiniGrid(col / 3);
                }

                SearchForPattern(strategyManager, colsUsed, nextMCU, current, number, result, row + 1);

                current.Remove(cell);
                colsUsed.Remove(col);
            }
        }
        else
        {
            int col = 0;
            for (; col < 9; col++)
            {
                if (strategyManager.Sudoku[row, col] == number) break;
            }

            colsUsed.Add(col);
            if ((row + 1) % 3 == 0) nextMCU = new LinePositions();
            else
            {
                nextMCU = miniColsUsed.Copy();
                nextMCU.FillMiniGrid(col / 3);
            }

            SearchForPattern(strategyManager, colsUsed, nextMCU, current, number, result, row + 1);
        }
    }
}

public class PatternCombinationEnumerator : IEnumerator<GridPositions>
{
    public GridPositions Current => GetCurrent();
    object IEnumerator.Current => Current;

    private readonly List<GridPositions>[] _allPatterns;
    private readonly int[] _patternsNumber;
    private readonly int[] _patternsCount;

    private bool _alreadyNexted;

    public PatternCombinationEnumerator(List<GridPositions>[] allPatterns, params int[] patternsNumber)
    {
        _allPatterns = allPatterns;
        _patternsNumber = patternsNumber;
        _patternsCount = new int[patternsNumber.Length];
    }
    
    public PatternCombinationEnumerator(List<GridPositions>[] allPatterns, List<int> patternsNumber)
    {
        _allPatterns = allPatterns;
        _patternsNumber = patternsNumber.ToArray();
        _patternsCount = new int[patternsNumber.Count];
    }
    
    public PatternCombinationEnumerator(List<GridPositions>[] allPatterns, LinePositions patternsNumber)
    {
        _allPatterns = allPatterns;
        _patternsNumber = patternsNumber.ToArray();
        _patternsCount = new int[patternsNumber.Count];
    }
    
    public bool MoveNext()
    {
        if (!_alreadyNexted)
        {
            _alreadyNexted = true;
            return true;
        }
        
        int i = 0;
        while (i < _patternsNumber.Length)
        {
            if (_patternsCount[i] < _allPatterns[_patternsNumber[i]].Count - 1)
            {
                _patternsCount[i]++;
                return true;
            }

            _patternsCount[i] = 0;
            i++;
        }

        return false;
    }

    public void Reset()
    {
        for (int i = 0; i < _patternsCount.Length; i++)
        {
            _patternsCount[i] = 0;
        }

        _alreadyNexted = false;
    }

    private GridPositions GetCurrent()
    {
        var result = _allPatterns[_patternsNumber[0]][_patternsCount[0]];

        for (int i = 1; i < _patternsNumber.Length; i++)
        {
            result = result.Or(_allPatterns[_patternsNumber[i]][_patternsCount[i]]);
        }

        return result;
    }
    
    public void Dispose()
    {
        
    }
}

public class PatternOverlayReportBuilder : IChangeReportBuilder
{
    private readonly List<GridPositions> _patterns;
    private readonly int _number;

    public PatternOverlayReportBuilder(List<GridPositions> patterns, int number)
    {
        _patterns = patterns;
        _number = number;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        Highlight[] highlights = new Highlight[_patterns.Count];

        for (int i = 0; i < _patterns.Count; i++)
        {
            var current = _patterns[i];
            highlights[i] = lighter =>
            {
                for (int row = 0; row < 9; row++)
                {
                    for (int col = 0; col < 9; col++)
                    {
                        if (!snapshot.PossibilitiesAt(row, col).Peek(_number)) continue;

                        lighter.HighlightCell(row, col, current.Peek(row, col) 
                            ? ChangeColoration.CauseOnOne : ChangeColoration.Neutral);
                    }
                }

                IChangeReportBuilder.HighlightChanges(lighter, changes);
            };
        }

        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", highlights);
    }
}