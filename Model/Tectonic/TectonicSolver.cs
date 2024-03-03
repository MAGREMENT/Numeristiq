using Model.Helpers.Changes;
using Model.Helpers.Changes.Buffers;
using Model.Sudoku.Solver.BitSets;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Tectonic.Strategies;
using Model.Utility;

namespace Model.Tectonic;

public class TectonicSolver : IStrategyUser, IChangeProducer
{
    private ITectonic _tectonic;
    private ReadOnlyBitSet16[,] _possibilities;

    private readonly TectonicStrategy[] _strategies = { new NakedSingleStrategy(), new HiddenSingleStrategy(),
        new CommonCellsStrategy(), new XChainsStrategy() };

    private int _possibilityRemovedBuffer;
    private int _solutionAddedBuffer;

    public IChangeBuffer ChangeBuffer { get; }

    public TectonicSolver()
    {
        _tectonic = new BlankTectonic();
        _possibilities = new ReadOnlyBitSet16[0, 0];

        ChangeBuffer = new FastChangeBuffer(this);
    }

    public void SetTectonic(ITectonic tectonic)
    {
        _tectonic = tectonic;
        _possibilities = new ReadOnlyBitSet16[_tectonic.RowCount, _tectonic.ColumnCount];
        InitCandidates();
    }

    public IReadOnlyTectonic Tectonic => _tectonic;

    public ReadOnlyBitSet16 PossibilitiesAt(Cell cell)
    {
        return _possibilities[cell.Row, cell.Column];
    }
    
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

    public void Solve()
    {
        for (int i = 0; i < _strategies.Length; i++)
        {
            _strategies[i].Apply(this);
            ChangeBuffer.Push(_strategies[i]);

            if (_solutionAddedBuffer == 0 && _possibilityRemovedBuffer == 0) continue;

            _solutionAddedBuffer = 0;
            _possibilityRemovedBuffer = 0;
            i = -1;
        }
    }
    
    //Private-----------------------------------------------------------------------------------------------------------
    
    private void InitCandidates()
    {
        foreach (var zone in _tectonic.Zones)
        {
            foreach (var cell in zone)
            {
                _possibilities[cell.Row, cell.Column] = ReadOnlyBitSet16.Filled(1, zone.Count);
            }
        }

        foreach (var cellNumber in _tectonic.EachCellNumber())
        {
            if (!cellNumber.IsSet()) continue;

            UpdatePossibilitiesAfterSolutionAdded(cellNumber.Cell.Row, cellNumber.Cell.Column, cellNumber.Number);
        }
    }

    private void UpdatePossibilitiesAfterSolutionAdded(int row, int col, int number)
    {
        _possibilities[row, col] = new ReadOnlyBitSet16();

        foreach (var neighbor in _tectonic.GetNeighbors(row, col))
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
        
        _possibilities[row, col] -= number;
        _possibilityRemovedBuffer++;

        return true;
    }

    private bool AddSolution(int row, int col, int number)
    {
        if (_tectonic[row, col] != 0) return false;
        
        _tectonic[row, col] = number;
        UpdatePossibilitiesAfterSolutionAdded(row, col, number);
        _solutionAddedBuffer++;
        
        return true;
    }
}