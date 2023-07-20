namespace Model.Logs;

public class PossibilityRemovedLog : ISolverLog
{
    public string Title { get; }
    public Intensity Intensity { get; }
    public string Text { get; private set; }

    public PossibilityRemovedLog(int number, int row, int col)
    {
        Title = "";
        Text = $"[{row + 1}, {col + 1}] {number} removed from possibilities";
        Intensity = Intensity.Zero;
    }
    
    public PossibilityRemovedLog(int number, int row, int col, IStrategy strategy)
    {
        Title = strategy.Name;
        Text = $"[{row + 1}, {col + 1}] {number} removed from possibilities";
        Intensity = (Intensity) strategy.Difficulty;
    }
    
    public void Another(int number, int row, int col)
    {
        Text += $"\n[{row + 1}, {col + 1}] {number} removed from possibilities";
    }

}