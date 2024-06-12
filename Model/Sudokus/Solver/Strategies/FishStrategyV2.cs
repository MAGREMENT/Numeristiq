using System.Collections.Generic;
using Model.Core;
using Model.Core.Settings.Types;
using Model.Sudokus.Solver.Position;
using Model.Sudokus.Solver.Utility;

namespace Model.Sudokus.Solver.Strategies;

public class FishStrategyV2 : SudokuStrategy
{
    public const string OfficialName = "Fish";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;
    
    private readonly MinMaxSetting _unitCount;
    
    private static readonly House[] CoverHouses =
    {
        new(Unit.Row, 0),
        new(Unit.Row, 1),
        new(Unit.Row, 2),
        new(Unit.Row, 3),
        new(Unit.Row, 4),
        new(Unit.Row, 5),
        new(Unit.Row, 6),
        new(Unit.Row, 7),
        new(Unit.Row, 8),
        new(Unit.Column, 0),
        new(Unit.Column, 1),
        new(Unit.Column, 2),
        new(Unit.Column, 3),
        new(Unit.Column, 4),
        new(Unit.Column, 5),
        new(Unit.Column, 6),
        new(Unit.Column, 7),
        new(Unit.Column, 8),
        new(Unit.MiniGrid, 0),
        new(Unit.MiniGrid, 1),
        new(Unit.MiniGrid, 2),
        new(Unit.MiniGrid, 3),
        new(Unit.MiniGrid, 4),
        new(Unit.MiniGrid, 5),
        new(Unit.MiniGrid, 6),
        new(Unit.MiniGrid, 7),
        new(Unit.MiniGrid, 8),
    };

    public FishStrategyV2(int minUnitCount, int maxUnitCount) : base("Fish", StepDifficulty.Extreme, DefaultInstanceHandling)
    {
        _unitCount = new MinMaxSetting("Unit count", 2, 4, 2, 5, 1, minUnitCount, maxUnitCount);

        AddSetting(_unitCount);
    }

    public override void Apply(ISudokuStrategyUser strategyUser)
    {
        for (int number = 1; number <= 9; number++)
        {
            var positions = strategyUser.PositionsFor(number);
            List<int> possibleCoverHouses = new();
            
            for (int i = 0; i < CoverHouses.Length; i++)
            {
                var current = CoverHouses[i];
                if (UnitMethods.Get(current.Unit).Count(positions, current.Number) > 0) possibleCoverHouses.Add(i);
            }

            for (int i = 0; i < possibleCoverHouses.Count; i++)
            {
                GridPositions baseSet = new();
                var house = CoverHouses[possibleCoverHouses[i]];
                UnitMethods.Get(house.Unit).Fill(baseSet, house.Number);

                for (int j = i + 1; j < possibleCoverHouses.Count; j++)
                {
                    house = CoverHouses[possibleCoverHouses[i]];
                    var methods = UnitMethods.Get(house.Unit);
                    if (methods.Count(baseSet, house.Number) == 0) continue;

                    GridPositions coverSet = new();
                    methods.Fill(coverSet, house.Number);
                    if (Search(strategyUser, possibleCoverHouses, j, baseSet.Copy(), coverSet, 1)) return;
                }
            }
        }
    }

    private bool Search(ISudokuStrategyUser strategyUser, List<int> possibleCoverHouses, int start, GridPositions baseSet,
        GridPositions coverSet, int count)
    {
        for (int i = start; i < possibleCoverHouses.Count; i++)
        {
            var house = CoverHouses[possibleCoverHouses[i]];
            var methods = UnitMethods.Get(house.Unit);
            if (methods.Count(baseSet, house.Number) > 0) continue;

            if (methods.Count(coverSet, house.Number) == 0) continue;

            var baseCopy = baseSet.Copy();
            methods.Fill(baseCopy, house.Number);

            for (int j = start; j < possibleCoverHouses.Count; j++)
            {
                house = CoverHouses[possibleCoverHouses[j]];
                methods = UnitMethods.Get(house.Unit);
                if (methods.Count(coverSet, house.Number) > 0) continue;

                if (methods.Count(baseSet, house.Number) == 0) continue;

                var coverCopy = coverSet.Copy();
                methods.Fill(coverCopy, house.Number);

                var c = count + 1;

                if (_unitCount.Value.IsInBetweenInclusive(c) && TryProcess(strategyUser, baseCopy, coverCopy))
                    return true;

                if (c < _unitCount.Value.Max && Search(strategyUser, possibleCoverHouses, start, baseCopy.Copy(),
                        coverCopy, c)) return true;
            }
        }
        
        return false;
    }

    private bool TryProcess(ISudokuStrategyUser strategyUser, GridPositions baseSet,
        GridPositions coverSet)
    {
        //TODO
        return false;
    }
}