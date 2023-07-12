namespace Model;

public interface IStrategy
{
    void ApplyOnce(ISolver solver);
}

public enum StrategyLevel
{
    None, Basic, Easy, Medium, Hard, Ultimate
}