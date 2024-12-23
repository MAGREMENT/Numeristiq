﻿using Model.Core;
using Model.Core.BackTracking;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Utility.AlmostLockedSets;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver;

public interface ISudokuSolverData : ISudokuSolvingState, IPossibilitiesGiver
{ 
    IReadOnlySudoku Sudoku { get; }
    
    ISudokuSolvingState CurrentState { get; }
    
    ISudokuSolvingState StartState { get; }

    NumericChangeBuffer<ISudokuSolvingState, ISudokuHighlighter> ChangeBuffer { get; }
    
    SudokuPreComputer PreComputer { get; }

    bool UniquenessDependantStrategiesAllowed { get; }
    
    public bool FastMode { get; set; }

    public ReadOnlyBitSet16 RawPossibilitiesAt(int row, int col);
    
    public ReadOnlyBitSet16 RawPossibilitiesAt(Cell cell) => RawPossibilitiesAt(cell.Row, cell.Column);
}





