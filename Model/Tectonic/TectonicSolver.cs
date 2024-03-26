using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Changes.Buffers;
using Model.Helpers.Highlighting;
using Model.Helpers.Logs;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Tectonic.Strategies;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Tectonic;

public class TectonicSolver : IStrategyUser, ILogManagedChangeProducer<IUpdatableTectonicSolvingState,
    ITectonicHighlighter>, ISolvingState
{
    private ITectonic _tectonic;
    private ReadOnlyBitSet16[,] _possibilities;

    private readonly TectonicStrategy[] _strategies = { new NakedSingleStrategy(), new HiddenSingleStrategy(),
        new CommonCellsStrategy(), new NeighboringZonesStrategy() };

    private int _possibilityRemovedBuffer;
    private int _solutionAddedBuffer;
    private IUpdatableTectonicSolvingState? _currentState;

    public LogManager<ITectonicHighlighter> LogManager { get; } = new();

    public IUpdatableTectonicSolvingState CurrentState
    {
        get
        {
            _currentState ??= new StateArraySolvingState(this);
            return _currentState;
        }
    }

    public IChangeBuffer<IUpdatableTectonicSolvingState, ITectonicHighlighter> ChangeBuffer { get; set; }
    public IReadOnlyList<ISolverLog<ITectonicHighlighter>> Logs => LogManager.Logs;

    public event OnProgressMade? ProgressMade;

    public TectonicSolver()
    {
        _tectonic = new BlankTectonic();
        _possibilities = new ReadOnlyBitSet16[0, 0];

        ChangeBuffer = new FastChangeBuffer<IUpdatableTectonicSolvingState, ITectonicHighlighter>(this);
    }

    public void SetTectonic(ITectonic tectonic)
    {
        _tectonic = tectonic;
        _possibilities = new ReadOnlyBitSet16[_tectonic.RowCount, _tectonic.ColumnCount];
        InitCandidates();
        
        LogManager.Clear();
    }

    public IReadOnlyTectonic Tectonic => _tectonic;

    public ReadOnlyBitSet16 PossibilitiesAt(Cell cell)
    {
        return _possibilities[cell.Row, cell.Column];
    }
    
    public ReadOnlyBitSet16 PossibilitiesAt(int row, int col)
    {
        return _possibilities[row, col];
    }
    
    public int this[int row, int col] => _tectonic[row, col];
    
    public ReadOnlyBitSet16 ZonePositionsFor(int zone, int n)
    {
        var result = new ReadOnlyBitSet16();
        var z = _tectonic.Zones[zone];

        for (int i = 0; i < z.Count; i++)
        {
            var cell = z[i];
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
            ? RemovePossibility(progress.Row, progress.Column, progress.Number) 
            : AddSolution(progress.Row, progress.Column, progress.Number);
    }

    public void Solve(bool stopAtProgress = false)
    {
        for (int i = 0; i < _strategies.Length; i++)
        {
            _strategies[i].Apply(this);
            ChangeBuffer.Push(_strategies[i]);

            if (_solutionAddedBuffer == 0 && _possibilityRemovedBuffer == 0) continue;

            ProgressMade?.Invoke();
            _solutionAddedBuffer = 0;
            _possibilityRemovedBuffer = 0;
            if (stopAtProgress) return;
            
            i = -1;
        }
    }

    #region Private

    private void InitCandidates()
    {
        for (int row = 0; row < _tectonic.RowCount; row++)
        {
            for (int col = 0; col < _tectonic.ColumnCount; col++)
            {
                _possibilities[row, col] = ReadOnlyBitSet16.Filled(1, _tectonic.GetZone(row, col).Count);
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
        _possibilities[row, col] = new ReadOnlyBitSet16();

        foreach (var neighbor in TectonicCellUtility.GetNeighbors(row, col, _tectonic.RowCount, _tectonic.ColumnCount))
        {
            _possibilities[neighbor.Row, neighbor.Column] -= number;
        }

        foreach (var cell in _tectonic.GetZone(row, col))
        {
            _possibilities[cell.Row, cell.Column] -= number;
        }
    }

    private bool RemovePossibility(int row, int col, int number)
    {
        if (!_possibilities[row, col].Contains(number)) return false;

        _currentState = null;
        _possibilities[row, col] -= number;
        _possibilityRemovedBuffer++;

        return true;
    }

    private bool AddSolution(int row, int col, int number)
    {
        if (_tectonic[row, col] != 0) return false;

        _currentState = null;
        _tectonic.Set(number, row, col);
        UpdatePossibilitiesAfterSolutionAdded(row, col, number);
        _solutionAddedBuffer++;
        
        return true;
    }

    #endregion
}

public delegate void OnProgressMade();