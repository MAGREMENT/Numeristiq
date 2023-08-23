using System.Collections.Generic; 
using Model.Positions;
using Model.StrategiesUtil;

namespace Model.Strategies;

public class PatternOverlayStrategy : IStrategy //TODO fixme : ".364.2...8......4...278.....2...873.6....49.87..6..........7.1....1..3..2....6..5"
{
    public string Name => "Pattern Overlay";
    public StrategyLevel Difficulty => StrategyLevel.Extreme;
    public int Score { get; set; }

    private readonly int _max;

    public PatternOverlayStrategy(int max)
    {
        _max = max;
    }

    public void ApplyOnce(IStrategyManager strategyManager)
    {
        var all = AllPositionsOfAllNumbers(strategyManager);

        HashSet<Pattern>?[] patterns = new HashSet<Pattern>[9];
        for (int i = 0; i < patterns.Length; i++)
        {
            if(all[i].Count > _max) continue;
            
            patterns[i] = Patterns(strategyManager, all[i], i + 1);
        }

        for (int i = 0; i < all.Length; i++)
        {
            var patternsExamined = patterns[i];
            if (patternsExamined is null || patternsExamined.Count == 0) continue;
            
            foreach (var coord in all[i])
            {
                int count = 0;
                foreach (var pattern in patternsExamined)
                {
                    if (pattern.Peek(coord)) count++;
                }
                
                if(count == 0) strategyManager.ChangeBuffer.AddPossibilityToRemove(i + 1, coord.Row, coord.Col);
                else if(count == patternsExamined.Count) strategyManager.ChangeBuffer.AddDefinitiveToAdd(i + 1, coord.Row, coord.Col);
            }

            if (strategyManager.ChangeBuffer.NotEmpty())
                strategyManager.ChangeBuffer.Push(this, new PatternOverlayReportBuilder());
        }
        
        //TODO add rule 2
    }

    private List<Coordinate>[] AllPositionsOfAllNumbers(IStrategyManager strategyManager)
    {
        List<Coordinate>[] possiblePositions =
        {
            new(), new(), new(),
            new(), new(), new(),
            new(), new(), new()
        };

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (strategyManager.Sudoku[row, col] != 0) continue;

                Coordinate current = new Coordinate(row, col);
                foreach (var possibility in strategyManager.Possibilities[row, col])
                {
                    possiblePositions[possibility - 1].Add(current);
                }
            }
        }

        return possiblePositions;
    }

    private HashSet<Pattern> Patterns(IStrategyManager strategyManager, List<Coordinate> all, int number)
    {
        HashSet<Pattern> result = new HashSet<Pattern>();
        if (all.Count == 0) return result;

        int firstRow = all[0].Row;
        foreach (var start in all)
        {
            if (start.Row != firstRow) break;
            
            GridPositions buildup = new GridPositions();
            buildup.Add(start);
            var copy = new List<Coordinate>(all);
            copy.RemoveAll(coord => coord.Row == start.Row || coord.Col == start.Col ||
                                    (coord.Row / 3 == start.Row / 3 && coord.Col / 3 == start.Col / 3));

            SearchForPatterns(strategyManager, copy, number, buildup, result);
        }

        return result;
    }

    private void SearchForPatterns(IStrategyManager strategyManager, List<Coordinate> toSearch, int number,
        GridPositions buildup, HashSet<Pattern> result)
    {
        foreach (var current in toSearch)
        {
            var buildupCopy = buildup.Copy();
            buildupCopy.Add(current);
            var copy = new List<Coordinate>(toSearch);
            copy.RemoveAll(coord => coord.Row == current.Row || coord.Col == current.Col ||
                                    (coord.Row / 3 == current.Row / 3 && coord.Col / 3 == current.Col / 3));

            if (copy.Count == 0)
            {
                for (int row = 0; row < 9; row++)
                {
                    int total = strategyManager.Sudoku.RowCount(row, number) + buildupCopy.RowCount(row);
                    if (total != 1) return;
                }

                for (int col = 0; col < 9; col++)
                {
                    int total = strategyManager.Sudoku.ColumnCount(col, number) + buildupCopy.ColumnCount(col);
                    if (total != 1) return;
                }

                for (int miniRow = 0; miniRow < 3; miniRow++)
                {
                    for (int miniCol = 0; miniCol < 3; miniCol++)
                    {
                        int total = strategyManager.Sudoku.MiniGridCount(miniRow, miniCol, number) +
                                    buildupCopy.MiniGridCount(miniRow, miniCol);
                        if (total != 1) return;
                    }
                }
                
                result.Add(new Pattern(buildupCopy));
            }
            else
            {
                SearchForPatterns(strategyManager, copy, number, buildupCopy, result);
            }
        }
    }

}

public class Pattern
{
    private readonly GridPositions _pattern;

    public Pattern(GridPositions positions)
    {
        _pattern = positions;
    }

    public bool Peek(Coordinate coordinate)
    {
        return _pattern.Peek(coordinate);
    }

    public override bool Equals(object? obj)
    {
        return obj is Pattern p && p._pattern.Equals(_pattern);
    }

    public override int GetHashCode()
    {
        return _pattern.GetHashCode();
    }
}

public class PatternOverlayReportBuilder : IChangeReportBuilder
{
    public ChangeReport Build(List<SolverChange> changes, IChangeManager manager)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes),
            lighter => IChangeReportBuilder.HighlightChanges(lighter, changes), "");
    }
}