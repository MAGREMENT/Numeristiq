﻿using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Changes.Buffers;
using Model.Helpers.Graphs;
using Model.Helpers.Highlighting;
using Model.Helpers.Steps;
using Model.Tectonics.Strategies;
using Model.Tectonics.Strategies.AlternatingInference;
using Model.Tectonics.Strategies.AlternatingInference.Types;
using Model.Tectonics.Utility;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Tectonics;

public class TectonicSolver : ITectonicStrategyUser, IStepManagingChangeProducer<IUpdatableTectonicSolvingState,
    ITectonicHighlighter>, ISolvingState
{
    private ITectonic _tectonic;
    private ReadOnlyBitSet8[,] _possibilities;

    private readonly TectonicStrategy[] _strategies = 
    { 
        new NakedSingleStrategy(),
        new HiddenSingleStrategy(),
        new ZoneInteractionStrategy(),
        new AlternatingInferenceGeneralization(new XChainType()),
        new GroupEliminationStrategy(),
        new AlternatingInferenceGeneralization(new AlternatingInferenceChainType()),
        new BruteForceStrategy()
    };

    private bool _changeWasMade;
    
    private IUpdatableTectonicSolvingState? _currentState;
    
    public bool StartedSolving { get; private set; }

    public StepHistory<ITectonicHighlighter> StepHistory { get; } = new();

    public IUpdatableTectonicSolvingState CurrentState
    {
        get
        {
            _currentState ??= new StateArraySolvingState(this);
            return _currentState;
        }
    }

    public IChangeBuffer<IUpdatableTectonicSolvingState, ITectonicHighlighter> ChangeBuffer { get; set; }

    public LinkGraphManager<ITectonicStrategyUser, ITectonicElement> Graphs { get; }

    public IReadOnlyTectonic Tectonic => _tectonic;

    public event OnProgressMade? ProgressMade;

    public TectonicSolver()
    {
        _tectonic = new BlankTectonic();
        _possibilities = new ReadOnlyBitSet8[0, 0];

        ChangeBuffer = new FastChangeBuffer<IUpdatableTectonicSolvingState, ITectonicHighlighter>(this);
        Graphs = new LinkGraphManager<ITectonicStrategyUser, ITectonicElement>(this, new TectonicConstructRuleBank());
    }

    public void SetTectonic(ITectonic tectonic)
    {
        _tectonic = tectonic;
        _possibilities = new ReadOnlyBitSet8[_tectonic.RowCount, _tectonic.ColumnCount];
        InitCandidates();
        
        StepHistory.Clear();
        Graphs.Clear();
        
        StartedSolving = false;
    }

    public void SetSolutionByHand(int number, int row, int col)
    {
        if (_tectonic[row, col] != 0) RemoveSolution(row, col);

        var before = CurrentState;
        if (!AddSolution(row, col, number, false)) return;
        
        if(StartedSolving && ChangeBuffer.IsManagingSteps)
            StepHistory.AddByHand(number, row, col, ProgressType.SolutionAddition, before);
    }

    public void RemoveSolutionByHand(int row, int col)
    {
        if (StartedSolving) return;

        RemoveSolution(row, col);
    }

    public void Solve(bool stopAtProgress = false)
    {
        for (int i = 0; i < _strategies.Length; i++)
        {
            _strategies[i].Apply(this);
            ChangeBuffer.Push(_strategies[i]);

            if (!_changeWasMade) continue;

            ProgressMade?.Invoke();
            ResetChangeTrackingVariables();
            i = -1;
            Graphs.Clear();
            
            if (stopAtProgress || Tectonic.IsComplete()) return; //TODO optimize isComplete with buffer
        }
    }

    ReadOnlyBitSet16 ISolvingState.PossibilitiesAt(int row, int col)
    {
        return ReadOnlyBitSet16.FromBitSet(PossibilitiesAt(row, col));
    }

    public ReadOnlyBitSet8 PossibilitiesAt(Cell cell)
    {
        return _possibilities[cell.Row, cell.Column];
    }
    
    public ReadOnlyBitSet8 PossibilitiesAt(int row, int col)
    {
        return _possibilities[row, col];
    }
    
    public int this[int row, int col] => _tectonic[row, col];
    
    public ReadOnlyBitSet8 ZonePositionsFor(int zone, int n) //TODO To buffer
    {
        var result = new ReadOnlyBitSet8();
        var z = _tectonic.Zones[zone];

        for (int i = 0; i < z.Count; i++)
        {
            var cell = z[i];
            if (_possibilities[cell.Row, cell.Column].Contains(n)) result += i;
        }

        return result;
    }
    
    public ReadOnlyBitSet8 ZonePositionsFor(IZone zone, int n)
    {
        var result = new ReadOnlyBitSet8();

        for (int i = 0; i < zone.Count; i++)
        {
            var cell = zone[i];
            if (_possibilities[cell.Row, cell.Column].Contains(n)) result += i;
        }

        return result;
    }

    public bool CanRemovePossibility(CellPossibility cp)
    {
        return _possibilities[cp.Row, cp.Column].Contains(cp.Possibility);
    }

    public bool CanAddSolution(CellPossibility cp)
    {
        return _tectonic[cp.Row, cp.Column] == 0;
    }

    public bool ExecuteChange(SolverProgress progress)
    {
        return progress.ProgressType == ProgressType.PossibilityRemoval 
            ? RemovePossibility(progress.Row, progress.Column, progress.Number, true) 
            : AddSolution(progress.Row, progress.Column, progress.Number, true);
    }

    public void FakeChange()
    {
        _changeWasMade = true;
    }

    #region Private

    private void ResetChangeTrackingVariables()
    {
        _changeWasMade = false;
    }
    
    private void InitCandidates()
    {
        for (int row = 0; row < _tectonic.RowCount; row++)
        {
            for (int col = 0; col < _tectonic.ColumnCount; col++)
            {
                _possibilities[row, col] = ReadOnlyBitSet8.Filled(1, _tectonic.GetZone(row, col).Count);
            }
        }
        
        for (int row = 0; row < _tectonic.RowCount; row++)
        {
            for (int col = 0; col < _tectonic.ColumnCount; col++)
            {
                var n = _tectonic[row, col];
                if(n != 0) UpdatePossibilitiesAfterSolutionAdded(row, col, n);
            }
        }
    }

    private void UpdatePossibilitiesAfterSolutionAdded(int row, int col, int number)
    {
        _possibilities[row, col] = new ReadOnlyBitSet8();

        foreach (var neighbor in TectonicCellUtility.GetNeighbors(row, col, _tectonic.RowCount, _tectonic.ColumnCount))
        {
            _possibilities[neighbor.Row, neighbor.Column] -= number;
        }

        foreach (var cell in _tectonic.GetZone(row, col))
        {
            _possibilities[cell.Row, cell.Column] -= number;
        }
    }

    private bool RemovePossibility(int row, int col, int number, bool fromSolving)
    {
        if (!_possibilities[row, col].Contains(number)) return false;

        _currentState = null;
        _possibilities[row, col] -= number;

        if (fromSolving)
        {
            _changeWasMade = true;
        }

        return true;
    }

    private bool AddSolution(int row, int col, int number, bool fromSolving)
    {
        if (_tectonic[row, col] != 0) return false;

        _currentState = null;
        _tectonic[row, col] = number;
        UpdatePossibilitiesAfterSolutionAdded(row, col, number);

        if (fromSolving)
        {
            _changeWasMade = true;
        }
        
        return true;
    }

    private void RemoveSolution(int row, int col)
    {
        if (_tectonic[row, col] == 0) return;

        _currentState = null;
        _tectonic[row, col] = 0;
        InitCandidates();
    }

    #endregion

    public IEnumerable<int> EnumeratePossibilitiesAt(int row, int col)
    {
        return _possibilities[row, col].EnumeratePossibilities();
    }
}

public delegate void OnProgressMade();