namespace Model.Logs;

public interface ISolverLog
{
    public string Title { get; }
    public Intensity Intensity { get; }
    public string Text { get; }
}

public enum Intensity
{
    Zero, One, Two, Three, Four, Five, Six
}