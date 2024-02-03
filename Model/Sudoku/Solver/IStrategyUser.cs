using Model.Sudoku.Solver.BitSets;
using Model.Sudoku.Solver.Helpers;
using Model.Sudoku.Solver.Helpers.Changes;
using Model.Sudoku.Solver.StrategiesUtility.AlmostLockedSets;
using Model.Utility;

namespace Model.Sudoku.Solver;

public interface IStrategyUser : IPossibilitiesHolder
{ 
    SolverState StartState { get; }
    
    bool LogsManaged { get; }

    ChangeBuffer ChangeBuffer { get; }
    
    PreComputer PreComputer { get; }
    
    AlmostHiddenSetSearcher AlmostHiddenSetSearcher { get; }
    
    AlmostNakedSetSearcher AlmostNakedSetSearcher { get; }

    bool UniquenessDependantStrategiesAllowed { get; }

    public ReadOnlyBitSet16 RawPossibilitiesAt(int row, int col);
    public ReadOnlyBitSet16 RawPossibilitiesAt(Cell cell)
    {
        return RawPossibilitiesAt(cell.Row, cell.Column);
    }
}





