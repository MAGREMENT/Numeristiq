namespace View;

public interface IPageHandler
{
    void ShowPage(PagesName pageName);
}

public enum PagesName
{
    First, Solver, Player, StrategyManager
}