using System.Collections.Generic;

namespace Model.Logs;

public interface ISolverLog
{
    public int Id { get; }
    public string Title { get; }
    public Intensity Intensity { get; }
    public string Text { get; }
    public string SolverState { get; }

    public void DefinitiveAdded(int n, int row, int col);
    public void PossibilityRemoved(int p, int row, int col);

    public IEnumerable<LogChange> AllChanges();
    public IEnumerable<LogCause> AllCauses();
}

public enum Intensity
{
    Zero, One, Two, Three, Four, Five, Six
}

public enum SolverNumberType
{
    Possibility, Definitive
}

public class LogChange
{
    public LogChange(SolverNumberType numberType, int number, int row, int column)
    {
        NumberType = numberType;
        Number = number;
        Row = row;
        Column = column;
    }

    public SolverNumberType NumberType { get; }
    public int Number { get; }
    public int Row { get; }
    public int Column { get; }
}

public class LogCause : LogChange
{
    public CauseColoration Coloration { get; }
    
    public LogCause(SolverNumberType numberType, int number, int row, int column, CauseColoration coloration)
        : base(numberType, number, row, column)
    {
        Coloration = coloration;
    }
}

public enum CauseColoration
{
    One, Two
}