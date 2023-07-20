namespace Model.Logs;

public class NumberAddedLog : ISolverLog
{
    public string Title { get; }
    public Intensity Intensity { get; }
    public string Text { get; private set; }

    public NumberAddedLog(int number, int row, int col)
    {
        Title = "";
        Text = $"[{row + 1}, {col + 1}] {number} added as definitive";
        Intensity = Intensity.Zero;
    }

    public NumberAddedLog(int number, int row, int col, IStrategy strategy)
    {
        Title = strategy.Name;
        Text = $"[{row + 1}, {col + 1}] {number} added as definitive";
        Intensity = (Intensity) strategy.Difficulty;
    }

    public void Another(int number, int row, int col)
    {
        Text += $"\n[{row + 1}, {col + 1}] {number} added as definitive";
    }

    
}