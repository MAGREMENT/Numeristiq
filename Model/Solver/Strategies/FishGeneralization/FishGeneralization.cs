using System;
using System.Collections.Generic;
using System.Text;
using Global.Enums;
using Model.Solver.Helpers.Changes;
using Model.Solver.Position;
using Model.Solver.StrategiesUtility;

namespace Model.Solver.Strategies.FishGeneralization;

public class FishGeneralization : AbstractStrategy
{
    public const string OfficialNameForBasic = "Fish";
    public const string OfficialNameForFinned = "Finned Fish";
    public const string OfficialNameForCannibalisticFinned = "Cannibalistic Finned Fish"; //TODO
    
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;

    private int _minUnitCount;
    private int _maxUnitCount;
    private readonly IFishType _type;

    public static readonly CoverHouse[] CoverHouses =
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
    
    public FishGeneralization(int minUnitCount, int maxUnitCount, IFishType type) : base("", StrategyDifficulty.Extreme, DefaultBehavior)
    {
        _minUnitCount = minUnitCount;
        _maxUnitCount = maxUnitCount;
        _type = type;
        Name = _type.Name;
        ArgumentsList.Add(new MinMaxIntStrategyArgument("Unit count", 2, 4, 2, 5, 1,
            () => _minUnitCount, i => _minUnitCount = i, () => _maxUnitCount,
            i => _maxUnitCount = i));
    }
    
    public override void Apply(IStrategyManager strategyManager)
    {
        for (int number = 1; number <= 9; number++)
        {
            var positions = strategyManager.PositionsFor(number);
            List<int> possibleCoverHouses = new();
            
            for (int i = 0; i < CoverHouses.Length; i++)
            {
                var current = CoverHouses[i];
                if (UnitMethods.Get(current.Unit).Count(positions, current.Number) > 0) possibleCoverHouses.Add(i);
            }
            
            for (int unitCount = _minUnitCount; unitCount <= _maxUnitCount; unitCount++)
            {
                foreach (var combination in CombinationCalculator.EveryCombinationWithSpecificCount(unitCount, possibleCoverHouses))
                {
                    if (_type.TryFind(strategyManager, number, combination, this)) return;
                }
            }
        }
    }
}

public class FishReportBuilder : IChangeReportBuilder
{
    private readonly HashSet<CoverHouse> _baseSet;
    private readonly CoverHouse[] _coveredSet;
    private readonly int _possibility;
    private readonly GridPositions _inCommon;
    private readonly GridPositions _fins;

    public FishReportBuilder(HashSet<CoverHouse> baseSet, CoverHouse[] coveredSet, int possibility, GridPositions inCommon)
    {
        _baseSet = baseSet;
        _coveredSet = coveredSet;
        _possibility = possibility;
        _inCommon = inCommon;
        _fins = new GridPositions();
    }
    public FishReportBuilder(HashSet<CoverHouse> baseSet, CoverHouse[] coveredSet, int possibility, GridPositions inCommon,
        GridPositions fins)
    {
        _baseSet = baseSet;
        _coveredSet = coveredSet;
        _possibility = possibility;
        _inCommon = inCommon;
        _fins = fins;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), Explanation(), lighter =>
        {
            foreach (var cell in _inCommon)
            {
                lighter.HighlightPossibility(_possibility, cell.Row, cell.Column, ChangeColoration.CauseOffOne);
            }

            foreach (var cell in _fins)
            {
                lighter.HighlightPossibility(_possibility, cell.Row, cell.Column, ChangeColoration.CauseOffTwo);
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

        var coverSetBuilder = new StringBuilder(_coveredSet.ToString());
        for (int i = 1; i < _coveredSet.Length; i++)
        {
            coverSetBuilder.Append(", " + _coveredSet[i]);
        }

        string isFinned = _fins.Count > 0 ? "Finned " : "";

        return $"{isFinned}{type} found :\nBase set : {baseSetBuilder.ToString()[..^2]}\nCover set : {coverSetBuilder}";
    }
}

public interface IFishType
{
    string Name { get; }
    
    bool TryFind(IStrategyManager strategyManager, int number, int[] combination, IStrategy strategy);
}