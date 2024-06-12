using Model.Core;
using Model.Sudokus.Player.HistoricEvents;

namespace Model.Sudokus.Player.Actions;

public class PasteAction : IPlayerGlobalAction
{
    private readonly PossibilitiesLocation _location;
    private readonly ISolvingState _state;

    public PasteAction(ISolvingState state, PossibilitiesLocation location)
    {
        _location = location;
        _state = state;
    }

    public bool CanExecute(IReadOnlyPlayerData data)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (!data.GetCellDataFor(row, col).Editable) return false;
            }
        }

        return true;
    }

    public IHistoricEvent Execute(IPlayerData data)
    {
        var before = data.CopyCellData();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var pc = data.GetCellDataFor(row, col);
                var number = _state[row, col];
                if (number != 0) pc.SetNumber(number);
                else pc.SetPossibilities(_state.PossibilitiesAt(row, col), _location);
                data.SetCellDataFor(row, col, pc);
            }
        }

        return new AllCellsChangeEvent(before, data.CopyCellData());
    }
}