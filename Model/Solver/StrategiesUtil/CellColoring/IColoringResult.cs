namespace Model.Solver.StrategiesUtil.CellColoring;

public interface IColoringResult<in T>
{
    public void AddColoredElement(T element, Coloring coloring);
    public bool TryGetColoredElement(T element, out Coloring coloring);
    public void NewStart();
}