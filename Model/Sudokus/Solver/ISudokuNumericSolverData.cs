using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.AlmostLockedSets;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver;

public interface ISudokuSolverData : ISudokuSolvingState, IPossibilitiesGiver
{ 
    IReadOnlySudoku Sudoku { get; }
    
    ISudokuSolvingState CurrentState { get; }
    
    ISudokuSolvingState StartState { get; }

    IChangeBuffer<ISudokuSolvingState, ISudokuHighlighter> ChangeBuffer { get; }
    
    SudokuPreComputer PreComputer { get; }
    
    AlmostHiddenSetSearcher AlmostHiddenSetSearcher { get; }
    
    AlmostNakedSetSearcher AlmostNakedSetSearcher { get; }

    bool UniquenessDependantStrategiesAllowed { get; }
    
    public bool FastMode { get; set; }

    public ReadOnlyBitSet16 RawPossibilitiesAt(int row, int col);
    
    public ReadOnlyBitSet16 RawPossibilitiesAt(Cell cell) => RawPossibilitiesAt(cell.Row, cell.Column);
}





