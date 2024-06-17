using Model.Sudokus.Player.HistoricEvents;
using Model.Sudokus.Solver.Position;

namespace Model.Sudokus.Player.Actions;

public class ComputePossibilitiesAction : IGlobalAction
{
    private readonly PossibilitiesLocation _location;
    private GridPositions[]? _buffer;

    public ComputePossibilitiesAction(PossibilitiesLocation location)
    {
        _location = location;
    }

    public bool CanExecute(IReadOnlyPlayerData data)
    {
        _buffer = new[]
        {
            new GridPositions(), new GridPositions(), new GridPositions(),
            new GridPositions(), new GridPositions(), new GridPositions(),
            new GridPositions(), new GridPositions(), new GridPositions()
        };
        bool changeWillBeMade = false;
        
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var pc = data.GetCellDataFor(row, col);
                if (pc.IsNumber())
                {
                    var gp = _buffer[pc.Number() - 1];
                    gp.FillRow(row);
                    gp.FillColumn(col);
                    gp.FillMiniGrid(row / 3, col / 3);
                }
                else
                {
                    if (!pc.Editable) return false;
                    if (pc.PossibilitiesCount(_location) == 0) changeWillBeMade = true;
                }
            }
        }

        if (!changeWillBeMade) changeWillBeMade = WillChangeBeMade(data);

        return changeWillBeMade;
    }

    private bool WillChangeBeMade(IReadOnlyPlayerData data)
    {
        if (_buffer is null) return false;
        
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var pc = data.GetCellDataFor(row, col);
                for (int p = 1; p <= 9; p++)
                {
                    if (_buffer[p - 1].Contains(row, col) && pc.PeekPossibility(p, _location)) return true;
                }
            }
        }

        return false;
    }

    public IHistoricEvent? Execute(IPlayerData data)
    {
        if (_buffer is null) return null;
        
        var before = data.CopyCellData();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var pc = data.GetCellDataFor(row, col);
                if (pc.PossibilitiesCount(_location) == 0) pc.FillPossibilities(_location);
                for (int p = 1; p <= 9; p++)
                {
                    if (_buffer[p - 1].Contains(row, col)) pc.RemovePossibility(p, _location);
                }
                
                data.SetCellDataFor(row, col, pc);
            }
        }

        _buffer = null;
        return new AllCellsChangeEvent(before, data.CopyCellData());
    }
}