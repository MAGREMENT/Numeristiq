using System.Collections.Generic;

namespace Model.Solver.StrategiesUtil.CellColoring.ColoringResults;

public class ColoringDictionary<T> : Dictionary<T, Coloring>, IColoringResult<T> where T : notnull
{
    
}