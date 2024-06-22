using System.Collections.Generic;
using Model.Core;
using Model.Core.Graphs;
using Model.Core.Highlighting;
using Model.Tectonics.Solver.Utility;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Tectonics.Solver;

public class TectonicSolver : NumericStrategySolver<Strategy<ITectonicSolverData>, IUpdatableTectonicSolvingState,
    ITectonicHighlighter>, ITectonicSolverData, INumericSolvingState
{
    private ITectonic _tectonic;
    private ReadOnlyBitSet8[,] _possibilities;

    public IReadOnlyTectonic Tectonic => _tectonic;
    public LinkGraphManager<ITectonicSolverData, ITectonicElement> Graphs { get; }

    public TectonicSolver()
    {
        _tectonic = new BlankTectonic();
        _possibilities = new ReadOnlyBitSet8[0, 0];
        
        Graphs = new LinkGraphManager<ITectonicSolverData, ITectonicElement>(this, new TectonicConstructRuleBank());
    }

    public void SetTectonic(ITectonic tectonic)
    {
        _tectonic = tectonic;
        _possibilities = new ReadOnlyBitSet8[_tectonic.RowCount, _tectonic.ColumnCount];
        InitCandidates();
        
        OnNewSolvable(_tectonic.GetSolutionCount());
        Graphs.Clear();
    }

    ReadOnlyBitSet16 INumericSolvingState.PossibilitiesAt(int row, int col)
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

    public override bool CanRemovePossibility(CellPossibility cp)
    {
        return _possibilities[cp.Row, cp.Column].Contains(cp.Possibility);
    }

    public override bool CanAddSolution(CellPossibility cp)
    {
        return _tectonic[cp.Row, cp.Column] == 0;
    }

    protected override IUpdatableTectonicSolvingState GetSolvingState()
    {
        return new StateArraySolvingState(this);
    }

    public override bool IsResultCorrect()
    {
        return _tectonic.IsCorrect();
    }

    public override bool HasSolverFailed()
    {
        return false; //TODO
    }

    protected override bool IsComplete()
    {
        return _solutionCount == _tectonic.RowCount * _tectonic.ColumnCount;
    }
    
    #region Private
    
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

    protected override bool RemovePossibility(int number, int row, int col)
    {
        if (!_possibilities[row, col].Contains(number)) return false;

        _currentState = null;
        _possibilities[row, col] -= number;
        
        return true;
    }

    protected override bool AddSolution(int number, int row, int col)
    {
        if (_tectonic[row, col] != 0) return false;

        _currentState = null;
        _tectonic[row, col] = number;
        UpdatePossibilitiesAfterSolutionAdded(row, col, number);
        
        return true;
    }

    protected override bool RemoveSolution(int row, int col)
    {
        if (_tectonic[row, col] == 0) return false;

        _currentState = null;
        _tectonic[row, col] = 0;
        InitCandidates();

        return true;
    }

    protected override void OnChangeMade()
    {
        Graphs.Clear();
    }

    protected override void ApplyStrategy(Strategy<ITectonicSolverData> strategy)
    {
        strategy.Apply(this);
    }

    #endregion

    public IEnumerable<int> EnumeratePossibilitiesAt(int row, int col)
    {
        return _possibilities[row, col].EnumeratePossibilities();
    }
}