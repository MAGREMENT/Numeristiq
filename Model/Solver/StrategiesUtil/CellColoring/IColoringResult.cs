namespace Model.Solver.StrategiesUtil.CellColoring;

public interface IColoringResult<in T>
{
    public void Add(T element, Coloring coloring);
}