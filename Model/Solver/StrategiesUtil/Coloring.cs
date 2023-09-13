namespace Model.Solver.StrategiesUtil;

public enum Coloring
{
    None, On, Off
}

public interface IColorable
{
    public Coloring Coloring { get; set; }
}