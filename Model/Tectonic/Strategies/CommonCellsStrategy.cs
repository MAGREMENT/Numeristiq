﻿namespace Model.Tectonic.Strategies;

public class CommonCellsStrategy : AbstractStrategy
{
    public override void Apply(ISolvable solvable)
    {
        foreach (var zone in solvable.Tectonic.Zones)
        {
            
        }
    }
}