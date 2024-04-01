using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Model.Utility;

namespace Model.Sudoku.Solver.Trackers;

public class StatisticsTracker : Tracker
{
    private readonly List<int> _retransmissions = new();
    private readonly List<StrategyStatistics> _statistics = new();
    private int _count;
    private int _success;
    private int _solverFails;
    
    public bool NotifySolveDone { get; set; }

    public override void Prepare(SudokuSolver solver)
    {
        foreach (var strategy in solver.StrategyManager.Strategies)
        {
            if (strategy.Enabled)
            {
                _retransmissions.Add(_statistics.Count);
                _statistics.Add(new StrategyStatistics(strategy.Name));
            }
            else _retransmissions.Add(-1);
        }
    }

    public override void OnStrategyStart(SudokuStrategy strategy, int index)
    {
        Get(index)?.StartUsing();
    }

    public override void OnStrategyEnd(SudokuStrategy strategy, int index, int solutionAdded, int possibilitiesRemoved)
    {
        Get(index)?.StopUsing(solutionAdded, possibilitiesRemoved);
    }

    public override void OnSolveDone(ISolveResult result)
    {
        var wasWrong = false;
        
        _count++;
        if (result.Sudoku.IsCorrect()) _success++;
        else if (result.IsWrong())
        {
            _solverFails++;
            wasWrong = true;
        }

        if (!NotifySolveDone) return;
        
        Console.Write($"#{_count} ");
        if(result.Sudoku.IsCorrect()) Console.WriteLine("Ok !");
        else
        {
            Console.Write(wasWrong ? "Solver failed" : "Solver did not find solution");
            Console.WriteLine($" => '{SudokuTranslator.TranslateLineFormat(result.Sudoku, SudokuLineFormatEmptyCellRepresentation.Points)}'");
        }
    }
    
    public override string ToString()
    {
        string[] columnTitles = { "Strategy name", "Usage", "Score", "Solutions added", "Possibilities removed",
            "Score percentage", "Total time", "Average time" };
        int[] widthCap = new int[columnTitles.Length];

        for (int i = 0; i < columnTitles.Length; i++)
        {
            widthCap[i] = columnTitles[i].Length + 2;
        }

        foreach (var report in _statistics)
        {
            widthCap[0] = Math.Max(widthCap[0], report.Name.Length + 2);
            widthCap[1] = Math.Max(widthCap[1], report.Usage.ToString().Length + 2);
            widthCap[2] = Math.Max(widthCap[2], report.Score.ToString().Length + 2);
            widthCap[3] = Math.Max(widthCap[3], report.SolutionsAdded.ToString().Length + 2);
            widthCap[4] = Math.Max(widthCap[4], report.PossibilitiesRemoved.ToString().Length + 2);
            widthCap[5] = Math.Max(widthCap[5], report.ScorePercentage()
                .ToString(CultureInfo.InvariantCulture).Length + 3);
            widthCap[6] = Math.Max(widthCap[6], report.TotalTimeInSecond()
                .ToString(CultureInfo.InvariantCulture).Length + 3);
            widthCap[7] = Math.Max(widthCap[7], report.AverageTime()
                .ToString(CultureInfo.InvariantCulture).Length + 4);
        }

        var totalWidth = columnTitles.Length - 1;

        foreach (var width in widthCap)
        {
            totalWidth += width;
        }

        var result = new StringBuilder(StringUtility.FillEvenlyWith("Result", '-', totalWidth) + "\n\n");

        result.Append($"Completion rate : {_success} / {_count}\n");
        result.Append($"Solver fails : {_solverFails}\n\n");

        result.Append(StringUtility.FillEvenlyWith(columnTitles[0], ' ', widthCap[0]) + "|"
            + StringUtility.FillEvenlyWith(columnTitles[1], ' ', widthCap[1]) + "|"
            + StringUtility.FillEvenlyWith(columnTitles[2], ' ', widthCap[2]) + "|"
            + StringUtility.FillEvenlyWith(columnTitles[3], ' ', widthCap[3]) + "|"
            + StringUtility.FillEvenlyWith(columnTitles[4], ' ', widthCap[4]) + "|"
            + StringUtility.FillEvenlyWith(columnTitles[5], ' ', widthCap[5]) + "|"
            + StringUtility.FillEvenlyWith(columnTitles[6], ' ', widthCap[6]) + "|"
            + StringUtility.FillEvenlyWith(columnTitles[7], ' ', widthCap[7]) + "\n");
        result.Append(CrossRow(widthCap));
        
        var totalStrategyTime = 0L;

        foreach (var report in _statistics)
        {
            result.Append(StringUtility.FillEvenlyWith(report.Name, ' ', widthCap[0]) + "|"
                + StringUtility.FillEvenlyWith(report.Usage.ToString(), ' ', widthCap[1]) + "|"
                + StringUtility.FillEvenlyWith(report.Score.ToString(), ' ', widthCap[2]) + "|"
                + StringUtility.FillEvenlyWith(report.SolutionsAdded.ToString(), ' ', widthCap[3]) + "|"
                + StringUtility.FillEvenlyWith(report.PossibilitiesRemoved.ToString(), ' ', widthCap[4]) + "|"
                + StringUtility.FillEvenlyWith(report.ScorePercentage()
                    .ToString(CultureInfo.InvariantCulture) + "%", ' ', widthCap[5]) + "|"
                + StringUtility.FillEvenlyWith(report.TotalTimeInSecond()
                    .ToString(CultureInfo.InvariantCulture) + "s", ' ', widthCap[6]) + "|"
                + StringUtility.FillEvenlyWith(report.AverageTime()
                    .ToString(CultureInfo.InvariantCulture) + "ms", ' ', widthCap[7]) + "\n");
            result.Append(CrossRow(widthCap));
            
            totalStrategyTime += report.TotalTime;
        }

        result.Append($"\nTotal strategy time : {Math.Round((double)totalStrategyTime / 1000, 4)}s\n");

        return result.ToString();
    }

    private static string CrossRow(int[] widthCap)
    {
        return StringUtility.Repeat('-', widthCap[0]) + "+" + StringUtility.Repeat('-', widthCap[1]) + "+"
               + StringUtility.Repeat('-', widthCap[2]) + "+" + StringUtility.Repeat('-', widthCap[3]) + "+"
               + StringUtility.Repeat('-', widthCap[4]) + "+" + StringUtility.Repeat('-', widthCap[5]) + "+"
               + StringUtility.Repeat('-', widthCap[6]) + "+" + StringUtility.Repeat('-', widthCap[7]) + "\n";
    }

    private StrategyStatistics? Get(int initialIndex)
    {
        if (initialIndex < 0 || initialIndex >= _retransmissions.Count) return null;

        var index = _retransmissions[initialIndex];
        if (index < 0 || index >= _statistics.Count) return null;

        return _statistics[index];
    }
}

public class StrategyStatistics
{
    public string Name { get; }
    public int Score { get; private set; }
    public int SolutionsAdded { get; private set; }
    public int PossibilitiesRemoved { get; private set; }
    public int Usage { get; private set; }
    public long TotalTime { get; private set; }
    
    private long _lastStartTime;

    public StrategyStatistics(string name)
    {
        Name = name;
    }

    public void StartUsing()
    {
        _lastStartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }

    public void StopUsing(int solutionsAdded, int possibilitiesRemoved)
    {
        Usage++;
        TotalTime += DateTimeOffset.Now.ToUnixTimeMilliseconds() - _lastStartTime;
        SolutionsAdded += solutionsAdded;
        PossibilitiesRemoved += possibilitiesRemoved;
        if (solutionsAdded + possibilitiesRemoved > 0) Score++;
    }
    
    public double ScorePercentage()
    {
        if (Usage == 0) return 0;
        return Math.Round((double)Score / Usage * 100, 2);  
    }

    public double AverageTime()
    {
        if (Usage == 0) return 0;
        return Math.Round((double)TotalTime / Usage, 4);
    }

    public double TotalTimeInSecond()
    {
        return Math.Round((double)TotalTime / 1000, 4);
    }
}