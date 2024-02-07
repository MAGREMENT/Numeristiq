using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using Model.Sudoku.Solver;
using Model.Sudoku.Solver.StrategiesUtility;

namespace Model.Sudoku.Generator;

/// <summary>
/// RCR = Random Cell Removal
/// </summary>
public class RCRSudokuPuzzleGenerator : ISudokuPuzzleGenerator //TODO => to 2 separate generator (with & without solver)
{
    private readonly Random _random = new();
    private readonly SudokuSolver _solver;
    private readonly HighestDifficultyFinder _finder;
    private readonly IFilledSudokuGenerator _filledGenerator;
    
    public StrategyDifficulty MaxDifficulty { get; set; }

    public RCRSudokuPuzzleGenerator(IFilledSudokuGenerator filledGenerator, IRepository<List<StrategyDAO>> repo, StrategyDifficulty maxDifficulty)
    {
        _filledGenerator = filledGenerator;
        _solver = new SudokuSolver
        {
            ChangeManagement = ChangeManagement.Fast
        };
        _solver.Bind(repo);

        _finder = new HighestDifficultyFinder(_solver);
        MaxDifficulty = maxDifficulty;
    }

    public Sudoku Generate()
    {
        var filled = _filledGenerator.Generate();

        var list = new List<int>(81);
        for (int i = 0; i < 81; i++) list.Add(i);

        while (list.Count > 0)
        {
            var i = _random.Next(list.Count);

            var row = list[i] / 9;
            var col = list[i] % 9;

            list.RemoveAt(i);

            var n = filled[row, col];
            filled[row, col] = 0;
            if (BackTracking.Fill(filled.Copy(), ConstantPossibilitiesGiver.Instance, 2).Length != 1)
            {
                filled[row, col] = n;
            }
            else if(MaxDifficulty != StrategyDifficulty.ByTrial)
            {
                _solver.SetSudoku(filled.Copy());
                
                _finder.Clear();
                _solver.Solve();
                
                if(_finder.Highest > MaxDifficulty) filled[row, col] = n;
            }
        }

        return filled;
    }

    public Sudoku[] Generate(int count)
    {
        var result = new Sudoku[count];

        for (int i = 0; i < count; i++)
        {
            result[i] = Generate();
        }

        return result;
    }
}

public class HighestDifficultyFinder
{
    public StrategyDifficulty Highest { get; private set; } = StrategyDifficulty.None;

    public HighestDifficultyFinder(SudokuSolver solver)
    {
        var info = solver.GetStrategyInfo();
        solver.StrategyStopped += (i, a, p) =>
        {
            if (i >= info.Length || a + p == 0) return;

            if (info[i].Difficulty > Highest) Highest = info[i].Difficulty;
        };
    }

    public void Clear()
    {
        Highest = StrategyDifficulty.None;
    }
}

public class ConstantPossibilitiesGiver : IPossibilitiesGiver
{
    public static ConstantPossibilitiesGiver Instance { get; } = new();

    public IEnumerable<int> EnumeratePossibilitiesAt(int row, int col)
    {
        for (int i = 1; i <= 9; i++)
        {
            yield return i;
        }
    }
}