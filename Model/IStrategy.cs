namespace Model;

public interface IStrategy
{
    bool ApplyOnce(ISolver solver);
    bool ApplyUntilProgress(ISolver solver);
}

public enum StrategyLevel
{
    None, Easy, Medium, Hard
}