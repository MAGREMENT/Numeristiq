using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Global.Enums;
using Model.Solver.Helpers.Changes;
using Model.Solver.Position;
using Model.Solver.StrategiesUtility;

namespace Model.Solver.Strategies;

public class FishStrategy : AbstractStrategy
{
    public const string OfficialName = "Fish";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;

    private readonly int _minUnitCount;
    private readonly int _maxUnitCount;

    private readonly CoverHouse[] _coverHouses =
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
    
    public FishStrategy(int minUnitCount, int maxUnitCount) : base(OfficialName, StrategyDifficulty.Extreme, DefaultBehavior)
    {
        _minUnitCount = minUnitCount;
        _maxUnitCount = maxUnitCount;
    }

    
    public override void Apply(IStrategyManager strategyManager)
    {
        for (int number = 1; number <= 9; number++)
        {
            var positions = strategyManager.PositionsFor(number);
            List<int> possibleCoverHouses = new();
            
            for (int i = 0; i < _coverHouses.Length; i++)
            {
                var current = _coverHouses[i];
                if (UnitMethods.GetMethods(current.Unit).Count(positions, current.Number) > 0) possibleCoverHouses.Add(i);
            }
            
            for (int unitCount = _minUnitCount; unitCount <= _maxUnitCount; unitCount++)
            {
                foreach (var combination in CombinationCalculator.EveryCombination(unitCount, possibleCoverHouses))
                {
                    if (Try(strategyManager, number, combination)) return;
                }
            }
        }
    }

    private bool Try(IStrategyManager strategyManager, int number, int[] combination)
    {
        if (combination.Contains(0) && combination.Contains(1) && combination.Contains(4) && combination.Contains(24))
        {
            int a = 0;
        }
        
        var gp = new GridPositions();
        HashSet<CoverHouse> baseSet = new();
        
        foreach (var n in combination)
        {
            var house = _coverHouses[n];
            var methods = UnitMethods.GetMethods(house.Unit);

            if (methods.Count(gp, house.Number) > 0) return false; //Cannibalism => TODO
            methods.Fill(gp, house.Number);
            baseSet.Add(house);
        }

        gp = gp.And(strategyManager.PositionsFor(number));

        foreach (var coverSet in gp.PossibleCoverHouses(combination.Length, baseSet,
                     UnitMethods.AllUnitMethods))
        {
            var gpOfCoverSet = new GridPositions();
            foreach (var set in coverSet)
            {
                UnitMethods.GetMethods(set.Unit).Fill(gpOfCoverSet, set.Number);
            }

            foreach (var cell in gpOfCoverSet.Difference(gp))
            {
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(number, cell);
            }

            if (strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
                    new FishReportBuilder(baseSet, coverSet, number, gp)) &&
                        OnCommitBehavior == OnCommitBehavior.Return) return true;
        }
        
        return false;
    }
}

public class FishReportBuilder : IChangeReportBuilder
{
    private readonly HashSet<CoverHouse> _baseSet;
    private readonly CoverHouse[] _coverSet;
    private readonly int _possibility;
    private readonly GridPositions _inCommon;

    public FishReportBuilder(HashSet<CoverHouse> baseSet, CoverHouse[] coverSet, int possibility, GridPositions inCommon)
    {
        _baseSet = baseSet;
        _coverSet = coverSet;
        _possibility = possibility;
        _inCommon = inCommon;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), Explanation(), lighter =>
        {
            foreach (var cell in _inCommon)
            {
                lighter.HighlightPossibility(_possibility, cell.Row, cell.Col, ChangeColoration.CauseOffOne);
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }

    private string Explanation()
    {
        var type = _baseSet.Count switch
        {
            3 => "Swordfish",
            4 => "Jellyfish",
            _ => throw new ArgumentOutOfRangeException()
        };

        var baseSetBuilder = new StringBuilder();
        foreach (var ch in _baseSet)
        {
            baseSetBuilder.Append(ch + ", ");
        }

        var coverSetBuilder = new StringBuilder(_coverSet[0].ToString());
        for (int i = 1; i < _coverSet.Length; i++)
        {
            coverSetBuilder.Append(", " + _coverSet[i]);
        }

        return $"{type} found :\nBase set : {baseSetBuilder.ToString()[..^2]}\nCover set : {coverSetBuilder}";
    }
}