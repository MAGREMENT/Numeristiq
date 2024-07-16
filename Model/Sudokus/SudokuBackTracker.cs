using Model.Core.BackTracking;
using Model.Sudokus.Solver.Position;

namespace Model.Sudokus;

public class SudokuBackTracker : BackTracker<Sudoku, IPossibilitiesGiver>
{
    private readonly GridPositions[] _positions = 
    {
        new(), new(), new(), new(), new(), new(), new(), new(), new()
    };
    
    public SudokuBackTracker() : base(new Sudoku(), new EmptyPossibilitiesGiver()) { }

    public SudokuBackTracker(Sudoku puzzle, IPossibilitiesGiver data) : base(puzzle, data) { }

    protected override bool Search(int position)
    {
        for (; position < 81; position++)
        {
            var row = position / 9;
            var col = position % 9;

            if (Current[row, col] != 0) continue;
            
            foreach (var possibility in _giver.EnumeratePossibilitiesAt(row, col))
            {
                var pos = _positions[possibility - 1];
                if (pos.IsRowNotEmpty(row) || pos.IsColumnNotEmpty(col) || pos.IsMiniGridNotEmpty(row / 3, col / 3)) continue;

                Current[row, col] = possibility;
                pos.Add(row, col);

                if (Search(position + 1)) return true;

                Current[row, col] = 0;
                pos.Remove(row, col);
            }

            return false;
        }

        return true;
    }
    
    protected override bool Search(IBackTrackingResult<Sudoku> result, int position)
    {
        for (; position < 81; position++)
        {
            var row = position / 9;
            var col = position % 9;

            if (Current[row, col] != 0) continue;
            
            foreach (var possibility in _giver.EnumeratePossibilitiesAt(row, col))
            {
                var pos = _positions[possibility - 1];
                if (pos.IsRowNotEmpty(row) || pos.IsColumnNotEmpty(col) || pos.IsMiniGridNotEmpty(row / 3, col / 3)) continue;

                Current[row, col] = possibility;
                pos.Add(row, col);

                bool search = Search(result, position + 1);
                
                Current[row, col] = 0;
                pos.Remove(row, col);

                if (search) return true;
            }

            return false;
        }
        
        result.AddNewResult(Current.Copy());
        return result.Count >= StopAt;
    }

    protected override void Initialize(bool reset)
    {
        if (reset)
        {
            foreach(var p in _positions) p.Void();
        }
        
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var n = Current[row, col];
                if (n == 0) continue;

                _positions[n - 1].Add(row, col);
            }
        }
    }
}