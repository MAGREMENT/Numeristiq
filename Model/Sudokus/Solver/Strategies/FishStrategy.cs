﻿using System;
using System.Collections.Generic;
using System.Text;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Core.Settings;
using Model.Core.Settings.Types;
using Model.Sudokus.Solver.Position;
using Model.Sudokus.Solver.Utility;
using Model.Utility;

namespace Model.Sudokus.Solver.Strategies;

public class FishStrategy : SudokuStrategy
{
    public const string OfficialName = "Fish";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    private readonly MinMaxSetting _unitCount;
    private readonly IntSetting _maxNumberOfExoFins;
    private readonly IntSetting _maxNumberOfEndoFins;
    private readonly BooleanSetting _allowCannibalism;

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
    
    public FishStrategy(int minUnitCount, int maxUnitCount, int maxNumberOfExoFins, int maxNumberOfEndoFins,
        bool allowCannibalism) : base(OfficialName, StepDifficulty.Extreme, DefaultInstanceHandling)
    {
        _unitCount = new MinMaxSetting("Unit count", 2, 4, 2, 5, 1, minUnitCount, maxUnitCount);
        _maxNumberOfExoFins = new IntSetting("Max number of exo fins", new SliderInteractionInterface(0, 5, 1), maxNumberOfExoFins);
        _maxNumberOfEndoFins = new IntSetting("Max number of endo fins", new SliderInteractionInterface(0, 5, 1), maxNumberOfEndoFins);
        _allowCannibalism = new BooleanSetting("Cannibalism allowed", allowCannibalism);
        AddSetting(_unitCount);
        AddSetting(_maxNumberOfExoFins);
        AddSetting(_maxNumberOfEndoFins);
        AddSetting(_allowCannibalism);
    }
    
    public override void Apply(ISudokuSolverData solverData)
    {
        for (int number = 1; number <= 9; number++)
        {
            var positions = solverData.PositionsFor(number);
            List<int> possibleCoverHouses = new();
            
            for (int i = 0; i < CoverHouses.Length; i++)
            {
                var current = CoverHouses[i];
                if (UnitMethods.Get(current.Unit).Count(positions, current.Number) > 0) possibleCoverHouses.Add(i);
            }
            
            for (int unitCount = _unitCount.Value.Min; unitCount <= _unitCount.Value.Max; unitCount++)
            {
                foreach (var combination in CombinationCalculator.EveryCombinationWithSpecificCount(unitCount, possibleCoverHouses))
                {
                    if (TryFind(solverData, number, combination)) return;
                }
            }
        }
    }

    private readonly GridPositions _toCover = new();
    private readonly HashSet<House> _baseSet = new();
    private readonly GridPositions _buffer = new();
    private readonly HashSet<Cell> _endoFins = new();
    
    private bool TryFind(ISudokuSolverData solverData, int number, int[] combination)
    {
        _toCover.Void();
        _baseSet.Clear();
        _endoFins.Clear();
        var positions = solverData.PositionsFor(number);
        
        foreach (var n in combination)
        {
            var house = CoverHouses[n];
            var methods = UnitMethods.Get(house.Unit);

            if (methods.Count(_toCover, house.Number) > 0)
            {
                if (_maxNumberOfEndoFins.Value == 0) return false;

                foreach (var c in methods.EveryCell(_toCover, house.Number))
                {
                    _endoFins.Add(c);
                    if (_endoFins.Count > _maxNumberOfEndoFins.Value) return false;
                }
            }
            
            methods.Fill(_buffer, house.Number);
            _buffer.ApplyAnd(positions);
            _toCover.ApplyOr(_buffer);
            
            _baseSet.Add(house);
            _buffer.Void();
        }

        if (_maxNumberOfExoFins.Value == 0)
        {
            foreach (var coverSet in _toCover.PossibleCoverHouses(combination.Length, _baseSet, UnitMethods.All))
            {
                if (Process(solverData, number, coverSet, _buffer)) return true;
            }
        }
        else
        {
            foreach (var coveredGrid in _toCover.PossibleCoveredGrids(combination.Length, 3, _baseSet,
                         UnitMethods.All))
            {
                if (Process(solverData, number, coveredGrid.CoverHouses, coveredGrid.Remaining)) return true;
            }
        }
        
        return false;
    }

    private readonly List<Cell> _fins = new();

    private bool Process(ISudokuSolverData solverData, int number, House[] coverSet, IReadOnlyGridPositions exoFins)
    {
        _fins.Clear();
        var gpOfCoverSet = new GridPositions();
        
        foreach (var set in coverSet)
        {
            UnitMethods.Get(set.Unit).Fill(gpOfCoverSet, set.Number);
        }
        var diff = gpOfCoverSet.Difference(_toCover);

        _fins.AddRange(exoFins);
        _fins.AddRange(_endoFins);
        
        if (_fins.Count == 0)
        {
            foreach (var cell in diff)
            {
                solverData.ChangeBuffer.ProposePossibilityRemoval(number, cell);
            } 
        }
        else
        {
            foreach (var ssc in SudokuCellUtility.SharedSeenCells(_fins))
            {
                if (!diff.Contains(ssc)) continue;
                    
                solverData.ChangeBuffer.ProposePossibilityRemoval(number, ssc);
            }
        }

        if (_allowCannibalism.Value) ProcessCannibalism(solverData, number, coverSet);
        
        return solverData.ChangeBuffer.NotEmpty() && solverData.ChangeBuffer.Commit(
                new FishReportBuilder(new HashSet<House>(_baseSet), coverSet, number,
                    _toCover.Copy(), new List<Cell>(_fins))) && StopOnFirstPush;
    }

    private void ProcessCannibalism(ISudokuSolverData solverData, int number, House[] coverSet)
    {
        foreach (var cell in _toCover)
        {
            var count = 0;
            foreach (var house in coverSet)
            {
                if (UnitMethods.Get(house.Unit).Contains(cell, house.Number))
                {
                    count++;
                    if (count >= 2) break;
                }
            }

            if (count < 2) return;
            
            bool ok = true;
            foreach (var fin in _fins)
            {
                if (!SudokuCellUtility.ShareAUnit(fin, cell))
                {
                    ok = false;
                    break;
                }
            }
            
            if(ok) solverData.ChangeBuffer.ProposePossibilityRemoval(number, cell);
        }
    }
}

public class FishReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly HashSet<House> _baseSet;
    private readonly House[] _coveredSet;
    private readonly int _possibility;
    private readonly GridPositions _inCommon;
    private readonly List<Cell> _fins;

    public FishReportBuilder(HashSet<House> baseSet, House[] coveredSet, int possibility,
        GridPositions inCommon, List<Cell> fins)
    {
        _baseSet = baseSet;
        _coveredSet = coveredSet;
        _possibility = possibility;
        _inCommon = inCommon;
        _fins = fins;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>( Explanation(), lighter =>
        {
            foreach (var cell in _inCommon)
            {
                lighter.HighlightPossibility(_possibility, cell.Row, cell.Column, ChangeColoration.CauseOffOne);
            }

            foreach (var cell in _fins)
            {
                lighter.HighlightPossibility(_possibility, cell.Row, cell.Column, ChangeColoration.CauseOffTwo);
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
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

        var coverSetBuilder = new StringBuilder(_coveredSet[0].ToString());
        for (int i = 1; i < _coveredSet.Length; i++)
        {
            coverSetBuilder.Append(", " + _coveredSet[i]);
        }

        string isFinned = _fins.Count > 0 ? "Finned " : "";
        string fins = "Fins : ";
        if (_fins.Count > 0)
        {
            var finsBuilder = new StringBuilder(_fins[0].ToString());
            for (int i = 1; i < _fins.Count; i++)
            {
                finsBuilder.Append(", " + _fins[i]);
            }

            fins += finsBuilder.ToString();
        }

        return $"{isFinned}{type} found :\nBase set : {baseSetBuilder.ToString()[..^2]}\nCover set : {coverSetBuilder}" +
               $"\n{fins}";
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}