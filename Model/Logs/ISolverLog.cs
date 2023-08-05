using System.Collections.Generic;

namespace Model.Logs;

public interface ISolverLog
{
    public int Id { get; }
    public string Title { get; }
    public Intensity Intensity { get; }
    public string Changes { get; }
    public string Explanation { get; }
    public string SolverState { get; }
    public HighLightSolver SolverHighLighter { get; }

}

public enum Intensity
{
    Zero, One, Two, Three, Four, Five, Six
}