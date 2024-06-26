﻿using Model.Utility;

namespace Model.Sudokus.Player.HistoricEvents;

public class CellChangeEvent : IHistoricEvent
{
    private readonly PlayerCell _from;
    private readonly PlayerCell _to;
    private readonly Cell _cell;

    public CellChangeEvent(PlayerCell from, PlayerCell to, Cell cell)
    {
        _from = from;
        _to = to;
        _cell = cell;
    }
    
    public void Do(IPlayerData data)
    {
        data.SetCellDataFor(_cell, _to);
    }

    public void Reverse(IPlayerData data)
    {
        data.SetCellDataFor(_cell, _from);
    }
}