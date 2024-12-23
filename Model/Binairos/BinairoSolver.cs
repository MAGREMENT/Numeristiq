﻿using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Graphs;
using Model.Core.Graphs.Implementations;
using Model.Core.Highlighting;
using Model.Utility;

namespace Model.Binairos;

public class BinairoSolver : BinaryStrategySolver<Strategy<IBinairoSolverData>, IBinarySolvingState,
    IBinairoHighlighter>, IBinairoSolverData
{
    private Binairo _binairo = new();
    private int _solutionCount;
    
    public override IBinarySolvingState StartState { get; protected set; } = new DefaultBinarySolvingState(0, 0);
    
    public ConstructedGraph<IBinairoSolverData, IGraph<CellPossibility, LinkStrength>> ManagedGraph { get; }

    public BinairoSolver()
    {
        ManagedGraph = new ConstructedGraph<IBinairoSolverData, IGraph<CellPossibility, LinkStrength>>(
            new HDictionaryLinkGraph<CellPossibility>(), this);
    }

    public void SetBinairo(Binairo binairo)
    {
        _binairo = binairo;

        OnNewSolvable();
        _solutionCount = _binairo.GetSolutionCount();
    }
    
    protected override IBinarySolvingState GetSolvingState()
    {
        return new DefaultBinarySolvingState(this);
    }

    public override bool IsResultCorrect()
    {
        return _binairo.IsCorrect();
    }

    public override bool HasSolverFailed()
    {
        return false; //TODO
    }

    protected override void OnChangeMade()
    {
        ManagedGraph.Clear();
    }

    protected override void ApplyStrategy(Strategy<IBinairoSolverData> strategy)
    {
        strategy.Apply(this);
    }

    protected override bool IsComplete()
    {
        return _solutionCount == _binairo.RowCount * _binairo.ColumnCount;
    }

    protected override IBinarySolvingState ApplyChangesToState(IBinarySolvingState state, IEnumerable<BinaryChange> changes)
    {
        var copy = DefaultBinarySolvingState.Copy(state);
        foreach (var change in changes)
        {
            copy[change.Row, change.Column] = change.Number;
        }

        return copy;
    }

    public override bool CanAddSolution(CellPossibility cell)
    {
        return _binairo[cell.Row, cell.Column] == 0;
    }

    protected override bool AddSolution(int number, int row, int col)
    {
        if (_binairo[row, col] != 0) return false;

        _currentState = null;
        _binairo[row, col] = number;
        _solutionCount++;
        return true;
    }

    public int RowCount => _binairo.RowCount;
    public int ColumnCount => _binairo.ColumnCount;

    public int this[int row, int col] => _binairo[row, col];
    public IReadOnlyBinairo Binairo => _binairo;
}

public interface IBinairoSolverData : IBinarySolvingState
{
    BinaryChangeBuffer<IBinarySolvingState, IBinairoHighlighter> ChangeBuffer { get; }
    IReadOnlyBinairo Binairo { get; }
    ConstructedGraph<IBinairoSolverData, IGraph<CellPossibility, LinkStrength>> ManagedGraph { get; }
}