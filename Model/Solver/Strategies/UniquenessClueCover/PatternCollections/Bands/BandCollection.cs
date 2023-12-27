using System.Collections.Generic;
using Global;
using Model.Solver.Possibility;

namespace Model.Solver.Strategies.UniquenessClueCover.PatternCollections.Bands;

public class BandCollection : IPatternCollection
{
    private readonly BandPattern[] _collection;

    public BandCollection(params BandPattern[] collection)
    {
        _collection = collection;
    }

    public bool Apply(IStrategyManager strategyManager)
    {
        for (int mini = 0; mini < 3; mini++)
        {
            if(Check(strategyManager, mini, Unit.Row)) return true;
            if(Check(strategyManager, mini, Unit.Column)) return true;
        }

        return false;
    }

    private Dictionary<Cell, int> _clues = new();
    private List<Cell> _used = new();

    private bool Check(IStrategyManager strategyManager, int mini, Unit unit)
    {
        foreach (var pattern in _collection)
        {
            if (!GetClues(strategyManager, mini, unit, ref _clues, pattern.ClueCount,
                    pattern.DifferentClueCount)) continue;

            foreach (var boxKey in OrderKeyGenerator.GenerateAll())
            {
                var boxes = pattern.PlacementsWithKey(boxKey);

                foreach (var rowKey in OrderKeyGenerator.GenerateAll())
                {
                    foreach (var colKey in OrderKeyGenerator.GenerateAll())
                    {
                        
                    }
                }
            }
        }

        return false;
    }

    private bool Try(IStrategyManager strategyManager, BandPattern pattern, Dictionary<BoxPosition, int>[] boxes,
        int[] rowKey, int colKey, int mini, Unit unit)
    {
        _used.Clear();
        int[] numberEquivalence = new int[pattern.DifferentClueCount];

        for (int i = 0; i < 3; i++)
        {
            foreach (var entry in boxes[i])
            {
                var cell = entry.Key.ToCell(mini, i, unit);
                if (_clues.ContainsKey(cell)) _used.Add(cell);
                
            }
        }

        return false;
    }
    
    private bool GetClues(IStrategyManager strategyManager, int mini, Unit unit, ref Dictionary<Cell, int> clues,
        int maxClueCount, int maxDifferentClueCount)
    {
        _clues.Clear();

        int clueCount = 0;
        Possibilities differentClues = Possibilities.NewEmpty();
        for (int w = 0; w < 3; w++)
        {
            for (int l = 0; l < 9; l++)
            {
                var cell = unit == Unit.Row ? new Cell(mini * 3 + w, l) : new Cell(l, mini * 3 + w);
                var clue = strategyManager.StartState[cell.Row, cell.Column];
                if (clue == 0) continue;

                clueCount++;
                differentClues.Add(clue);
                if (clueCount > maxClueCount || differentClues.Count > maxDifferentClueCount) return false;

                clues.Add(cell, clue);
            }
        }

        return true;
    }
}