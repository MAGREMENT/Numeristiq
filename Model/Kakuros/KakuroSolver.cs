using Model.Core;
using Model.Core.Highlighting;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Kakuros;

public class KakuroSolver : NumericStrategySolver<Strategy<IKakuroSolverData>, IUpdatableSolvingState, ISolvingStateHighlighter>,
    IKakuroSolverData
{
    private IKakuro _kakuro = new SumListKakuro();
    private ReadOnlyBitSet16[,] _possibilities = new ReadOnlyBitSet16[0, 0];
    
    public IKakuroCombinationCalculator CombinationCalculator { get; }
    public IReadOnlyKakuro Kakuro => _kakuro;

    public KakuroSolver(IKakuroCombinationCalculator combinationCalculator)
    {
        CombinationCalculator = combinationCalculator;
    }

    public void SetKakuro(IKakuro kakuro)
    {
        _kakuro = kakuro;
        _possibilities = new ReadOnlyBitSet16[kakuro.RowCount, kakuro.ColumnCount];
        InitPossibilities();
        OnNewSolvable(_kakuro.GetSolutionCount());
    }

    public int this[int row, int col] => _kakuro[row, col];

    public ReadOnlyBitSet16 PossibilitiesAt(int row, int col) => _possibilities[row, col];

    public override bool CanRemovePossibility(CellPossibility cp) => _possibilities[cp.Row, cp.Column].Contains(cp.Possibility);

    public override bool CanAddSolution(CellPossibility cp) => _possibilities[cp.Row, cp.Column].Contains(cp.Possibility)
                                                      && _kakuro[cp.Row, cp.Column] == 0;

    protected override IUpdatableSolvingState GetSolvingState()
    {
        return new KakuroSolvingState(this);
    }

    public override bool IsResultCorrect()
    {
        return _kakuro.IsComplete(); //TODO
    }

    public override bool HasSolverFailed()
    {
        return false; //TODO
    }

    protected override bool IsComplete()
    {
        return _solutionCount == _kakuro.GetCellCount();
    }

    #region Private

    private void InitPossibilities()
    {
        foreach (var cell in _kakuro.EnumerateCells())
        {
            _possibilities[cell.Row, cell.Column] = ReadOnlyBitSet16.Filled(1, 9);
        }

        foreach (var sum in _kakuro.Sums)
        {
            var pos = CombinationCalculator.CalculatePossibilities(sum.Amount, sum.Length);
            foreach (var cell in sum)
            {
                _possibilities[cell.Row, cell.Column] &= pos;
            }
        }
    }

    protected override bool RemoveSolution(int row, int col)
    {
        if (_kakuro[row, col] == 0) return false;
        
        _currentState = null;
        _kakuro[row, col] = 0;
        InitPossibilities();
        return true;
    }

    protected override bool RemovePossibility(int n, int row, int col)
    {
        if (!_possibilities[row, col].Contains(n)) return false;

        _currentState = null;
        _possibilities[row, col] -= n;
        return true;
    }

    protected override void OnChangeMade()
    {
        
    }

    protected override void ApplyStrategy(Strategy<IKakuroSolverData> strategy)
    {
        strategy.Apply(this);
    }

    protected override bool AddSolution(int n, int row, int col)
    {
        if (_kakuro[row, col] != 0) return false;

        _currentState = null;
        _possibilities[row, col] = new ReadOnlyBitSet16();
        _kakuro[row, col] = n;
        UpdatePossibilitiesAfterSolutionAdded(row, col);

        return true;
    }

    private void UpdatePossibilitiesAfterSolutionAdded(int row, int col)
    {
        foreach (var sum in _kakuro.SumsFor(new Cell(row, col)))
        {
            var pos = CombinationCalculator.CalculatePossibilities(sum.Amount,
                sum.Length, _kakuro.GetSolutions(sum));

            foreach (var cell in sum)
            {
                if (cell.Row == row && cell.Column == col) continue;

                _possibilities[cell.Row, cell.Column] &= pos;
            }
        }
    }

    #endregion
}